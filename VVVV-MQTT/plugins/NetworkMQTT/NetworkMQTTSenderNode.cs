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
	[PluginInfo(Name = "MQTT Sender", Category = "Network", Version = "beta 0.1", Help = "Send Mqtt Messages.", Tags = "IoT, MQTT", Credits = "M2MQTT m2mqtt.wordpress.com",  Author = "Jochen Leinberger, explorative-environments.net", AutoEvaluate = true)]
	#endregion PluginInfo
	public class C0_1NetworkMQTTSenderNode : IPluginEvaluate
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

		[Input("ClientID", DefaultString = "v4Sender", IsSingle = true)]
		public ISpread<string> FInputMqttClientId;
		
		[Input("Topic", DefaultString = "/vvvv/", IsSingle = false)]
		public ISpread<string> FInputMqttTopic;

		[Input("Message", DefaultString = "hello vvvv", IsSingle = false)]
		public ISpread<string> FInputMqttMessage;
		
		[Input("Qualiy of Service", DefaultEnumEntry = "QoS_0")]
		public IDiffSpread<QOS> FInputQoS;
		
		[Input("Retained", IsBang = true, DefaultValue = 0, IsSingle = false)]
		IDiffSpread<bool> FInputRetained;

		[Input("Broker adress", DefaultString = "192.168.1.1", IsSingle = true)]
		public ISpread<string> FInputMqttBrokerAdress;

		[Input("Port", DefaultValue = 1883, IsSingle = true)]
		public IDiffSpread<int> FInputMqttPort;

		[Input("Setup Connection", IsBang = true, DefaultValue = 0, IsSingle = true)]
		IDiffSpread<bool> FInputInitMqtt;
		
		[Input("Do send", IsBang = true, DefaultValue = 0, IsSingle = true)]
		IDiffSpread<bool> FInputSendMqtt;
		
		[Output("Is Connected", DefaultValue = 0, IsSingle = true)]
		public ISpread<bool> FOutputIsConnected;
		
		[Output("Message Status")]
		public ISpread<string> FOutputMessageStatus;
		public string MessageStatus = null;
		
		[Output("Connection Status")]
		public ISpread<string> FOutputConnectionStatus;
		public string ConnectionStatus = null;
		
		public bool ConnectionInProgress = false;
		
		public string ID = "Mqtt Sender: ";
		
		[Import()]
		public ILogger FLogger;

		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
			//public async Task Evaluate(int SpreadMax)
		{
			if (FInputInitMqtt[0])
			{
				Task.Run(() => init(FInputMqttBrokerAdress[0], FInputMqttPort[0], FInputMqttClientId[0]));
			}
			
			//FOutputIsConnected.SliceCount = 1;
			
			if (FInputSendMqtt[0])
			{
				try
				{
					if (client.IsConnected)
					{
						// ConnectionStatus = getTimestamp() + "Connected. " + Environment.NewLine;
						FOutputIsConnected[0] = client.IsConnected;
						
						for (int i = 0; i < SpreadMax; i++)
						{
							try
							{
								// FLogger.Log(LogType.Debug, "Sync.");
								switch ((int)FInputQoS[i])
								{
									case 0:
										client.Publish(FInputMqttTopic[i], System.Text.Encoding.UTF8.GetBytes(FInputMqttMessage[i]), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, FInputRetained[i]);
										//client.Disconnect();
										break;
										
									case 1:
										client.Publish(FInputMqttTopic[i], System.Text.Encoding.UTF8.GetBytes(FInputMqttMessage[i]), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, FInputRetained[i]);
										//client.Disconnect();
										break;
										
									case 2:
										client.Publish(FInputMqttTopic[i], System.Text.Encoding.UTF8.GetBytes(FInputMqttMessage[i]), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, FInputRetained[i]);
										//client.Disconnect();
										break;
								}
								
							}
							catch (Exception e)
							{
								FOutputMessageStatus[0] = getTimestamp() + "Failed to publish Message. \r\n";
								FLogger.Log(LogType.Message, ID + "Failed to publish Message. \r\n" + e);
							}
						}
					}
					else if (!client.IsConnected)
					{
						FOutputMessageStatus[0] = getTimestamp() + "Tried to publish message, but lost connection. Try to reconnect. \r\n";
						// FLogger.Log(LogType.Debug, client.IsConnected.ToString() + "not connected!");
						FOutputIsConnected[0] = client.IsConnected;
						//connectClient(FInputMqttClientId[0]); // will try to connect.
						//Task.Run(() => connectMqttClient(FInputMqttClientId[0]));
					}
				}
				catch (Exception e)
				{
					FOutputConnectionStatus[0] = getTimestamp() + "Try to Initialize! \r\n";
					FLogger.Log(LogType.Message, ID + "Try to Initialize! \r\n" + e);
				}
			}
			
			FOutputIsConnected[0] = FOutputIsConnected[0];
			//FOutputConnectionStatus[0] = ConnectionStatus;
			// FOutputMessageStatus[0] = MessageStatus;
		}
		
		//MQTT Definition/////////////////////////////////////////////////////////
		MqttClient client = null;
		
		// private void init()
		private async Task init(string MqttBrocker, int MqttPort, string MqttClientID)
		{
			if (!ConnectionInProgress)
			{
				ConnectionInProgress = true;
				FOutputConnectionStatus[0] = null;
				// create a new instance of the MqttClient
				// client.Disconnect;
				try
				{
					try
					{
						await Task.Run(() => client.Disconnect());
					}
					catch
					{
						FLogger.Log(LogType.Message, ID + "Tried to disconnect but failed.");
					}
					FOutputConnectionStatus[0] += getTimestamp() + "Trying to setup client to connect to broker: " + MqttBrocker + " at Port: " + MqttPort + ".\r\n";
					FOutputConnectionStatus[0] += getTimestamp() + "This might take a momment ... \r\n";
					await Task.Run(() => client = new MqttClient(MqttBrocker, MqttPort, false, null));
					
					// Setup Mqtt Message Listeners
					client.MqttMsgPublished += client_MqttMsgPublished; 		// Message is delivered message.
					client.MqttMsgDisconnected += client_MqttMsgDisconnected; 	// Message lost connection to brocker message.
					
					await Task.Run(() => connectMqttClient(MqttClientID));
				}
				catch (Exception e)
				{
					FOutputConnectionStatus[0] += getTimestamp() + "Failed to setup Client. \r\n";
					FLogger.Log(LogType.Message, ID + "Failed to setup client  details: " + e);
				}
				ConnectionInProgress = false;
			}
			else
			{
				FOutputConnectionStatus[0] += getTimestamp() + "Allready trying to connect. Wait until finished. \r\n";
			}
		}
		
		private async Task connectMqttClient(string MqttClientID)
		{
			try
			{
				await Task.Run(() => client.Connect(MqttClientID));
				//client.Connect(MqttClientID);
				
				if (client.IsConnected)
				{
					FOutputConnectionStatus[0] += getTimestamp() + "Connected sucsessfully to broker: " + FInputMqttBrokerAdress[0] + " at Port: " + FInputMqttPort[0] + ".\r\n";
					FLogger.Log(LogType.Message, ID + "Connected sucsessfully to broker: " + FInputMqttBrokerAdress[0] + " at Port: " + FInputMqttPort[0] + ".\r\n");
					FOutputIsConnected[0] = client.IsConnected;
				}
				else
				{
					FOutputConnectionStatus[0] += getTimestamp() + "Something went wrong.\r\n";
					FLogger.Log(LogType.Message, ID + "Could not connect client");
				}
			}
			catch (Exception e)
			{
				FOutputConnectionStatus[0] += getTimestamp() + "Failed to establish connection to Brocker. 218 \r\n";
				FLogger.Log(LogType.Message, ID + "Failed to establish connection to Brocker.\r\n" + e);
			}
		}
		
		
		// private async Task sendMqtt(string MqttTopic, string MqttMessage, int MqttQoS)
		private void sendMqtt(string MqttTopic, string MqttMessage, int MqttQoS)
		{
			try
			{
				switch (MqttQoS)
				{
					case 0:
						Task.Run(() =>  client.Publish(MqttTopic, System.Text.Encoding.UTF8.GetBytes(MqttMessage), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true));
						//client.Disconnect();
						break;
						
					case 1:
						Task.Run(() => client.Publish(MqttTopic, System.Text.Encoding.UTF8.GetBytes(MqttMessage), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true));
						//client.Disconnect();
						break;
						
					case 2:
						Task.Run(() => client.Publish(MqttTopic, System.Text.Encoding.UTF8.GetBytes(MqttMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true));
						//client.Disconnect();
						break;
				}
			}
			catch (Exception e)
			{
				FOutputConnectionStatus[0] += getTimestamp() + "Failed to publish Message. \r\n";
				FLogger.Log(LogType.Message, ID + "Failed to publish Message. \r\n" + e);
				
			}
		}
		
		private void client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
		{
			// FLogger.Log(LogType.Debug, e.MessageId.ToString());
			FOutputMessageStatus[0] = getTimestamp() + "Delivered Message with ID " + e.MessageId + ".\r\n";
		}

		private void client_MqttMsgDisconnected(object sender, EventArgs e)
		{
			FOutputConnectionStatus[0] += getTimestamp() + "Lost Connection to Broker.\r\n";
			FOutputIsConnected[0] = client.IsConnected;
			FLogger.Log(LogType.Debug, "Lost Connection to Broker. Client Connection Status: " + client.IsConnected);
		}
		
		private string getTimestamp()
		{
			string timeStamp = DateTime.Now.ToString() + ": ";
			return timeStamp;
		}
	}
}