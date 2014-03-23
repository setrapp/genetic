using UnityEngine;
using System.Collections;

public class ProgressionController : MonoBehaviour
{
	// Car
	public Car car;

	// GUI Display
	public GUIText distanceText;
	public GUIText lapCountText;

	void Update() {
		distanceText.text = "" + car.distance;
		lapCountText.text = "" + car.lapCount;
	}
}

