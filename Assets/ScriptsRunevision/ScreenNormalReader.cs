using UnityEngine;
using System.Collections;

public class ScreenNormalReader : MonoBehaviour {

	public Material mat;
	Camera cam;
	RenderTexture rt;
	Texture2D tex;
	int size = 32;

	public Vector3 normal;
	public float depthNormalized;

	void Start () {
		rt = new RenderTexture (size, size, 24);
		tex = new Texture2D (size, size, TextureFormat.ARGB32, false);

		cam = GetComponent<Camera> ();
		cam.depthTextureMode = DepthTextureMode.DepthNormals;
		cam.targetTexture = rt;
	}

	// Update is called once per frame
	void Update () {
		cam.Render ();
		Color[] pixels = tex.GetPixels ();

		float sum = 0;
		normal = Vector3.forward * sum;
		for (int i = 0; i < pixels.Length; i++) {
			Color color = pixels[i];
			float closeness = 1 - color.a;
			if (closeness > 0) {
				Vector2 uv = new Vector2 (i % size, i / size) / (size - 1f);
				Vector3 thisNormal = -(Vector3)(uv - Vector2.one * 0.5f);
				normal += thisNormal * closeness;
				sum += closeness;
			}
		}
		normal = (sum == 0 ? Vector3.forward : (normal / sum));
		depthNormalized = 1 - (sum / pixels.Length);
	}

	// Called by the camera to apply the image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination){
		//mat is the material containing your shader
		Graphics.Blit(source,destination,mat);
		// Read pixels
		tex.ReadPixels (new Rect (0, 0, size, size), 0, 0);
	}
}
