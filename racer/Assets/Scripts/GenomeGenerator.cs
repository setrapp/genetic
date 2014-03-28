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

	[HideInInspector]
	public int targetFrameRate = 60;
	public float timeScaling = 1;
	public bool done = false;
	[HideInInspector]
	public List<Car> cars;
	public Car winningCar;
	public int winningCarIndex;
	public StatsController statsController;
	private List<Genome> population;

	// Random
	public int genomeSeed;
	private System.Random genomeRandom;
	public int statSeed;
	private System.Random statRandom;
	public int driverSeed;

	// Mutation
	public float statMutationRate = 0.1f;
	public float swapStatRate = 0.25f;
	public float shareStatRate = 0.65f;
	public float randomizeStatRate = 0.1f;
	public float moveMutationRate = 0.1f;
	public float swapMoveRate = 0.2f;
	public float flipMoveRate = 0.2f;
	public float enableMoveRate = 0.4f;
	public float randomizeMoveRate = 0.2f;
	public float copyMoveRate = 0.2f;

	// Generation Constants
	public int generationMax;
	public int currentGeneration;
	public int membersInGeneration;
	public float elitePortion = 0.1f;
	public float statCrossoverRate = 0.7f;
	public float moveCrossoverRate = 0.7f;
	public float moveSecondParentRate = 0.3f;
	public float catastropheRate = 0.1f;
	public float dangerousCloneCount = 4;
	public float dangerousSimilarity = 0.7f;

	void Start() {
		targetFrameRate = (int)(60 * timeScaling);
		Application.targetFrameRate = targetFrameRate;
		done = false;
		genomeRandom = new System.Random(genomeSeed);
		statRandom = new System.Random(statSeed);
		cars.Clear();
		GameObject[] carObjects = GameObject.FindGameObjectsWithTag("Car");
		for (int i = 0; i < carObjects.Length; i++) {
			cars.Add(carObjects[i].GetComponent<Car>());
		}
		membersInGeneration = cars.Count;
		population = new List<Genome>();
		for (int i = 0; i < membersInGeneration; i++) {
			int newTopSpeed, newAcceleration, newHandling;
			RandomizeStats(cars[i]);
			population.Add(new Genome());
			float color = (float)i / (membersInGeneration - 1);
			cars[i].numberText.text = "" + (i + 1);
			cars[i].driver.SeedRandom(driverSeed + i);
			cars[i].driver.Init(Timer.Instance.durationSec * 1000);
			cars[i].driver.GenerateAllMoves();
			population[i].car = cars[i];
			population[i].moves = population[i].car.driver.moves;
		}
		currentGeneration = 0;
		winningCarIndex = 0;
		winningCar = cars[winningCarIndex];
	}

	void Update() {
		winningCarIndex = 0;
		for (int i = 1; i < cars.Count; i++) {
			if (cars[i].Fitness > cars[winningCarIndex].Fitness) {
				winningCarIndex = i;
			}
		}
		winningCar = cars[winningCarIndex];
	}

	public void TimerDone() {
		// Record fitness.
		for (int i = 0; i < membersInGeneration; i++) {
			population[i].endingFitness = cars[i].Fitness;
		}

		CreateNextGeneration();
		for (int i = 0; i < membersInGeneration; i++) {
			cars[i].ResetCar();
			//cars[i].driver.currentMove = 0;
		}
		currentGeneration++;
		if (currentGeneration >= generationMax) {
			done = true;
		}

		if (!done) {
			SendMessage("ResetTimer");
		} else {
			/*for (int i = 0; i < membersInGeneration; i++) {
				cars[i].gameObject.SetActive(false);
			}*/
		}
	}

	private void CreateNextGeneration() {
		List<Genome> newPopulation = new List<Genome>();
		float avgFitness = 0;
		// Copy parent drivers to avoid overwriting them.
		for (int i = 0; i < membersInGeneration; i++) {
			List<GeneticMove> newMoves = new List<GeneticMove>();
			for (int j = 0; j < population[i].moves.Count; j++) {
				newMoves.Add(population[i].moves[j]);
			}
			population[i].moves = newMoves;
			avgFitness += population[i].car.Fitness;
		}
		avgFitness /= membersInGeneration;

		// Occasionally, eliminate poor performing cars and introduce random new ones.
		/*if ((float)genomeRandom.NextDouble() < catastropheRate) {
			float minFitness = avgFitness / 4;
			for (int i = 0; i < population.Count; i++) {
				if (population[i].car.distanceOnTrack < minFitness) {
					RandomizeStats(population[i].car);
					population[i].car.driver.GenerateAllMoves();
				}
			}
		}*/

		// Store a list of elite cars.
		List<Genome> elites = new List<Genome>();
		int eliteCount = (int)(population.Count * elitePortion);
		for (int i = 0; i < population.Count; i++) {
			bool addToElites = false;
			if (elites.Count < eliteCount || population[i].car.Fitness > elites[elites.Count - 1].car.Fitness) {
				addToElites = true;
			} else {
				population[i].elite =false;
			}

			if (addToElites) {
				bool elitePlaced = false;
				for (int j = 0; j < elites.Count && !elitePlaced; j++) {
					if (population[i].car.Fitness > elites[j].car.Fitness) {
						elites.Insert(j, population[i]);
						population[i].elite = true;
						elitePlaced = true;
					}
				}
				if (!elitePlaced) {
					elites.Add(population[i]);
					population[i].elite = true;
				}
				if (elites.Count > eliteCount) {
					elites[elites.Count - 1].elite = false;
					elites.RemoveAt(elites.Count - 1);
				}
			}
		}

		// Attempt to introduce diversity by altering cars that are too similar.
		/*int statCloneCount = 0, moveCloneCount = 0;
		for (int i = 0; i < membersInGeneration; i++) {
			for(int j = i + 1; j < membersInGeneration; j++) {
				if (!population[j].elite) {
					// Stat Similarity
					float statSimilarity = SimilarityToStats(population[i], population[j]);
					if (statSimilarity > dangerousSimilarity) {
						if (statCloneCount >= dangerousCloneCount) {
							RandomizeStats(population[i].car);
						} else {
							statCloneCount++;
						}
					}

					// Move Similarity
					float moveSimilarity = population[j].car.driver.SimilarityToMoveList(population[i].car.driver.moves);
					if (moveSimilarity > dangerousSimilarity) {
						if (moveCloneCount >= dangerousCloneCount) {
							population[i].car.driver.GenerateAllMoves();
							//float saveMovePortion = (float)genomeRandom.NextDouble();
							//saveMovePortion *= (population[j].car.timeOnTrack / population[j].car.timeRacing);
							//population[i].car.driver.GenerateLaterMoves(saveMovePortion);
						} else {
							moveCloneCount++;
						}
					}
				}
			}
		}*/

		// Save elites.
		for (int i = 0; i < elites.Count; i++) {
			newPopulation.Add(CreateChild(elites[i], elites[i], i, false, false, true));
		}

		// Generate next generation.
		ReproductionRange[] reproductionRanges = WeightParents();
		for (int i = newPopulation.Count; i < membersInGeneration; i += 2) {
			int parentIndex1 = PickParent(reproductionRanges);
			int parentIndex2 = PickParent(reproductionRanges, parentIndex1);
			Genome parent1 = population[parentIndex1];
			Genome parent2 = population[parentIndex2];
			bool crossoverStat = (float)genomeRandom.NextDouble() < statCrossoverRate;
			bool crossoverMove = (float)genomeRandom.NextDouble() < moveCrossoverRate;
			newPopulation.Add(CreateChild(parent1, parent2, i, crossoverStat, crossoverMove));
			if (newPopulation.Count < membersInGeneration) {
				newPopulation.Add(CreateChild(parent2, parent1, i + 1, crossoverStat, crossoverMove));
			}
		}
		population = newPopulation;
	}

	private ReproductionRange[] WeightParents() {
		int rangeMin = 0;
		ReproductionRange[] reproductionRanges = new ReproductionRange[population.Count];
		for (int i = 0; i < population.Count; i++) {
			reproductionRanges[i] = new ReproductionRange();
			reproductionRanges[i].min = rangeMin;
			reproductionRanges[i].max = rangeMin + population[i].endingFitness;
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

	private Genome CreateChild(Genome parent1, Genome parent2, int memberIndex, bool crossoverStat, bool crossoverMove, bool isElite = false) {
		Genome child = new Genome();
		child.car = cars[memberIndex];
		child.moves = child.car.driver.moves;

		if (isElite) {
			child.elite = true;
			return child;
		}

		// Stats
		CrossOverStats(parent1, parent2, child, crossoverStat);
		if ((float)genomeRandom.NextDouble() < statMutationRate) {
			MutateStats(child);
		}
		// Keep stats at least at minimum
		if (child.car.topSpeed < child.car.statMin) {
			int neededPoints = child.car.statMin - child.car.topSpeed;
			child.car.topSpeed += neededPoints;
			if (child.car.acceleration > child.car.handling) {
				child.car.acceleration -= neededPoints;
			} else {
				child.car.handling -= neededPoints;
			}
		}
		if (child.car.acceleration < child.car.statMin) {
			int neededPoints = child.car.statMin - child.car.acceleration;
			child.car.acceleration += neededPoints;
			if (child.car.topSpeed > child.car.handling) {
				child.car.topSpeed -= neededPoints;
			} else {
				child.car.handling -= neededPoints;
			}
		}
		if (child.car.handling < child.car.statMin) {
			int neededPoints = child.car.statMin - child.car.handling;
			child.car.handling += neededPoints;
			if (child.car.topSpeed > child.car.acceleration) {
				child.car.topSpeed -= neededPoints;
			} else {
				child.car.acceleration -= neededPoints;
			}
		}


		// Driver
		CrossOverDriver(parent1, parent2, child, crossoverMove);
		if ((float)genomeRandom.NextDouble() < moveMutationRate) {
			MutateDriver(child);
		}
		//child.moves = child.car.driver.moves;
		return child;
	}

	private void CrossOverStats(Genome parent1, Genome parent2, Genome child, bool crossoverStat) {
		// Serialize parent genomes.
		int[] stat1 = new int[]{parent1.car.topSpeed, parent1.car.acceleration, parent1.car.handling};
		int[] stat2 = new int[]{parent2.car.topSpeed, parent2.car.acceleration, parent2.car.handling};
		int[] statChild = new int[3];

		// Determine priority.
		int firstPriority = genomeRandom.Next(0, 3);
		int secondPriority = firstPriority;
		while (secondPriority == firstPriority) {
			secondPriority = genomeRandom.Next(0, 3);
		}
		int thirdPriority = 3 - firstPriority - secondPriority;

		if (!crossoverStat) {
			statChild[firstPriority] = stat1[firstPriority];
			statChild[secondPriority] = stat1[secondPriority];
			statChild[thirdPriority] = stat1[thirdPriority];
		} else {
			// If crossing over, take each stat from the parent with more of it.
			statChild[firstPriority] = Mathf.Max(stat1[firstPriority], stat2[firstPriority]);
			statChild[secondPriority] = Mathf.Max(stat1[secondPriority], stat2[secondPriority]);
			statChild[thirdPriority] = Mathf.Max(stat1[thirdPriority], stat2[thirdPriority]);
		}

		// Fix stats to stay within stat pool size. Preserve the most important stat.
		int statPoolMax = (parent1.car.statPoolSize + parent2.car.statPoolSize) / 2;
		int statFix = ((statChild[firstPriority] + statChild[secondPriority] + statChild[thirdPriority]) - statPoolMax) / 2;
		statChild[secondPriority] -= statFix;
		statChild[thirdPriority] = statPoolMax - (statChild[firstPriority] + statChild[secondPriority]);

		// Apply child genome.
		child.car.topSpeed = statChild[0];
		child.car.acceleration = statChild[1];
		child.car.handling = statChild[2];
	}

	private void CrossOverDriver(Genome parent1, Genome parent2, Genome child, bool crossoverMove) {
		child.car.driver.Init(Timer.Instance.durationSec * 1000);
		int numSteps = child.car.driver.numSteps;
		bool useParent2 = false;
		for (int i = 0; i < numSteps; i++) {
			GeneticMove parentMove = parent1.moves[i];
			if (useParent2 && crossoverMove) {
				parentMove = parent2.moves[i];
			}
			useParent2 = genomeRandom.NextDouble() < moveSecondParentRate;
			child.car.driver.moves.Add(parentMove);
		}
		child.moves = child.car.driver.moves;
	}

	private void MutateStats(Genome child) {
		// Chain rates together in ranges.
		float internalSwapStatRate = swapStatRate;
		float internalShareStatRate = shareStatRate + swapStatRate;
		float internalRandomizeStatRate = randomizeStatRate + shareStatRate;

		float mutationType = (float)genomeRandom.NextDouble();
		if (mutationType < internalSwapStatRate) {
			// Swap two stats.
			int[] statChild = new int[]{child.car.topSpeed, child.car.acceleration, child.car.handling};
			int swap1 = statRandom.Next(0, 3);
			int swap2 = swap1;
			while (swap2 == swap1) {
				swap2 = statRandom.Next(0, 3);
			}
			int tempStat = statChild[swap1];
			statChild[swap1] = statChild[swap2];
			statChild[swap2] = tempStat;
			child.car.topSpeed = statChild[0];
			child.car.acceleration = statChild[1];
			child.car.handling = statChild[2];
		} else if (mutationType < internalShareStatRate) {
			// Move half of one stat to another.
			int[] statChild = new int[]{child.car.topSpeed, child.car.acceleration, child.car.handling};
			int share1 = statRandom.Next(0, 3);
			int share2 = share1;
			while (share2 == share1) {
				share2 = statRandom.Next(0, 3);
			}
			int half1 = statChild[share1] / 2;
			statChild[share1] -= half1;
			statChild[share2] += half1;
			child.car.topSpeed = statChild[0];
			child.car.acceleration = statChild[1];
			child.car.handling = statChild[2];
		} else if (mutationType < internalRandomizeStatRate) {
			// Create a new random set of stats.
			RandomizeStats(child.car);
		}
	}

	private void MutateDriver(Genome child) {
		// Chain rates together in ranges.
		float internalSwapMoveRate = swapMoveRate;
		float internalFlipMoveRate = flipMoveRate + swapMoveRate;
		float internalEnableMoveRate = enableMoveRate + flipMoveRate;
		float internalRandomizeMoveRate = randomizeMoveRate + enableMoveRate;
		float internalCopyMoveRate = copyMoveRate + randomizeMoveRate;

		//Determine how many moves to alter and where to start.
		/*int changeMoveCount = genomeRandom.Next(0, child.car.driver.moves.Count);
		int changeMoveStart = genomeRandom.Next(0, child.car.driver.moves.Count);
		if (changeMoveStart + changeMoveCount >= child.car.driver.moves.Count - 1) {
			changeMoveStart = child.car.driver.moves.Count - changeMoveCount;
		}
		int changeMoveEnd = changeMoveStart + changeMoveCount;*/
		int moveCount = child.car.driver.moves.Count;
		int changeMoveStart, changeMoveCount, changeMoveEnd;

		float mutationType = (float)genomeRandom.NextDouble();
		if (mutationType < internalSwapMoveRate) {
			// Swap a group of moves with another group.
			CalculateMoveCrossoverRange (moveCount, out changeMoveStart, out changeMoveCount, out changeMoveEnd);
			int changeMoveHalfCount = changeMoveCount / 2;
			int changeMoveHalfEnd = changeMoveStart + changeMoveHalfCount;
			for (int i = 0; i < changeMoveHalfCount; i++) {
				GeneticMove tempMove = child.car.driver.moves[i];
				child.car.driver.moves[changeMoveStart + i] = child.car.driver.moves[changeMoveHalfEnd + i];
				child.car.driver.moves[changeMoveHalfEnd + i] = tempMove;
			}
		} 

		mutationType = (float)genomeRandom.NextDouble();
		if (mutationType < internalFlipMoveRate) {
			// Switch a type of action in a group of moves to the opposite direction.
			CalculateMoveCrossoverRange (moveCount, out changeMoveStart, out changeMoveCount, out changeMoveEnd);
			float actionType = (float)genomeRandom.NextDouble();
			for (int i = changeMoveStart; i < changeMoveEnd; i++) {
				if (actionType < 0.5f) {
					if (child.car.driver.moves[i].turnLeft) {
						child.car.driver.moves[i].turnLeft = false;
						child.car.driver.moves[i].turnRight = true;
					} else if (child.car.driver.moves[i].turnRight) {
						child.car.driver.moves[i].turnRight = false;
						child.car.driver.moves[i].turnLeft = true;
					}
				} else { 
					if (child.car.driver.moves[i].accelerate) {
						child.car.driver.moves[i].decelerate = false;
						child.car.driver.moves[i].accelerate = true;
					} else if (child.car.driver.moves[i].decelerate) {
						child.car.driver.moves[i].decelerate = false;
						child.car.driver.moves[i].accelerate = true;
					}
				}
			}
		} 

		mutationType = (float)genomeRandom.NextDouble();
		if (mutationType < internalEnableMoveRate) {
			// Turn one type of action in a groups of moves on or off.
			CalculateMoveCrossoverRange (moveCount, out changeMoveStart, out changeMoveCount, out changeMoveEnd);
			float actionType = (float)genomeRandom.NextDouble();
			float actionDirection = (float)genomeRandom.NextDouble();
			for (int i = changeMoveStart; i < changeMoveEnd; i++) {
				if (actionType < 0.2f) {
					if (child.car.driver.moves[i].turnLeft || child.car.driver.moves[i].turnRight) {
						child.car.driver.moves[i].turnLeft = false;
						child.car.driver.moves[i].turnRight = false;
					} else {
						if (actionDirection < 0.5f) {
							child.car.driver.moves[i].turnRight = true;
						} else {
							child.car.driver.moves[i].turnLeft = true;
						}
					}
				} else { 
					if (child.car.driver.moves[i].accelerate || child.car.driver.moves[i].decelerate) {
						child.car.driver.moves[i].decelerate = false;
						child.car.driver.moves[i].accelerate = false;
					} else {
						if (actionDirection < 0.5f) {
							child.car.driver.moves[i].decelerate = true;
						} else {
							child.car.driver.moves[i].accelerate = true;
						}
					}
				}
			}

		}

		mutationType = (float)genomeRandom.NextDouble();
		if (mutationType < internalRandomizeMoveRate) {
			// Create chunks of moves randomly.
			CalculateMoveCrossoverRange (moveCount, out changeMoveStart, out changeMoveCount, out changeMoveEnd);
			for (int i = changeMoveStart; i < changeMoveEnd; i++) {
				child.car.driver.GenerateMove(i);
			}
		}

		mutationType = (float)genomeRandom.NextDouble();
		if (mutationType < internalCopyMoveRate) {
			// Copy moves across group sections.
			CalculateMoveCrossoverRange (moveCount, out changeMoveStart, out changeMoveCount, out changeMoveEnd);
			int numSections = Mathf.Max(1, genomeRandom.Next(changeMoveCount / 8, changeMoveCount / 4));
			int sectionSize = changeMoveCount / numSections;
			int mod = 0;
			GeneticMove copiedMove = child.car.driver.moves[changeMoveStart];
			for (int i = changeMoveStart; i < changeMoveEnd; i++) {
				if (mod == 0) {
					copiedMove = child.car.driver.moves[i];
				} else {
					child.car.driver.moves[i].accelerate = copiedMove.accelerate;
					child.car.driver.moves[i].decelerate = copiedMove.accelerate;
					child.car.driver.moves[i].turnLeft = copiedMove.turnLeft;
					child.car.driver.moves[i].turnRight = copiedMove.turnRight;
				}
				mod++;
				if (mod >= numSections) {
					mod = 0;
				}
			}
		}
	}

	private void CalculateMoveCrossoverRange(int moveCount, out int changeMoveStart, out int changeMoveCount, out int changeMoveEnd) {
		changeMoveCount = genomeRandom.Next(0, moveCount);
		changeMoveStart = genomeRandom.Next(0, moveCount);
		if (changeMoveStart + changeMoveCount >= moveCount - 1) {
			changeMoveStart = moveCount - changeMoveCount;
		}
		changeMoveEnd = changeMoveStart + changeMoveCount;
	}

	private float SimilarityToStats(Genome target, Genome test) {
		float topSpeedSimilarity = 1.0f - Mathf.Abs((target.car.topSpeed - test.car.topSpeed) / (float)target.car.topSpeed);
		float accelerationSimilarity = 1.0f - Mathf.Abs((target.car.acceleration - test.car.acceleration) / (float)target.car.acceleration);
		float handlingSimilarity = 1.0f - Mathf.Abs((target.car.handling - test.car.handling) / (float)target.car.handling);
		return (topSpeedSimilarity / 3) + (accelerationSimilarity / 3) + (handlingSimilarity / 3);
	}
	
	private void RandomizeStats(Car car) {
		// Generate random stats that does not exceed stat pool.
		car.topSpeed = car.statMin + statRandom.Next(0, car.statMax - car.statMin);
		car.acceleration = car.statMin + statRandom.Next(0, car.statMax - car.statMin);
		car.handling = car.statMin + statRandom.Next(0, car.statMax - car.statMin);
		if (car.topSpeed + car.acceleration + car.handling != car.statPoolSize) {
			int statFix = ((car.topSpeed + car.acceleration + car.statMin) - car.statPoolSize) / 3;
			car.topSpeed -= statFix;
			car.acceleration -= statFix;
			car.handling = car.statPoolSize - (car.topSpeed + car.acceleration);
		}
	}
}

public class Genome {
	public Car car;
	public List<GeneticMove> moves;
	public int endingFitness;
	public bool elite;
}

public class ReproductionRange {
	public int min;
	public int max;
}

