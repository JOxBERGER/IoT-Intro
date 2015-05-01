char* baseAddress = "/node/jochen";

// MQTT Brocker adress
//byte _server[] = {192, 10, 10, 10}; // use this to declare the broker address based on ist IP address. declaration via hostname is done in the client definition directly.
int _port = 1883;

// Led Pin
int ledPin = 9; 


//// YÚN specifics ////
//#include <SPI.h>
#include <Bridge.h>
#include <YunClient.h>

//// MQTT Specific ////
#include <PubSubClient.h>
#define MQTT_MAX_PACKET_SIZE 1024


// Callback function header
void callback(char* topic, byte* payload, unsigned int length) {
  // handle message arrived
  char p[length + 1];
    memcpy(p, payload, length);
    p[length] = NULL;
    //String message(p);
    
    int receiveValue;
    receiveValue = atoi(p);
    
    analogWrite(ledPin, receiveValue);
    /*
    char brightnessChar[10];
    sprintf(brightnessChar, "%d", receiveValue);
    client.publish("/convert/jochen", receiveValue);
    */
}

YunClient yun;
PubSubClient client("test.mosquitto.org", _port, callback, yun);

void setup() {

  delay(10000); //The bare minimum (2500) needed to be able to reboot both linino and leonardo.
  // if reboot fails try increasing this number
  // The more you run on linux the higher this number should be
  Bridge.begin();
  delay(4000);

  //// MQTT Specific ////
  if (client.connect("JochenYUN")) {
    client.publish(baseAddress, "I am there");
    client.subscribe("/node2015/color");
  }
  
  pinMode(ledPin, OUTPUT);
  delay(1000);

}

int sensorOld = 0;
void loop() {
    client.loop();

    int sensorNew = analogRead(0);
    int difference = sensorOld - sensorNew;
    difference = abs(difference);

    if (difference > 6) {
      char sensorChar[10];
      sprintf(sensorChar, "%d", sensorNew);
      client.publish("/node/jochen/sensor", sensorChar);
      sensorOld = sensorNew;
    }
}
