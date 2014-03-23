﻿using UnityEngine;
using System.Collections;

public class StatsController : MonoBehaviour {
	// Car
	public Car car;
	public int topSpeed;
	public int acceleration;
	public int handling;

	// GUI Display
	public GUIText topSpeedText;
	public GUIText accelerationText;
	public GUIText handlingText;

	public void ApplyStats() {
		topSpeedText.text = "" + topSpeed;
		accelerationText.text = "" + acceleration;
		handlingText.text = "" + handling;
	}
}
