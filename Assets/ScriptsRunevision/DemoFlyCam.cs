using UnityEngine;
using System.Collections;
using Runevision.Structures;

public class DemoFlyCam : MonoBehaviour {

	public float radius = 20;
	public float speed = 5;
	public float minHeight = 1.5f;
	public float maxHeight = 8.0f;
	public float goalSwitchFrequency = 8;
	public float sceneSwitchFrequency = 10;
	public ObjectPlacer placer;

	float nextGoalTime = 0;
	float nextSceneTime = 0;
	Vector3 goal;

	Vector3 velocity;
	float angleVelocity;
	Rand rand = new Rand ();
	//float heightVelocity;

	// Use this for initialization
	void Start () {
		SetNewGoal ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > nextSceneTime)
			SetNewScene ();

		if (Time.time > nextGoalTime)
			SetNewGoal ();

		transform.Translate (Vector3.forward * speed * Time.deltaTime, Space.Self);

		transform.eulerAngles = new Vector3 (
			0,
			Mathf.SmoothDampAngle (
				transform.eulerAngles.y,
				Quaternion.LookRotation (goal - transform.position).eulerAngles.y,
				ref angleVelocity,
				10f,
				40f
			),
			0
		);

		Debug.DrawRay (goal, Vector3.up * 20, Color.white);
	}

	void SetNewGoal () {
		nextGoalTime += goalSwitchFrequency;

		goal = Quaternion.Euler (0, rand.Range (0, 360), 0) * Vector3.right * radius;
		if (rand.value < 0.5f) {
			if (rand.value < 0.7f)
				goal.y = minHeight;
			else
				goal.y = maxHeight;
		}
	}

	void SetNewScene () {
		nextSceneTime += sceneSwitchFrequency;
		placer.Randomize ();
	}
}
