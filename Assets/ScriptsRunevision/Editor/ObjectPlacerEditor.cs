using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (ObjectPlacer))]
public class ObjectPlacerEditor : Editor {

	bool autoUpdate = true;

	public override void OnInspectorGUI () {
		EditorGUI.BeginChangeCheck ();

		base.OnInspectorGUI ();

		if (EditorGUI.EndChangeCheck ()) {
			if (autoUpdate)
				UpdateScene ();
		}

		EditorGUILayout.Space ();

		autoUpdate = EditorGUILayout.Toggle ("Auto-update", autoUpdate);

		if (GUILayout.Button ("Update"))
			UpdateScene ();

		if (Application.isPlaying) {
			if (GUILayout.Button ("Randomize"))
				RandomizeScene ();
		}
	}

	void UpdateScene () {
		ObjectPlacer placer = target as ObjectPlacer;
		placer.Place ();
		placer.UpdateGlobals ();
	}

	void RandomizeScene () {
		ObjectPlacer placer = target as ObjectPlacer;
		placer.Randomize ();
	}
}
