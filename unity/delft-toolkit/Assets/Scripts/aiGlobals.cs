using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aiGlobals : MonoBehaviour {

	public enum StringCompare
	{
		EqualTo,
		Contains,
		StartsWith
	}

	public enum MovementTypes{
		stop, forward, backward, turnRight, turnLeft, ledsOn, ledsOff, servoWiggle, 
		mlImuOff, mlImuRun, mlImuTrain1, mlImuTrain2, mlImuTrainStop, 
		analogOff, analogOn0,
		recognize
	};


	public static string GetCompareString(StringCompare cm) {
		if (cm == aiGlobals.StringCompare.EqualTo) {
			return "EqualTo";
		} 
		if (cm == aiGlobals.StringCompare.Contains) {
			return "Contains";
		}
		if (cm == aiGlobals.StringCompare.StartsWith) {
			return "StartsWith";
		}
		return string.Empty;
	}

	public static bool CompareString(string a, string b, StringCompare cm) {
		if (cm == aiGlobals.StringCompare.EqualTo) {
			return a.ToLower() == b.ToLower();
		} 
		if (cm == aiGlobals.StringCompare.Contains) {
			return a.ToLower().Contains(b.ToLower());
		}
		if (cm == aiGlobals.StringCompare.StartsWith) {
			return a.ToLower().StartsWith(b.ToLower());
		}
		return false;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
