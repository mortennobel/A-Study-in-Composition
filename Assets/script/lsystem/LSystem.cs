using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LSystem : MonoBehaviour {
	public MeshFilter branches;
	public MeshFilter leaves;
	public Material branchesMat;
	public Material leavesMat;
	List<LSElement> str;

	public int seed = 0;
	public float smoothSeed = 0;
	[MinMaxRange(5,200)]
	public Vector2 initialLength = new Vector2(100,100);
	[MinMaxRange(5,300)]
	public Vector2 initialWidth = new Vector2(20,20);

	[Range(0.01f, 0.5f)]
	public float minTerminalWidthRatio = 0.05f;
	[Range(0.005f, 0.5f)]
	public float minBranchRatio = 0.05f;
	[MinMaxRange(0.0f, 1.0f)]
	public Vector2 smallBranchBias = Vector2.one * 0.5f;

	[Space (6)]

	[MinMaxRange(-180,180)]
	public Vector2 turn1 = Vector2.one * 30;
	[MinMaxRange(-180,180)]
	public Vector2 turn2 = Vector2.one * -30;
	[MinMaxRange(-180,180)]
	public Vector2 turn3 = Vector2.one * -30;
	[MinMaxRange(-180,180)]
	public Vector2 roll1 = Vector2.one * 137;
	[MinMaxRange(-180,180)]
	public Vector2 roll2 = Vector2.one * 137;
	[MinMaxRange(-180,180)]
	public Vector2 roll3 = Vector2.one * 137;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 lengthScale1 = Vector2.one * 0.8f;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 lengthScale2 = Vector2.one * 0.8f;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 lengthScale3 = Vector2.one * 0.8f;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 e = Vector2.one * 0.5f;
	[MinMaxRange(0,2)]
	public Vector2 smin = Vector2.one * 0;
	[MinMaxRange(1,3)]
	public Vector2 branchNo = Vector2.one * 2;
	public int iter = 8;

	[Space (6)]

	public bool showLeaves = true;

	[MinMaxRange(0,1)]
	public Vector2 leafMid = Vector2.one * 0.5f;
	[MinMaxRange(0.01f,5)]
	public Vector2 leafLength = Vector2.one * 2;
	[MinMaxRange(0.01f,5)]
	public Vector2 leafWidth = Vector2.one * 2;
	[MinMaxRange(-100,100)]
	public Vector2 leafRotate = Vector2.one * 0;

	[Space (6)]

	[MinMaxRange(-1,1)]
	public float gravity = 0.25f;

	List<Vector3> vertices = new List<Vector3>();
	List<int> indices = new List<int>();
	List<Vector2> uvs = new List<Vector2>();

	List<Vector3> verticesLeaf = new List<Vector3>();
	List<int> indicesLeaf = new List<int>();
	List<Vector2> uvLeafs = new List<Vector2>();

	public int vertCount;
	public int vertLeafCount;

	void Start(){
		seed = GetInstanceID ();
		str = new List<LSElement>();
		UpdateTree ();
	}

	public GameObject BuildGameObject () {
		seed = GetInstanceID ();
		str = new List<LSElement>();

		ResetRandom (seed);
		ExpandRules();
		Mesh meshBranches;
		Mesh meshLeaves;
		Interpret(out meshBranches, out meshLeaves);

		GameObject go = new GameObject ("Tree");
		GameObject goLeaves = new GameObject ("Leaves");
		goLeaves.transform.parent = go.transform;

		go.transform.localScale = Vector3.one * 0.01f;
		go.transform.localEulerAngles = -Vector3.right * 90;

		SetupMeshOnGameObject (go, meshBranches, branchesMat);
		SetupMeshOnGameObject (goLeaves, meshLeaves, leavesMat);
		if (!showLeaves)
			goLeaves.SetActive (false);

		return go;
	}

	void SetupMeshOnGameObject (GameObject go, Mesh mesh, Material material) {
		var filter = go.AddComponent<MeshFilter> ();
		filter.sharedMesh = mesh;
		var renderer = go.AddComponent<MeshRenderer> ();
		renderer.material = material;
	}

	public Mesh[] Build(){
		ResetRandom (seed);		 		
		ExpandRules();		 		
					
		Mesh meshBranches;
		Mesh meshLeaves;
		Interpret(out meshBranches, out meshLeaves);
		return new Mesh[] { meshBranches, meshLeaves };
 	}

	public void UpdateTree(){
		var meshes = Build ();
		var meshFilters = new MeshFilter[]{branches,leaves};
		for (int i = 0; i < meshes.Length; i++) {
			meshFilters[i].mesh = meshes[i];
		}
	}

	void PostprocessMesh (Mesh mesh) {
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();
		var bounds = mesh.bounds;
		bounds.size = bounds.size * 1.5f;
		mesh.bounds = bounds;
	}

	void Update(){
		if (Input.GetKey(KeyCode.R)){
			UpdateTree ();
		}
	}

	public static void ResetRandom(int seed1){
		Random.seed = seed1;
	}

	public float Eval(Vector2 v){
		float val = Runevision.Structures.SimplexNoise.Noise(new Vector3(Random.value*100000,smoothSeed));
		val = val * 0.5f + 0.5f;

		// penalize values around 0.5
		//val = Mathf.SmoothStep(0,1,val);
		return Mathf.Lerp(v.x, v.y, val);
	}

	void ExpandRules(){
		str.Clear();
		str.Add(new LSElement(LSElement.LSSymbol.APEX, Eval(initialLength), Eval(initialWidth)));

		// string debug = "Expanding "+str[0]+" to ";

		Rule r = new Rule(this);
		bool continueLoop = true;
		for (int i=0;continueLoop;i++){
			continueLoop = false;
			List<LSElement> outList = new List<LSElement>();
			bool lastIter = (i == iter-1);
			foreach (var s in str){
				continueLoop |= r.Apply(s, outList, lastIter);
			}
			str = outList;
		}
		/*foreach (var s in str){
			debug += s;
		}
		Debug.Log(debug);*/
	}

	void AddLeaf(Matrix4x4 m, float len, float width, float dist){
		len *= Eval(leafLength);
		width *= Eval(leafWidth);

		var direction = Turtle.QuaternionFromMatrix( m)* (Vector3.forward);
		var rot = Quaternion.LookRotation (direction,Vector3.up);
		var translate = m.GetColumn (3);
		m.SetTRS (Vector3.zero, rot, Vector3.one);
		m.SetColumn (3, translate);

		float leafRot = Eval (leafRotate);
		float w = width;
		float l = len;
		float midFrac = Eval (leafMid);
		float mid = l * midFrac;
		float gravityFactor = 1.0f*gravity;
		Vector3 p0 = m.MultiplyPoint(new Vector3(0, 0, 0));
		Vector2 d0 = Vector2.one * dist;
		var m1 = m * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,leafRot), Vector3.one);
		Vector3 p1 = Turtle.Gravity(m1, gravityFactor*midFrac).MultiplyPoint(Quaternion.Euler(0,0,leafRot) * new Vector3(0, w, mid));
		Vector2 d1 = Vector2.one * (dist + mid);
		Vector3 p2 = Turtle.Gravity(m, gravityFactor).MultiplyPoint(new Vector3(0, 0, l));
		Vector2 d2 = Vector2.one * (dist + l);
		var m2 = m * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,-leafRot), Vector3.one);
		Vector3 p3 = Turtle.Gravity(m2, gravityFactor*midFrac).MultiplyPoint(Quaternion.Euler(0,0,-leafRot)  * new Vector3(0, -w, mid));
		Vector2 d3 = Vector2.one * (dist + mid);
		Vector3 p4 = Turtle.Gravity(m, gravityFactor*0.3f).MultiplyPoint(new Vector3(0, 0, l*0.3f));
		Vector2 d4 = Vector2.one * (dist + l*0.3f);

		int index = verticesLeaf.Count;

		verticesLeaf.Add (p0);// 0
		uvLeafs.Add(d0);
		verticesLeaf.Add (p1);// 1
		uvLeafs.Add(d1);
		verticesLeaf.Add (p4);// 2
		uvLeafs.Add(d4);

		verticesLeaf.Add (p0);// 3
		uvLeafs.Add(d0);
		verticesLeaf.Add (p4);// 4
		uvLeafs.Add(d4);
		verticesLeaf.Add (p3);// 5
		uvLeafs.Add(d3);


		verticesLeaf.Add (p2);// 6
		uvLeafs.Add(d2);

		verticesLeaf.Add (p2);// 7
		uvLeafs.Add(d2);

		for (int j=0;j<6;j++){
			indicesLeaf.Add(index+j);
		}
		indicesLeaf.Add(index+1);
		indicesLeaf.Add(index+6);
		indicesLeaf.Add(index+2);


		indicesLeaf.Add(index+5);
		indicesLeaf.Add(index+4);
		indicesLeaf.Add(index+7);


		// backsides
		index = verticesLeaf.Count;
		verticesLeaf.Add (p0);
		uvLeafs.Add(d0);
		verticesLeaf.Add (p4);
		uvLeafs.Add(d4);
		verticesLeaf.Add (p1);
		uvLeafs.Add(d1);

		verticesLeaf.Add (p0);
		uvLeafs.Add(d0);
		verticesLeaf.Add (p3);
		uvLeafs.Add(d3);
		verticesLeaf.Add (p4);
		uvLeafs.Add(d4);


		verticesLeaf.Add (p2);
		uvLeafs.Add(d2);

		verticesLeaf.Add (p2);
		uvLeafs.Add(d2);

		for (int j=0;j<6;j++){
			indicesLeaf.Add(index+j);
		}
		indicesLeaf.Add(index+1);
		indicesLeaf.Add(index+6);
		indicesLeaf.Add(index+2);


		indicesLeaf.Add(index+5);
		indicesLeaf.Add(index+4);
		indicesLeaf.Add(index+7);
	}

	void AddCone(Matrix4x4 m, float l, float w0, float w1, float dist){
		//float len = Mathf.Sqrt(l*l + Mathf.Sqrt(w0-w1));
		// float a = l/len;
		//float b = (w0-w1)/len;
		const int N = 6;
		int initialIndex = vertices.Count;
		for (int i=0;i<N;i++){
			float alpha = 2.0f*Mathf.PI*i/(float)N;
			Vector3 p0 = m.MultiplyPoint(new Vector3(w0 * Mathf.Cos(alpha), w0 * Mathf.Sin(alpha), 0));
			Vector3 p1 = m.MultiplyPoint(new Vector3(w1 * Mathf.Cos(alpha), w1 * Mathf.Sin(alpha), l));
			Vector3 p2 = m.MultiplyPoint(new Vector3(0.5f*w1 * Mathf.Cos(alpha), 0.5f*w1 * Mathf.Sin(alpha), l+ w1));
			vertices.Add(p0);
			uvs.Add(Vector2.one*dist);
			vertices.Add(p1);
			uvs.Add(Vector2.one*(dist+l));
			vertices.Add(p2);
			uvs.Add(Vector2.one*(dist+l+w1));
		}
		for (int i = 0; i < N; i++) {
			int offset = i * 3;
			indices.Add (initialIndex+offset+1);
			indices.Add (initialIndex+offset);
			indices.Add (initialIndex+(offset+3)%(N*3));

			indices.Add (initialIndex+offset+1);
			indices.Add (initialIndex+(offset+3)%(N*3));
			indices.Add (initialIndex+(offset+4)%(N*3));

			indices.Add (initialIndex+offset+2);
			indices.Add (initialIndex+offset+1);
			indices.Add (initialIndex+(offset+4)%(N*3));

			indices.Add (initialIndex+offset+2);
			indices.Add (initialIndex+(offset+4)%(N*3));
			indices.Add (initialIndex+(offset+5)%(N*3));
		}
	}

	public void Interpret(out Mesh meshBranches, out Mesh meshLeaves){
		Turtle turtle = new Turtle(Eval(initialWidth));

		foreach (var elem in str){
			switch (elem.symbol){
			case LSElement.LSSymbol.LEAF:
				AddLeaf (turtle.Peek().M,elem.data [0], elem.data [1], turtle.GetDist()); 
				break;
			case LSElement.LSSymbol.DRAW:
				float movDist = elem.data [0];
				AddCone(turtle.Peek().M, movDist, turtle.GetWidth(), turtle.GetWidth() * elem.data[1], turtle.GetDist());
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
			case LSElement.LSSymbol.GRAVITY:
				turtle.Gravity(elem.data[0]);
				break;
			}
		}

		float max = 0;
		foreach (var u in uvLeafs) {
			max = Mathf.Max (u.x, max);
		}
		for (int i = 0; i < uvs.Count; i++) {
			uvs [i] = new Vector2( uvs [i].x/max,0);
		}
		for (int i = 0; i < uvLeafs.Count; i++) {
			uvLeafs [i] = new Vector2( uvLeafs[i].x/max ,0);
		}

		meshBranches = new Mesh();
		if (vertices.Count >= 65536) {
			Debug.LogError ("Tree - too many verts: "+vertices.Count);
		} else {
			vertCount = vertices.Count;
			meshBranches.vertices = vertices.ToArray ();
			meshBranches.triangles = indices.ToArray ();
			meshBranches.uv = uvs.ToArray ();
			PostprocessMesh (meshBranches);
			//Debug.Log ("vertices "+vertices.Count);
		}
		uvs.Clear();
		vertices.Clear();
		indices.Clear();

		meshLeaves = new Mesh();
		if (verticesLeaf.Count >= 65536) {
			Debug.LogError ("Tree leaves - too many verts: "+verticesLeaf.Count);
		} else {
			vertLeafCount = verticesLeaf.Count;
			meshLeaves.vertices = verticesLeaf.ToArray ();
			meshLeaves.triangles = indicesLeaf.ToArray ();
			meshLeaves.uv = uvLeafs.ToArray ();
			PostprocessMesh (meshLeaves);
		}
		uvLeafs.Clear();
		verticesLeaf.Clear();
		indicesLeaf.Clear();
	}

	void OnGUI(){
		if (GUI.Button(new Rect(0,0,50,30), "Fig.a")){
			lengthScale1 = Vector2.one*0.75f;
			lengthScale2 = Vector2.one*0.77f;
			turn1 = Vector2.one*35;
			turn2 = Vector2.one*-35;
			roll1 = Vector2.one*0;
			roll2 = Vector2.one*0;
			initialWidth = new Vector2(30,30);
			e = Vector2.one*0.4f;
			smin = Vector2.one*0;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(50,0,50,30), "Fig.b")){
			lengthScale1 = Vector2.one*0.65f;
			lengthScale2 = Vector2.one*0.71f;
			turn1 = Vector2.one*27;
			turn2 = Vector2.one*-68;
			roll1 = Vector2.one*0;
			roll2 = Vector2.one*0;
			initialWidth = new Vector2(20,20);
			e = Vector2.one*0.5f;
			smin = Vector2.one*1.7f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(100,0,50,30), "Fig.c")){
			lengthScale1 = Vector2.one*0.5f;
			lengthScale2 = Vector2.one*0.85f;
			turn1 = Vector2.one*25;
			turn2 = Vector2.one*-15;
			roll1 = Vector2.one*180;
			roll2 = Vector2.one*0;
			initialWidth = new Vector2(20,20);
			e = Vector2.one*0.5f;
			smin = Vector2.one*0.5f;
			iter = 9;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(150,0,50,30), "Fig.d")){
			lengthScale1 = Vector2.one*0.6f;
			lengthScale2 = Vector2.one*0.85f;
			turn1 = Vector2.one*25;
			turn2 = Vector2.one*-15;
			roll1 = Vector2.one*180;
			roll2 = Vector2.one*180;
			initialWidth = new Vector2(20,20);
			e = Vector2.one*0.5f;
			smin = Vector2.one*0.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(200,0,50,30), "Fig.e")){
			lengthScale1 = Vector2.one*0.58f;
			lengthScale2 = Vector2.one*0.83f;
			turn1 = Vector2.one*30;
			turn2 = Vector2.one*15;
			roll1 = Vector2.one*0;
			roll2 = Vector2.one*180;
			initialWidth = new Vector2(20,20);
			e = Vector2.one*0.5f;
			smin = Vector2.one*1.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(250,0,50,30), "Fig.f")){
			lengthScale1 = Vector2.one*0.92f;
			lengthScale2 = Vector2.one*0.37f;
			turn1 = Vector2.one*0;
			turn2 = Vector2.one*60;
			roll1 = Vector2.one*180;
			roll2 = Vector2.one*0;
			initialWidth = new Vector2(2,2);
			e = Vector2.one*0.0f;
			smin = Vector2.one*0.5f;
			iter = 12;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(300,0,50,30), "Fig.g")){
			lengthScale1 = Vector2.one*0.80f;
			lengthScale2 = Vector2.one*0.80f;
			turn1 = Vector2.one*30;
			turn2 = Vector2.one*-30;
			roll1 = Vector2.one*137;
			roll2 = Vector2.one*137;
			initialWidth = new Vector2(30,30);
			e = Vector2.one*0.5f;
			smin = Vector2.one*0.0f;
			iter = 10;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(350,0,50,30), "Fig.h")){
			lengthScale1 = Vector2.one*0.95f;
			lengthScale2 = Vector2.one*0.75f;
			turn1 = Vector2.one*5;
			turn2 = Vector2.one*-30;
			roll1 = Vector2.one*-90;
			roll2 = Vector2.one*90;
			initialWidth = new Vector2(40,40);
			e = Vector2.one*0.45f;
			smin = Vector2.one*25.0f;
			iter = 12;
			UpdateTree ();
		}
		if (GUI.Button(new Rect(400,0,50,30), "Fig.i")){
			lengthScale1 = Vector2.one*0.55f;
			lengthScale2 = Vector2.one*0.95f;
			turn1 = Vector2.one*-5;
			turn2 = Vector2.one*30;
			roll1 = Vector2.one*137;
			roll2 = Vector2.one*137;
			initialWidth = new Vector2(5,5);
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
