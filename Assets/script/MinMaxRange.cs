using UnityEngine;
using System.Collections;

public class MinMaxRange : PropertyAttribute {
	public float min;
	public float max;

	public MinMaxRange (float min, float max) {
		this.min = min;
		this.max = max;
	}
}