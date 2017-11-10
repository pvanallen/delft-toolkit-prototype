using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;

public class OscCentral : MonoBehaviour {

	private Dictionary<string, ServerLog> servers;
	private Dictionary<string, ClientLog> clients;

	private Dictionary<string, long> lastOscMsgIn = new Dictionary<string, long>();
	private Dictionary<string, string> lastOscMsgOut = new Dictionary<string, string>();

	void Start () {

		servers = new Dictionary<string, ServerLog>();
		clients = new Dictionary<string,ClientLog> ();

		OSCHandler.Instance.UpdateLogs();

		servers = OSCHandler.Instance.Servers;
		clients = OSCHandler.Instance.Clients;

		foreach (KeyValuePair<string, ServerLog> item in servers) {
			//print ("server: " + item.Key);
			lastOscMsgIn.Add (item.Key, 0);
		}
		foreach (KeyValuePair<string, ClientLog> item in clients) {
			//print ("client: " + item.Key);
			lastOscMsgOut.Add (item.Key, "");
		}
	}

	void Update () {

		OSCHandler.Instance.UpdateLogs();

		servers = OSCHandler.Instance.Servers;
		clients = OSCHandler.Instance.Clients;

		// log received messages
		//
		foreach (KeyValuePair<string, ServerLog> item in servers) {
			// get the most recent NEW OSC message received
			if (item.Value.packets.Count > 0 && item.Value.packets[item.Value.packets.Count - 1].TimeStamp != lastOscMsgIn[item.Key]) {

				// count back until we find the matching timestamp
				int lastMsgIndex = item.Value.packets.Count - 1;
				while (lastMsgIndex > 0 && item.Value.packets [lastMsgIndex].TimeStamp != lastOscMsgIn [item.Key]) {
					lastMsgIndex--;
				}
					
				// set how many messages are queued up
				int msgsQd = 1;
				if (item.Value.packets.Count > 1) { // not the first item
					msgsQd = item.Value.packets.Count - lastMsgIndex - 1;
				}

				// print the queued messages
				for (int msgIndex = item.Value.packets.Count - msgsQd; msgIndex < item.Value.packets.Count; msgIndex++) {
					if (!item.Value.packets [msgIndex].Address.StartsWith("/analog/")) {
						UnityEngine.Debug.Log (String.Format ("OSC RECEIVED: {0} Address: {1} {2} {3} {4}",
							item.Key, // Server name
							item.Value.packets [msgIndex].Address, // OSC address
							(item.Value.packets [msgIndex].Data.Count > 0 ? item.Value.packets [msgIndex].Data [0].ToString () : "null"),
							(item.Value.packets [msgIndex].Data.Count > 1 ? item.Value.packets [msgIndex].Data [1].ToString () : "null"),
							(item.Value.packets [msgIndex].Data.Count > 2 ? item.Value.packets [msgIndex].Data [2].ToString () : "null")
						)                                                     
						);
					}
				}
				lastOscMsgIn[item.Key] = item.Value.packets[item.Value.packets.Count - 1].TimeStamp;
			}
		}

		// log sent messages
		//
		foreach( KeyValuePair<string, ClientLog> item in clients )
		{
			
			// get the most recent NEW OSC message sent

			if (item.Value.log.Count > 0 && item.Value.log[item.Value.log.Count - 1] != lastOscMsgOut[item.Key]) {

				// count back until we find the matching timestamp
				int lastMsgIndex = item.Value.log.Count - 1;
				while (lastMsgIndex > 0 && item.Value.log[lastMsgIndex] != lastOscMsgOut [item.Key]) {
					lastMsgIndex--;
				}

				// set how many messages are queued up
				int msgsQd = 1;
				if (item.Value.log.Count > 1) { // not the first item
					msgsQd = item.Value.log.Count - lastMsgIndex - 1;
				}
				//print ("SEND Queued up: " + msgsQd);

				//print ("MESSAGES OUT: " + msgsQd + " " + item.Value.log.Count + " " + item.Value.messages.Count);
				for (int msgIndex = item.Value.messages.Count - msgsQd; msgIndex < item.Value.messages.Count; msgIndex++) {

						UnityEngine.Debug.Log (String.Format ("OSC SENT: {0} Address: {1} {2} {3} {4}", 
						item.Key, // Server name
						item.Value.messages [msgIndex].Address, // OSC address
						(item.Value.messages [msgIndex].Data.Count > 0 ? item.Value.messages [msgIndex].Data [0].ToString () : "null"),
						(item.Value.messages [msgIndex].Data.Count > 1 ? item.Value.messages [msgIndex].Data [1].ToString () : "null"),
						(item.Value.messages [msgIndex].Data.Count > 2 ? item.Value.messages [msgIndex].Data [2].ToString () : "null")
					)
					);
				}
				lastOscMsgOut[item.Key] = item.Value.log[item.Value.log.Count - 1];
			}
		}
	}
}
