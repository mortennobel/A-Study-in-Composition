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

	// turn the turtle around Y axis
	public void Turn(float angle){
#if SOLUTION_CODE
		turtleState.M = turtleState.M * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,angle,0), Vector3.one);
#endif
	}

	// turn the turtle around Z axis
	public void Roll(float angle){
		#if SOLUTION_CODE
		turtleState.M = turtleState.M * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,angle), Vector3.one);
		#endif
	}

	// Move the turtle along z-axis
	public void Move(float dist){
		#if SOLUTION_CODE
		turtleState.M = turtleState.M * Matrix4x4.TRS(new Vector3(0,0,dist), Quaternion.identity, Vector3.one);
#endif
	}

	// Store the current state
	public void Push(){
		#if SOLUTION_CODE
		tss.Push(turtleState);
#endif
	}

	// Restore a previous state
	public void Pop(){
		#if SOLUTION_CODE
		if (tss.Count==0){
			Debug.LogError("Invalid pop. Stack is empty (more pop than push)");
		}
		turtleState = tss.Pop();
#endif
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
