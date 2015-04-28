var mqtt_client;
var reconnectTimeout = 2000;
var messageQueueLenght = 30;
var messageQueue = [];


$(document).ready(function() {
	setupMessageArray(messageQueueLenght, messageQueue);
	printMessageArray(messageQueue, messageQueueLenght, "start");

	MQTTconnect();
	writeConfigToGUI();	
});


function MQTTconnect() {
	console.log("MQTTconnect()");
	
	// new  mqtt client
	mqtt_client = new Paho.MQTT.Client(
		mqtt_host,
		mqtt_port,
		mqtt_client_id );

		var options = {
			timeout: 1000,
			useSSL: useTLS,
			cleanSession: cleansession,
			onSuccess: onConnect,
			onFailure: function (message) {
				$('#status').text("Connection failed: " + message.errorMessage + "Retrying");
				setTimeout(MQTTconnect, reconnectTimeout);
			}
		};

/*		if (username != null) {
			options.userName = username;
			options.password = password;
		}
*/
		mqtt_client.onConnectionLost = onConnectionLost;
		mqtt_client.onMessageArrived = onMessageArrived;

		mqtt_client.connect(options);
	};

	function onConnect() {
		console.log("onConnect()");

		$('body').css("opacity", "1");
		$('#status').text("Connected to " +  mqtt_host + " at port " + mqtt_port);

		//Hide connect Button
		$('#status').css( {
			"background": "none",
		});
		$('#status').html("Connected to " + mqtt_host + " at port " + mqtt_port);
		$('#status').prop("disabled", true);
		$('#status').blur();
		$('#client_id').css("background", "none")
		$('#client_id').prop("disabled", true);


		// Subscribe 
		mqtt_subscribe_topic = doSubscribe(mqtt_client, mqtt_subscribe_topic, 0)
		console.log("obj 01" + mqtt_client);
		// Send a frist message.
		mqtt_out_message = new Paho.MQTT.Message(mqtt_client_id + " connected");
		mqtt_out_message.destinationName = "/client_id/" + mqtt_client_id;
		mqtt_client.send(mqtt_out_message);
	};


	function onConnectionLost(responseObject) {
		console.log(responseObject.errorMessage);
		$('#status').text('lost Connection to' +  mqtt_host + ':' + mqtt_port + "//" + responseObject.errorMessage);
		setTimeout(MQTTconnect, reconnectTimeout);
	};


	function onMessageArrived(mqtt_in_message) {
		var topic = mqtt_in_message.destinationName;
		var payload = mqtt_in_message.payloadString;
		printMessageArray(messageQueue, messageQueueLenght, topic + "  " + payload);
		var newColor = colorFromDeg(payload);
		$("body").css("background-color", newColor);
	};

	function colorFromDeg(_colorIn){
		var _newColor = parseInt(_colorIn);
		var _color = "hsla("+ _newColor +", 50%, 50%, 1)";
		return _color
	};


	function subscribeNewTopic(){
		//// unscubscribe to curent topic
		mqtt_client.unsubscribe(mqtt_subscribe_topic);

		//// subscribe to new topic
		// Get the values from the input:
		mqtt_subscribe_topic = $("#suscribe_topic").val();
		mqtt_subscribe_qos = parseInt( $("#qos_subscribe").val());
		
		//subscribe
		mqtt_client.subscribe(mqtt_subscribe_topic, {qos: mqtt_subscribe_qos});
		printMessageArray(messageQueue, messageQueueLenght, "New Topic " + mqtt_subscribe_topic);
	};
	
	//// Subscribe client to topic
	function doSubscribe(_client, _topic, _qos){
		console.log("doSubscribe " + _topic)
		_client.subscribe(_topic, {qos: _qos});
		printMessageArray(messageQueue, messageQueueLenght, "New Topic " + _topic);
		return _topic
	};
	

	function doPublishMessage(){			
		//// Get the values from the input:
		mqtt_publish_topic = $("#publish_topic").val();
		mqtt_publish_message = $("#publish_message").val();
		mqtt_out_retained = $("#publish_retained").prop("checked");
		mqtt_qos_publish = $("#qos_publish").val();

		//// Create a new message:
		mqtt_out_message = new Paho.MQTT.Message(mqtt_publish_message);
		mqtt_out_message.destinationName = mqtt_publish_topic;
		mqtt_out_message.qos = parseInt(mqtt_qos_publish);
		mqtt_out_message.retained = mqtt_out_retained;
		mqtt_client.send(mqtt_out_message);
	};

	function writeConfigToGUI(){
		//Brocker
		$('#client_id').append(" " + mqtt_client_id);
		$('#status').html("Connect to MQTT Brocker " + mqtt_host + ":" + mqtt_port);

		//Publish
		$('#publish_topic').val(mqtt_publish_topic);
		$('#publish_message').val(mqtt_publish_message);
		$('#publish_retained').prop("checked", mqtt_publish_retained);
		$('#qos_publish').val(mqtt_publish_qos);
	
		//Subscribe
		$('#suscribe_topic').val(mqtt_subscribe_topic);
		$('#qos_subscribe').val(mqtt_subscribe_qos);

	};


	//////
	function printMessageArray(_inputArray, _maxLength, _newContent){
		_inputArray.shift();

		_inputArray.push(_newContent)
		for(var i=0; i<_maxLength; i++){
			var id = '#arrayID' + i;
			$(id).text(_inputArray[i]);
		}
	};

	function setupMessageArray(_maxLength, _array){
		for(var i=0; i<_maxLength; i++){
			$('#mqtt_log').prepend('<li id="arrayID' + i +'" ></li>');
			_array.push("");
		}
	};