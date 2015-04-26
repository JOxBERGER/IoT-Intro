// Config for Website
var autoconnect = true; // Connect to Brocker on load.
var publishOnChange = false; // Publish values when sliding the slider. 
var locked = true; // Disable User interaction.
var lockedTopic = "/color@node2015";

var messageQueueLenght = 30;
var messageQueue = [];

// host = '172.16.153.122';	// hostname or IP address
mqtt_host = '192.10.10.10';
// port
mqtt_port = 9011;

mqtt_client_id = "CLIENT_" + parseInt(Math.random() * 100000);

mqtt_subscribe_topic = '/color/' + mqtt_client_id;		// topic to subscribe to

mqtt_publish_topic = '/color/' + mqtt_client_id;		// topic to subscribe to
mqtt_publish_message = '222';
mqtt_publish_retained = true;
mqtt_publish_qos = 0;
useTLS = false;
username = null;
password = null;
// username = "jjolie";
// password = "aa";
cleansession = true;
