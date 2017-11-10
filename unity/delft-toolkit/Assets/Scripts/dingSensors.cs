using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;

public class dingSensors : MonoBehaviour {

	public int InComingPort;

	private Dictionary<string, ServerLog> servers;

	public delegate void DingDataEvent(string oscMessage, float val0, float val1, float val2);
	public static event DingDataEvent DingEvent;

	public delegate void DingStringEvent(string oscMessage, string val);
	public static event DingStringEvent DingStrEvent;

	private long lastOscMessageIn = 0;

	private const string OSC_SERVER = "DingServer";

	// Script initialization
	void Awake() {
		// using awake so that it happens before oscCentral initializes in Start()

		//OSCHandler.Instance.Init(); //init OSC
		//OSCHandler.Instance.Init(OSC_SERVER_CLIENT, TargetAddr, OutGoingPort, InComingPort);
		OSCHandler.Instance.CreateServer("DingServer",InComingPort);
		servers = new Dictionary<string, ServerLog>();
	}

	void Update () {

		OSCHandler.Instance.UpdateLogs();

		servers = OSCHandler.Instance.Servers;

		foreach (KeyValuePair<string, ServerLog> item in servers) {
			// get the most recent NEW OSC message received
			if (OSC_SERVER == item.Key && item.Value.packets.Count > 0 && item.Value.packets[item.Value.packets.Count - 1].TimeStamp != lastOscMessageIn) {

				// count back until we find the matching timestamp
				int lastMsgIndex = item.Value.packets.Count - 1;
				while (lastMsgIndex > 0 && item.Value.packets [lastMsgIndex].TimeStamp != lastOscMessageIn) {
					lastMsgIndex--;
				}

				// set how many messages are queued up
				int msgsQd = 1;
				if (item.Value.packets.Count > 1) { // not the first item
					msgsQd = item.Value.packets.Count - lastMsgIndex - 1;
				}

				// print the queued messages
				for (int msgIndex = item.Value.packets.Count - msgsQd; msgIndex < item.Value.packets.Count; msgIndex++) {
					//
					string address = item.Value.packets [msgIndex].Address;
					if (item.Value.packets [msgIndex].Address.StartsWith ("/analog/")) {
						float value0 = item.Value.packets [msgIndex].Data.Count > 0 ? float.Parse (item.Value.packets [msgIndex].Data [0].ToString ()) : 0.0f;
						float value1 = item.Value.packets [msgIndex].Data.Count > 1 ? float.Parse (item.Value.packets [msgIndex].Data [1].ToString ()) : 0.0f;
						float value2 = item.Value.packets [msgIndex].Data.Count > 2 ? float.Parse (item.Value.packets [msgIndex].Data [2].ToString ()) : 0.0f;
						if (DingEvent != null)
							DingEvent (address, value0, value1, value2);
					} else if (item.Value.packets [msgIndex].Address.StartsWith ("/string/")) {
						
						string value = item.Value.packets [msgIndex].Data.Count > 0 ? item.Value.packets [msgIndex].Data [0].ToString () : "null";
						print ("sending Event" + address + value);
						if (DingStrEvent != null)
							DingStrEvent (address, value);
					}
					//print(OSC_SERVER + ": " + address + " " + value0 + " " + value1 + " " + value2);
				}
				lastOscMessageIn = item.Value.packets[item.Value.packets.Count - 1].TimeStamp;
			}
		}
	}
}