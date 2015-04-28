#include "MQTT/MQTT.h"

byte rgbLed[3] = {0, 255, 0}; 
char baseAddress[50] = "/node/jochen";

/**
 * if want to use IP address,
 * byte server[] = { XXX,XXX,XXX,XXX };
 * MQTT client(server, 1883, callback);
 * want to use domain name,
 * MQTT client("www.sample.com", 1883, callback);
 **/
byte server[] = {192,10,10,10};
MQTT client(server, 1883, callback);
//MQTT client("server_name", 1883, callback);

125
123
lenght = 3
125_
// recieve message
void callback(char* topic, byte* payload, unsigned int length) {
    // handle message arrived
	char p[length + 1];
    memcpy(p, payload, length);
    p[length] = NULL;
    //String message(p);
    
    int receiveValue;
    receiveValue = atoi(p);
    
    RGB.color(receiveValue, receiveValue, receiveValue);
    /*
    char brightnessChar[10];
    sprintf(brightnessChar, "%d", receiveValue);
    client.publish("/convert/jochen", receiveValue);
    */
}


void setup() {
    RGB.control(true);
    // Show colors on the LED
    RGB.color(rgbLed[0], rgbLed[1], rgbLed[2]);
    
    // connect to the server
    client.connect("jochen_spark_001");

    // publish/subscribe
    if (client.isConnected()) {
        client.publish(baseAddress,"I am there.");
        client.subscribe("node2015/jochen/color");
    }
    delay(1000);
}

int sensorOld = 0;
void loop() {
    if (client.isConnected()){
        client.loop();
        
        int sensorNew = analogRead(0);
        int difference = sensorOld - sensorNew;
        difference = abs(difference);
        
        if(difference > 3){
            char sensorChar[10];
            sprintf(sensorChar, "%d", sensorNew);
            client.publish("/node/jochen/sensor", sensorChar);
            sensorOld = sensorNew;
        }
    }
}