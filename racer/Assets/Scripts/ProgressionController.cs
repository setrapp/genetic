using UnityEngine;
using System.Collections;

public class ProgressionController : MonoBehaviour
{
	// GUI Display
	public GUIText distanceText;
	public GUIText lapCountText;

	void Update() {
		Car winningCar = GenomeGenerator.Instance.winningCar;
		distanceText.text = "" + winningCar.distance;
		lapCountText.text = "" + winningCar.lapCount;
	}
}

