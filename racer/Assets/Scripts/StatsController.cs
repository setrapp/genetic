using UnityEngine;
using System.Collections;

public class StatsController : MonoBehaviour {
	// GUI Display
	public GUIText topSpeedText;
	public GUIText accelerationText;
	public GUIText handlingText;

	void Update() {
		Car winningCar = GenomeGenerator.Instance.winningCar;
		topSpeedText.text = "" + winningCar.topSpeed;
		accelerationText.text = "" + winningCar.acceleration;
		handlingText.text = "" + winningCar.handling;
	}
}
