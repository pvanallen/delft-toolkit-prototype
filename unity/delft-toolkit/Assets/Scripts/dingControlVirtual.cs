using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dingControlVirtual : dingControlBase {

	public override void Update () {
		//base.Update ();
		printNewMode ("DING-VIRTUAL", moveMode);
		switch (moveMode) {
		case "stop":
			break;
		case "forward":
			transform.position += transform.forward * speed * Time.deltaTime;
			break;
		case "backward":
			transform.position -= transform.forward * speed * Time.deltaTime;
			break;
		case "turnRight":
			transform.Rotate(Vector3.up, 100f * Time.deltaTime);
			break;
		case "turnLeft":
			transform.Rotate(Vector3.up, -1 * 100f * Time.deltaTime);
			break;
		case "ledsOn":
			this.GetComponent<Renderer> ().material.color = new Color(0.236f, 0.0f, 0.5f);
			break;
		case "ledsOff":
			this.GetComponent<Renderer> ().material.color = new Color(0.0f, 0.0f, 0.0f);
			break;
		case "":
			break;
		default:
			break;
		}
	}
}
