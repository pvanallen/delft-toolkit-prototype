using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category(" DelftToolkit")]
	public class MarrionetteEvent : ConditionTask{

		public enum EventTypes{button1, button2, button3, button4, button5, button6, button7, button8, button9, button10};
		public EventTypes eventType = EventTypes.button1;
		public bool toggleState = false;

		public int eventValue = 0;

		private bool toggleValue = false;
		private string savedEvent = "";


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
			bool eventCheck = false;
			if (toggleState) {
				if (savedEvent == eventType.ToString ()) {
					toggleValue = !toggleValue;
					eventCheck = toggleValue;
				}
			} else {
				if (savedEvent == eventType.ToString ())
					eventCheck = true;
			}
			// so we only process incoming event once
			savedEvent = "";
			return eventCheck;
		}

		void setEvent(string mrntEvent) {
			savedEvent = mrntEvent;
		}
	}
}