using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dingControlVirtual : dingControlBase {

	public override void Update () {
		//base.Update ();
		printNewMode ("DING-VIRTUAL", action);
		switch (action) {
		case aiGlobals.ActionTypes.stop:
			break;
		case aiGlobals.ActionTypes.forward:
			transform.position += transform.forward * speed * Time.deltaTime;
			break;
		case aiGlobals.ActionTypes.backward:
			transform.position -= transform.forward * speed * Time.deltaTime;
			break;
		case aiGlobals.ActionTypes.turnRight:
			transform.Rotate(Vector3.up, 100f * Time.deltaTime);
			break;
		case aiGlobals.ActionTypes.turnLeft:
			transform.Rotate(Vector3.up, -1 * 100f * Time.deltaTime);
			break;
		case aiGlobals.ActionTypes.ledsOn:
			this.GetComponent<Renderer> ().material.color = new Color(0.236f, 0.0f, 0.5f);
			break;
		case aiGlobals.ActionTypes.ledsOff:
			this.GetComponent<Renderer> ().material.color = new Color(0.0f, 0.0f, 0.0f);
			break;
		default:
			break;
		}
	}
}
