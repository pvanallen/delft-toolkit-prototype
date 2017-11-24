// control the physical thing (ding) via OSC to a nodejs server which 
// communicates by Bluetooth to the robot
//

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;

public class dingControlPhysical : dingControlBase {

    public string TargetAddr;
    public int OutGoingPort;
    public int InComingPort;


    private Dictionary<string, ServerLog> servers;
	private Dictionary<string, ClientLog> clients;

	private const string OSC_SERVER_CLIENT = "DelftDingOSC";

    // Script initialization
    void Awake() { 
		// using awake so that it happens before oscCentral initializes in Start()

        //OSCHandler.Instance.Init(); //init OSC
		OSCHandler.Instance.Init(OSC_SERVER_CLIENT, TargetAddr, OutGoingPort, InComingPort);
        servers = new Dictionary<string, ServerLog>();
		clients = new Dictionary<string, ClientLog> ();
	}

	public override void handleAction () {
		//base.Update ();
		List<object> oscValues = new List<object>();
		string oscString = "/robot/";

		if (device == aiGlobals.Devices.ding2) {
			// DingAction may needs to be broken into different objects for different devices
			oscString = "/ding2/";
		}

		printNewMode ("DING-PHYSICAL", action);

		oscString += action.ToString();
		oscValues.AddRange(new object[]{param1, param2});
		OSCHandler.Instance.SendMessageToClient (OSC_SERVER_CLIENT, oscString, oscValues);
	}
}