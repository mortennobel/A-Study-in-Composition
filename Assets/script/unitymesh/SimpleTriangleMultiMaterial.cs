using UnityEngine;
using System.Collections;

public class SimpleTriangleMultiMaterial : MonoBehaviour {
	public Transform target1;
	public Transform target2;
	public Transform target3;
	public Transform target4;
	Mesh mesh;

	void Start () {
		mesh = new Mesh();
	}
	
	void Update () {
		mesh.vertices = new Vector3[]{target1.position,target2.position,target3.position,target4.position};
		mesh.normals = new Vector3[]{Vector3.up,Vector3.up,Vector3.up,Vector3.up};
		mesh.uv = new Vector2[]{Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero};
		mesh.subMeshCount = 2;
		mesh.SetIndices(new int[]{0,1,2},MeshTopology.Triangles,0);
		mesh.SetIndices(new int[]{0,1,3},MeshTopology.Triangles,1);
		GetComponent<MeshFilter>().mesh = mesh;
	}

	void OnGUI(){
		GUI.Label(new Rect(0,0,Screen.width,50), " Move handles in scene editor to change shape.");
	}
}
