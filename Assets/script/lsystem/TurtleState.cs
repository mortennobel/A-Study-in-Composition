using UnityEngine;
using System.Collections;

public struct TurtleState {
	public float w;
	public Matrix4x4 M;
	public float dist;

	public TurtleState(float w, Matrix4x4 M){
		this.w = w;
		this.M = M;
		this.dist = 0;
	}
}
