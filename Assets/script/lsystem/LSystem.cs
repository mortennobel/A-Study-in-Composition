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
	public Vector2 q = Vector2.one * 0.5f;
	[MinMaxRange(0.1f,1.0f)]
	public Vector2 e = Vector2.one * 0.5f;
	[MinMaxRange(0,2)]
	public Vector2 smin = Vector2.one * 0;
	[MinMaxRange(1,3)]
	public Vector2 branchNo = Vector2.one * 2;
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

	void UpdateTree(){
		var meshes = Build ();
		var meshFilters = new MeshFilter[]{branches,leaves};
		for (int i = 0; i < meshes.Length; i++) {
			meshFilters[i].mesh = meshes[i];
		}
	}

	void PostprocessMesh (Mesh mesh) {
		mesh.RecalculateNormals ();
		mesh.uv = new Vector2[mesh.vertexCount];
		mesh.RecalculateBounds ();
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

		// penalize values around 0.5
		//val = Mathf.SmoothStep(0,1,val);
		return Mathf.Lerp(v.x, v.y, val);
	}

	void ExpandRules(){
		str.Clear();
		str.Add(new LSElement(LSElement.LSSymbol.APEX, Eval(initialLength), Eval(initialWidth)));

		// string debug = "Expanding "+str[0]+" to ";

		Rule r = new Rule(turn1, turn2, turn3, roll1, roll2, roll3, lengthScale1, lengthScale2, lengthScale3,  q, e, smin, branchNo, this);
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

		verticesLeaf.Add (p0);
		verticesLeaf.Add (p2);
		verticesLeaf.Add (p1);


		verticesLeaf.Add (p0);
		verticesLeaf.Add (p3);
		verticesLeaf.Add (p2);



		for (int j=0;j<12;j++){
			indicesLeaf.Add(indicesLeaf.Count);
		}
	}

	void AddCone(Matrix4x4 m, float l, float w0, float w1){
		//float len = Mathf.Sqrt(l*l + Mathf.Sqrt(w0-w1));
		// float a = l/len;
		//float b = (w0-w1)/len;
		const int N = 6;
		int initialIndex = vertices.Count;
		for (int i=0;i<N;i++){
			float alpha = 2.0f*Mathf.PI*i/(float)N;
			Vector3 p0 = m.MultiplyPoint(new Vector3(w0 * Mathf.Cos(alpha), w0 * Mathf.Sin(alpha), 0));
			Vector3 p1 = m.MultiplyPoint(new Vector3(w1 * Mathf.Cos(alpha), w1 * Mathf.Sin(alpha), l));
			vertices.Add(p0);
			vertices.Add(p1);
		}
		for (int i = 0; i < N; i++) {
			int offset = i * 2;
			indices.Add (initialIndex+offset+1);
			indices.Add (initialIndex+offset);

			indices.Add (initialIndex+(offset+2)%(N*2));

			indices.Add (initialIndex+offset+1);
			indices.Add (initialIndex+(offset+2)%(N*2));
			indices.Add (initialIndex+(offset+3)%(N*2));

		}
	}

	public void Interpret(out Mesh meshBranches, out Mesh meshLeaves){
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

		meshBranches = new Mesh();
		if (vertices.Count >= 65536) {
			Debug.LogError ("Tree - too many verts: "+vertices.Count);
		} else {
			meshBranches.vertices = vertices.ToArray ();
			meshBranches.triangles = indices.ToArray ();
			PostprocessMesh (meshBranches);
		}
		vertices.Clear();
		indices.Clear();

		meshLeaves = new Mesh();
		if (vertices.Count >= 65536) {
			Debug.LogError ("Tree leaves - too many verts: "+vertices.Count);
		} else {
			meshLeaves.vertices = verticesLeaf.ToArray ();
			meshLeaves.triangles = indicesLeaf.ToArray ();
			PostprocessMesh (meshLeaves);
		}

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
			q = Vector2.one*0.5f;
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
			q = Vector2.one*0.53f;
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
			q = Vector2.one*0.45f;
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
			q = Vector2.one*0.45f;
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
			q = Vector2.one*0.40f;
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
			q = Vector2.one*0.50f;
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
			q = Vector2.one*0.50f;
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
			q = Vector2.one*0.60f;
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
