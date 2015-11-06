using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LSystem : MonoBehaviour {
	public MeshFilter branches;
	public MeshFilter leaves;
	List<LSElement> str;

	public int seed = 0;
	public float smoothSeed = 0;
	[MinMaxRange(5,200)]
	public Vector2 initialLength = new Vector2(100,100);
	[MinMaxRange(5,300)]
	public Vector2 initialWidth = new Vector2(20,20);
	[MinMaxRange(-180,180)]
	[MinMaxRange(-180,180)]
	public Vector2 alpha2 = Vector2.one * -30;
	[MinMaxRange(-180,180)]
	[MinMaxRange(-180,180)]
	public Vector2 phi2 = Vector2.one * 137;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 r1 = Vector2.one * 0.8f;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 r2 = Vector2.one * 0.8f;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 q = Vector2.one * 0.5f;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 e = Vector2.one * 0.5f;
	[MinMaxRange(0,2)]
	public Vector2 smin = Vector2.one * 0;
	public int iter = 8;
	[MinMaxRange(0,1)]
	public Vector2 leafMid = Vector2.one * 0.5f;
	[MinMaxRange(0.01f,5)]
	public Vector2 leafLength = Vector2.one * 2;
	[MinMaxRange(0.01f,5)]
	public Vector2 leafWidth = Vector2.one * 2;
	[MinMaxRange(-100,100)]
	public Vector2 leafRotate = Vector2.one * 0;
	List<Vector3> vertices = new List<Vector3>();
	List<int> indices = new List<int>();

	List<Vector3> verticesLeaf = new List<Vector3>();
	List<int> indicesLeaf = new List<int>();

	void Start(){
		seed = GetInstanceID ();
		str = new List<LSElement>();
		UpdateTree ();
	}

	public Mesh[] Build(){
		ResetRandom (seed);
		ExpandRules();
		return Interpret();
	}

	void UpdateTree(){
		var meshes = Build ();
		var meshFilters = new MeshFilter[]{branches,leaves};
		for (int i = 0; i < meshes.Length; i++) {
			meshes[i].RecalculateNormals ();
			meshes[i].uv = new Vector2[meshes[i].vertexCount];
			meshes [i].RecalculateBounds ();
			meshFilters[i].mesh = meshes[i];
		}
	}

	int count = 0;
	void Update(){
		if (Input.GetKey(KeyCode.R)){
			UpdateTree ();
		}
#if UNITY_EDITOR
		if (count++ % 30==0) {
			UpdateTree ();
		}
#endif
	}

	public static void ResetRandom(int seed1){
		Random.seed = seed1;
	}

	public float Eval(Vector2 v){
		float val = Runevision.Structures.SimplexNoise.Noise(new Vector3(Random.value*100000,smoothSeed));
		val = val * 0.5f + 0.5f;
		return Mathf.Lerp(v.x, v.y, val);
	}

	void ExpandRules(){
		str.Clear();
		str.Add(new LSElement(LSElement.LSSymbol.APEX, Eval(initialLength), Eval(initialWidth)));

		// string debug = "Expanding "+str[0]+" to ";

		for (int i=0;i<iter;i++){
			List<LSElement> outList = new List<LSElement>();
			bool lastIter = (i == iter-1);
			foreach (var s in str){
				r.Apply(s, outList, lastIter);
			}
			str = outList;
		}
		/*foreach (var s in str){
			debug += s;
		}
		Debug.Log(debug);*/
	}

	void AddLeaf(Matrix4x4 m, float len, float width){
		len *= Eval(leafLength);
		width *= Eval(leafWidth);

		float leafRot = Eval (leafRotate);
		float w = width;
		float l = len;
		float mid = l * Eval(leafMid);
		Vector3 p0 = m.MultiplyPoint(new Vector3(0, 0, 0));
		Vector3 p1 = m.MultiplyPoint(Quaternion.Euler(0,0,leafRot) * new Vector3(0, w, mid));
		Vector3 p2 = m.MultiplyPoint(new Vector3(0, 0, l));
		Vector3 p3 = m.MultiplyPoint(Quaternion.Euler(0,0,-leafRot) * new Vector3(0, -w, mid));
		verticesLeaf.Add (p0);
		verticesLeaf.Add (p1);
		verticesLeaf.Add (p2);

		verticesLeaf.Add (p0);
		verticesLeaf.Add (p2);
		verticesLeaf.Add (p3);
		for (int j=0;j<6;j++){
			indicesLeaf.Add(indicesLeaf.Count);
		}
	}

	void AddCone(Matrix4x4 m, float l, float w0, float w1){
		//float len = Mathf.Sqrt(l*l + Mathf.Sqrt(w0-w1));
		// float a = l/len;
		//float b = (w0-w1)/len;
			float alpha = 2.0f*Mathf.PI*i/(float)N;
			Vector3 p0 = m.MultiplyPoint(new Vector3(w0 * Mathf.Cos(alpha), w0 * Mathf.Sin(alpha), 0));
			Vector3 p1 = m.MultiplyPoint(new Vector3(w1 * Mathf.Cos(alpha), w1 * Mathf.Sin(alpha), l));
			vertices.Add(p0);
			vertices.Add(p1);

		}
	}

	public Mesh[] Interpret(){
		Turtle turtle = new Turtle(Eval(initialWidth));

		foreach (var elem in str){
			switch (elem.symbol){
			case LSElement.LSSymbol.LEAF:
				AddLeaf (turtle.Peek().M,elem.data [0], elem.data [1]); 
				break;
			case LSElement.LSSymbol.DRAW:
				float movDist = elem.data [0];
				AddCone(turtle.Peek().M, movDist, turtle.GetWidth(), turtle.GetWidth() * elem.data[1]);
				turtle.Move(movDist);
				break;
			case LSElement.LSSymbol.TURN:
				turtle.Turn(elem.data[0]);
				break;
			case LSElement.LSSymbol.ROLL:
				turtle.Roll(elem.data[0]);
				break;
			case LSElement.LSSymbol.PUSH_STATE:
				turtle.Push();
				break;
			case LSElement.LSSymbol.POP_STATE:
				turtle.Pop();
				break;
			case LSElement.LSSymbol.WIDTH:
				turtle.SetWidth(elem.data[0]);
				break;
			}
		}
		Mesh mesh = new Mesh();
		vertices.Clear();
		indices.Clear();
		Mesh meshLeafs = new Mesh();
		verticesLeaf.Clear();
		indicesLeaf.Clear();

		return new Mesh[]{mesh, meshLeafs};
	}

	void OnGUI(){
		if (GUI.Button(new Rect(0,0,50,30), "Fig.a")){
			r1 = Vector2.one*0.75f;
			r2 = Vector2.one*0.77f;
			initialWidth = new Vector2(30,30);
			q = Vector2.one*0.5f;
			e = Vector2.one*0.4f;
			smin = Vector2.one*0;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(50,0,50,30), "Fig.b")){
			r1 = Vector2.one*0.65f;
			r2 = Vector2.one*0.71f;
			initialWidth = new Vector2(20,20);
			q = Vector2.one*0.53f;
			e = Vector2.one*0.5f;
			smin = Vector2.one*1.7f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(100,0,50,30), "Fig.c")){
			r1 = Vector2.one*0.5f;
			r2 = Vector2.one*0.85f;
			initialWidth = new Vector2(20,20);
			q = Vector2.one*0.45f;
			e = Vector2.one*0.5f;
			smin = Vector2.one*0.5f;
			iter = 9;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(150,0,50,30), "Fig.d")){
			r1 = Vector2.one*0.6f;
			r2 = Vector2.one*0.85f;
			initialWidth = new Vector2(20,20);
			q = Vector2.one*0.45f;
			e = Vector2.one*0.5f;
			smin = Vector2.one*0.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(200,0,50,30), "Fig.e")){
			r1 = Vector2.one*0.58f;
			r2 = Vector2.one*0.83f;
			initialWidth = new Vector2(20,20);
			q = Vector2.one*0.40f;
			e = Vector2.one*0.5f;
			smin = Vector2.one*1.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(250,0,50,30), "Fig.f")){
			r1 = Vector2.one*0.92f;
			r2 = Vector2.one*0.37f;
			initialWidth = new Vector2(2,2);
			q = Vector2.one*0.50f;
			e = Vector2.one*0.0f;
			smin = Vector2.one*0.5f;
			iter = 12;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(300,0,50,30), "Fig.g")){
			r1 = Vector2.one*0.80f;
			r2 = Vector2.one*0.80f;
			initialWidth = new Vector2(30,30);
			q = Vector2.one*0.50f;
			e = Vector2.one*0.5f;
			smin = Vector2.one*0.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(350,0,50,30), "Fig.h")){
			r1 = Vector2.one*0.95f;
			r2 = Vector2.one*0.75f;
			initialWidth = new Vector2(40,40);
			q = Vector2.one*0.60f;
			e = Vector2.one*0.45f;
			smin = Vector2.one*25.0f;
			iter = 12;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(400,0,50,30), "Fig.i")){
			r1 = Vector2.one*0.55f;
			r2 = Vector2.one*0.95f;
			initialWidth = new Vector2(5,5);
			q = Vector2.one*0.40f;
			e = Vector2.one*0.00f;
			smin = Vector2.one*5.0f;
			iter = 12;
			UpdateTree ();
		}
		if (GUI.Button (new Rect (450, 0, 50, 30), "Update")) {
			UpdateTree ();
		}

	}
}
