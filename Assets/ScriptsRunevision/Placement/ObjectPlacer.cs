using UnityEngine;
using System.Collections;
using Runevision.Structures;

public class ObjectPlacer : MonoBehaviour {

	public Bounds bounds = new Bounds (Vector3.zero, Vector3.one * 20);
	public GameObject prefab;

	[Range (1, 10)]
	public float baseDist = 1;

	[Range (1, 100)]
	public float noiseSize1 = 10;

	[Range (1, 100)]
	public float noiseSize2 = 3;

	[Range (0, 1)]
	public float randomness = 0.5f;

	Transform dynamicRoot;
	static RandomHash hash = new RandomHash (0);

	void Start () {
		Place ();
	}
	
	// Update is called once per frame
	public void Place () {
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
				Vector3 pos = new Vector3 (x * baseDist, 0, z * baseDist);
				float noiseVal1 = SimplexNoise.Noise (pos / noiseSize1);
				float noiseVal2 = SimplexNoise.Noise (pos / noiseSize2);

				float randomVal = randomness * 2 * (hash.Value (x, z, 0) - 0.5f);

				float noise = (noiseVal1 + 1) * (noiseVal2 + 1) / 2 - 1;
				if (noise > randomVal)
					PlaceObject (pos);
			}
		}
	}

	void PlaceObject (Vector3 pos) {
		float rotation = SimplexNoise.Noise (pos) * 360 * 10;
		GameObject go =
			(GameObject)Instantiate (prefab, pos, Quaternion.Euler (0, rotation, 0));
		go.transform.SetParent (dynamicRoot, false);
	}
}
