// Config for Website
var autoconnect = true; // Connect to Brocker on load.
var publishOnChange = false; // Publish values when sliding the slider. 
var locked = false; // Disable User interaction.
var lockedTopic = "node/2015/mainstage/color";

var messageQueueLenght = 30;
var messageQueue = [];

// host = '172.16.153.122';	// hostname or IP address
mqtt_host = 'test.mosquitto.org';
// port
mqtt_port = 8080;

mqtt_client_id = "CLIENT_" + parseInt(Math.random() * 100000);

mqtt_subscribe_topic = 'node/2015/' + mqtt_client_id + '/color'; // topic to subscribe to
mqtt_subscribe_qos = 2;

mqtt_publish_topic = 'node/2015/' + mqtt_client_id + '/color'; // topic to subscribe to
mqtt_publish_message = '222';
mqtt_publish_retained = false;
mqtt_publish_qos = 0;
useTLS = false;
username = null;
password = null;
// username = "jjolie";
// password = "aa";
cleansession = true;
