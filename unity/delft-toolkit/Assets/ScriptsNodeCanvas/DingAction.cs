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

		//public enum MovementTypes{stop, forward, backward, turnRight, turnLeft, ledsOn, ledsOff, servoWiggle, mlImuOff, mlImuRun, mlImuTrain1, mlImuTrain2, mlImuTrainStop, analogOff, analogOn0};
		public aiGlobals.MovementTypes movementType = aiGlobals.MovementTypes.stop;

//		[RequiredField]
//		public BBParameter<float> speed = 2;
		public BBParameter<float> actionSeconds = 1.0f;
		public bool waitActionFinish = true;
		public bool stopAtFinish = true;

		private bool started = false;

		public delegate void BtEvent(string moveType);
		public static event BtEvent BtMove;

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
			if (BtMove != null) {
				BtMove (movementType.ToString());
			}
		}

		protected override string info{
			get {return "Action: " + movementType.ToString();}
		}
		protected override void OnStop() {
			if (stopAtFinish) {
				if (BtMove != null)
					BtMove ("stop");
			}
		}
		protected override void OnPause() {
			if (stopAtFinish) {
				if (BtMove != null)
					BtMove ("stop");
			}
		}
	}
}
