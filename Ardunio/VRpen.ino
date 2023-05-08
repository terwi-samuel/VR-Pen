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

double x;
double y;
double z;
double w;

uint8_t buf1[6];
uint8_t buf2[6];
uint8_t buf3[6];
uint8_t buf4[6];
uint8_t buf5[2]; // Button 1
uint8_t buf6[2]; // Button 2
uint8_t buf7[1]; // Button 3

imu::Quaternion raw_quat;
imu::Quaternion offset_quat; 
imu::Quaternion pen_quat;


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

#define BUTTON1 16
#define BUTTON2 17
#define BUTTON3 21
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

  bno.setExtCrystalUse(true);

  // Setup Bluetooth and push buttons
  SerialBT.begin("ESP32_PEN"); //Bluetooth device name
  Serial.println("The device started, now you can pair it with bluetooth!");
  delay(100);
  pinMode(BUTTON1, INPUT_PULLUP);
  pinMode(BUTTON2, INPUT_PULLUP);
  pinMode(BUTTON3, INPUT_PULLUP);  
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

  //Get Quaternion vector and rotate it by 90 degrees to line up the front of the IMU with the pen
  raw_quat = bno.getQuat();
  offset_quat = imu::Quaternion(-0.7071,0,0,-0.7071); // 0.7071,0,0,-0.7071
  pen_quat = raw_quat * offset_quat;
  x = pen_quat.x();
  y = pen_quat.y();
  z = pen_quat.z();
  w = pen_quat.w();

  Serial.print(x);
  Serial.print(",");
  Serial.print(y);
  Serial.print(",");
  Serial.print(z);
  Serial.print(",");
  Serial.println(w);

  // Take Quaternion vector elements and turn them into strings  
  String string_x;
  String string_y;
  String string_z;
  String string_w;
  String string_button1;
  String string_button2;
  String string_button3;
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

  // Take Quaternion strings and prepare uint8_t buffers with commas at the end of each buffer except the last
  // The bushbutton buffers are simlpy filled by reading their status and adding the decimal value 48 to get '1' and '0' characters 
  // Buffers are what will be transmitted over Bluetooth to Unity
  memcpy(buf1, string_x.c_str(),5);
  memcpy(buf2, string_y.c_str(),5);
  memcpy(buf3, string_z.c_str(),5);
  memcpy(buf4, string_w.c_str(),5);
  buf1[5] = ',';
  buf2[5] = ',';
  buf3[5] = ',';
  buf4[5] = ',';
  buf5[0] = digitalRead(BUTTON1) + 48;
  buf5[1] = ',';
  buf6[0] = digitalRead(BUTTON2) + 48;
  buf6[1] = ',';
  buf7[0] = digitalRead(BUTTON3) + 48;
  SerialBT.write(buf1, 6);
  SerialBT.write(buf2, 6);
  SerialBT.write(buf3, 6);
  SerialBT.write(buf4, 6);
  SerialBT.write(buf5, 2);
  SerialBT.write(buf6, 2);
  SerialBT.write(buf7, 1);
  SerialBT.println();

  delay(15);
}
