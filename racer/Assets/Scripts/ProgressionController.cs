using UnityEngine;
using System.Collections;

public class ProgressionController : MonoBehaviour
{
	// GUI Display
	public GUIText distanceText;
	public GUIText trackDistText;
	public GUIText lapCountText;

	void Update() {
		Car winningCar = GenomeGenerator.Instance.winningCar;
		distanceText.text = "" + winningCar.distance;
		trackDistText.text = "" + (int)winningCar.timeOnTrack;
		lapCountText.text = "" + winningCar.lapCount;
	}
}

