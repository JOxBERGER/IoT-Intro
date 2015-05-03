// This Code is developed and tested for a Spark Core. Copy the code into your Spark IDE found at: build.spark.io 
// You need to include the following Librarys:
// - SparkJson
// - MQTT 
// Details on installing Librarys: http://docs.spark.io/build/#flash-apps-with-spark-build-using-libraries

// This Code is developed and tested for a Spark Core. Copy the code into your Spark IDE found at: build.spark.io 
// You need to include the following Librarys:
// - SparkJson
// - MQTT 
// Details on installing Librarys: http://docs.spark.io/build/#flash-apps-with-spark-build-using-libraries


// Use a unique space here e.g. node/2015/iot/core-yourname/sub etc.
// otherwise some one else might overwrite your topics, as long as 
// you are running on a public Server.
char AddressColor[50] = "node/2015/iot/core-yourname/color"; 
char AddressAnalog01[50] = "node/2015/iot/core-yourname/analog01"; 
char AddressDebug[50] = "node/2015/iot/core-yourname/debug"; 



char ID[50];// We define a char array to contain the ID.

int LEDcolor []= {0, 0, 0}; //Array to store values for colors
    

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
    // store payload of arrived message.
	char payloadIn[length + 1];
    memcpy(payloadIn, payload, length);
    payloadIn[length] = NULL;
    //String message(inout);
    
    StaticJsonBuffer<200> jsonBuffer;
    JsonObject& arrayInput = jsonBuffer.parseObject(payloadIn);
    

    if (!arrayInput.success()) {
    //client.publish(baseAddressDebug, "Json Parsing failed.");
    return;
    }
    else if (arrayInput.success()){
        LEDcolor[0] = arrayInput["red"];
        LEDcolor[1] = arrayInput["green"];
        LEDcolor[2] = arrayInput["blue"];
    }
    
    
    
    //int receiveValue;
    //receiveValue = atoi(p);
    //receiveValue = 125;
    
    //RGB.color(_val, _val, _val);
    RGB.color(LEDcolor[0], LEDcolor[1], LEDcolor[2]);
    
    /*
    char brightnessChar[10];
    sprintf(brightnessChar, "%d", receiveValue);
    client.publish("/convert/jochen", receiveValue);
    */
}


void setup() {
    RGB.control(true);
    // blink red green blue after startup
    RGB.color(255, 0, 0);
    delay(1000);
    RGB.color(0, 255, 0);
    delay(1000);
    RGB.color(0, 0, 255);
    delay(1000);

    
    //generate a random ID
    char IDrandom[5];
    sprintf(IDrandom, "%d", random(9999));
    strcpy(ID, "ID_"); //IDstart); // copy string one into the result.
    strcat(ID,IDrandom); // append string two to the result.
    
    // connect to the server
    client.connect(ID);
    delay(1000);

    // publish/subscribe
    int connectattemps = 0;
    while (!client.isConnected()){
       connectattemps++;
       client.connect(ID);
       delay(1000);
    }
    if (client.isConnected()) {
        char attemps[30];
        sprintf(attemps, "%d", connectattemps);
        strcat(attemps, " attemps"); // append string two to the result.
        client.publish(AddressDebug, attemps);
        client.publish(AddressDebug, ID);
        
        client.subscribe(AddressColor);
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
            client.publish(AddressAnalog01, sensorChar); // better change that topic to be unique
            sensorOld = sensorNew;
        }
    }
    else{
        // try to reconnect
        client.connect(ID);
        delay(1000);
        if (client.isConnected()) {
            client.publish(AddressDebug, "reconnected!");
        }
    }
    
}