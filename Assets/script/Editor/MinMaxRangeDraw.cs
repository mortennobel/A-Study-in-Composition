using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer (typeof (MinMaxRange))]
public class MinMaxRangeDraw : PropertyDrawer {
	public override void OnGUI (Rect positionOld, SerializedProperty property, GUIContent label) {
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty (positionOld, label, property);

		// Draw label
		var position = EditorGUI.PrefixLabel (positionOld, GUIUtility.GetControlID (FocusType.Passive), label);

		// Don't make child fields be indented
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		MinMaxRange range = attribute as MinMaxRange;

		Vector2 prop = property.vector2Value;

		float x = prop.x;
		float y = prop.y;
		EditorGUI.MinMaxSlider (position, ref x,ref  y, range.min, range.max);

		prop.x = x;
		prop.y = y;

		property.vector2Value = prop;

		// Set indent back to what it was
		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty ();

		positionOld.y += EditorGUIUtility.singleLineHeight;

		EditorGUI.PropertyField (positionOld, property,new GUIContent(" "));
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight * 2;
	}
}
