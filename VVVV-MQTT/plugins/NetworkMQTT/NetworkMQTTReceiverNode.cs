#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;

//using System.Net;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "MQTT Receiver", Category = "Network", Version = "beta 0.1", Help = "Send MQTT Messages.", Tags = "IoT, MQTT", Credits = "M2MQTT m2mqtt.wordpress.com",  Author = "Jochen Leinberger, explorative-environments.net", AutoEvaluate = true)]
	#endregion PluginInfo
	public class C0_1NetworkMQTTReceiverNode : IPluginEvaluate
	{
		#region enum
		public enum QOS
		{
			QoS_0,
			QoS_1,
			QoS_2,
		}
		#endregion enum
		
		#region fields & pins

		[Input("ClientID", DefaultString = "v4Receiver", IsSingle = true)]
		public IDiffSpread<string> FInputMqttClientId;
		
		[Input("Topic", DefaultString = "#", IsSingle = false)]
		public IDiffSpread<string> FInputMqttTopic;
		
		[Input("Qualiy of Service", DefaultEnumEntry = "QoS_0")]
		public IDiffSpread<QOS> FInputQoS;

		[Input("Broker adress", DefaultString = "localhost", IsSingle = true)]
		public IDiffSpread<string> FInputMqttBrokerAdress;
		
		[Input("Port", DefaultValue = 1883, IsSingle = true)]
		public IDiffSpread<int> FInputMqttPort;
		
		[Input("Setup Connection", IsBang = true, DefaultValue = 0, IsSingle = true)]
		IDiffSpread<bool> FInputInitMqtt;
		
		[Output("Topic")]
		public ISpread<string> FOutputMqttTopic;
		
		[Output("Message")]
		public ISpread<string> FOutputMqttMessage;
		
		[Output("Is Connected")]
		public ISpread<bool> FOutputIsConnected;

		[Output("Connection Status")]
		public ISpread<string> FOutputConnectionStatus;
		public string ConnectionStatus = null;
		
		public bool ConnectionInProgress = false;
		
		public string ID = "Mqtt Receiver: ";

		[Import()]
		public ILogger FLogger;

		
		
		//Read the incomming Messages ////////////////////////////////
		//public string message;
		
		public Queue MqttTopicQueue = new Queue();
		public Queue MqttMessageQueue = new Queue();
		
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			// is defined SpreadMax Equals Mqtt Topic Queue
			SpreadMax =  MqttTopicQueue.Count;
			
			// Setup the MQTT if forced via init or when Topic or Quality of Service changed.
			if (FInputInitMqtt[0] || FInputMqttTopic.IsChanged || FInputQoS.IsChanged )
			{
				Task.Run(() => init(FInputMqttBrokerAdress[0], FInputMqttPort[0]));
			}
			
			// Slice Count equals MqttTopicQueue Count.
			FOutputMqttTopic.SliceCount = MqttTopicQueue.Count;
			FOutputMqttMessage.SliceCount = MqttTopicQueue.Count;
			
			try
			{
				FOutputIsConnected[0] = client.IsConnected;
			}
			catch
			{
				FOutputIsConnected[0] = false;
			}
			
			for (int i = 0; i < SpreadMax; i++) {
				FOutputMqttTopic[i] = (String)MqttTopicQueue.Dequeue();
				FOutputMqttMessage[i] = (String)MqttMessageQueue.Dequeue();
			}
			FOutputConnectionStatus[0] = FOutputConnectionStatus[0];
			FOutputIsConnected[0] = FOutputIsConnected[0];
		}

		//MQTT /////////////////////////////////////////////////////////
		MqttClient client = null;
		private async Task init(string MqttBrocker, int MqttPort)
		{
			
			if (!ConnectionInProgress)
			{
				ConnectionInProgress = true;
				FOutputConnectionStatus[0] = null;
				FOutputConnectionStatus[0] += getTimestamp() + "Trying to setup client to connect to broker: " + MqttBrocker + " at Port: " + MqttPort + ".\r\n";
				FOutputConnectionStatus[0] += getTimestamp() + "This might take a momment ... \r\n";
				
				// 01 - disconnect.
				try
				{
					await Task.Run(() => client.Disconnect());
				}
				catch (Exception e)
				{
					FLogger.Log(LogType.Message, ID + "Tried to disconnect but failed." + e.Message);
				}
				
				// 02 - setup the new client.
				try
				{
					await Task.Run(() => client = new MqttClient(MqttBrocker, MqttPort, false, null));

					// define the clientID
					client.Connect(FInputMqttClientId[0].ToString());
					client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
					
					// Setup Mqtt Message Listeners
					client.MqttMsgSubscribed += client_MqttMsgSubscribed;               // Define what function to call when a subscription is acknowledged
					client.MqttMsgDisconnected += client_MqttMsgDisconnected; 	// Message lost connection to brocker message.
					
					try
					{
						if (client.IsConnected)
						{
							FOutputConnectionStatus[0] += getTimestamp() + "Connected to broker: " + MqttBrocker + " at Port: " + MqttPort + ".\r\n";
							for (int i=0; i < FInputMqttTopic.SliceCount; i++)
							{
								subscribeToTopic( FInputMqttTopic[i], (int)FInputQoS[i]);
							}
						}
					}
					catch
					{
						FOutputConnectionStatus[0] += getTimestamp() + "Failed to connected to broker: " + MqttBrocker + " at Port: " + MqttPort + ".\r\n";
					}
				}
				
				catch (Exception e)
				{
					FLogger.Log(LogType.Message, ID + "Could not connect: " + e.Message);
					FOutputConnectionStatus[0] += getTimestamp() + "Failed to connected to broker: " + MqttBrocker + " at Port: " + MqttPort + ".\r\n";
				}
				ConnectionInProgress = false;
			}
			else
			{
				FOutputConnectionStatus[0] += getTimestamp() + "Allready trying to connect. Wait until finished. \r\n";
			}
		}
		
		public void subscribeToTopic(string MqttTopic, int MqttQoS)
		{
			//client.Unsubscribe(new string[] {"#"});
			
			switch (MqttQoS)
			{
				case 0:
					client.Subscribe(new string[] { MqttTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
					break;
				case 1:
					client.Subscribe(new string[] { MqttTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
					break;
				case 2:
					client.Subscribe(new string[] { MqttTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
					break;
			}
			
		}
		
		private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			// Get the coressponding Topics for the Message
			MqttTopicQueue.Enqueue(e.Topic);
			
			// Get the Message Content
			System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
			string message = enc.GetString(e.Message);
			// FLogger.Log(LogType.Debug, "message: " + message);
			MqttMessageQueue.Enqueue(message);
			// FLogger.Log(LogType.Debug, e.QosLevel.ToString());
		}
		
		// Handle subscription acknowledgements
		private void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
		{
			FLogger.Log(LogType.Message, ID + "Subscribed.");
		}

		private void client_MqttMsgDisconnected(object sender, EventArgs e)
		{
			FOutputConnectionStatus[0] += getTimestamp() + "Lost Connection to Broker.\r\n";
			FOutputIsConnected[0] = client.IsConnected;
			FLogger.Log(LogType.Message, ID + "Lost Connection to Broker. Client Connection Status: " + client.IsConnected);
		}
		
		private string getTimestamp()
		{
			string timeStamp = DateTime.Now.ToString() + ": ";
			return timeStamp;
		}
	}
}