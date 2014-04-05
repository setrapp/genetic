using UnityEngine;
using System.Collections;

public class StartUp : MonoBehaviour {
	public string trackFile;
	public string trackDemoFile;
	public string dragFile;
	public string dragDemoFile;
	public Checkbox newPopultion;
	public Checkbox readDemoFile;
	public Checkbox writePopulation;
	public Checkbox runDragStrip;
	private string readFile;
	private string writeFile;
	private bool readFromFile;
	private bool readFromDemoFile;
	private bool writeToFile;
	private bool dragStrip;
	public int trackDurationSec;
	public int dragDurationSec;

	void Start() {
		DontDestroyOnLoad(gameObject);
	}

	public void StartRun() {
		readFile = trackFile;
		writeFile = trackFile;
		readFromFile = !newPopultion.isChecked;
		readFromDemoFile = readDemoFile.isChecked;
		writeToFile = writePopulation.isChecked;
		dragStrip = runDragStrip.isChecked;
		if (readFromFile) {
			if (readFromDemoFile) {
				if (dragStrip) {
					readFile = dragDemoFile;
				} else {
					readFile = trackDemoFile;
				}
			} else {
				if (dragStrip) {
					readFile = dragFile;
				} else {
					readFile = trackFile;
				}
			}
			writeFile = trackFile;
		}
		if (writeToFile) {
			if (dragStrip) {
				writeFile = dragFile;
			} else {
				writeFile = trackFile;
			}
		}

		Application.LoadLevel("racer");
	}

	void OnLevelWasLoaded(int level) {
		if (GameObject.FindGameObjectWithTag("Globals") != null) {
			GenomeGenerator.Instance.readFromFile = readFromFile;
			GenomeGenerator.Instance.writeToFile = writeToFile;
			GenomeGenerator.Instance.writerReader.readFileName = readFile;
			GenomeGenerator.Instance.writerReader.writeFileName = writeFile;

			if (!dragStrip) {
				Timer.Instance.durationSec = trackDurationSec;
				GameObject.Find("DragStrip").SetActive(false);
			} else {
				Timer.Instance.durationSec = dragDurationSec;
				GameObject.Find("Track").SetActive(false);
			}

			Timer.Instance.ResetTimer();
			GenomeGenerator.Instance.Init();
		}
	}
}
