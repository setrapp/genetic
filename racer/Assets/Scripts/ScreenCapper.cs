using UnityEngine;
using System.Collections;
using System.IO;

public class ScreenCapper : MonoBehaviour {

	public bool captureGeneration;
	public string baseFileName = "Captures/generation";
	public int generationsPerRecord;

	public bool CheckCapture()
	{
		if (captureGeneration && (GenomeGenerator.Instance.currentGeneration + 1) % generationsPerRecord == 0) {
			SendMessage("CaptureScene", SendMessageOptions.DontRequireReceiver);
			return true;
		}
		return false;
	}

	public IEnumerator CaptureScene()
	{
		yield return new WaitForEndOfFrame();
		Texture2D capture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
		capture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		capture.Apply();
		byte[] captureBytes = capture.EncodeToPNG();
		Destroy(capture);
		File.WriteAllBytes(baseFileName + "_" + (GenomeGenerator.Instance.currentGeneration + 1) + ".png", captureBytes);
		SendMessage("PostScreenCapture", SendMessageOptions.DontRequireReceiver);
	}
}
