using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dingControlBase : MonoBehaviour {

	public float speed = 1.0f; 

	public string moveMode = "stop";

	private string lastMode = "";

	void OnEnable()
	{
		simpleDispatcher.Move += setMove;
	}
		
	void OnDisable()
	{
		simpleDispatcher.Move -= setMove;
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	public virtual void Update () {
		//printNewMode ("base", moveMode);
	}

	void setMove(string moveType) {
		moveMode = moveType;
		handleCommand (moveMode);
	}

	public virtual void handleCommand (string command) {
		// override in child
	}

	public void printNewMode(string receiver, string mode) {
		if (mode != lastMode && mode != "") {
			print (receiver + ": " + mode);
			lastMode = mode;
		}
	}
}
