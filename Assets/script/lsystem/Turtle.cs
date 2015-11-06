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

	public void Turn(float angle){
		turtleState.M = turtleState.M * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,angle,0), Vector3.one);
	}

	public void Roll(float angle){
		turtleState.M = turtleState.M * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,angle), Vector3.one);
	}

	public void Move(float dist){
		turtleState.M = turtleState.M * Matrix4x4.TRS(new Vector3(0,0,dist), Quaternion.identity, Vector3.one);
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
