using UnityEngine;
using System.Collections;

public class StatsController : MonoBehaviour {
	// Car
	public Car car;
	public int topSpeed;
	public int acceleration;
	public int handling;

	// GUI Display
	public GUIText topSpeedText;
	public GUIText accelerationText;
	public GUIText handlingText;

	// Random
	private System.Random random;
	public int statSeed;

	void Start() {
		topSpeed = car.topSpeed;
		acceleration = car.acceleration;
		handling = car.handling;
		ApplyStats();

		random = new System.Random(statSeed);
		// TODO Randomize on generation.
		RandomizeStats();
		ApplyStats();
	}

	public void RandomizeStats() {
		// Generate random stats that does not exceed stat pool.
		topSpeed = car.statMin + random.Next(0, car.statMax - car.statMin);
		acceleration = car.statMin + random.Next(0, (car.statMax - car.statMin));
		if (topSpeed + acceleration + car.statMin > car.statPoolSize) {
			int statFix = ((topSpeed + acceleration + car.statMin) - car.statPoolSize) / 2;
			topSpeed -= statFix;
			acceleration -= statFix;
		}
		handling = car.statPoolSize - topSpeed - acceleration;
	}

	public void ApplyStats() {
		car.topSpeed = topSpeed;
		car.acceleration = acceleration;
		car.handling = handling;
		
		topSpeedText.text = "" + topSpeed;
		accelerationText.text = "" + acceleration;
		handlingText.text = "" + handling;
	}
}
