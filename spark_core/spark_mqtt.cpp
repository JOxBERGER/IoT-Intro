//#include "MQTT/MQTT.h" you need to include the MQTT library into via spark IDE at https://build.spark.io after creating a new app. just search for MQTT at the Library functions inside build.spark.io

char baseAddress[50] = "node/2015/core"; 
char ID[50];// We define a char array to contain the ID.

/**
 * if want to use IP address,
 * byte server[] = { XXX,XXX,XXX,XXX };
 * MQTT client(server, 1883, callback);
 * want to use domain name,
 * MQTT client("www.sample.com", 1883, callback);
 **/
byte server[] = {192,10,10,10};
//MQTT client(server, 1883, callback);
//MQTT client("server_name", 1883, callback);
//MQTT client(server, 1883, callback);
MQTT client("test.mosquitto.org", 1883, callback);

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
    // blink red green blue
    RGB.color(255, 0, 0);
    delay(1000);
    RGB.color(0, 255, 0);
    delay(1000);
    RGB.color(0, 0, 255);
    delay(1000);

    
    //generate a random ID (hopefully unique)
    char IDstart[4] = "ID_";
    char IDrandom[5];
    sprintf(IDrandom, "%d", random(9999));
    
    
    strcpy(ID,IDstart); // copy string one into the result.
    strcat(ID,IDrandom); // append string two to the result.
    
    // connect to the server
    client.connect(ID);

    // publish/subscribe
    if (client.isConnected()) {
        client.publish(baseAddress, ID);
        client.subscribe("node/2015/core001/color"); // better change that topic to be unique
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
            client.publish("node/2015/mainstage001/color", sensorChar); // better change that topic to be unique
            sensorOld = sensorNew;
        }
    }
    else{
        // try to reconnect
        client.connect(ID);
        client.publish(baseAddress, "reconnected!");
    }
    
}