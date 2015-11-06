using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Runevision.Structures;
using System.Linq;

[System.Serializable]
public struct Variation {
	public Color color1;
	public Color color2;
}

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

	public Variation branchesVariation;
	public Variation leavesVariation;

	[Space (6)]

	public Material ground;
	public Material skybox;
	public Light sunLight;

	public Color groundColor;
	public Color skyColor;
	public Color horizonColor;
	public Color lightColor;
	[Range (0.0f, 1.0f)]
	public float fogDensity;

	Transform dynamicRoot;
	MaterialPropertyBlock propertyBlock;
	static RandomHash hash = new RandomHash (0);

	void Start () {
		Place ();
		UpdateGlobals ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Return))
			Randomize ();
	}

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

	public void UpdateGlobals () {
		// Ground
		ground.SetColor ("_Color", groundColor);

		// Sky
		skybox.SetColor ("_SkyColor1", skyColor);
		skybox.SetColor ("_SkyColor2", horizonColor);

		// Sunlight
		skybox.SetColor ("_SunColor", lightColor);
		sunLight.color = lightColor;

		// Fog
		float skyIntensity = skybox.GetFloat ("_SkyIntensity");
		RenderSettings.fogColor = horizonColor * skyIntensity;
		RenderSettings.fogDensity = fogDensity * 0.1f;
		// Make horizon color go further up the sky the denser the fog.
		skybox.SetFloat ("_SkyExponent1", Mathf.Max (0, 3 - fogDensity * 7));
	}

	void PlaceObject (GameObject prefab, Vector3 pos, int x, int z) {
		// Calculate independent random values.
		// Showing them all together makes it easier to see if they're all independent.
		float randX = hash.Range (-0.5f, 0.5f, x, z, 1);
		float randZ = hash.Range (-0.5f, 0.5f, x, z, 2);
		float randR = hash.Range (-0.5f, 0.5f, x, z, 3);
		float randS = hash.Range (-1.0f, 1.0f, x, z, 4);
		float randC = hash.Range ( 0.0f, 1.0f, x, z, 5);

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
		propertyBlock.SetColor ("_Color", GetColor (branchesVariation, randC));
		rend.SetPropertyBlock (propertyBlock);

		rend = go.transform.GetChild (0).GetComponent<MeshRenderer> ();
		rend.GetPropertyBlock (propertyBlock);
		propertyBlock.SetColor ("_Color", GetColor (leavesVariation, randC));
		rend.SetPropertyBlock (propertyBlock);
	}

	Color GetColor (Variation variation, float t) {
		return LerpColorInHSV (variation.color1, variation.color2, t);
	}

	Color LerpColorInHSV (Color a, Color b, float t) {
		Vector4 aHsv = ColorUtility.RGBToHSV (a);
		Vector4 bHsv = ColorUtility.RGBToHSV (b);

		aHsv = WrapHueAround (aHsv, bHsv);

		Vector4 combinedHsv = Vector4.Lerp (aHsv, bHsv, t);
		Color rgb = ColorUtility.HSVToRGB (combinedHsv);
		return rgb;
	}

	Color WrapHueAround (Vector4 colorToModify, Vector4 colorToCompareWith) {
		// Wrap hue since hue is in a circular space.
		if (colorToModify.x < colorToCompareWith.x - 0.5f)
			colorToModify.x += 1;
		if (colorToModify.x > colorToCompareWith.x + 0.5f)
			colorToModify.x -= 1;

		return colorToModify;
	}

	static float FullToPositiveRange (float full) {
		return full * 0.5f + 0.5f;
	}

	public void Randomize () {
		int seed = new Rand ().Next ();
		Debug.Log ("Randomizing with seed "+seed);
		Randomize (seed);
	}

	ObjectPlacer referenceParameters;

	public void Randomize (int seed) {
		if (!Application.isPlaying) {
			Debug.LogError ("You may only randomize in play mode.");
			return;
		}

		if (referenceParameters == null) {
			gameObject.SetActive (false);
			var go = Instantiate (gameObject);
			gameObject.SetActive (true);
			referenceParameters = go.GetComponent<ObjectPlacer> ();
		}

		Rand hash = new Rand (seed);

		RandomizePlacement (hash);
		RandomizeColors (hash);

		Place ();
		UpdateGlobals ();
	}

	void RandomizePlacement (Rand hash) {
		noiseSize1 = RandomVariation (hash, referenceParameters.noiseSize1, 0.5f);
		noiseSize2 = RandomVariation (hash, referenceParameters.noiseSize2, 0.5f);
		threshold = RandomVariation (hash, referenceParameters.threshold, 0.4f);
		randomness = RandomVariation (hash, referenceParameters.randomness, 1.0f);
		scaleBase = RandomVariation (hash, referenceParameters.scaleBase, 0.3f);
	}

	float RandomVariation (Rand hash, float reference, float fraction) {
		float variation = reference * fraction;
		return hash.Range (reference - variation, reference + variation);
	}

	void RandomizeColors (Rand hash) {
		Color baseColor = ColorUtility.HSVToRGB (
			hash.value,
			0.3f + 0.7f * Mathf.Sqrt (hash.value),
			0.1f + 0.9f * Mathf.Sqrt (hash.value)
		);

		List<Color> primaryColors = GetPrimaryColors (baseColor, hash);
		AddVariationColors (primaryColors, hash);

		branchesVariation.color1 = PickBestColor (primaryColors, referenceParameters.branchesVariation.color1);
		branchesVariation.color2 = PickBestColor (primaryColors, referenceParameters.branchesVariation.color2);
		leavesVariation.color1   = PickBestColor (primaryColors, referenceParameters.leavesVariation.color1);
		leavesVariation.color2   = PickBestColor (primaryColors, referenceParameters.leavesVariation.color2);

		groundColor              = PickBestColor (primaryColors, referenceParameters.groundColor);

		skyColor                 = CalculateColor (primaryColors, referenceParameters.skyColor, e => {
			e.y *= 0.9f; // decrease saturation
			e.z = Mathf.Sqrt (e.z); // increase value
			return e;
		});
		horizonColor             = CalculateColor (primaryColors, referenceParameters.horizonColor, e => {
			e.y *= 0.4f; // decrease saturation
			e.z = 0.1f + 0.9f * Mathf.Sqrt (e.z); // increase value
			return e;
		});
		lightColor               = PickBestColor (primaryColors, referenceParameters.lightColor);

		fogDensity = Mathf.Pow (hash.value, 2);
	}

	List<Color> GetPrimaryColors (Color baseColor, Rand hash) {
		List<Color> colors = new List<Color> ();
		colors.Add (baseColor);

		Vector4 hsv = ColorUtility.RGBToHSV (baseColor);

		int selector = hash.Range (0, 2);
		if (selector == 0) {
			hsv.x += 0.49f;
			colors.Add (ColorUtility.HSVToRGB (hsv));
		}
		else if (selector == 1) {
			hsv.x += 1/3f;
			colors.Add (ColorUtility.HSVToRGB (hsv));
			hsv.x += 1/3f;
			colors.Add (ColorUtility.HSVToRGB (hsv));
		}

		return colors;
	}

	void AddVariationColors (List<Color> colors, Rand rand) {
		int index = rand.Range (0, colors.Count);
		float t = rand.Range (0.3f, 0.7f);
		Color newColor = LerpColorInHSV (
			colors[index],
			colors[(index + 1) % colors.Count],
			t
		);
		colors.Add (newColor);
	}

	Color PickBestColor (List<Color> colors, Color reference) {
		Vector4 hsvReference = ColorUtility.RGBToHSV (reference);

		float smallestDist = 100;
		Vector4 bestColor = Vector4.zero;
		for (int i = 0; i < colors.Count; i++) {
			Vector4 hsvOption = ColorUtility.RGBToHSV (colors[i]);

			hsvOption = WrapHueAround (hsvOption, hsvReference);

			float dist = Vector4.Distance (hsvReference, hsvOption);
			if (dist < smallestDist) {
				smallestDist = dist;
				bestColor = hsvOption;
			}
		}

		return ColorUtility.HSVToRGB (bestColor);
	}

	delegate Vector4 HSVModifier (Vector4 inputHSV);

	Color CalculateColor (List<Color> colors, Color reference, HSVModifier modifier) {
		Color picked = PickBestColor (colors, reference);
		Vector4 modified = modifier (ColorUtility.RGBToHSV (picked));
		return ColorUtility.HSVToRGB (modified);
	}
}
