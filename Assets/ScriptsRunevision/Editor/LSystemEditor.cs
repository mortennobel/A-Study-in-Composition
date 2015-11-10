using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (LSystem))]
public class LSystemEditor : Editor {

	bool autoUpdate = true;

	public override void OnInspectorGUI () {
		EditorGUI.BeginChangeCheck ();

		base.OnInspectorGUI ();

		if (EditorGUI.EndChangeCheck ()) {
			if (autoUpdate)
				UpdateObject ();
		}

		EditorGUILayout.Space ();

		autoUpdate = EditorGUILayout.Toggle ("Auto-Update", autoUpdate);

		if (GUILayout.Button ("Update"))
			UpdateObject ();
	}

	void UpdateObject () {
		LSystem obj = target as LSystem;
		obj.UpdateTree ();
	}
}
