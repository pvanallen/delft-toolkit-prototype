// handles communication over BLE from arduino 101 to the toolkit server
//
// part of the delft toolkit for smart things
// by Philip van Allen, pva@philvanallen.com
// created at TU Delft
//
// some code adapted from  https://www.hackster.io/gov/imu-to-you-ae53e1
//
#include "CurieIMU.h"
#include <CurieBLE.h>
#include <TaskScheduler.h>
#include <Adafruit_MotorShield.h>
#include <Adafruit_NeoPixel.h>
#include <Servo.h>

// used by Node.js to select which robot to connect to if
// so more than one can be in a space at the same time
#define BOTNAME "delftbt0"

// TaskScheduler Setup
//

// Callback methods prototypes
void transmitSensors();
void waitForImu();
void transmitImu();
//void runMotors();
void blinkLeds();

// Tasks
Task tTransmitSensors(20, TASK_FOREVER, &transmitSensors);
Task tWaitForImu(10, TASK_FOREVER, &waitForImu);
Task tTransmitImu(20, 75, &transmitImu);
//Task tRunMotors(20, TASK_FOREVER, &runMotors);
Task tBlinkLeds(250, 5, &blinkLeds);

Scheduler runner;

// NeoPixel setup
//
#define NEOPIN 6
#define NUMPIXELS 12
//Adafruit_NeoPixel pixel = Adafruit_NeoPixel(NUMPIXELS, NEOPIN, NEO_GRB + NEO_KHZ800 );
Adafruit_NeoPixel pixel = Adafruit_NeoPixel(NUMPIXELS, NEOPIN, NEO_RGBW + NEO_KHZ800 );

char commandCharLocal[32] ;

bool blinkState = false;
int blinkR = 60;
int blinkG = 0;
int blinkB = 127;

// Create the motor shield object with the default I2C address
//
Adafruit_MotorShield AFMS = Adafruit_MotorShield();
// And connect 2 DC motors to port M1 & M2 !
Adafruit_DCMotor *L_MOTOR = AFMS.getMotor(1);
Adafruit_DCMotor *R_MOTOR = AFMS.getMotor(2);

// Setup BLE
BLEPeripheral blePeripheral; // create peripheral instance
BLEService delftBot("20B10010-E8F2-537E-4F6C-D104768A1214"); // create service
// create characteristic and allow remote device to read and write
BLECharacteristic commandChar("19B10011-E8F2-537E-4F6C-D104768A1214", BLERead | BLEWrite, 16);
BLECharacteristic imuAccCharacteristic("5667f3b1-d6a2-4fb2-a917-4bee580a9c84", BLERead | BLENotify, 12 );
BLECharacteristic sensorCharacteristic("917649A2-D98E-11E5-9EEC-0002A5D5C51B", BLERead | BLENotify, 16 );

// Setup Servos
Servo servo1;

// movement characteristics
int speed = 120; // 0-255

// imu & machine learning setup
int mlTrainingCount = 8;
bool mlTrainingOn = false;
bool imuOn = false;
float mlTrainingCategory = 1.0;
unsigned long xmitStartTime = 0;
const float imuTrigger = 1.10;

// sensors setup
bool useAnalog = false;
int analogPort = 0;


/**
   The union directive allows 3 variables to share the same memory location. Please see the
   tutorial covering this project for further discussion of the use of the union
   directive in C. https://www.hackster.io/gov/imu-to-you-ae53e1 */
// note: these need to use four byte boundaries to work right
// accelerometer data structure
union {
  float a[3];
  unsigned char bytes[12];
} accData;
// sensor data structure
union {
  struct {
    char sensorType;
    char sensorPort;
    char sensorOther1; // unused to fill out to 4 bytes before floats
    char sensorOther2;
    float values[3];
  } data;
  unsigned char bytes[16];
} sensorData;

void setup() {
  Serial.begin(9600);

  // Init TaskScheduler
  runner.init();
  runner.addTask(tWaitForImu);
  runner.addTask(tTransmitImu);
  runner.addTask(tTransmitSensors);
  runner.addTask(tBlinkLeds);
  
  // Motor setup
  AFMS.begin();  // create with the default frequency 1.6KHz
  // turn off both motors
  L_MOTOR->setSpeed(0);
  R_MOTOR->setSpeed(0);
  L_MOTOR->run(RELEASE);
  R_MOTOR->run(RELEASE);

  // Attach servo
  servo1.attach(9);

  // Setup the neopixel
  pixel.begin();
  pixel.setBrightness(127); //medium brightness
  pixel.show();


  // Setup BLE
  //
  // set the local name peripheral advertises - used by Node.js server to select which robot to connect to
  blePeripheral.setLocalName(BOTNAME);

  // set the UUID for the service this peripheral advertises
  blePeripheral.setAdvertisedServiceUuid(delftBot.uuid());
  // add service and characteristic
  blePeripheral.addAttribute(delftBot);
  blePeripheral.addAttribute(commandChar);
  blePeripheral.addAttribute(imuAccCharacteristic);
  blePeripheral.addAttribute(sensorCharacteristic);
  // assign event handlers for characteristic
  commandChar.setEventHandler(BLEWritten, commandCharacteristicReceived);
  // set an initial value for the characteristic
  sprintf(commandCharLocal, "AZERTYUIOPQSDFGH");
  commandChar.setValue((unsigned char *)commandCharLocal, 16) ;
  // advertise the service
  blePeripheral.begin();

  // set up imu output
  const unsigned char initializerAcc[12] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
  imuAccCharacteristic.setValue( initializerAcc, 12);
  // set up sensor output
  const unsigned char initializerSensor[16] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
  sensorCharacteristic.setValue( initializerSensor, 16);

  CurieIMU.begin();
  CurieIMU.setAccelerometerRange(2);  // Set the accelerometer range to 2G
  
  Serial.println("Delft AI Toolkit Started");

  startBlinkLeds(100,3);
}

void loop() {
  // poll ble
  blePeripheral.poll();
  // run TaskScheduler
  runner.execute();
}

void transmitSensors() {
  int value = analogRead(analogPort);
  sensorData.data.sensorType = 'A';
  sensorData.data.sensorPort = analogPort;
  sensorData.data.values[0] = (float) value;
  sensorData.data.values[1] = 0.0;
  sensorData.data.values[2] = 0.0;
  unsigned char *senseDatum = (unsigned char *)&sensorData;
  sensorCharacteristic.setValue( senseDatum, 16 );
  Serial.println(value);
}
void waitForImu() {
  int axRaw, ayRaw, azRaw;
  CurieIMU.readAccelerometer(axRaw, ayRaw, azRaw);
  accData.a[0] = convertRawAcceleration(axRaw);
  accData.a[1] = convertRawAcceleration(ayRaw);
  accData.a[2] = convertRawAcceleration(azRaw);
  if (accData.a[0] > imuTrigger ||
      accData.a[1] > imuTrigger ||
      accData.a[2] > imuTrigger) { // send first message of gesture
    if (mlTrainingOn) {
      float val = -100.0 - mlTrainingCategory;
      accData.a[0] = val; accData.a[1] = val; accData.a[2] = val;
      Serial.println("START TRAINING");
    } else {
      accData.a[0] = -1.0; accData.a[1] = -1.0; accData.a[2] = -1.0;
      Serial.println("START CLASSIFYING");
    }
    unsigned char *acc = (unsigned char *)&accData;
    imuAccCharacteristic.setValue( acc, 12 );
    accData.a[0] = convertRawAcceleration(axRaw);
    accData.a[1] = convertRawAcceleration(ayRaw);
    accData.a[2] = convertRawAcceleration(azRaw);
    // start data transmision
    tWaitForImu.disable();
    tTransmitImu.restart();
    setAllLedsColor(0,127,0);
  }
}
void transmitImu() {
  // see for more info https://www.hackster.io/gov/imu-to-you-ae53e1
  int axRaw, ayRaw, azRaw;
  CurieIMU.readAccelerometer(axRaw, ayRaw, azRaw);
  accData.a[0] = convertRawAcceleration(axRaw);
  accData.a[1] = convertRawAcceleration(ayRaw);
  accData.a[2] = convertRawAcceleration(azRaw);
  //    Serial.print( "(ax,ay,az): " );
  //    Serial.print("("); Serial.print(accData.a[0]); Serial.print(","); Serial.print(accData.a[1]);
  //    Serial.print(","); Serial.print(accData.a[2]); Serial.print(")");Serial.println();
  unsigned char *acc = (unsigned char *)&accData;
  imuAccCharacteristic.setValue( acc, 12 );
  if (tTransmitImu.isLastIteration()) { // last message of gesture
    if (mlTrainingOn) {
      accData.a[0] = 100.0; accData.a[1] = 100.0; accData.a[2] = 100.0;
      Serial.println("STOP TRAINING");
    } else {
      accData.a[0] = 1.0; accData.a[1] = 1.0; accData.a[2] = 1.0;
      Serial.println("STOP CLASSIFYING");
    }
    unsigned char *acc = (unsigned char *)&accData;
    imuAccCharacteristic.setValue( acc, 12 );
    startBlinkLeds(100,3);
    if (mlTrainingOn) {
      tWaitForImu.enable();
    }
  }
}

void servoWiggle(int n) {
  for (int i = 0; i <= n; i++) {
    servo1.write(70);
    delay(120);
    servo1.write(110);
    delay(120);
  }
  servo1.write(90);
}

float convertRawAcceleration(int aRaw) {
  // since we are using 2G range
  // -2g maps to a raw value of -32768
  // +2g maps to a raw value of 32767
  float a = (aRaw * 2.0) / 32768.0;
  return a;
}

void commandCharacteristicReceived(BLECentral& central, BLECharacteristic& characteristic) {
  // central wrote new value to characteristic, update Robot
  char command;
  int val1, val2, val3;

  //Serial.print("Characteristic event received: ");
  //Serial.println((char*)commandChar.value());

  command = commandChar.value()[0];
  val1 = commandChar.value()[1];
  val2 = commandChar.value()[2];
  val3 = commandChar.value()[3];

  Serial.print("Command: ");
  Serial.print(command);
  Serial.print(" ");
  Serial.print(val1);
  Serial.print(" ");
  Serial.print(val2);
  Serial.print(" ");
  Serial.println(val3);

  if (command == 'C') { // set the color of the neopixel
    Serial.println(" LEDs");
    setAllLedsColor(val1,val2,val3);
//    uint8_t red = val1;
//    uint8_t green = val2;
//    uint8_t blue = val3;
//
//    for (int i = 0; i < NUMPIXELS; i++) {
//      pixel.setPixelColor(i, pixel.Color(red, green, blue));
//    }
//    pixel.show();
  }

  if (command == 'M') { // move using motors
    Serial.print(" Motor ");
    if (val1 == 0) { // STOP
      Serial.println("Stop");
      L_MOTOR->setSpeed(0);
      R_MOTOR->setSpeed(0);
      L_MOTOR->run(RELEASE);
      R_MOTOR->run(RELEASE);
    } else { // GO
      if (val1 == 1) {
        Serial.println("Forward");
        L_MOTOR->run(FORWARD);
        R_MOTOR->run(FORWARD);
      } else if (val1 == 2) {
        Serial.println("Backward");
        L_MOTOR->run(BACKWARD);
        R_MOTOR->run(BACKWARD);
      } else if (val1 == 3) {
        Serial.println("Turn right");
        L_MOTOR->run(BACKWARD);
        R_MOTOR->run(FORWARD);
      } else if (val1 == 4) {
        Serial.println("Turn Left");
        L_MOTOR->run(FORWARD);
        R_MOTOR->run(BACKWARD);
      }
      // speed up the motors
      L_MOTOR->setSpeed(speed);
      R_MOTOR->setSpeed(speed);
    }
  }

  if (command == 'S') { // servo commands
    Serial.print(" Servo ");
    //servoWiggle(2);
  }
  if (command == 'L') { // machine learning commands
    Serial.print(" ML ");
    if (val1 == 0) { // shut off imu
      imuOn = false;
      tWaitForImu.disable();
    } else if (val1 == 1) { // running mode
      imuOn = true;
      tWaitForImu.enable();
      mlTrainingOn = false;
    } else if (val1 == 2) { // training mode
      imuOn = true;
      tWaitForImu.enable();
      mlTrainingOn = true;
      mlTrainingCategory = val2;
    } else {
      imuOn = false;
      tWaitForImu.disable();
    }
  }
  if (command == 'A') { // analog sensor commands
    Serial.print(" Analog ");
    if (val1 == 0) { // shut off analog
      Serial.println("Disable ");
      tTransmitSensors.disable();
    } else if (val1 == 1) { // running mode
      Serial.print("Enable ");
      analogPort = val2;
      Serial.println(analogPort);
      tTransmitSensors.restart();
    }
  }
}
void blinkLeds () {
  if (blinkState) {
    setAllLedsColor(0,0,0);
    Serial.println("leds off");
  } else {
    setAllLedsColor(blinkR,blinkG,blinkB);
    Serial.println("Leds on");
  }
  blinkState = !blinkState;
}

void startBlinkLeds(int ms, int blinks) {
  blinkState = false;
  tBlinkLeds.setInterval(ms);
  tBlinkLeds.setIterations(blinks*2);
  tBlinkLeds.restart();
}

void setAllLedsColor(uint8_t red, uint8_t green, uint8_t blue) {
  for (int i = 0; i < NUMPIXELS; i++) {
    pixel.setPixelColor(i, pixel.Color(red, green, blue));
  }
  pixel.show();
}

//void runMotors(leftDir, rightDir, leftSpeed, rightSpeed) {
//  
//}

