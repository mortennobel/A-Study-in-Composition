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
	public GameObject title;
	public CanvasGroup credits;

	float nextSceneTime = 0;
	Vector3 goal;
	int sceneCount = 0;
	float height;
	Vector3 flyDir = Vector3.forward;
	bool showingCredits = false;

	float angleVelocity;
	Rand rand = new Rand ();
	bool isBusy = false;

	// Use this for initialization
	IEnumerator Start () {
		Cursor.visible = false;

		height = minHeight;
		SetNewSceneInstant (true);
		nextSceneTime += fadeFromBlackTime;
		blackScreen.gameObject.SetActive (true);
		yield return new WaitForSeconds (1);
		yield return StartCoroutine (FadeScreen (blackScreen, 0, fadeFromBlackTime));
		title.SetActive (true);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (flyDir * speed * Time.deltaTime, Space.Self);

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

		if (showingCredits) {
			if (Input.GetKeyDown (KeyCode.Escape))
				Application.Quit ();
			return;
		}

		if (Time.time > nextSceneTime)
			SetNewScene ();

		if (Input.GetKeyDown (KeyCode.Return))
			SetNewScene ();

		if (Input.GetKeyDown (KeyCode.Escape))
			ShowCredits ();
	}

	void SetNewScene () {
		if (isBusy)
			return;
		StartCoroutine (SetNewSceneRoutine ());
	}

	IEnumerator SetNewSceneRoutine () {
		sceneCount++;

		// Every n'th change we cut to black and maybe do a change in perspective.
		bool newTheme = (sceneCount % 3 == 0);
		if (newTheme) {

			// Somtimes fly above the trees (most often not).
			if (rand.value < 0.3f)
				height = maxHeight;
			else
				height = minHeight;

			// Sometimes fly sideways (most often not).
			if (rand.value < 0.4f)
				flyDir = Vector3.right * (rand.value < 0.5f ? 1 : -1);
			else
				flyDir = Vector3.forward;

			// Cut to black.
			yield return StartCoroutine (BlockScreen (blackScreen, 1, 1.0f));
			if (title.activeInHierarchy)
				yield return new WaitForSeconds (2);
			yield return StartCoroutine (BlockScreen (blackScreen, 0, 0.5f));
			title.SetActive (false);
		}

		SetNewSceneInstant (newTheme);
	}

	void SetNewSceneInstant (bool newScene) {
		nextSceneTime = Time.time + sceneSwitchFrequency;
		placer.Randomize (newScene);

		float angle = rand.Range (0f, 360f);
		Quaternion rotation = Quaternion.Euler (0, angle, 0);

		transform.position = rotation * new Vector3 (0, height, -radius);
		transform.forward = rotation * Vector3.forward;
		float sideways = rand.Range (-0.5f, 0.5f) * radius;
		goal = rotation * new Vector3 (sideways, goal.y, radius);
	}

	IEnumerator FadeScreen (CanvasGroup screen, float targetAlpha, float duration) {
		isBusy = true;
		screen.gameObject.SetActive (true);

		while (screen.alpha != targetAlpha) {
			screen.alpha = Mathf.MoveTowards (
				screen.alpha,
				targetAlpha,
				Time.deltaTime / duration
			);
			yield return null;
		}

		screen.gameObject.SetActive (screen.alpha > 0);
		isBusy = false;
	}

	IEnumerator BlockScreen (CanvasGroup screen, float targetAlpha, float duration) {
		isBusy = true;
		screen.gameObject.SetActive (true);
		screen.alpha = 1;

		yield return new WaitForSeconds (duration);

		screen.alpha = targetAlpha;
		screen.gameObject.SetActive (screen.alpha > 0);
		isBusy = false;
	}

	void ShowCredits () {
		if (isBusy)
			return;
		StartCoroutine (ShowCreditsRoutine ());
	}

	IEnumerator ShowCreditsRoutine () {
		showingCredits = true;
		title.SetActive (false);
		yield return StartCoroutine (FadeScreen (blackScreen, 1, 3));
		yield return StartCoroutine (FadeScreen (credits, 1, 1));
	}
}
