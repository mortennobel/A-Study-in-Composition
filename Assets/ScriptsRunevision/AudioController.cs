using UnityEngine;
using System.Collections;
using Runevision.Structures;

public class AudioController : MonoBehaviour {

	[System.Serializable]
	public struct AudioLayer {
		public AudioSource source;

		[Range (0, 1)]
		public float targetVolume;

		[Space (6)]

		[Range (0.01f, 1)]
		public float volumeMultiplier;

		[Range (1, 30)]
		public float fullChangeDuration;
	}

	public AudioLayer[] layers;
	public float eventFrequency = 5;

	float nextEvent = 0;
	int lastChangedLayer = 0;
	Rand rand = new Rand ();
	
	// Update is called once per frame
	void Update () {
		if (Time.time > nextEvent)
			DoEvent ();

		for (int i = 0; i < layers.Length; i++) {
			AudioLayer layer = layers[i];
			layer.source.volume = Mathf.MoveTowards (
				layer.source.volume / layer.volumeMultiplier,
				layer.targetVolume,
				Time.deltaTime / layer.fullChangeDuration
			) * layer.volumeMultiplier;
		}
	}

	void DoEvent () {
		nextEvent += eventFrequency;
		int newLayer = lastChangedLayer;
		while (newLayer == lastChangedLayer)
			newLayer = rand.Range (0, layers.Length);

		// Make large possibility that volume is 0 (below 0).
		float volume = rand.Range (-1.0f, 1.0f);
		volume = Mathf.Clamp01 (volume);

		layers[newLayer].targetVolume = volume;
		lastChangedLayer = newLayer;
	}
}
