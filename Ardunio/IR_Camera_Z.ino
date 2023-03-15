// Wii Remote IR sensor  test sample code  by kako http://www.kako.com
// modified output for Wii-BlobTrack program by RobotFreak http://www.letsmakerobots.com/user/1433
// modified for https://dfrobot.com by Lumi, Jan. 2014
// modifed for OmniPen senior deisgn project Spring 2023

#include <Wire.h>
#include <stdint.h>
#include "BluetoothSerial.h"
#include <string.h>

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

BluetoothSerial SerialBT;

int IRsensorAddress = 0xB0;
//int IRsensorAddress = 0x58;
int slaveAddress;
int ledPin = 13;
boolean ledState = false;
byte data_buf[16];
int i;

float Ix[4];
float Iy[4];
int s;

#define SENSITIVITY2

#ifdef SENSITIVITY1
  uint8_t p0 = 0x72;
  uint8_t p1 = 0x20;
  uint8_t p2 = 0x1F;
  uint8_t p3 = 0x03;
#endif

#ifdef SENSITIVITY2
  uint8_t p0 = 0xC8;
  uint8_t p1 = 0x36;
  uint8_t p2 = 0x35;
  uint8_t p3 = 0x03;
#endif

#ifdef SENSITIVITY3
  uint8_t p0 = 0xAA;
  uint8_t p1 = 0x64;
  uint8_t p2 = 0x63;
  uint8_t p3 = 0x03;
#endif

#ifdef SENSITIVITY4
  uint8_t p0 = 0x96;
  uint8_t p1 = 0xB4;
  uint8_t p2 = 0xB3;
  uint8_t p3 = 0x04;
#endif

#ifdef SENSITIVITY5
  uint8_t p0 = 0x96;
  uint8_t p1 = 0xFE;
  uint8_t p2 = 0xFE;
  uint8_t p3 = 0x05;
#endif

void Write_2bytes(byte d1, byte d2)
{
    Wire.beginTransmission(slaveAddress);
    Wire.write(d1); delay(50); Wire.write(d2); delay(50);
    Wire.endTransmission();
}

void Write_3bytes(byte d1, byte d2, byte d3)
{
    Wire.beginTransmission(slaveAddress);
    Wire.write(d1); delay(50); Wire.write(d2); delay(50); Wire.write(d3); delay(50);
    Wire.endTransmission();
}

void Write_8bytes(byte d1, byte d2, byte d3, byte d4, byte d5, byte d6, byte d7, byte d8)
{
    Wire.beginTransmission(slaveAddress);
    Wire.write(d1); delay(50); Wire.write(d2); delay(50);
    Wire.write(d3); delay(50); Wire.write(d4); delay(50);
    Wire.write(d5); delay(50); Wire.write(d6); delay(50);
    Wire.write(d7); delay(50); Wire.write(d8); delay(50);
    Wire.endTransmission();
}

void setup()
{
    slaveAddress = IRsensorAddress >> 1;   // This results in 0x21 as the address to pass to TWI
    Serial.begin(19200);
    pinMode(ledPin, OUTPUT);      // Set the LED pin as output
    Wire.begin();
    SerialBT.begin("ESP32test"); //Bluetooth device name
    Serial.println("The device started, now you can pair it with bluetooth!");
    // IR sensor initialize
    // Write_2bytes(0x30,0x01); delay(10);
    // Write_2bytes(0x00,0x02); delay(10);
    // Write_2bytes(0x01,0x00); delay(10);
    // Write_2bytes(0x02,0x00); delay(10);
    // Write_2bytes(0x03,0x71); delay(10);
    // Write_2bytes(0x04,0x01); delay(10);
    // Write_2bytes(0x05,0x00); delay(10);
    // Write_2bytes(0x06,p0); delay(10);
    // Write_2bytes(0x07,0x00); delay(10);
    // Write_2bytes(0x08,p1); delay(10);
    // Write_2bytes(0x1A,p2); delay(10);
    // Write_2bytes(0x1B,p3); delay(10);
    // Write_2bytes(0x33,0x03); delay(10);
    // Write_2bytes(0x30,0x08); delay(10);
    Write_2bytes(0x30,0x08); delay(100);
    Write_8bytes(0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xFF); delay(100); //0x90 for high, 0xFF for max
    Write_3bytes(0x07,0x00,0x0C); delay(100); //0x41 for high, 0x0C for max
    Write_3bytes(0x1A,0x00,0x00); delay(100);  //0x40 for high, 0x00 for max
    Write_2bytes(0x33,0x01); delay(100); // 3 for extended,  1 for basic
    Write_2bytes(0x30,0x08); delay(100); 
    delay(100);
}
void loop()
{
    ledState = !ledState;
    if (ledState) { digitalWrite(ledPin,HIGH); } else { digitalWrite(ledPin,LOW); }

    //IR sensor read
    Wire.beginTransmission(slaveAddress);
    Wire.write(0x36);
    Wire.endTransmission();

    Wire.requestFrom(slaveAddress, 16);        // Request the 2 byte heading (MSB comes first)
    for (i=0;i<16;i++) { data_buf[i]=0; }
    i=0;
    while(Wire.available() && i < 16) {
        data_buf[i] = Wire.read();
        i++;
    }

    Ix[0] = data_buf[1];
    Iy[0] = data_buf[2];
    s   = data_buf[3];
    Ix[0] += (s & 0x30) <<4;
    Iy[0] += (s & 0xC0) <<2;

    // Ix[1] = data_buf[4];
    // Iy[1] = data_buf[5];
    // s   = data_buf[6];
    // Ix[1] += (s & 0x30) <<4;
    // Iy[1] += (s & 0xC0) <<2;

    // Ix[2] = data_buf[7];
    // Iy[2] = data_buf[8];
    // s   = data_buf[9];
    // Ix[2] += (s & 0x30) <<4;
    // Iy[2] += (s & 0xC0) <<2;

    // Ix[3] = data_buf[10];
    // Iy[3] = data_buf[11];
    // s   = data_buf[12];
    // Ix[3] += (s & 0x30) <<4;
    // Iy[3] += (s & 0xC0) <<2;

    for(i=0; i<1; i++)
    {
      if (Ix[i] < 1000)
        Serial.print("");
      if (Ix[i] < 100)
        Serial.print("");
      if (Ix[i] < 10)
        Serial.print("");
      //SerialBT.write('x');
      //SerialBT.write(':');
      String string_x = String(float(Ix[i] / 1023),5);
      int j;
      for(j = 0; j < 7; j++)
      {
        SerialBT.write(string_x[j]);        
      }
      SerialBT.write(',');
      //SerialBT.write('y');
      //SerialBT.write(':');
      String string_y = String(float(Iy[i] / 1023),5); // max value of y is 767
      for(j = 0; j < 7; j++)
      {
        SerialBT.write(string_y[j]);
      }
      SerialBT.write('\n');
      SerialBT.write('\n');
      Serial.print(float(Ix[i] / 1023),5);
      // if (i<3)
      //   Serial.print(",");
    }
    Serial.println("");
    delay(15);
}