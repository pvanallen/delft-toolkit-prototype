# Copyright 2015 The TensorFlow Authors. All Rights Reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ==============================================================================

# modified to classify objects in photo taken when requested by OSC message
# returns list of object identifies by OSC

# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com
# created at TU Delft

# inspired by https://hackaday.com/2017/06/14/diy-raspberry-neural-network-sees-all-recognizes-some/



"""Simple image classification with Inception.

Run image classification with Inception trained on ImageNet 2012 Challenge data
set.

This program creates a graph from a saved GraphDef protocol buffer,
and runs inference on an input JPEG image. It outputs human readable
strings of the top 5 predictions along with their probabilities.

Change the --image_file argument to any jpg image to compute a
classification of that image.

Please see the tutorial and website for a detailed description of how
to use this script to perform image recognition.

https://tensorflow.org/tutorials/image_recognition/
"""

from __future__ import absolute_import
from __future__ import division
from __future__ import print_function

import argparse
import os.path
import os
import re
import sys
import tarfile

import numpy as np
from six.moves import urllib
import tensorflow as tf

import time
import picamera
from pythonosc import dispatcher
from pythonosc import osc_server
from pythonosc import osc_message_builder
from pythonosc import udp_client

from threading import Thread
import socket

# speech to text
import sys
import json
import urllib
import urllib.request
import subprocess
import pycurl
#import StringIO
from io import BytesIO
import os.path
import base64
import time
import RPi.GPIO as GPIO
import subprocess
from subprocess import Popen, PIPE, STDOUT

FLAGS = None

# pylint: disable=line-too-long
DATA_URL = 'http://download.tensorflow.org/models/image/imagenet/inception-2015-12-05.tgz'
# pylint: enable=line-too-long


class NodeLookup(object):
  """Converts integer node ID's to human readable labels."""

  def __init__(self,
               label_lookup_path=None,
               uid_lookup_path=None):
    if not label_lookup_path:
      label_lookup_path = os.path.join(
          FLAGS.model_dir, 'imagenet_2012_challenge_label_map_proto.pbtxt')
    if not uid_lookup_path:
      uid_lookup_path = os.path.join(
          FLAGS.model_dir, 'imagenet_synset_to_human_label_map.txt')
    self.node_lookup = self.load(label_lookup_path, uid_lookup_path)

  def load(self, label_lookup_path, uid_lookup_path):
    """Loads a human readable English name for each softmax node.

    Args:
      label_lookup_path: string UID to integer node ID.
      uid_lookup_path: string UID to human-readable string.

    Returns:
      dict from integer node ID to human-readable string.
    """
    if not tf.gfile.Exists(uid_lookup_path):
      tf.logging.fatal('File does not exist %s', uid_lookup_path)
    if not tf.gfile.Exists(label_lookup_path):
      tf.logging.fatal('File does not exist %s', label_lookup_path)

    # Loads mapping from string UID to human-readable string
    proto_as_ascii_lines = tf.gfile.GFile(uid_lookup_path).readlines()
    uid_to_human = {}
    p = re.compile(r'[n\d]*[ \S,]*')
    for line in proto_as_ascii_lines:
      parsed_items = p.findall(line)
      uid = parsed_items[0]
      human_string = parsed_items[2]
      uid_to_human[uid] = human_string

    # Loads mapping from string UID to integer node ID.
    node_id_to_uid = {}
    proto_as_ascii = tf.gfile.GFile(label_lookup_path).readlines()
    for line in proto_as_ascii:
      if line.startswith('  target_class:'):
        target_class = int(line.split(': ')[1])
      if line.startswith('  target_class_string:'):
        target_class_string = line.split(': ')[1]
        node_id_to_uid[target_class] = target_class_string[1:-2]

    # Loads the final mapping of integer node ID to human-readable string
    node_id_to_name = {}
    for key, val in node_id_to_uid.items():
      if val not in uid_to_human:
        tf.logging.fatal('Failed to locate: %s', val)
      name = uid_to_human[val]
      node_id_to_name[key] = name

    return node_id_to_name

  def id_to_string(self, node_id):
    if node_id not in self.node_lookup:
      return ''
    return self.node_lookup[node_id]


def create_graph():
  """Creates a graph from saved GraphDef file and returns a saver."""
  # Creates graph from saved graph_def.pb.
  with tf.gfile.FastGFile(os.path.join(
      FLAGS.model_dir, 'classify_image_graph_def.pb'), 'rb') as f:
    graph_def = tf.GraphDef()
    graph_def.ParseFromString(f.read())
    _ = tf.import_graph_def(graph_def, name='')


def run_inference_on_image(image):
  """Runs inference on an image.

  Args:
    image: Image file name.

  Returns:
    Nothing
  """
  if not tf.gfile.Exists(image):
    tf.logging.fatal('File does not exist %s', image)
  image_data = tf.gfile.FastGFile(image, 'rb').read()

  # Creates graph from saved GraphDef.
  #create_graph() #moved to main

  with tf.Session() as sess:
    # Some useful tensors:
    # 'softmax:0': A tensor containing the normalized prediction across
    #   1000 labels.
    # 'pool_3:0': A tensor containing the next-to-last layer containing 2048
    #   float description of the image.
    # 'DecodeJpeg/contents:0': A tensor containing a string providing JPEG
    #   encoding of the image.
    # Runs the softmax tensor by feeding the image_data as input to the graph.
    softmax_tensor = sess.graph.get_tensor_by_name('softmax:0')
    predictions = sess.run(softmax_tensor,
                           {'DecodeJpeg/contents:0': image_data})
    predictions = np.squeeze(predictions)

    # Creates node ID --> English string lookup.
    node_lookup = NodeLookup()
    top_k = predictions.argsort()[-FLAGS.num_top_predictions:][::-1]
    results = ""
    for node_id in top_k:
      human_string = node_lookup.id_to_string(node_id)
      score = predictions[node_id]
      results = results + ('%.5f' % score) + "," + human_string + "/"
      #print('%s (score = %.5f)' % (human_string, score))
  return results

def maybe_download_and_extract():
  """Download and extract model tar file."""
  dest_directory = FLAGS.model_dir
  if not os.path.exists(dest_directory):
    os.makedirs(dest_directory)
  filename = DATA_URL.split('/')[-1]
  filepath = os.path.join(dest_directory, filename)
  if not os.path.exists(filepath):
    def _progress(count, block_size, total_size):
      sys.stdout.write('\r>> Downloading %s %.1f%%' % (
          filename, float(count * block_size) / float(total_size) * 100.0))
      sys.stdout.flush()
    filepath, _ = urllib.request.urlretrieve(DATA_URL, filepath, _progress)
    print()
    statinfo = os.stat(filepath)
    print('Successfully downloaded', filename, statinfo.st_size, 'bytes.')
  tarfile.open(filepath, 'r:gz').extractall(dest_directory)

def take_picture_recognize(address, arg1):
  global picture_ready
  global picture_being_taken
  # take and write a snapshot to a file
  if arg1 == 1 and picture_ready == False and picture_being_taken == False:

    picture_being_taken = True
    image = os.path.join(FLAGS.model_dir, 'object-image.jpg')
    print("taking picture...")
    camera.capture(image)
    print("false phone message")
    client.send_message("/ding2/nomessage", "phone")
    print("picture ready")
    time.sleep(0.2)
    picture_ready = True
    picture_being_taken = False


def speak(address, arg1):
  print("Speaking...")
  os.system("pico2wave -w speaknow.wav '" + arg1 + "' && sox speaknow.wav -c 2 speaknowstereo.wav && aplay -Dhw:1 speaknowstereo.wav" )

def isAudioPlaying():

  audioPlaying = False

  #Check processes using ps
  #---------------------------------------
  cmd = 'ps -C omxplayer,mplayer'
  lineCounter = 0
  p = Popen(cmd, shell=True, stdin=PIPE, stdout=PIPE, stderr=STDOUT, close_fds=True)
  for ln in p.stdout:
    lineCounter = lineCounter + 1
    if lineCounter > 1:
      audioPlaying = True
      break

  return audioPlaying ;


def speech2text(duration):

  key = 'AIzaSyCWsDZ6eZlqfyZtj-vV8ukDl_rxtsprSDI'
  stt_url = 'https://speech.googleapis.com/v1beta1/speech:syncrecognize?key=' + key
  filename = 'speech2text.flac'

  #Do nothing if audio is playing
  #------------------------------------
  if isAudioPlaying():
    print (time.strftime("%Y-%m-%d %H:%M:%S ") + "Audio is playing")
    return ""



  #Record sound
  #----------------
  print ("listening for " + str(duration) + " seconds...")
  os.system('arecord -D plughw:1 -f cd -c 1 -t wav -d ' + str(duration) + '  -q -r 16000 | flac - -s -f --best -o ' + filename)


  #Check if the amplitude is high enough
  #---------------------------------------
  cmd = 'sox ' + filename + ' -n stat'
  p = Popen(cmd, shell=True, stdin=PIPE, stdout=PIPE, stderr=STDOUT, close_fds=True)
  soxOutput = p.stdout.read()
  #print "Popen output" + soxOutput


  maxAmpStart = soxOutput.find(b"Maximum amplitude")+24
  maxAmpEnd = maxAmpStart + 7

  #print "Max Amp Start: " + str(maxAmpStart)
  #print "Max Amop Endp: " + str(maxAmpEnd)

  maxAmpValueText = soxOutput[maxAmpStart:maxAmpEnd]


  #print "Max Amp: " + maxAmpValueText

  maxAmpValue = float(maxAmpValueText)

  if maxAmpValue < 0.1 :
    print ("Audio too quiet, not sending to Google")
    #Exit if sound below minimum amplitude
    return ""


  #Send sound  to Google Cloud Speech Api to interpret
  #----------------------------------------------------

  print (time.strftime("%Y-%m-%d %H:%M:%S ")  + "Sending to google api")


    # send the file to google speech api
  c = pycurl.Curl()
  c.setopt(pycurl.VERBOSE, 0)
  c.setopt(pycurl.URL, stt_url)
  fout = BytesIO()
  c.setopt(pycurl.WRITEFUNCTION, fout.write)

  c.setopt(pycurl.POST, 1)
  c.setopt(pycurl.HTTPHEADER, ['Content-Type: application/json'])

  with open(filename, 'rb') as speech:
    # Base64 encode the binary audio file for inclusion in the JSON
          # request.
          speech_content = base64.b64encode(speech.read())

  jsonContentTemplate = """{
      'config': {
            'encoding':'FLAC',
            'sampleRate': 16000,
            'languageCode': 'en-GB',
      'speechContext': {
            'phrases': [
              'jarvis'
          ],
        },
      },
      'audio': {
          'content':'XXX'
      }
  }"""


  jsonContent = jsonContentTemplate.replace("XXX",speech_content.decode("utf-8"))

  #print jsonContent

  start = time.time()

  c.setopt(pycurl.POSTFIELDS, jsonContent)
  c.perform()


  #Extract text from returned message from Google
  #----------------------------------------------
  response_data = fout.getvalue()


  end = time.time()
  #print "Time to run:"
  #print(end - start)


  #print response_data

  c.close()

  start_loc = response_data.find(b"transcript")
  temp_str = response_data[start_loc + 14:]
  #print "temp_str: " + temp_str
  end_loc = temp_str.find(b"\""+b",")
  final_result = temp_str[:end_loc]
  print (time.strftime("%Y-%m-%d %H:%M:%S ") + " transcription: " + final_result.decode("utf-8"))
  return final_result.decode("utf-8")

def listen_speech2text(address, duration):
  text  = speech2text(duration).replace("'","")
  if (text != ""):
    client.send_message("/ding2/speech2text", text)
  else:
    print("no transcription")
    client.send_message("/ding2/speech2text", "notranscription")

def osc_loop():
  # runs as a thread waiting for incoming OSC messages
  # set up client
  global client
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  # set up server
  server = osc_server.ThreadingOSCUDPServer((get_ip(), 5005), dispatcher)
  print("Serving on {}".format(server.server_address))
  # blocks on this
  server.serve_forever()

def get_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        # doesn't even have to be reachable
        s.connect(('8.8.8.8', 80))
        IP = s.getsockname()[0]
    except:
        IP = '127.0.0.1'
    finally:
        s.close()
    return IP

def main(_):
  global client
  global picture_ready
  print("network: " + socket.gethostname() + " " + get_ip())
  print("initializing model...")
  maybe_download_and_extract()
  # Creates graph from saved GraphDef
  print("creating graph...")
  create_graph()
  print("finished creating graph")
  server_thread = Thread(target=osc_loop,args=())
  server_thread.start() # run in background as a thread

  while True:

    if picture_ready:
      print("analyzing...")
      image = os.path.join(FLAGS.model_dir, 'object-image.jpg')
      match_results = run_inference_on_image(image)
      print("sending to computer: " + match_results)
      time.sleep(0.1)
      client.send_message("/ding2/objIdent", match_results)
      picture_ready = False
    time.sleep(0.1)


if __name__ == '__main__':
  parser = argparse.ArgumentParser()
  # classify_image_graph_def.pb:
  #   Binary representation of the GraphDef protocol buffer.
  # imagenet_synset_to_human_label_map.txt:
  #   Map from synset ID to a human readable string.
  # imagenet_2012_challenge_label_map_proto.pbtxt:
  #   Text representation of a protocol buffer mapping a label to synset ID.
  parser.add_argument(
      '--model_dir',
      type=str,
      #default='/tmp/imagenet',
      default='/home/pi/inception',
      help="""\
      Path to classify_image_graph_def.pb,
      imagenet_synset_to_human_label_map.txt, and
      imagenet_2012_challenge_label_map_proto.pbtxt.\
      """
  )
  parser.add_argument(
      '--image_file',
      type=str,
      default='',
      help='Absolute path to image file.'
  )
  parser.add_argument(
      '--num_top_predictions',
      type=int,
      default=5,
      help='Display this many predictions.'
  )
  parser.add_argument(
      '--server_ip',
      type=str,
      default='127.0.0.1',
      help='IP of server to send recognition results to.'
  )
  FLAGS, unparsed = parser.parse_known_args()
  # set up handlers for incoming OSC messages
  dispatcher = dispatcher.Dispatcher()
  dispatcher.map("/ding2/recognize", take_picture_recognize)
  dispatcher.map("/ding2/speak", speak)
  dispatcher.map("/ding2/listen", listen_speech2text)
  dispatcher.map("/1/push*", take_picture_recognize)
  # set up camera
  camera = picamera.PiCamera()
  picture_ready = False
  picture_being_taken = False
  tf.app.run(main=main, argv=[sys.argv[0]] + unparsed)
