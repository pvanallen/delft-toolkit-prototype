using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category(" DelftToolkit")]
	public class DingData : ConditionTask{

		//public enum EventTypes{button1, button2, button3, button4, button5, button6, button7, button8};
		//public EventTypes eventType = EventTypes.button1;
		//public bool toggleState = false;
		public string matchDingMessage = "/analog/0";
		public CompareMethod checkType = CompareMethod.EqualTo;
		public BBParameter<float> valueB;

		[SliderField(0,250f)]
		public float differenceThreshold = 0.0f;

		[SliderField(0,1023.0f)]
		public float value0 = 0.0f;

		public string incomingDingMessage = "";

		protected override string info{
			get	{return matchDingMessage + " " + value0 + OperationTools.GetCompareString(checkType) + valueB;}
		}

		protected override bool OnCheck(){
			if (incomingDingMessage == matchDingMessage) {
				incomingDingMessage = "";
				return OperationTools.Compare ((float)value0, (float)valueB.value, checkType, differenceThreshold);
			} else {
				return false;
			}
		}

		protected override string OnInit() {
			//UnityEngine.Debug.Log ("dingData INIT");
			dingSensors.DingEvent += handleEvent;
			return null;
		}

		protected override void OnDisable() {
			dingSensors.DingEvent -= handleEvent;
		}

		void handleEvent(string adrs, float val0, float val1, float val2) {

			incomingDingMessage = adrs;
			value0 = val0;
			//UnityEngine.Debug.Log ("MARRIONETTE Condition (" + eventType.ToString() + "): " + mrntEvent);
		}
	}
}