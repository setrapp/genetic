using UnityEngine;
using System.Collections;

public class TrackInfoController : MonoBehaviour
{
	public GUIText timeText;
	public GUIText generationText;
	public GUIText memberText;

	void Update() {
		timeText.text = Timer.Instance.timeString;
	}
}

