using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Tasks.Actions;

public class dingControlBase : MonoBehaviour {

	public float speed = 1.0f; 

	protected aiGlobals.Devices device;
	protected aiGlobals.ActionTypes action = aiGlobals.ActionTypes.stop;
	protected int param1;
	protected string param2;

	private aiGlobals.ActionTypes? lastAction = null;

	void OnEnable()
	{
		DingAction.DingEvent += setAction;
	}
		
	void OnDisable()
	{
		DingAction.DingEvent -= setAction;
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	public virtual void Update () {
		//printNewMode ("base", moveMode);
	}

	void setAction(aiGlobals.Devices aDevice, aiGlobals.ActionTypes anAction, int a, string b) {
		device = aDevice;
		action = anAction;
		param1 = a;
		param2 = b;
		handleAction ();
	}

	public virtual void handleAction () {
		// override in child
	}

	public void printNewMode(string receiver, aiGlobals.ActionTypes newAction) {
		if (newAction != lastAction) {
			print (receiver + ": " + newAction.ToString());
			lastAction = newAction;
		}
	}
}
