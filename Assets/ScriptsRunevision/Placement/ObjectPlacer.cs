using UnityEngine;
using System.Collections;
using Runevision.Structures;

[ExecuteInEditMode]
public class ObjectPlacer : MonoBehaviour {

	public Bounds bounds = new Bounds (Vector3.zero, Vector3.one * 40);
	public GameObject prefab;
	public LSystem generator;

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

	[Space (6)]

	[Range (0, 10)]
	public float scaleBase = 1.0f;

	[Range (0, 3)]
	public float scaleVariation = 0.5f;

	[Space (6)]

	public Color color1 = Color.red;
	public Color color2 = Color.green;

	Transform dynamicRoot;
	MaterialPropertyBlock propertyBlock;
	static RandomHash hash = new RandomHash (0);

	void Start () {
		Place ();
	}
	
	// Update is called once per frame
	public void Place () {
		if (propertyBlock == null)
			propertyBlock = new MaterialPropertyBlock ();

		if (transform.childCount != 0)
			DestroyImmediate (transform.GetChild (0).gameObject);
		if (dynamicRoot != null)
			DestroyImmediate (dynamicRoot.gameObject);

		dynamicRoot = new GameObject ("Objects").transform;
		dynamicRoot.SetParent (transform, false);

		GameObject prefab = generator.BuildGameObject ();

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
					PlaceObject (prefab, pos, x, z);
			}
		}

		DestroyImmediate (prefab);
	}

	void PlaceObject (GameObject prefab, Vector3 pos, int x, int z) {
		// Calculate independent random values.
		// Showing them all together makes it easier to see if they're all independent.
		float randX = hash.Range (-0.5f, 0.5f, x, z, 1);
		float randZ = hash.Range (-0.5f, 0.5f, x, z, 2);
		float randR = hash.Range (-0.5f, 0.5f, x, z, 3);
		float randS = hash.Range (-1.0f, 1.0f, x, z, 4);
		float randC = hash.Range (-1.0f, 1.0f, x, z, 5);

		// Create object.
		GameObject go = (GameObject)Instantiate (prefab);
		go.transform.SetParent (dynamicRoot, false);

		// Set position, rotation, and scale.

		pos += new Vector3 (
			(randX * positionJitter) * baseDist,
			0,
			(randZ * positionJitter) * baseDist
		);
		go.transform.localPosition = pos;

		float rotation = randR * 360;
		go.transform.Rotate (0, rotation, 0, Space.World);

		float scale = scaleBase * Mathf.Pow (2, randS * scaleVariation);
		go.transform.localScale *= scale;

		// Set colors.
		var rend = go.GetComponent<MeshRenderer> ();

		rend.GetPropertyBlock (propertyBlock);

		Vector4 color1hsv = ColorUtility.RGBToHSV (color1);
		Vector4 color2hsv = ColorUtility.RGBToHSV (color2);
		// Wrap hue since hue is in a circular space.
		if (color1hsv.w < color2hsv.w - 0.5f)
			color1hsv.w += 1;
		if (color1hsv.w > color2hsv.w + 0.5f)
			color1hsv.w -= 1;
		Vector4 combinedHsv = Vector4.Lerp (color1hsv, color2hsv, randC);
		Color combined = ColorUtility.HSVToRGB (combinedHsv);

		propertyBlock.SetColor ("_Color", combined);
		rend.SetPropertyBlock (propertyBlock);
	}

	static float FullToPositiveRange (float full) {
		return full * 0.5f + 0.5f;
	}
}
