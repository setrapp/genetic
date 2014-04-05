using UnityEngine;
using System.Collections;

public class Checkbox : MonoBehaviour {
	public bool isChecked = false;
	public GameObject checkbox = null;
	public Checkbox dependsOn = null;
	public bool dependsOnTrue = true;
	public Material uncheckedMat = null;
	public Material checkedMat = null;
	public Material uncheckableMat = null;
	private bool wasClickable = true;
	private bool clickable = true;

	void Start() {
		if (!isChecked) {
			checkbox.renderer.material = uncheckedMat;
		} else {
			checkbox.renderer.material = checkedMat;
		}
	}

	void Update() {
		clickable = (dependsOn == null || dependsOn.isChecked == dependsOnTrue);
		if (clickable != wasClickable) {
			SetMaterial();
			wasClickable = clickable;
		}
	}

	void ButtonDown() {
		if (clickable) {
			if (Input.GetMouseButtonDown(0)) {
				isChecked = !isChecked;
				if (!isChecked) {
					checkbox.renderer.material = uncheckedMat;
				} else {
					checkbox.renderer.material = checkedMat;
				}
			}
		}
	}

	void SetMaterial() {
		if (clickable) {
			if (!isChecked) {
				checkbox.renderer.material = uncheckedMat;
			} else {
				checkbox.renderer.material = checkedMat;
			}
		} else {
			checkbox.renderer.material = uncheckableMat;
		}
	}
}
