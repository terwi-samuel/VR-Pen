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
    pinMode(ledPin, OUTPUT);      // Set the LED pin as output
    Wire.begin();
    SerialBT.begin("ESP32_XY_Camera"); //Bluetooth device name
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

    // Convert XY position values to strings and prepare uint8_t buffers for Bluetooth transmission
    String string_x = String(float(Ix[0] / 1023),5);
    uint8_t buf1[string_x.length() + 1];
    memcpy(buf1, string_x.c_str(),string_x.length());
    buf1[string_x.length()] = ',';
    SerialBT.write(buf1, string_x.length()+1);
    String string_y = String(float(Iy[0] / 1023),5); // max value of y is 767
    uint8_t buf2[string_y.length()];
    memcpy(buf2, string_y.c_str(),string_y.length());
    SerialBT.write(buf2, string_y.length());
    SerialBT.println();

    delay(15);
}