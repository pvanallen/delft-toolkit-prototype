using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Tasks.Actions;

public class simpleDispatcher : MonoBehaviour {

	public delegate void MoveEvent(string moveType);
	public static event MoveEvent Move;

	public string moveMode = "stop";

	void OnEnable()
	{
		DingAction.BtMove += setMove;
	}
	void OnDisable()
	{
		DingAction.BtMove -= setMove;
	}
		
	void Start () {
		
	}

	void Update () {

	}

	void setMove(string moveType) {
		moveMode = moveType;
		print ("DISPATCHING Behavior Tree event: " + moveType);
		if (Move != null && moveMode != "") {
			Move (moveMode);
		}
		moveMode = "";
	}
}
