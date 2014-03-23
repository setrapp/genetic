using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour
{
	private static Timer instance = null;
	public static Timer Instance {
		get {
			if (instance == null) {
				instance = GameObject.FindGameObjectWithTag("Globals").GetComponent<Timer>();
			}
			return instance;
		}
	}

	public int durationSec;
	private float timeMillisec;
	public string timeString;
	public bool done;

	void Start() {
		ResetTimer();
	}

	void Update() {
		if (!done) {
			timeMillisec -= Time.deltaTime * 1000;
			if (timeMillisec <= 0) {
				timeMillisec = 0;
				done = true;
				SendMessage("TimerDone");
			}
			TimeToString();
		}
	}

	private void ResetTimer() {
		timeMillisec = durationSec * 1000.0f;
		done = false;
		TimeToString();
	}

	private void TimeToString() {
		int milliSec = (int)timeMillisec % 1000;
		int sec = ((int)timeMillisec / 1000) % 60;
		int min = (int)timeMillisec / 60000;

		string milliSecString = "" + milliSec;
		if (milliSec < 10) {
			milliSecString = "00" + milliSec;
		} else if (milliSec < 100) {
			milliSecString = "0" + milliSec;
		}
		string secString = "" + sec;
		if (sec < 10) {
			secString = "0" + sec;
		}
		string minString = "" + min;

		timeString = /*minString + ":" +*/ secString + ":" + milliSecString;
	}
}

