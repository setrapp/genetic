using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenomeGenerator : MonoBehaviour
{
	private static GenomeGenerator instance = null;
	public static GenomeGenerator Instance {
		get {
			if (instance == null) {
				instance = GameObject.FindGameObjectWithTag("Globals").GetComponent<GenomeGenerator>();
			}
			return instance;
		}
	}

	public bool done = false;
	public Car car;
	public StatsController statsController;
	private List<Genome> population;

	// Random
	public int genomeSeed;
	private System.Random genomeRandom;
	public int statSeed;
	private System.Random statRandom;

	// Generation Constants
	public int generationMax;
	public int currentGeneration;
	public int membersInGeneration;
	public int currentMember;

	void Start() {
		done = false;
		genomeRandom = new System.Random(genomeSeed);
		statRandom = new System.Random(statSeed);
		population = new List<Genome>();
		for (int i = 0; i < membersInGeneration; i++) {
			int newTopSpeed, newAcceleration, newHandling;
			RandomizeStats(out newTopSpeed, out newAcceleration, out newHandling);
			population.Add(new Genome(newTopSpeed, newAcceleration, newHandling));
		}
		currentGeneration = 0;
		currentMember = 0;
		ApplyStats();
	}

	public void TimerDone() {
		// Record fitness.
		population[currentMember].fitness = car.distance;

		// Start next car.
		currentMember++;
		car.ResetCar();
		if (currentMember >= membersInGeneration) {
			CreateNextGeneration();
			currentGeneration++;
			currentMember = 0;
			if (currentGeneration >= generationMax) {
				done = true;
			}
		}

		if (!done) {
			ApplyStats();
			SendMessage("ResetTimer");
		} else {
			car.gameObject.SetActive(false);
		}
	}

	private void CreateNextGeneration() {
		List<Genome> newPopulation = new List<Genome>();
		for (int i = 0; i < membersInGeneration; i++) {
			/* TODO Actually us GA */
			int newTopSpeed, newAcceleration, newHandling;
			RandomizeStats(out newTopSpeed, out newAcceleration, out newHandling);
			newPopulation.Add(new Genome(newTopSpeed, newAcceleration, newHandling));
		}
		population = newPopulation;
	}

	private Genome PickParent(int ignoreParent = -1) {
		/* TODO Grab parent randomly based on fitness, ignoring one if needed*/
		return null;
	}

	private void RandomizeStats(out int topSpeed, out int acceleration, out int handling) {
		// Generate random stats that does not exceed stat pool.
		topSpeed = car.statMin + statRandom.Next(0, car.statMax - car.statMin);
		acceleration = car.statMin + statRandom.Next(0, (car.statMax - car.statMin));
		if (topSpeed + acceleration + car.statMin > car.statPoolSize) {
			int statFix = ((topSpeed + acceleration + car.statMin) - car.statPoolSize) / 2;
			topSpeed -= statFix;
			acceleration -= statFix;
		}
		handling = car.statPoolSize - topSpeed - acceleration;
	}

	private void ApplyStats() {
		statsController.topSpeed = population[currentMember].topSpeed;
		statsController.acceleration = population[currentMember].acceleration;
		statsController.handling = population[currentMember].handling;
		statsController.ApplyStats ();
	}
}

public class Genome {
	public float fitness;
	public int topSpeed;
	public int acceleration;
	public int handling;
	
	public Genome(int topSpeed, int acceleration, int handling) {
		this.topSpeed = topSpeed;
		this.acceleration = acceleration;
		this.handling = handling;
	}
}

