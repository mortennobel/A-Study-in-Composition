using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Turtle {

	TurtleState turtleState;
	Stack<TurtleState> tss;

	public Turtle(float w){
		turtleState = new TurtleState(w, Matrix4x4.identity);
		tss = new Stack<TurtleState>();
	}

	public void Gravity(float fraction){
		turtleState.M = Gravity (turtleState.M, fraction);

	}

	public float GetDist(){
		return turtleState.dist;
	}

	public static Matrix4x4 Gravity(Matrix4x4 m, float fraction){
		Quaternion q = QuaternionFromMatrix(m);
		Quaternion q1 = Quaternion.Euler (180, 0, 0);
		float currentAngle = Quaternion.Angle (q, q1);
		Quaternion newDir = Quaternion.Slerp (q, q1, fraction);
		return m * Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse (q) * newDir, Vector3.one);
	}

	public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		return q;
	}

	public void Turn(float angle){
		turtleState.M = turtleState.M * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,angle,0), Vector3.one);
	}

	public void Roll(float angle){
		turtleState.M = turtleState.M * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,angle), Vector3.one);
	}

	public void Move(float dist){
		turtleState.M = turtleState.M * Matrix4x4.TRS(new Vector3(0,0,dist), Quaternion.identity, Vector3.one);
		turtleState.dist += dist;
	}

	// Store the current state
	public void Push(){
		tss.Push(turtleState);
	}

	// Restore a previous state
	public void Pop(){
		if (tss.Count==0){
			Debug.LogError("Invalid pop. Stack is empty (more pop than push)");
		}
		turtleState = tss.Pop();
	}

	public TurtleState Peek(){
		return turtleState;
	} 

	public void SetWidth(float w){
		turtleState.w = w;

	}

	public float GetWidth(){
		return turtleState.w;
	}

	public Matrix4x4 GetTransform(){
		return turtleState.M;
	}
}
