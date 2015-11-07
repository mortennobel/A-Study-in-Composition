using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Runevision.Structures;

public class DemoFlyCam : MonoBehaviour {

	public float radius = 20;
	public float speed = 1;
	public float minHeight = 1.5f;
	public float maxHeight = 1.5f;
	public float sceneSwitchFrequency = 10;
	public float fadeFromBlackTime = 5;

	public ObjectPlacer placer;
	public CanvasGroup blackScreen;

	float nextSceneTime = 0;
	Vector3 goal;

	float angleVelocity;
	Rand rand = new Rand ();

	// Use this for initialization
	void Start () {
		SetNewScene ();
		nextSceneTime += fadeFromBlackTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > nextSceneTime)
			SetNewScene ();

		transform.Translate (Vector3.forward * speed * Time.deltaTime, Space.Self);

		transform.eulerAngles = new Vector3 (
			0,
			Mathf.SmoothDampAngle (
				transform.eulerAngles.y,
				Quaternion.LookRotation (goal - transform.position).eulerAngles.y,
				ref angleVelocity,
				20f,
				40f
			),
			0
		);

		if (blackScreen != null && blackScreen.alpha > 0) {
			blackScreen.alpha = Mathf.MoveTowards (
				blackScreen.alpha,
				0,
				Time.deltaTime / fadeFromBlackTime
			);
			if (blackScreen.alpha == 0)
				blackScreen.gameObject.SetActive (false);
		}
	}

	void SetNewScene () {
		nextSceneTime += sceneSwitchFrequency;
		placer.Randomize ();

		float angle = rand.Range (0f, 360f);
		Quaternion rotation = Quaternion.Euler (0, angle, 0);

		float height = transform.position.y;
		transform.position = rotation * new Vector3 (0, height, -radius);
		transform.forward = rotation * Vector3.forward;
		float sideways = rand.Range (-0.5f, 0.5f) * radius;
		goal = rotation * new Vector3 (sideways, goal.y, radius);
	}
}
