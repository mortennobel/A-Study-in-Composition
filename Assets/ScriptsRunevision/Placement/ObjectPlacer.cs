using UnityEngine;
using System.Collections;
using Runevision.Structures;

public class ObjectPlacer : MonoBehaviour {

	public Bounds bounds = new Bounds (Vector3.zero, Vector3.one * 40);
	public GameObject prefab;

	[Range (1, 10)]
	public float baseDist = 1;

	[Range (1, 100)]
	public float noiseSize1 = 40;

	[Range (1, 100)]
	public float noiseSize2 = 5;

	[Range (0, 1)]
	public float threshold = 0.5f;

	[Range (0, 1)]
	public float randomness = 0.2f;

	[Range (0, 1)]
	public float positionJitter = 0.6f;

	[Range (0, 3)]
	public float scaleVariation = 0.5f;

	Transform dynamicRoot;
	static RandomHash hash = new RandomHash (0);

	void Start () {
		Place ();
	}
	
	// Update is called once per frame
	public void Place () {
		if (transform.childCount != 0)
			DestroyImmediate (transform.GetChild (0).gameObject);
		if (dynamicRoot != null)
			DestroyImmediate (dynamicRoot.gameObject);

		dynamicRoot = new GameObject ("Objects").transform;
		dynamicRoot.SetParent (transform, false);

		int xMin = Mathf.CeilToInt (bounds.min.x / baseDist);
		int zMin = Mathf.CeilToInt (bounds.min.z / baseDist);
		int xMax = Mathf.FloorToInt (bounds.max.x / baseDist);
		int zMax = Mathf.FloorToInt (bounds.max.z / baseDist);
		for (int x = xMin; x <= xMax; x++) {
			for (int z = zMin; z <= zMax; z++) {
				// Calculate position.
				Vector3 pos = new Vector3 (x * baseDist, 0, z  * baseDist);

				// Calculate two different noise values.
				float noiseVal1 = FullToPositiveRange (SimplexNoise.Noise (pos / noiseSize1));
				float noiseVal2 = FullToPositiveRange (SimplexNoise.Noise (pos / noiseSize2));

				// Combine the two noise values.
				float noise = Mathf.Sqrt (noiseVal1 * noiseVal2);

				// Add randomness to threshold value.
				float rand = hash.Range (-0.5f, 0.5f, x, z, 0);
				float thresholdWithRandomness = threshold + randomness * rand;

				// Place object if noise value is higher than threshold.
				if (noise > thresholdWithRandomness)
					PlaceObject (pos, x, z);
			}
		}
	}

	void PlaceObject (Vector3 pos, int x, int z) {
		// Calculate independent random values.
		float randX = hash.Range (-0.5f, 0.5f, x, z, 1);
		float randZ = hash.Range (-0.5f, 0.5f, x, z, 2);
		float randR = hash.Range (-0.5f, 0.5f, x, z, 3);
		float randS = hash.Range (-1.0f, 1.0f, x, z, 4);

		pos += new Vector3 (
			(randX * positionJitter) * baseDist,
			0,
			(randZ * positionJitter) * baseDist
		);
		float rotation = randR * 360;

		GameObject go =
			(GameObject)Instantiate (prefab, pos, Quaternion.Euler (0, rotation, 0));

		float scale = Mathf.Pow (2, randS * scaleVariation);
		go.transform.localScale = Vector3.one * scale;

		go.transform.SetParent (dynamicRoot, false);
	}

	static float FullToPositiveRange (float full) {
		return full * 0.5f + 0.5f;
	}
}
