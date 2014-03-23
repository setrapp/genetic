using UnityEngine;
using System.Collections;

public class TrackInfoController : MonoBehaviour
{
	// GUI Display
	public GUIText timeText;
	public GUIText generationText;
	public GUIText memberText;

	void Update() {
		timeText.text = Timer.Instance.timeString;
		generationText.text = "" + (GenomeGenerator.Instance.currentGeneration + 1);
		memberText.text = "" + (GenomeGenerator.Instance.currentMember + 1) + " / " + GenomeGenerator.Instance.membersInGeneration;
	}
}

