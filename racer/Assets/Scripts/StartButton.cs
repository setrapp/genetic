using UnityEngine;
using System.Collections;

public class StartButton : MonoBehaviour {
	void ButtonUp() {
		GameObject.FindGameObjectWithTag("Universals").GetComponent<StartUp>().StartRun();
	}
}
