using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category(" DelftToolkit")]
	public class DingNumber : ConditionTask{

		//public enum EventTypes{button1, button2, button3, button4, button5, button6, button7, button8};
		//public EventTypes eventType = EventTypes.button1;
		//public bool toggleState = false;
		public string matchDingMessage = "/ding1/a/0";
		public CompareMethod checkType = CompareMethod.EqualTo;
		public BBParameter<float> valueB;

		[SliderField(0,250f)]
		public float differenceThreshold = 0.0f;

		[SliderField(0,1023.0f)]
		public float value0 = 0.0f;

		public string incomingDingMessage = "";

		protected override string info{
			get	{return matchDingMessage + "\n" + value0 + OperationTools.GetCompareString(checkType) + valueB;}
		}

		protected override bool OnCheck(){
			if (incomingDingMessage == "/num" + matchDingMessage) {
				incomingDingMessage = "";
				return OperationTools.Compare ((float)value0, (float)valueB.value, checkType, differenceThreshold);
			} else {
				incomingDingMessage = "";
				return false;
			}
		}

		protected override string OnInit() {
			//UnityEngine.Debug.Log ("dingData INIT");
			dingSensors.DingNumEvent += handleEvent;
			return null;
		}

		protected override void OnDisable() {
			dingSensors.DingNumEvent -= handleEvent;
		}

		void handleEvent(string adrs, float val0, float val1, float val2) {
			incomingDingMessage = adrs;
			if (incomingDingMessage == "/num" + matchDingMessage) {
				value0 = val0;
			} else {
				value0 = 0;
			}
			UnityEngine.Debug.Log ("DING DATA Condition (" + adrs + "): " + val0);
		}
	}
}