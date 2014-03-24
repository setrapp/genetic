using UnityEngine;
using System.Collections;

public class TrackInfoController : MonoBehaviour
{
	// GUI Display
	public GUIText timeText;
	public GUIText generationText;
	public GUIText winnerText;

	void Update() {
		timeText.text = Timer.Instance.timeString;
		generationText.text = "" + (GenomeGenerator.Instance.currentGeneration + 1);
		winnerText.text = "" + (GenomeGenerator.Instance.winningCarIndex + 1);
	}
}

