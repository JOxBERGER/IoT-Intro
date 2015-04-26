// host = '172.16.153.122';	// hostname or IP address
mqtt_host = '192.10.10.10';
// port
mqtt_port = 9011;

mqtt_client_id = "CLIENT_" + parseInt(Math.random() * 100000);

mqtt_subscribe_topic = '/color/' + mqtt_client_id;		// topic to subscribe to
mqtt_subscribe_qos = 2;

mqtt_publish_topic = '/color/' + mqtt_client_id;		// topic to subscribe to
mqtt_publish_message = '222';
mqtt_publish_retained = true;
mqtt_publish_qos = 1;
useTLS = false;
username = null;
password = null;
// username = "jjolie";
// password = "aa";
cleansession = true;
