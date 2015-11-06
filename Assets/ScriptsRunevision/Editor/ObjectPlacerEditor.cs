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
				(target as ObjectPlacer).Place ();
		}

		EditorGUILayout.Space ();

		autoUpdate = EditorGUILayout.Toggle ("Auto-update", autoUpdate);

		if (GUILayout.Button ("Update"))
			(target as ObjectPlacer).Place ();
	}
}
