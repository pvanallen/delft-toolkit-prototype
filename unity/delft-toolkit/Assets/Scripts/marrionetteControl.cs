using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;

public class marrionetteControl : MonoBehaviour {

	public string TargetAddr;
	public int OutGoingPort;
	public int InComingPort;

	private Dictionary<string, ServerLog> servers;
	private Dictionary<string, ClientLog> clients;

	public delegate void MarrionetteEvent(string moveType);
	public static event MarrionetteEvent MrntEvent;

	private int lastOscMessageIn = 0;
	private int lastOscMessageOut = 0;

	private const string OSC_SERVER_CLIENT = "DelftMarrionetteOSC";

	// Script initialization
	void Awake() {
		// using awake so that it happens before oscCentral initializes in Start()

		//OSCHandler.Instance.Init(); //init OSC
		OSCHandler.Instance.Init(OSC_SERVER_CLIENT, TargetAddr, OutGoingPort, InComingPort);
		servers = new Dictionary<string, ServerLog>();
		clients = new Dictionary<string,ClientLog> ();
	}

	void Update () {

		OSCHandler.Instance.UpdateLogs();

		servers = OSCHandler.Instance.Servers;
		clients = OSCHandler.Instance.Clients;

		OSCHandler.Instance.UpdateLogs();

		foreach (KeyValuePair<string, ServerLog> item in servers) {
			// get the most recent NEW OSC message from the Marrionette device, and send corresponding event to the Behavior Tree(s)
			//
			if (OSC_SERVER_CLIENT == item.Key && item.Value.log.Count > lastOscMessageIn) {

				lastOscMessageIn = item.Value.packets.Count;
				int lastPacketIndex = item.Value.packets.Count - 1;

				// make sure it has an argument
				if (item.Value.packets[lastPacketIndex].Data.Count > 0)
				{
					string address = item.Value.packets [lastPacketIndex].Address;
					int arg1 = int.Parse(item.Value.packets[lastPacketIndex].Data[0].ToString());

					address += " " + arg1;
					switch (address) {
					case "/1/push1 1":
						if (MrntEvent != null)
							MrntEvent ("button1");
						break;
					case "/1/push2 1":
						if (MrntEvent != null)
							MrntEvent ("button2");
						break;
					case "/1/push3 1":
						if (MrntEvent != null)
							MrntEvent ("button3");
						break;
					case "/1/push4 1":
						if (MrntEvent != null)
							MrntEvent ("button4");
						break;
					case "/1/push5 1":
						if (MrntEvent != null)
							MrntEvent ("button5");
						break;
					case "/1/push6 1":
						if (MrntEvent != null)
							MrntEvent ("button6");
						break;
					case "/1/push7 1":
						if (MrntEvent != null)
							MrntEvent ("button7");
						break;
					case "/1/push8 1":
						if (MrntEvent != null)
							MrntEvent ("button8");
						break;
					default:
						print (OSC_SERVER_CLIENT + " default stop");
						if (MrntEvent != null)
							MrntEvent ("stop");
						break;
					}
				}
			}
		}
	}
}