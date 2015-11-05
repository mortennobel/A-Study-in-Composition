using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LSystem : MonoBehaviour {
	public MeshFilter branches;
	public MeshFilter leaves;
	List<LSElement> str;
	public int length0 = 100;
	public float w0 = 20;
	public float alpha1 = 30;
	public float alpha2 = -30;
	public float phi1 = 137;
	public float phi2 = 137;
	public float r1 = 0.8f;
	public float r2 = 0.8f;
	public float q = 0.5f;
	public float e = 0.5f;
	public float smin = 0;
	public int iter = 8;
	public float leafMid = 0.5f;
	public float leafLength = 2;
	public float leafWidth = 2;
	public float leafRotate = 0;
	List<Vector3> vertices = new List<Vector3>();
	List<int> indices = new List<int>();

	List<Vector3> verticesLeaf = new List<Vector3>();
	List<int> indicesLeaf = new List<int>();

	void Start(){
		str = new List<LSElement>();
		UpdateTree ();
	}

	void UpdateTree(){
		ExpandRules();
		var meshes = Interpret();
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

	void ExpandRules(){
		str.Clear();
		str.Add(new LSElement(LSElement.LSSymbol.APEX, length0, w0));

		// string debug = "Expanding "+str[0]+" to ";

		Rule r = new Rule(alpha1, alpha2, phi1, phi2, r1, r2, q, e, smin);
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
		len *= leafLength;
		width *= leafWidth;

		Vector3 p0 = m.MultiplyPoint(new Vector3(0, 0, 0));
		Vector3 p1 = m.MultiplyPoint(Quaternion.Euler(0,0,leafRotate) * new Vector3(0, width, len*leafMid));
		Vector3 p2 = m.MultiplyPoint(new Vector3(0, 0, len));
		Vector3 p3 = m.MultiplyPoint(Quaternion.Euler(0,0,-leafRotate) * new Vector3(0, -width, len*leafMid));
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
		float len = Mathf.Sqrt(l*l + Mathf.Sqrt(w0-w1));
		float a = l/len;
		float b = (w0-w1)/len;
		const int N = 5;
		for (int i=0;i<=N;i++){
			float alpha = 2.0f*Mathf.PI*i/(float)N;
			Vector3 p0 = m.MultiplyPoint(new Vector3(w0 * Mathf.Cos(alpha), w0 * Mathf.Sin(alpha), 0));
			Vector3 p1 = m.MultiplyPoint(new Vector3(w1 * Mathf.Cos(alpha), w1 * Mathf.Sin(alpha), l));

			alpha = 2.0f*Mathf.PI*(i+1)/(float)N;
			Vector3 p2 = m.MultiplyPoint(new Vector3(w0 * Mathf.Cos(alpha), w0 * Mathf.Sin(alpha), 0));
			Vector3 p3 = m.MultiplyPoint(new Vector3(w1 * Mathf.Cos(alpha), w1 * Mathf.Sin(alpha), l));

			vertices.Add(p0);
			vertices.Add(p2);
			vertices.Add(p1);

			vertices.Add(p1);
			vertices.Add(p2);
			vertices.Add(p3);
			for (int j=0;j<6;j++){
				indices.Add(indices.Count);
			}
		}
	}

	public Mesh[] Interpret(){
		Turtle turtle = new Turtle(w0);

		foreach (var elem in str){
			switch (elem.symbol){
			case LSElement.LSSymbol.LEAF:
				AddLeaf (turtle.Peek().M,elem.data [0], elem.data [1]); 
				break;
			case LSElement.LSSymbol.DRAW:
				AddCone(turtle.Peek().M, elem.data[0], turtle.GetWidth(), turtle.GetWidth() * elem.data[1]);
				turtle.Move(elem.data[0]);
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
		mesh.vertices = vertices.ToArray();
		mesh.triangles = indices.ToArray();
		vertices.Clear();
		indices.Clear();
		Mesh meshLeafs = new Mesh();
		meshLeafs.vertices = verticesLeaf.ToArray();
		meshLeafs.triangles = indicesLeaf.ToArray();
		verticesLeaf.Clear();
		indicesLeaf.Clear();

		return new Mesh[]{mesh, meshLeafs};
	}

	void OnGUI(){
		if (GUI.Button(new Rect(0,0,50,30), "Fig.a")){
			r1 = 0.75f;
			r2 = 0.77f;
			alpha1 = 35;
			alpha2 = -35;
			phi1 = 0;
			phi2 = 0;
			w0 = 30;
			q = 0.5f;
			e = 0.4f;
			smin = 0;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(50,0,50,30), "Fig.b")){
			r1 = 0.65f;
			r2 = 0.71f;
			alpha1 = 27;
			alpha2 = -68;
			phi1 = 0;
			phi2 = 0;
			w0 = 20;
			q = 0.53f;
			e = 0.5f;
			smin = 1.7f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(100,0,50,30), "Fig.c")){
			r1 = 0.5f;
			r2 = 0.85f;
			alpha1 = 25;
			alpha2 = -15;
			phi1 = 180;
			phi2 = 0;
			w0 = 20;
			q = 0.45f;
			e = 0.5f;
			smin = 0.5f;
			iter = 9;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(150,0,50,30), "Fig.d")){
			r1 = 0.6f;
			r2 = 0.85f;
			alpha1 = 25;
			alpha2 = -15;
			phi1 = 180;
			phi2 = 180;
			w0 = 20;
			q = 0.45f;
			e = 0.5f;
			smin = 0.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(200,0,50,30), "Fig.e")){
			r1 = 0.58f;
			r2 = 0.83f;
			alpha1 = 30;
			alpha2 = 15;
			phi1 = 0;
			phi2 = 180;
			w0 = 20;
			q = 0.40f;
			e = 0.5f;
			smin = 1.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(250,0,50,30), "Fig.f")){
			r1 = 0.92f;
			r2 = 0.37f;
			alpha1 = 0;
			alpha2 = 60;
			phi1 = 180;
			phi2 = 0;
			w0 = 2;
			q = 0.50f;
			e = 0.0f;
			smin = 0.5f;
			iter = 12;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(300,0,50,30), "Fig.g")){
			r1 = 0.80f;
			r2 = 0.80f;
			alpha1 = 30;
			alpha2 = -30;
			phi1 = 137;
			phi2 = 137;
			w0 = 30;
			q = 0.50f;
			e = 0.5f;
			smin = 0.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(350,0,50,30), "Fig.h")){
			r1 = 0.95f;
			r2 = 0.75f;
			alpha1 = 5;
			alpha2 = -30;
			phi1 = -90;
			phi2 = 90;
			w0 = 40;
			q = 0.60f;
			e = 0.45f;
			smin = 25.0f;
			iter = 12;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(400,0,50,30), "Fig.i")){
			r1 = 0.55f;
			r2 = 0.95f;
			alpha1 = -5;
			alpha2 = 30;
			phi1 = 137;
			phi2 = 137;
			w0 = 5;
			q = 0.40f;
			e = 0.00f;
			smin = 5.0f;
			iter = 12;
			UpdateTree ();
		}
		if (GUI.Button (new Rect (450, 0, 50, 30), "Update")) {
			UpdateTree ();
		}

	}
}
