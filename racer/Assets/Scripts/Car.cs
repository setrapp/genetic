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
	public float timeOnTrack = 0.0f;
	public float timeRacing = 0.0f;
	public float timeAtTopSpeed = 0;
	public int lapCount = 0;
	public Vector3 lastTrackPos;

	public GeneticDriver driver;
	public TextMesh numberText;
	public int Fitness {
		get {
			//float onTrackProportion = (distanceOnTrack / distance);
			//return Mathf.Max(1, (int)(distanceOnTrack * (distanceOnTrack - (distance - distanceOnTrack)) * 10));
			//(distance * distanceOnTrack * timeOnTrack);
			float distanceMetric = distanceOnTrack * distance;
			float timeMetric = ((timeOnTrack + timeAtTopSpeed) / timeRacing);
			float scale = 1;

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
	}

	void Update () {
		// Check if on track.
		bool onTrack = false;
		if (IsOnTrack()) {
			velocity *= onTrackFriction;
			onTrack = true;
		} else {
			velocity *= offTrackFriction;
		}

		// Apply velocity
		transform.Translate(velocity * Time.deltaTime);
		distance += velocity.magnitude * Time.deltaTime;
		timeRacing += Time.deltaTime;
		if (onTrack) {
			distanceOnTrack += velocity.magnitude * Time.deltaTime;
			timeOnTrack += Time.deltaTime;
			lastTrackPos = transform.position;
		}
	}

	public void Accelerate(bool decelerate) {
		// Accelerate
		if (!decelerate) {
			velocity += Vector3.up * (acceleration * accelerationScale * Time.deltaTime);
			if (velocity.sqrMagnitude > (topSpeed * topSpeed) * (topSpeedScale * topSpeedScale)) {
				velocity = Vector3.up * topSpeed * topSpeedScale;
				timeAtTopSpeed += Time.deltaTime;
			}
		}
		// Decelerate
		else {
			velocity -= Vector3.up * (handling * handlingBrakeScale * Time.deltaTime);
			if (Vector3.Dot(velocity, Vector3.up) < 0) {
				velocity = Vector3.zero;
			}
		}
	}

	public void Turn(int direction) {
		transform.Rotate(0, 0, handling * handlingTurnScale * direction * Time.deltaTime);
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
		distance = 0;
		distanceOnTrack = 0.0f;
		timeOnTrack = 0.0f;
		timeRacing = 0.0f;
		lapCount = 0;
		lastTrackPos = transform.position;
		driver.currentMove = 0;
	}
}
