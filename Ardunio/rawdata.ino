#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>
#include "BluetoothSerial.h"
#include <string.h>

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

BluetoothSerial SerialBT;

/* This driver reads raw data from the BNO055

   Connections
   ===========
   Connect SCL to analog 5
   Connect SDA to analog 4
   Connect VDD to 3.3V DC
   Connect GROUND to common ground

   History
   =======
   2015/MAR/03  - First release (KTOWN)
*/

/* Set the delay between fresh samples */
#define BNO055_SAMPLERATE_DELAY_MS (100)

// Check I2C device address and correct line below (by default address is 0x29 or 0x28)
//                                   id, address
Adafruit_BNO055 bno = Adafruit_BNO055(-1, 0x28, &Wire);

/**************************************************************************/
/*
    Arduino setup function (automatically called at startup)
*/
/**************************************************************************/
void setup(void)
{
  Serial.begin(19200);

  while (!Serial) delay(10);  // wait for serial port to open!

  Serial.println("Orientation Sensor Raw Data Test"); Serial.println("");

  /* Initialise the sensor */
  if(!bno.begin())
  {
    /* There was a problem detecting the BNO055 ... check your connections */
    Serial.print("Ooops, no BNO055 detected ... Check your wiring or I2C ADDR!");
    while(1);
  }

  delay(1000);

  /* Display the current temperature */
  // int8_t temp = bno.getTemp();
  // Serial.print("Current Temperature: ");
  // Serial.print(temp);
  // Serial.println(" C");
  // Serial.println("");

  bno.setExtCrystalUse(true);

  Serial.println("Calibration status values: 0=uncalibrated, 3=fully calibrated");
  SerialBT.begin("ESP32_IMU"); //Bluetooth device name
  Serial.println("The device started, now you can pair it with bluetooth!");
}

/**************************************************************************/
/*
    Arduino loop function, called once 'setup' is complete (your own code
    should go here)
*/
/**************************************************************************/
void loop(void)
{
  // Possible vector values can be:
  // - VECTOR_ACCELEROMETER - m/s^2
  // - VECTOR_MAGNETOMETER  - uT
  // - VECTOR_GYROSCOPE     - rad/s
  // - VECTOR_EULER         - degrees
  // - VECTOR_LINEARACCEL   - m/s^2
  // - VECTOR_GRAVITY       - m/s^2
  imu::Vector<3> euler = bno.getVector(Adafruit_BNO055::VECTOR_EULER);

  //Get Quaternion vector
  imu::Quaternion quat = bno.getQuat();

  double x = quat.x();
  double y = quat.y();
  double z = quat.z();
  double w = quat.w();

  Serial.print(x);
  Serial.print(",");
  Serial.print(y);
  Serial.print(",");
  Serial.print(z);
  Serial.print(",");
  Serial.println(w);

  String string_x;
  String string_y;
  String string_z;
  String string_w;
  if(x >= 0)
    string_x = String(x,3);
  else
    string_x = String(x,2);

  if(y >= 0)
    string_y = String(y,3);
  else
    string_y = String(y,2);

  if(z >= 0)
    string_z = String(z,3);
  else
    string_z = String(z,2);

  if(w >= 0)
    string_w = String(w,3);
  else
    string_w = String(w,2);

  uint8_t buf1[6];
  uint8_t buf2[6];
  uint8_t buf3[6];
  uint8_t buf4[5];
  memcpy(buf1, string_x.c_str(),5);
  memcpy(buf2, string_y.c_str(),5);
  memcpy(buf3, string_z.c_str(),5);
  memcpy(buf4, string_w.c_str(),5);
  buf1[5] = ',';
  buf2[5] = ',';
  buf3[5] = ',';
  SerialBT.write(buf1, 6);
  SerialBT.write(buf2, 6);
  SerialBT.write(buf3, 6);
  SerialBT.write(buf4, 5);
  SerialBT.println();

  /* Display the floating point data */
  //Serial.print("X: ");
  // Serial.print(euler.x());
  // Serial.print(",");
  // Serial.print(euler.y());
  // Serial.print(",");
  // Serial.println(euler.z());

  //Serial.print("\t\t");

  /*
  // Quaternion data
  imu::Quaternion quat = bno.getQuat();
  Serial.print("qW: ");
  Serial.print(quat.w(), 4);
  Serial.print(" qX: ");
  Serial.print(quat.x(), 4);
  Serial.print(" qY: ");
  Serial.print(quat.y(), 4);
  Serial.print(" qZ: ");
  Serial.print(quat.z(), 4);
  Serial.print("\t\t");
  */

  /* Display calibration status for each sensor. */
  uint8_t system, gyro, accel, mag = 0;
  bno.getCalibration(&system, &gyro, &accel, &mag);
  // Serial.print("CALIBRATION: Sys=");
  // Serial.print(system, DEC);
  // Serial.print(" Gyro=");
  // Serial.print(gyro, DEC);
  // Serial.print(" Accel=");
  // Serial.print(accel, DEC);
  // Serial.print(" Mag=");
  // Serial.println(mag, DEC);

  delay(15);
}
