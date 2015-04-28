// host = '172.16.153.122';	// hostname or IP address
//mqtt_host = '192.10.10.10';
mqtt_host = 'test.mosquitto.org'
// port
mqtt_port = 8080;

mqtt_client_id = "CLIENT_" + parseInt(Math.random() * 100000);

mqtt_subscribe_topic = 'node/2015/' + mqtt_client_id + '/color';		// topic to subscribe to
mqtt_subscribe_qos = 0;

mqtt_publish_topic = 'node/2015/' + mqtt_client_id + '/color';		// topic to subscribe to
mqtt_publish_message = '222';
mqtt_publish_retained = false;
mqtt_publish_qos = 0;
useTLS = false;
username = null;
password = null;
// username = "jjolie";
// password = "aa";
cleansession = true;
