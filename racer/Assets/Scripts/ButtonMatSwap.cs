using UnityEngine;
using System.Collections;

public class ButtonMatSwap : MonoBehaviour {
	public Material upMat = null;
	public Material downMat = null;
	public GameObject target;

	void Start() {
		target.renderer.material = upMat;
	}

	void ButtonDown() {
		target.renderer.material = downMat;
	}

	void ButtonUp() {
		target.renderer.material = upMat;
	}
}
