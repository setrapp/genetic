using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour {
	public GameObject target;

	void OnMouseDown() {
		target.SendMessage("ButtonDown", SendMessageOptions.DontRequireReceiver);
	}

	void OnMouseUp() {
		target.SendMessage("ButtonUp", SendMessageOptions.DontRequireReceiver);
	}
}
