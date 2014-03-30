using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class WriterReader : MonoBehaviour
{
	public string geneticFileName;

	public bool WritePopulation(List<Genome> population) {
		if (geneticFileName == null || geneticFileName.Length < 1) {
			return false;
		}

		if (File.Exists(geneticFileName)) {
			File.Delete(geneticFileName);
		}

		StreamWriter writer = File.CreateText(geneticFileName);
		writer.WriteLine("generation: " + GenomeGenerator.Instance.currentGeneration);
		for (int i = 0; i < population.Count; i++) {
			writer.WriteLine("id: " + population[i].id);
			writer.WriteLine("fitness: " + population[i].endingFitness);
			writer.WriteLine("topSpeed: " + population[i].car.topSpeed);
			writer.WriteLine("acceleration: " + population[i].car.acceleration);
			writer.WriteLine("handling: " + population[i].car.handling);
			int movesCount = population[i].moves.Count;
			for (int j = 0; j < movesCount; j++) {
				int accelerateInt = population[i].moves[j].accelerate ? 1 : 0;
				int decelerateInt = population[i].moves[j].decelerate ? 1 : 0;
				int turnLeftInt = population[i].moves[j].turnLeft ? 1 : 0;
				int turnRightInt = population[i].moves[j].turnRight ? 1 : 0;
				writer.WriteLine("move" + j + ": " + accelerateInt + " " + decelerateInt + " " + turnLeftInt + " " + turnRightInt);
			}
		}
		writer.Close();
		return true;
	}

	public bool ReadPopulation(List<Genome> population) {
		if (geneticFileName == null || geneticFileName.Length < 1) {
			return false;
		}
		
		if (!File.Exists(geneticFileName)) {
			return false;
		}
		
		StreamReader reader = new StreamReader(geneticFileName);
		int populationIndex = -1;
		char[] seperator = new char[]{' '};
		while (!reader.EndOfStream) {
			string line = reader.ReadLine();
			if (line.IndexOf("generation") >= 0) {
				string generationString = line.Substring(line.IndexOf(" ") + 1);
				int generation = 0;
				bool readGeneration = int.TryParse(generationString, out generation);
				if (readGeneration) {
					GenomeGenerator.Instance.currentGeneration = generation;
				} else {
					Debug.Log("Failed to read generation");
				}
			} else if (line.IndexOf("id") >= 0) {
				string idString = line.Substring(line.IndexOf(" ") + 1);
				float id = 0;
				bool createdNewCar = float.TryParse(idString, out id);
				if (createdNewCar) {
					populationIndex++;
					population[populationIndex].id = id;
					population[populationIndex].moves.Clear();
				} else {
					Debug.Log("Failed to create car " + idString);
				}
			} else if (line.IndexOf("fitness") >= 0) {
				string fitnessString = line.Substring(line.IndexOf(" ") + 1);
				int fitness = 0;
				bool readFitness = int.TryParse(fitnessString, out fitness);
				if (readFitness) {
					population[populationIndex].endingFitness = fitness;
				} else {
					Debug.Log("Failed to read fitness of car " + population[populationIndex].id);
				}
			} else if (line.IndexOf("topSpeed") >= 0) {
				string topSpeedString = line.Substring(line.IndexOf(" ") + 1);
				int topSpeed = 0;
				bool readTopSpeed = int.TryParse(topSpeedString, out topSpeed);
				if (readTopSpeed) {
					population[populationIndex].car.topSpeed = topSpeed;
				} else {
					Debug.Log("Failed to read top speed of car " + population[populationIndex].id);
				}
			} else if (line.IndexOf("acceleration") >= 0) {
				string accelerationString = line.Substring(line.IndexOf(" ") + 1);
				int acceleration = 0;
				bool readAcceleration = int.TryParse(accelerationString, out acceleration);
				if (readAcceleration) {
					population[populationIndex].car.acceleration = acceleration;
				} else {
					Debug.Log("Failed to read acceleration of car " + population[populationIndex].id);
				}
			} else if (line.IndexOf("handling") >= 0) {
				string handlingString = line.Substring(line.IndexOf(" ") + 1);
				int handling = 0;
				bool readHandling = int.TryParse(handlingString, out handling);
				if (readHandling) {
					population[populationIndex].car.handling = handling;
				} else {
					Debug.Log("Failed to read handling of car " + population[populationIndex].id);
				}
			} else if (line.IndexOf("move") >= 0) {
				string[] moveStrings = line.Split(seperator);
				int accelerate, decelerate, turnLeft, turnRight;
				bool ableToReadMove = true;
				ableToReadMove = ableToReadMove && int.TryParse(moveStrings[1], out accelerate);
				ableToReadMove = ableToReadMove && int.TryParse(moveStrings[2], out decelerate);
				ableToReadMove = ableToReadMove && int.TryParse(moveStrings[3], out turnLeft);
				ableToReadMove = ableToReadMove && int.TryParse(moveStrings[4], out turnRight);
				if (ableToReadMove) {
					population[populationIndex].moves.Add(new GeneticMove());
					int moveIndex = population[populationIndex].moves.Count - 1;
					population[populationIndex].moves[moveIndex].accelerate = (accelerate != 0);
					population[populationIndex].moves[moveIndex].decelerate = (decelerate != 0);
					population[populationIndex].moves[moveIndex].turnLeft = (turnLeft != 0);
					population[populationIndex].moves[moveIndex].turnRight = (turnRight != 0);
				} else {
					Debug.Log("Failed to read a move of car " + population[populationIndex].id);
				}
			}
		}

		for (int i = 0; i < population.Count; i++) {
			population[i].car.driver.moves = population[i].moves;
		}

		reader.Close();
		return true;
	}
}