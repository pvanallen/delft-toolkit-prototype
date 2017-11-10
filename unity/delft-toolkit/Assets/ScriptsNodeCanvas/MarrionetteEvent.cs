using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category(" DelftToolkit")]
	public class MarrionetteEvent : ConditionTask{

		public enum EventTypes{button1, button2, button3, button4, button5, button6, button7, button8};
		public EventTypes eventType = EventTypes.button1;
		public bool toggleState = false;

		public int eventValue = 0;

		private bool eventCheck = false;

		protected override string OnInit() {
			//UnityEngine.Debug.Log ("marrionette INIT");
			marrionetteControl.MrntEvent += setEvent;
			return null;
		}

		protected override void OnDisable()
		{
			marrionetteControl.MrntEvent -= setEvent;
		}

		protected override string info{
			get {return "Marrionette " + eventType.ToString();}
		}

		protected override bool OnCheck(){
			if (eventCheck)
				return true;

			return false;
		}

		void setEvent(string mrntEvent) {
			if (toggleState) {
				if (mrntEvent == eventType.ToString ()) {
					eventCheck = !eventCheck;
				}
			} else {
				eventCheck = false;
				if (mrntEvent == eventType.ToString ())
					eventCheck = true;
			}
	
			//UnityEngine.Debug.Log ("MARRIONETTE Condition (" + eventType.ToString() + "): " + mrntEvent);
		}
	}
}