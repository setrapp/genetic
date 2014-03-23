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

	// Mutation
	public float mutationRate = 0.1f;
	public float swapStatRate = 0.25f;
	public float shareStatRate = 0.65f;
	public float randomizeStatRate = 0.1f;

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
		population[currentMember].fitness = (int)(car.distance * car.distance * 1000);
		Debug.Log("" + population[currentMember].fitness + ": " + 
		          population[currentMember].topSpeed + " " + 
		          population[currentMember].acceleration + " " + 
		          population[currentMember].handling + " " + 
		          (population[currentMember].topSpeed + population[currentMember].acceleration + population[currentMember].handling));

		// Start next car.
		currentMember++;
		car.ResetCar();
		if (currentMember >= membersInGeneration) {
			Debug.Log("----------");
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

		ReproductionRange[] reproductionRanges = WeightParents();
		for (int i = 0; i < membersInGeneration; i++) {
			int parentIndex1 = PickParent(reproductionRanges);
			int parentIndex2 = PickParent(reproductionRanges, parentIndex1);
			Genome parent1 = population[parentIndex1];
			Genome parent2 = population[parentIndex2];
			newPopulation.Add(CreateChild(parent1, parent2));
		}
		population = newPopulation;
	}

	private ReproductionRange[] WeightParents() {
		int rangeMin = 0;
		ReproductionRange[] reproductionRanges = new ReproductionRange[population.Count];
		for (int i = 0; i < population.Count; i++) {
			reproductionRanges[i] = new ReproductionRange();
			reproductionRanges[i].min = rangeMin;
			reproductionRanges[i].max = rangeMin + population[i].fitness;
			rangeMin = reproductionRanges[i].max;
		}
		return reproductionRanges;
	}

	private int PickParent(ReproductionRange[] reproductionRanges, int ignoreParent = -1) {
		// Returning the same parent, is better than an infinite loop. Why is there only one parent?
		if (ignoreParent == 0 && population.Count == 1) {
			return 0;
		}

		int parentIndex = -1;
		while (parentIndex < 0 || parentIndex == ignoreParent) {
			int reproductionNum = genomeRandom.Next(0, reproductionRanges[reproductionRanges.Length - 1].max);
			//TODO A Binary search might speed this up if the population is large
			for (int i = 0; i < reproductionRanges.Length; i++) {
				if (reproductionNum >= reproductionRanges[i].min && reproductionNum < reproductionRanges[i].max) {
					parentIndex = i;
					break;
				}
			}
		}
		return parentIndex;
	}

	private Genome CreateChild(Genome parent1, Genome parent2) {
		Genome child = new Genome();
		CrossOverStats(parent1, parent2, child);
		if ((float)genomeRandom.NextDouble() < mutationRate) {
			MutateStats(child);
		}
		return child;
	}

	private void CrossOverStats(Genome parent1, Genome parent2, Genome child) {
		// Serialize parent genomes.
		int[] stat1 = new int[]{parent1.topSpeed, parent1.acceleration, parent1.handling};
		int[] stat2 = new int[]{parent2.topSpeed, parent2.acceleration, parent2.handling};
		int[] statChild = new int[3];

		// Determine priority.
		int firstPriority = genomeRandom.Next(0, 3);
		int secondPriority = firstPriority;
		while (secondPriority == firstPriority) {
			secondPriority = genomeRandom.Next(0, 3);
		}
		int thirdPriority = 3 - firstPriority - secondPriority;

		// Take each stat from the parent with more of it.
		statChild[firstPriority] = Mathf.Max(stat1[firstPriority], stat2[firstPriority]);
		statChild[secondPriority] = Mathf.Max(stat1[secondPriority], stat2[secondPriority]);
		statChild[thirdPriority] = Mathf.Max(stat1[thirdPriority], stat2[thirdPriority]);

		// Fix stats to stay within stat pool size. Pres
		int statFix = ((statChild[firstPriority] + statChild[secondPriority] + statChild[thirdPriority]) - car.statPoolSize) / 2;
		statChild[secondPriority] -= statFix;
		statChild[thirdPriority] = car.statPoolSize - (statChild[firstPriority] + statChild[secondPriority]);

		// Apply child genome.
		child.topSpeed = statChild[0];
		child.acceleration = statChild[1];
		child.handling = statChild[2];
	}

	private void MutateStats(Genome child) {
		float mutationType = (float)genomeRandom.NextDouble();
		if (mutationType < swapStatRate) {
			// Swap two stats.
			int[] statChild = new int[]{child.topSpeed, child.acceleration, child.handling};
			int swap1 = statRandom.Next(0, 3);
			int swap2 = swap1;
			while (swap2 == swap1) {
				swap2 = statRandom.Next(0, 3);
			}
			int tempStat = statChild[swap1];
			statChild[swap1] = statChild[swap2];
			statChild[swap2] = tempStat;
			child.topSpeed = statChild[0];
			child.acceleration = statChild[1];
			child.handling = statChild[2];
		} else if (mutationType < swapStatRate + shareStatRate) {
			// Move half of one stat to another.
			int[] statChild = new int[]{child.topSpeed, child.acceleration, child.handling};
			int share1 = statRandom.Next(0, 3);
			int share2 = share1;
			while (share2 == share1) {
				share2 = statRandom.Next(0, 3);
			}
			int half1 = statChild[share1] / 2;
			statChild[share1] -= half1;
			statChild[share2] += half1;
			child.topSpeed = statChild[0];
			child.acceleration = statChild[1];
			child.handling = statChild[2];
		} else if (mutationType < swapStatRate + shareStatRate + randomizeStatRate) {
			// Create a new random set of stats.
			RandomizeStats(out child.topSpeed, out child.acceleration, out child.handling);
		}
	}

	private void RandomizeStats(out int topSpeed, out int acceleration, out int handling) {
		// Generate random stats that does not exceed stat pool.
		topSpeed = car.statMin + statRandom.Next(0, car.statMax - car.statMin);
		acceleration = car.statMin + statRandom.Next(0, car.statMax - car.statMin);
		handling = car.statMin + statRandom.Next(0, car.statMax - car.statMin);//car.statPoolSize - topSpeed - acceleration;
		if (topSpeed + acceleration + handling != car.statPoolSize) {
			int statFix = ((topSpeed + acceleration + car.statMin) - car.statPoolSize) / 3;
			topSpeed -= statFix;
			acceleration -= statFix;
			handling = car.statPoolSize - (topSpeed + acceleration);
		}
	}

	private void ApplyStats() {
		statsController.topSpeed = population[currentMember].topSpeed;
		statsController.acceleration = population[currentMember].acceleration;
		statsController.handling = population[currentMember].handling;
		statsController.ApplyStats();
	}
}

public class Genome {
	public int fitness;
	public int topSpeed;
	public int acceleration;
	public int handling;

	public Genome() {
		this.topSpeed = 0;
		this.acceleration = 0;
		this.handling = 0;
	}
	public Genome(int topSpeed, int acceleration, int handling) {
		this.topSpeed = topSpeed;
		this.acceleration = acceleration;
		this.handling = handling;
	}
}

public class ReproductionRange {
	public int min;
	public int max;
}

