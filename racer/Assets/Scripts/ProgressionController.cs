using UnityEngine;
using System.Collections;

public class ProgressionController : MonoBehaviour
{
	// GUI Display
	public GUIText distanceText;
	public GUIText fitnessText;
	public GUIText lapCountText;

	void Update() {
		Car winningCar = GenomeGenerator.Instance.winningCar;
		if (winningCar) {
			//distanceText.text = "" + winningCar.distance;
			fitnessText.text = "" + (int)winningCar.Fitness;
			//lapCountText.text = "" + winningCar.lapCount;
		}
	}
}

