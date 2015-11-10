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

	public int placementSeed = 0;

	[Range (0.5f, 1.0f)]
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
	public StarField starField;

	public Color groundColor;
	public Color skyColor;
	public Color horizonColor;
	public Color lightColor;
	public Color starsColor;
	[Range (0.0f, 1.0f)]
	public float fogDensity;

	Transform dynamicRoot;
	MaterialPropertyBlock propertyBlock;
	static RandomHash hash = new RandomHash (0);

	void Start () {
		Place ();
		starField = FindObjectOfType<StarField> ();
		UpdateGlobals ();
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

		// Calculate spacing.
		Mesh mainMesh = prefab.GetComponent<MeshFilter> ().sharedMesh;
		Vector3 size = mainMesh.bounds.size * prefab.transform.localScale.x;
		float spacing = baseDist * Mathf.Max (4, size.x, size.y);

		int xMin = Mathf.CeilToInt (bounds.min.x / spacing);
		int zMin = Mathf.CeilToInt (bounds.min.z / spacing);
		int xMax = Mathf.FloorToInt (bounds.max.x / spacing);
		int zMax = Mathf.FloorToInt (bounds.max.z / spacing);
		for (int x = xMin; x <= xMax; x++) {
			for (int z = zMin; z <= zMax; z++) {
				// Calculate position.
				Vector3 pos = new Vector3 (x * spacing, 0, z  * spacing);
				Vector3 noisePos = pos + Vector3.up * placementSeed;

				// Calculate two different noise values.
				float noiseVal1 = FullToPositiveRange (SimplexNoise.Noise (noisePos / noiseSize1));
				float noiseVal2 = FullToPositiveRange (SimplexNoise.Noise (noisePos / noiseSize2));

				// Combine the two noise values.
				float noise = Mathf.Sqrt (noiseVal1 * noiseVal2);

				// Add randomness to threshold value.
				float rand = hash.Range (-0.5f, 0.5f, x, z, 0);
				float thresholdWithRandomness = threshold + randomness * rand;

				// Place object if noise value is higher than threshold.
				if (noise > thresholdWithRandomness)
					PlaceObject (prefab, pos, x, z, spacing);
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

		// Stars
		bool enableStarField = fogDensity < 0.4f && skyColor.grayscale < 0.5f;
		if (starField) {
			starField.SetColor (starsColor, enableStarField);
		}
	}

	void PlaceObject (GameObject prefab, Vector3 pos, int x, int z, float spacing) {
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
			(randX * positionJitter) * spacing,
			0,
			(randZ * positionJitter) * spacing
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
	LSystem referenceGenerator;

	public void Randomize (int seed) {
		if (!Application.isPlaying) {
			Debug.LogError ("You may only randomize in play mode.");
			return;
		}

		if (referenceParameters == null) {
			gameObject.SetActive (false);
			var go = Instantiate (gameObject);
			var gen = Instantiate (generator.gameObject);
			gameObject.SetActive (true);
			referenceParameters = go.GetComponent<ObjectPlacer> ();
			referenceGenerator = gen.GetComponent<LSystem> ();
		}

		Rand hash = new Rand (seed);

		RandomizePlacement (hash);
		RandomizeColors (hash);
		RandomizeTrees (hash);

		Place ();
		UpdateGlobals ();
	}

	void RandomizePlacement (Rand hash) {
		placementSeed = hash.Next () % 1000;
		baseDist = hash.Range (0.50f, 0.75f);
		noiseSize1 = RandomVariation (hash, referenceParameters.noiseSize1, 0.3f);
		noiseSize2 = RandomVariation (hash, referenceParameters.noiseSize2, 0.5f);
		threshold = RandomVariation (hash, referenceParameters.threshold, 0.15f);
		randomness = RandomVariation (hash, referenceParameters.randomness, 0.5f);
		scaleBase = RandomVariation (hash, referenceParameters.scaleBase, 0.3f);
	}

	void RandomizeTrees (Rand hash) {
		generator.initialLength = RandomVariation (hash, referenceGenerator.initialLength, 0.3f);
		generator.initialWidth = RandomVariation (hash, referenceGenerator.initialWidth, 0.7f);
		generator.smallBranchBias = RandomVariation (hash, referenceGenerator.smallBranchBias, 1.0f);
		generator.turn1 = RandomVariation (hash, referenceGenerator.turn1, 1.3f);
		generator.turn2 = RandomVariation (hash, referenceGenerator.turn2, 1.3f);
		generator.turn3 = RandomVariation (hash, referenceGenerator.turn3, 1.3f);
		generator.roll1 = RandomVariation (hash, referenceGenerator.roll1, 1.3f);
		generator.roll2 = RandomVariation (hash, referenceGenerator.roll2, 1.3f);
		generator.roll3 = RandomVariation (hash, referenceGenerator.roll3, 1.3f);
		generator.lengthScale1 = RandomVariation (hash, referenceGenerator.lengthScale1, 0.3f);
		generator.lengthScale2 = RandomVariation (hash, referenceGenerator.lengthScale2, 0.3f);
		generator.lengthScale3 = RandomVariation (hash, referenceGenerator.lengthScale3, 0.3f);
		generator.lengthScale1 = Clamp (generator.lengthScale1, 0.05f, 0.95f);
		generator.lengthScale2 = Clamp (generator.lengthScale2, 0.05f, 0.95f);
		generator.lengthScale3 = Clamp (generator.lengthScale3, 0.05f, 0.95f);
		generator.e = RandomVariation (hash, referenceGenerator.e, 0.2f);
		generator.branchNo = referenceGenerator.branchNo;
		generator.iter = hash.Range (9, 10+1);
		generator.leafMid = RandomVariation (hash, referenceGenerator.leafMid, 0.9f);
		generator.leafRotate = RandomVariation (hash, referenceGenerator.leafRotate, 0.5f);
		generator.gravity = RandomVariation (hash, referenceGenerator.gravity, 1.5f);
	}

	float RandomVariation (Rand hash, float reference, float fraction) {
		float variation = reference * fraction;
		return hash.Range (reference - variation, reference + variation);
	}

	Vector2 RandomVariation (Rand hash, Vector2 reference, float fraction) {
		float variationMin = reference.x * fraction;
		float variationMax = reference.y * fraction;
		return new Vector2 (
			hash.Range (reference.x - variationMin, reference.x + variationMin),
			hash.Range (reference.y - variationMax, reference.y + variationMax)
		);
	}

	Vector2 Clamp (Vector2 value, float min, float max) {
		value.x = Mathf.Clamp (value.x, min, max);
		value.y = Mathf.Clamp (value.y, min, max);
		return value;
	}

	void RandomizeColors (Rand hash) {
		Color baseColor = ColorUtility.HSVToRGB (
			hash.value,
			0.2f + 0.5f * Mathf.Sqrt (hash.value),
			0.1f + 0.9f * Mathf.Sqrt (hash.value)
		);

		List<Color> primaryColors = GetPrimaryColors (baseColor, hash);
		AddVariationColors (primaryColors, hash);

		branchesVariation.color1 = PickBestColor (primaryColors, referenceParameters.branchesVariation.color1);
		branchesVariation.color2 = PickBestColor (primaryColors, referenceParameters.branchesVariation.color2);
		leavesVariation.color1   = PickBestColor (primaryColors, referenceParameters.leavesVariation.color1);
		leavesVariation.color2   = PickBestColor (primaryColors, referenceParameters.leavesVariation.color2);

		groundColor              = PickBestColor (primaryColors, referenceParameters.groundColor);

		starsColor                 = CalculateColor (primaryColors, referenceParameters.starsColor, e => {
			e.y *= 0.5f; // decrease saturation
			e.z = 0.7f + 0.3f * Mathf.Sqrt (e.z); // increase value
			return e;
		});

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
