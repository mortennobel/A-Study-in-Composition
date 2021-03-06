﻿using UnityEngine;
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

		// Min volume can be used to set the minimum target volume (before multiplier).
		// or if below zero, to make it less likely to make this sound heard.
		[Range (-1, 1)]
		public float minVolume;

		[Range (1, 30)]
		public float fullChangeDuration;
	}

	public AudioLayer[] layers;
	public float eventFrequency = 5;

	float nextEvent = 0;
	int lastChangedLayer = 0;
	Rand rand = new Rand ();
	public bool ending = false;

	public void FadeOut () {
		ending = true;
		for (int i = 0; i < layers.Length; i++) {
			layers[i].targetVolume = 0;
		}
	}

	// Update is called once per frame
	void Update () {
		if (!ending && Time.time > nextEvent)
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

		float volume = rand.Range (layers[newLayer].minVolume, 1.0f);
		volume = Mathf.Clamp01 (volume);

		layers[newLayer].targetVolume = volume;
		lastChangedLayer = newLayer;
	}
}
