using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour {
	// Starting State
	private Vector3 startingPos;
	private Quaternion startingRot;
	private Vector3 startingSca;

	// Car Stats
	public int topSpeed;
	public int acceleration;
	public int handling;
	public int statPoolSize;
	public int statMin;
	public int statMax;
	public GameObject topSpeedBar;
	public GameObject accelerationBar;
	public GameObject handlingBar;

	// Stat Conversions
	public float statScale = 1;
	public float topSpeedScale = 1;
	public float accelerationScale = 1;
	public float handlingBrakeScale = 1;
	public float handlingTurnScale = 1;

	// Car Physics
	public Vector3 velocity = new Vector3();
	public float onTrackFriction = 0.99f;
	public float offTrackFriction = 0.95f;

	// Progession
	public float distance = 0.0f;
	public float distanceOnTrack = 0.0f;
	public float durationOnTrack = 0.0f;
	public float lastDurationOnTrack = 0.0f;
	public float durationRacing = 0.0f;
	public float durationAtTopSpeed = 0;
	public float durationAboveHalfSpeed = 0;
	public int lapCount = 0;
	public Vector3 lastTrackPos;

	public GeneticDriver driver;
	public TextMesh numberText;
	public int Fitness {
		get {
			float distanceMetric = distanceOnTrack * (distanceOnTrack - (distance - distanceOnTrack));
			float timeMetric = (durationOnTrack + durationAboveHalfSpeed) / durationRacing;
			float scale = 10;

			return (int)(Mathf.Max(1, distanceMetric * timeMetric * scale));
		}
	}

	void Start() {
		startingPos = transform.position;
		startingRot = transform.rotation;
		startingSca = transform.localScale;

		// Scale stat conversions by a common scale and the applications time scaling.
		topSpeedScale *= statScale * GenomeGenerator.Instance.timeScaling;
		accelerationScale *= statScale * GenomeGenerator.Instance.timeScaling;
		handlingBrakeScale *= statScale * GenomeGenerator.Instance.timeScaling;
		handlingTurnScale *= statScale * GenomeGenerator.Instance.timeScaling;

		ResetCar();
	}

	void FixedUpdate () {
		// Check if on track.
		bool onTrack = false;
		if (IsOnTrack()) {
			velocity *= onTrackFriction;
			onTrack = true;
		} else {
			velocity *= offTrackFriction;
		}

		// Apply velocity
		transform.Translate(velocity * Time.fixedDeltaTime);
		distance += velocity.magnitude * Time.fixedDeltaTime;
		durationRacing += Time.fixedDeltaTime;
		if (onTrack) {
			distanceOnTrack += velocity.magnitude * Time.fixedDeltaTime;
			durationOnTrack += Time.fixedDeltaTime;
			lastDurationOnTrack += Time.fixedDeltaTime;
			lastTrackPos = transform.position;
		} else {
			lastDurationOnTrack = 0.0f;
		}
	}

	public void Accelerate(bool decelerate) {
		// Accelerate
		if (!decelerate) {
			velocity += Vector3.up * (acceleration * accelerationScale * Time.fixedDeltaTime);
			if (velocity.sqrMagnitude > (topSpeed * topSpeed) * (topSpeedScale * topSpeedScale)) {
				velocity = Vector3.up * topSpeed * topSpeedScale;
				durationAtTopSpeed += Time.fixedDeltaTime;
			}
			if (velocity.sqrMagnitude > ((topSpeed * topSpeed) * (topSpeedScale * topSpeedScale)) / 4) {
				durationAboveHalfSpeed += Time.fixedDeltaTime;
			}
		}
		// Decelerate
		else {
			velocity -= Vector3.up * (handling * handlingBrakeScale * Time.fixedDeltaTime);
			if (Vector3.Dot(velocity, Vector3.up) < 0) {
				velocity = Vector3.zero;
			}
		}
	}

	public void Turn(int direction) {
		transform.Rotate(0, 0, handling * handlingTurnScale * direction * Time.fixedDeltaTime);
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

	public void ResetCar() {
		transform.position = startingPos;
		transform.rotation = startingRot;
		transform.localScale = startingSca;
		Vector3 statScale = topSpeedBar.transform.localScale;
		topSpeedBar.transform.localScale = new Vector3(statScale.x, (float)topSpeed, statScale.z);
		accelerationBar.transform.localScale = new Vector3(statScale.x, (float)acceleration, statScale.z);
		handlingBar.transform.localScale = new Vector3(statScale.x, (float)handling, statScale.z);
		distance = 0;
		distanceOnTrack = 0.0f;
		lastDurationOnTrack = 0.0f;
		durationRacing = 0.0f;
		lapCount = 0;
		lastTrackPos = transform.position;
		driver.ResetDriver();

	}
}
