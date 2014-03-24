using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneticDriver : MonoBehaviour
{
	public Car car;
	public int numSteps;
	private int timeStepMilli = 100;
	private int milliSinceStep = 0;
	private System.Random random;
	public float accelerateRate = 0.5f;
	public float decelerateRate = 0.5f;
	public float turnRate = 0.5f;
	public List<GeneticMove> moves;
	private int currentMove = 0;

	void Update() {
		milliSinceStep += (int)(Time.deltaTime * 1000);
		if (milliSinceStep >= timeStepMilli) {
			milliSinceStep = 0;
			ReadNextMove();
		}
	}

	public void SeedRandom(int seed) {
		random = new System.Random(seed);
	}

	public void Init(int fullTimeMilli) {
		moves = new List<GeneticMove>();
		timeStepMilli = fullTimeMilli / numSteps;
		currentMove = 0;
	}

	public void GenerateAllMoves() {
		for (int i = 0; i < numSteps; i++) {
			moves.Add(new GeneticMove());
			GenerateMove(i);
		}
	}

	public void GenerateMove(int moveIndex) {
		moves[moveIndex].accelerate = false;
		moves[moveIndex].decelerate = false;
		moves[moveIndex].accelerate = false;
		moves[moveIndex].decelerate = false;

		// Determine if car should change speed.
		if ((float)random.NextDouble() < accelerateRate) {
			if ((float)random.NextDouble() < decelerateRate) {
				moves[moveIndex].decelerate = true;
			} else { 
				moves[moveIndex].accelerate = true;
			}
		}

		// Determine if car should turn.
		if ((float)random.NextDouble() < turnRate) {
			if ((float)random.NextDouble() < 0.5) {
				moves[moveIndex].turnLeft = true;
			} else { 
				moves[moveIndex].turnRight = true;
			}
		}
	}

	private void ReadNextMove() {
		// Acceleration
		if (moves[currentMove].accelerate) {
			car.Accelerate(true);
		} else if (moves[currentMove].decelerate) { 
			car.Accelerate(false);
		}

		// Turning
		if (moves[currentMove].turnLeft) {
			car.Turn(1);
		} else if (moves[currentMove].turnRight) { 
			car.Turn(-1);
		}
	}
}

public class GeneticMove {
	public bool accelerate;
	public bool decelerate;
	public bool turnLeft;
	public bool turnRight;	
}