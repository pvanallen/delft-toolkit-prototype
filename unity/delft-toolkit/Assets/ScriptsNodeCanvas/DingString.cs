using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category(" DelftToolkit")]
	public class DingString : ConditionTask{

		public string matchDingMessage = "/ding2/objIdent";
		public aiGlobals.StringCompare checkType = aiGlobals.StringCompare.Contains;
		public BBParameter<string> textValue;
		public BBParameter<string> savedText;

		public string incomingDingMessage = "";
		public string sentText;

		private string comparison = "";

		protected override string info{
			get	{return matchDingMessage + "\n" + aiGlobals.GetCompareString(checkType) + " " + textValue;}
		}

		protected override bool OnCheck(){
			if (incomingDingMessage == "/str" + matchDingMessage) {
				incomingDingMessage = "";
				savedText.value = sentText;
				return aiGlobals.CompareString (sentText, (string)textValue.value, checkType);
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
			sentText = val;
		}
	}
}
