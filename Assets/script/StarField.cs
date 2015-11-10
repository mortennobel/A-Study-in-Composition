using UnityEngine;
using System.Collections;

public class StarField : MonoBehaviour {

	public Camera cam;
	public ParticleSystem ps;
	ParticleSystem.Particle[] particles;

	public Color color;

	[MinMaxRange(0,10)]
	public Vector2 size = new Vector2(0,1);

	// Use this for initialization
	void Start () {
		cam = Camera.main;
		ps = GetComponent<ParticleSystem> ();	
		particles = new ParticleSystem.Particle[ps.particleCount];
		SetColor (Color.white, false);
	}

	void UpdateParticles(){
		if (ps == null || particles == null)
			return;

		if (ps.particleCount > particles.Length) {
			particles = new ParticleSystem.Particle[ps.particleCount];
		}

		ps.GetParticles (particles);
		for (int i = 0; i < particles.Length; i++) {
			particles [i].color = new Color(color.r,color.g,color.b,Random.value);
			particles [i].size = Random.Range (size.x, size.y);
		}
		ps.SetParticles (particles,particles.Length);
	}

	public void SetColor(Color color, bool enable){
		if (ps == null) {
			Start ();
		}
		if (!enable) {
			color = new Color (0, 0, 0, 0);
		}
		this.color = color;
		UpdateParticles ();

		ps.GetComponent<Renderer> ().sharedMaterial.color = color;

	}

	void LateUpdate(){
		transform.position = cam.transform.position;
	}
}
