using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aiGlobals : MonoBehaviour {

	public enum StringCompare
	{
		Contains,
		StartsWith,
		EqualTo,
		DoesNotContain
	}

	public enum ActionTypes{
		stop, forward, backward, turnRight, turnLeft, ledsOn, ledsOff, servoWiggle, 
		mlImuOff, mlImuRun, mlImuTrain1, mlImuTrain2, mlImuTrainStop, 
		analogOff, analogOn0,
		recognize, speak, listen
	};

	public enum Devices{
		ding1, ding2, ding3
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
		if (cm == aiGlobals.StringCompare.DoesNotContain) {
			return "DoesNotContain";
		}
		return string.Empty;
	}

	public static bool CompareString(string a, string b, StringCompare cm) {
		if (cm == aiGlobals.StringCompare.EqualTo) {
			return a.ToLower() == b.ToLower();
		} 
		if (cm == aiGlobals.StringCompare.Contains) {
			//return a.ToLower().Contains(b.ToLower());
			bool comparison = false;
			if (b.Contains (",")) {
				// check for multiple strings, any of which must be in target
				string[] theStrings = b.Split (',');
				foreach (string token in theStrings) {
					comparison = a.ToLower ().Contains (token.ToLower ());
					if (comparison)
						break;
				}
				return comparison;
			} else {
				return a.ToLower ().Contains (b.ToLower ());
			}
		}
		if (cm == aiGlobals.StringCompare.StartsWith) {
			return a.ToLower().StartsWith(b.ToLower());
		}
		if (cm == aiGlobals.StringCompare.DoesNotContain) {
			bool comparison = true;
			if (b.Contains (",")) {
				// check for multiple strings, all of which must not be in target
				string[] theStrings = b.Split (',');
				foreach (string token in theStrings) {
					comparison = !(a.ToLower ().Contains (token.ToLower ()));
					if (!comparison)
						break;
				}
				return comparison;
			} else {
				return !a.ToLower ().Contains (b.ToLower ());
			}
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
