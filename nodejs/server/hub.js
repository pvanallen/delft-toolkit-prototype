// central hub handling communication between arduino(ble), raspi(osc) and Unity(osc)
//
// part of the delft toolkit for smart things
// by Philip van Allen, pva@philvanallen.com
// created at TU Delft
//
// some code adapted from https://github.com/virgilvox/curie-ble-example-js/
//

var noble = require('noble');
var osc = require('node-osc');
var rapidMix = require('rapidlib');
var fs = require('fs');

// set up file system for writing training data
var trainingLog = fs.createWriteStream('training.csv', {
  flags: 'a' // 'a' means appending (old data will be preserved)
});

var dataCategory = 0;

// setup for processing of data for classification training
var xStream = new rapidMix.StreamBuffer(9);
var yStream = new rapidMix.StreamBuffer(9);
var zStream = new rapidMix.StreamBuffer(9);
var xProcessed = 0;
var yProcessed = 0;
var zProcessed = 0;

// set up RapidMix classification
//
// Create a series classificationm object
var myDTW = new rapidMix.SeriesClassification();
// these arrays will hold two training sets and one to test against.
var gesture1TrainingSet = { input: [], label: "gesture1" };
var gesture2TrainingSet = { input: [], label: "gesture2" };
var gestureTrainingSet = { input: [], label: "gesture" };
var seriesSet = [];
var testSet = [];

var match = -1;
var costs = [-1, -1];

// define OSC Servers and Clients
var toCtrl = new osc.Client('127.0.0.1', 8001); // send to Unity
var fromCtrl = new osc.Server(3333, '0.0.0.0'); // receive from Unity

//var toPi = new osc.Client('145.94.216.183', 5005); // send to RasPi
var toPi = new osc.Client('145.94.217.178', 5005); // send to RasPi
var fromPi = new osc.Server(5006, '0.0.0.0'); // receive from RasPi

console.log("start training on saved data...");
var trainingComplete = false;
readFeaturesTrain(); // async

// set up for communication with RasPi
fromPi.on("message", function (msg, rinfo) {
  console.log("Incoming Pi OSC msg: " + msg);
  // send to Unity
  switch(msg[0]) {
    case "/ding2/objIdent":
      console.log(msg[1]);
      // SEND BY OSC TO UNITY
      var ctrlMessageStr = "";
      ctrlMessageStr = "/str/ding2/objIdent";
      var ctrlMessage = new osc.Message(ctrlMessageStr);
      ctrlMessage.append(msg[1]);
      toCtrl.send(ctrlMessage);
      break;
    default:
      console.log("unknown message: " + msg)
      break;
  }
});


// setup for bluetooth
// Search only for the Service UUID of the device (remove dashes)
var serviceUuids = ['20B10010E8F2537E4F6CD104768A1214'];

// Search only for the led charateristic
var characteristicUuids = [
  '19B10011E8F2537E4F6CD104768A1214',
  '5667f3b1d6a24fb2a9174bee580a9c84',
  '917649A2D98E11E59EEC0002A5D5C51B'
];

// start scanning when internal bluetooth is powered on
noble.on('stateChange', function(state) {
  if (state === 'poweredOn') {
    noble.startScanning(serviceUuids);
  } else {
    noble.stopScanning();
  }
});

// Search for BLE peripherals
noble.on('discover', function(peripheral) {
  peripheral.connect(function(error) {
    console.log('connected to peripheral: ' + peripheral.uuid);
    // Only discover the service we specified above
    peripheral.discoverServices(serviceUuids, function(error, services) {
      var service = services[0];
      console.log('discovered robot service');

      service.discoverCharacteristics(characteristicUuids, function(error, characteristics) {
        console.log('discovered robot characteristics');
        // Assign Characteristics
        var commandCharacteristic = characteristics[0];
        var imuCharacteristic = characteristics[1];
        var sensorCharacteristic = characteristics[2];

        // SUBSCRIBE TO AND HANDLE IMU MESSAGES FROM ARDUINO
        //
        imuCharacteristic.subscribe(function(error) {
          console.log("subscribed to imu")
          if(error) console.log(error);
        });

        // BUILD DATA FROM IMU, PROCESS, AND CLASSIFY OR TRAIN
        imuCharacteristic.on('data', handleImu);

        // SUBSCRIBE TO AND HANDLE DATA FROM SENSORS ON ARDUINO
        //
        sensorCharacteristic.subscribe(function(error) {
          console.log("subscribed to sensors")
          if(error) console.log(error);
        });
        sensorCharacteristic.on('data', function(data, isNotification) {
          var type = String.fromCharCode(data.readInt8(0));
          var port = data.readInt8(1);
          var v1 = data.readFloatLE(4);
          var v2 = data.readFloatLE(8);
          var v3 = data.readFloatLE(12);

          // SEND BY OSC TO UNITY
          var ctrlMessageStr = "";
          if (type == "A") {
            ctrlMessageStr = "/num/ding1/a/" + port;
          } else {
            ctrlMessageStr = "/num/ding1/other/" + port;
          }
          var ctrlMessage = new osc.Message(ctrlMessageStr);
          ctrlMessage.append(v1);
          ctrlMessage.append(v2);
          ctrlMessage.append(v3);
          toCtrl.send(ctrlMessage);

          //console.log("sensor: " + type + " " + port + " " + v1 + "," + v2 + "," + v3);
        });

        // HANDLE MESSAGES FROM UNITY AND FORWARD TO ARDUINO
        //
        var bufferToSend = new Buffer.alloc(16);
        var motorOn = false;

        fromCtrl.on("message", function (msg, rinfo) {

              if (msg[0].indexOf("/robot/") >=0) {
                // send to robot
                console.log("Incoming OSC msg: " + msg);
                bufferToSend.writeUInt8(0,1);
                bufferToSend.writeUInt8(0,2);
                bufferToSend.writeUInt8(0,3);

                // send bluetooth commands based on OSC message
                switch(msg[0]) {
                  // wheel motor commands
                  case "/robot/stop":
                    bufferToSend.write("M",0);
                    bufferToSend.writeUInt8(0,1);
                    break;
                  case "/robot/forward":
                    bufferToSend.write("M",0);
                    bufferToSend.writeUInt8(1,1);
                    break;
                  case "/robot/backward":
                    bufferToSend.write("M",0);
                    bufferToSend.writeUInt8(2,1);
                    break;
                  case "/robot/turnRight":
                    bufferToSend.write("M",0);
                    bufferToSend.writeUInt8(3,1);
                    break;
                  case "/robot/turnLeft":
                    bufferToSend.write("M",0);
                    bufferToSend.writeUInt8(4,1);
                    break;
                  // LED commands
                  case "/robot/ledsOn":
                    bufferToSend.write("C",0);
                    bufferToSend.writeUInt8(60,1);
                    bufferToSend.writeUInt8(0,2);
                    bufferToSend.writeUInt8(127,3);
                    break;
                  case "/robot/ledsOff":
                    bufferToSend.write("C",0);
                    bufferToSend.writeUInt8(0,1);
                    bufferToSend.writeUInt8(0,2);
                    bufferToSend.writeUInt8(0,3);
                    break;
                  // servo commands
                  case "/robot/servoWiggle":
                    bufferToSend.write("S",0);
                    break;
                  // machine learning/imu commands
                  case "/robot/mlImuOff":
                    bufferToSend.write("L",0);
                    bufferToSend.writeUInt8(0,1);
                    break;
                  case "/robot/mlImuRun":
                      bufferToSend.write("L",0);
                      bufferToSend.writeUInt8(1,1);
                      break;
                  case "/robot/mlImuTrain1":
                      bufferToSend.write("L",0);
                      bufferToSend.writeUInt8(2,1);
                      bufferToSend.writeUInt8(1,2);
                      break;
                  case "/robot/mlImuTrain2":
                      bufferToSend.write("L",0);
                      bufferToSend.writeUInt8(2,1);
                      bufferToSend.writeUInt8(2,2);
                      break;
                  case "/robot/mlImuTrainStop":
                      bufferToSend.write("L",0);
                      bufferToSend.writeUInt8(0,1);
                      readFeaturesTrain();
                      break;
                  // analog inputs
                  case "/robot/analogOff":
                      bufferToSend.write("A",0);
                      bufferToSend.writeUInt8(0,1);
                      break;
                  case "/robot/analogOn0":
                      bufferToSend.write("A",0);
                      bufferToSend.writeUInt8(1,1);
                      bufferToSend.writeUInt8(0,2);
                      break;
                  default:
                      bufferToSend.write("M",0);
                }

                console.log("Sending BT msg " + bufferToSend + " " + bufferToSend[1].toString() + " " + bufferToSend[2].toString() + " " + bufferToSend[3].toString(),bufferToSend);
                commandCharacteristic.write(bufferToSend, false);
              } else if (msg[0].indexOf("/ding2/") >=0) {
                // send to raspberry pi
                var validMsg = true;
                var ctrlMessage = new osc.Message(msg[0]);
                switch(msg[0]) {
                  case "/ding2/recognize":
                    ctrlMessage.append(msg[1]);
                    break;
                  case "/ding2/speak":
                    ctrlMessage.append(msg[2]);
                    console.log("message: " + msg)
                    break;
                  default:
                    console.log("unknown message: " + msg)
                    validMsg = false;
                    break;
                }
                if (validMsg) {
                  console.log("SENDING TO PI: " + msg[0]);
                  toPi.send(ctrlMessage);
                }
              }

        });
      });
    });
  });
});

// evaluate case for classification and send result to unity
function mlEvaluate() {
  if (trainingComplete) {
    console.log("testSet: " + testSet.length);
    console.log("run");
    // pad with zeros to make 128 to work around rapidmix bug
    // for (var i=testSet.length; i<128; i++) {
    //   var rapidInput = [0.0,0.0,0.0];
    //   testSet.push(rapidInput);
    // }
    console.log("testSet: " + testSet.length);
    match = myDTW.run(testSet);
    console.log("getCosts");
    costs = myDTW.getCosts();

    var ctrlMessageStr = "/num/ding1/category";
    var ctrlMessage = new osc.Message(ctrlMessageStr);

    var gestureCategory = parseFloat(match.replace(/\D/g,''));
    console.log("cat " +  gestureCategory);
    ctrlMessage.append(gestureCategory);

    ctrlMessage.append(0); // empty fields
    ctrlMessage.append(0);
    toCtrl.send(ctrlMessage);

    console.log("send to Unity: ");
    //console.log("testSet ", testSet);
    console.log("match ", match);
    console.log("costs ", costs);
  } else {
    console.log("can't classify - NO TRAINING DATA")
  }
};

function readFeaturesTrain() {
  // set up for reading the data file
  var lines = [];
  var lineReader = require('readline').createInterface({
    input: require('fs').createReadStream('training.csv')
  });
  lineReader.on('line', function (line) {
    var lineArr = line.split(",");
    lines.push(lineArr);
  });

  // once the file is loaded, process the data
  lineReader.on('close', function (line) {
    console.log("all lines in file read");

    // go through the data lines
    for (var i=0; i<lines.length; i++) {
      // set the category for data
      var gestureTrainingSet = { input: [], label: lines[i][0] };
      // get the x,y,z data
      for (var n=1; n<lines[i].length; n+=3) {
        var x = parseFloat(lines[i][n]);
        var y = parseFloat(lines[i][n+1]);
        var z = parseFloat(lines[i][n+2]);
        var rapidInput = [x,y,z];
        gestureTrainingSet.input.push(rapidInput);
      }

      // add a gesture dataset to series
      console.log("trainingset " + lines[i][0] + ": " + gestureTrainingSet.input.length);
      seriesSet.push(gestureTrainingSet);
    }
    // train the model
    if (seriesSet.length > 2) {
      //console.log(seriesSet[0].input);
      myDTW.reset();
      var trainingOk = myDTW.train(seriesSet)
      console.log("training complete: " + trainingOk);
      trainingComplete = true;
    } else {
      console.log("no data to train");
      trainingComplete = false;
    }
  });
}

function handleImu(data, isNotification) {
  var x = data.readFloatLE(0); // 4 byte floats
  var y = data.readFloatLE(4);
  var z = data.readFloatLE(8);

  //console.log("xyz: " + x + " " + y + " " + z);
  if (x== -1 && y== -1 && z== -1) {
    console.log("START CLASSIFYING");
    dataCategory = 0;
    testSet = [];
  } else if (x== 1 && y== 1 && z== 1) {
    console.log("STOP CLASSIFYING");
    mlEvaluate();
  } else if (x < -100 && y < -100 && z < -100) {
    dataCategory = Math.abs(x + 100);
    console.log("START RECORDING");
    trainingLog.write("gesture" + dataCategory);
  } else if (x== 100 && y== 100 && z== 100) {
    console.log("STOP RECORDING");
    trainingLog.write("\n");
  } else { // receive data
    xStream.push(x);
    yStream.push(y);
    zStream.push(z);
    xProcessed = xStream.velocity();
    yProcessed = yStream.velocity();
    zProcessed = zStream.velocity();

    if (dataCategory == 0) { // classifying
      var rapidInput = [xProcessed, yProcessed, zProcessed];
      testSet.push(rapidInput);
      //console.log("logging data: " + testSet);
    } else { // recording training set
      var writeData = "," + xProcessed + ',' + yProcessed + ',' + zProcessed;
      trainingLog.write(writeData); // write data to a file
      //console.log("logging data: " + writeData);
    }
    //console.log("xyz: " + xProcessed + " " + yProcessed + " " + zProcessed);
  }
}
