using UnityEngine;
using System;

namespace Runevision.Structures {

public class Rand : System.Random {
	
	public Rand () : base () {}
	public Rand (int seed) : base (seed) {}
	
	public float value { get { return (float)NextDouble (); } }
	
	public float Range (float min, float max) {
		return (float)NextDouble () * (max - min) + min;
	}
	
	public int Range (int min, int max) {
		return Next (max - min) + min;
	}
}

}
