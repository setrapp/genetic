using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour {
	// Car Stats
	public int topSpeed;
	public int acceleration;
	public int handling;
	public int statPoolSize;
	public int statMin;
	public int statMax;

	// Stat Conversions
	public float topSpeedScale = 1;
	public float accelerationScale = 1;
	public float handlingBrakeScale = 1;
	public float handlingTurnScale = 1;

	// Car Physics
	public Vector3 velocity = new Vector3();
	public float onTrackFriction = 0.99f;
	public float offTrackFriction = 0.95f;


	void Update () {
		// Accelerate
		if (Input.GetAxis("Vertical") > 0) {
			velocity += Vector3.up * (acceleration * accelerationScale * Time.deltaTime);
			if (velocity.sqrMagnitude > (topSpeed * topSpeed) * (topSpeedScale * topSpeedScale)) {
				velocity = Vector3.up * topSpeed * topSpeedScale;
			}
		}
		// Deccelerate
		else if (Input.GetAxis("Vertical") < 0) {
			velocity -= Vector3.up * (handling * handlingBrakeScale * Time.deltaTime);
			if (Vector3.Dot(velocity, Vector3.up) < 0) {
				velocity = Vector3.zero;
			}
		}

		// Friction
		if (IsOnTrack()) {
			velocity *= onTrackFriction;
		} else {
			velocity *= offTrackFriction;
		}

		// Turn
		if (Input.GetAxis("Horizontal") != 0) {
			transform.Rotate(0, 0, handling * handlingTurnScale * -Input.GetAxis("Horizontal") * Time.deltaTime);
		}

		// Apply velocity
		transform.Translate(velocity * Time.deltaTime);
	}

	private bool IsOnTrack() {
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity)) {
			if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Slow Terrain")) {
				return true;
			}
		}
		return false;
	}
}
