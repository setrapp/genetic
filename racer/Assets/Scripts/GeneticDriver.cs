using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneticDriver : MonoBehaviour
{
	public Car car;
	public int numStepsPerSec;
	[HideInInspector]
	public int numSteps;
	private int timeStepMilli = 100;
	private int milliSinceStep = 0;
	private System.Random random;
	public float accelerateRate = 0.5f;
	public float decelerateRate = 0.5f;
	public float turnRate = 0.5f;
	public List<GeneticMove> moves;
	public int currentMove = 0;
	private bool firstMove = false;

	void FixedUpdate() {
		milliSinceStep += (int)(Time.deltaTime * 1000);
		if (firstMove) {
			milliSinceStep = 0;
			firstMove = false;
		}

		if (milliSinceStep >= timeStepMilli) {
			milliSinceStep = 0;
			currentMove++;
		}
		ReadCurrentMove();
	}

	public void SeedRandom(int seed) {
		random = new System.Random(seed);
	}

	public void Init(int fullTimeMilli) {
		moves = new List<GeneticMove>();
		if (numStepsPerSec <= 0) {
			numStepsPerSec = GenomeGenerator.Instance.targetFrameRate;
		}
		numSteps = (int)(numStepsPerSec * fullTimeMilli / 1000.0f);
		timeStepMilli = (int)(fullTimeMilli / (float)numSteps);

		ResetDriver();
	}

	public void GenerateAllMoves() {
		moves.Clear();
		for (int i = 0; i < numSteps; i++) {
			moves.Add(new GeneticMove());
			GenerateMove(i);
		}
	}

	public void GenerateLaterMoves(float portionToKeep) {
		int startingStep = (int)(numSteps * portionToKeep);
		for (int i = startingStep; i < numSteps; i++) {
			if (moves.Count < i + 1) {
				moves.Add(new GeneticMove());
			}
			GenerateMove(i);
		}
	}

	public GeneticMove GenerateMove(int moveIndex = -1) {
		GeneticMove newMove = new GeneticMove();
		newMove.accelerate = false;
		newMove.decelerate = false;
		newMove.turnLeft = false;
		newMove.turnRight = false;

		// Determine if car should change speed.
		if ((float)random.NextDouble() < accelerateRate) {
			if ((float)random.NextDouble() < decelerateRate) {
				newMove.decelerate = true;
			} else { 
				newMove.accelerate = true;
			}
		}

		// Determine if car should turn.
		if ((float)random.NextDouble() < turnRate) {
			if ((float)random.NextDouble() < 0.5) {
				newMove.turnLeft = true;
			} else { 
				newMove.turnRight = true;
			}
		}

		// If the index is out of bounds, simply return the move.
		if (moveIndex < 0 || moveIndex > moves.Count - 1) {
			return newMove;
		}

		// Store the new move at the specified index.
		moves.RemoveAt(moveIndex);
		moves.Insert(moveIndex, newMove);

		return newMove;
	}

	private void ReadCurrentMove() {
		if (currentMove < 0 || moves == null || currentMove >= moves.Count) {
			return;
		}

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

	public float SimilarityToMoveList(List<GeneticMove> targetMoves) {
		float similarity = 0;
		for (int i = 0; i < targetMoves.Count && i < moves.Count; i++) {
			if (moves[i].accelerate == targetMoves[i].accelerate && moves[i].decelerate == targetMoves[i].decelerate) {
				similarity += 0.5f;
			}
			if (moves[i].turnLeft == targetMoves[i].turnLeft && moves[i].turnRight == targetMoves[i].turnRight) {
				similarity += 0.5f;
			}
		}
		similarity /= Mathf.Max(targetMoves.Count, moves.Count);
		return similarity;
	}

	public void ResetDriver() {
		currentMove = 0;
		firstMove = true;
	}
}

public class GeneticMove {
	public bool accelerate;
	public bool decelerate;
	public bool turnLeft;
	public bool turnRight;	
}