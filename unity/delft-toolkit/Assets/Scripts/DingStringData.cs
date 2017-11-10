using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category(" DelftToolkit")]
	public class DingStringData : ConditionTask{

		public string matchDingMessage = "/string/objIdent";
		public aiGlobals.StringCompare checkType = aiGlobals.StringCompare.Contains;
		public BBParameter<string> valueB;

		public string valueA;

		public string incomingDingMessage = "";

		private string comparison = "";

		protected override string info{
			get	{return matchDingMessage + " " + aiGlobals.GetCompareString(checkType) + " " + valueB;}
		}

		protected override bool OnCheck(){
			if (incomingDingMessage == matchDingMessage) {
				incomingDingMessage = "";
				return aiGlobals.CompareString (valueA, (string)valueB.value, checkType);
			} else {
				return false;
			}
		}

		protected override string OnInit() {
			dingSensors.DingStrEvent += handleEvent;
			return null;
		}

		protected override void OnDisable() {
			dingSensors.DingStrEvent -= handleEvent;
		}

		void handleEvent(string adrs, string val) {
			UnityEngine.Debug.Log ("received Event" + adrs + val);
			incomingDingMessage = adrs;
			valueA = val;
		}
	}
}