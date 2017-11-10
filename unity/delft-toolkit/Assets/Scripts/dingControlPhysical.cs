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

//	private int lastOscMessageIn = 0;
//	private int lastOscMessageOut = 0;

	private const string OSC_SERVER_CLIENT = "DelftDingOSC";

	//private enum Commands {stop, forward, backward, turnRight, turnLeft, ledsOn, ledsOff, servoWiggle, mlImuOff, mlImuRun, mlImuTrain1, mlImuTrain2, mlImuTrainStop, analogOff, analogOn0};

    // Script initialization
    void Awake() { 
		// using awake so that it happens before oscCentral initializes in Start()

        //OSCHandler.Instance.Init(); //init OSC
		OSCHandler.Instance.Init(OSC_SERVER_CLIENT, TargetAddr, OutGoingPort, InComingPort);
        servers = new Dictionary<string, ServerLog>();
		clients = new Dictionary<string, ClientLog> ();
	}

	public override void handleCommand (string command) {
		//base.Update ();

		bool commandValid = false;
		string oscCommand = "/robot/";

		if (command == "recognize") {
			// this needs refactoring to have a formal system for specifying the appropriate device
			// DingAction needs to be broken into different objects for different devices, or in widget, set which device to send command to
			oscCommand = "/ding2/";
		}

		printNewMode ("DING-PHYSICAL", command);

		// check to ensure that the moveMode command received is valid
		foreach (aiGlobals.MovementTypes iCommand in Enum.GetValues(typeof(aiGlobals.MovementTypes))) {
			if (command == iCommand.ToString ())
				commandValid = true;
		}

		// take command from the event and send out via OSC to the Nodejs server -> Bluetooth to Arduino
		if (commandValid) {
			oscCommand += command;
			OSCHandler.Instance.SendMessageToClient (OSC_SERVER_CLIENT, oscCommand, 1);
		}
	}
}