using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NodeCanvas.Tasks.Actions{

	[Category(" DelftToolkit")]
	[Description("Starts a thing action for seconds amount of time")]


	public class DingAction : ActionTask<Transform> {

		public aiGlobals.Devices device = aiGlobals.Devices.ding1;
		public aiGlobals.ActionTypes actionType = aiGlobals.ActionTypes.stop;
		public int parameter1 = 1;
		//public string parameter2 = "Hello ";
		public BBParameter<string> parameter2 = "Hello ";
		public BBParameter<string> parameter3 = "world";
//		[RequiredField]
//		public BBParameter<float> speed = 2;
		public BBParameter<float> actionSeconds = 1.0f;
		public bool addOnParameter3 = false;
		public bool waitActionFinish = true;
		public bool stopAtFinish = true;

		private bool started = false;

		public delegate void DingActionEvent(aiGlobals.Devices device, aiGlobals.ActionTypes action, int a, string b);
		public static event DingActionEvent DingEvent;

		protected override void OnUpdate() {
			
			if (elapsedTime > actionSeconds.value) {
				UnityEngine.Debug.Log ("end of time");
				EndAction (true);
			}

			if (!waitActionFinish){
				
				EndAction();
			}
		}

		protected override void OnExecute() {
			if (DingEvent != null) {
				string param2 = (string)parameter2.value;
				if (addOnParameter3) {
					string addOn = ((string)parameter3.value.Split ('/') [0]).Split(',')[1];
					param2 += addOn;
				}
				DingEvent (device, actionType, parameter1, param2);
			}
		}

		protected override string info{
			get {return "Action\n" + device.ToString() + "->" + actionType.ToString();}
		}
		protected override void OnStop() {
			if (stopAtFinish) {
				if (DingEvent != null)
					DingEvent(device, aiGlobals.ActionTypes.stop, parameter1, parameter2.value);
			}
		}
		protected override void OnPause() {
			if (stopAtFinish) {
				if (DingEvent != null)
					DingEvent(device, aiGlobals.ActionTypes.stop, parameter1, parameter2.value);
			}
		}
	}
}
