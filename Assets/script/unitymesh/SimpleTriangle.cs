using UnityEngine;
using System.Collections;

public class SimpleTriangle : MonoBehaviour {
	public Transform target1;
	public Transform target2;
	public Transform target3;
	Mesh mesh;

	void Start () {
		mesh = new Mesh();
	}
	
	void Update () {
		mesh.vertices = new Vector3[]{target1.position,target2.position,target3.position, target1.position, -target3.position, target2.position};
		mesh.normals = new Vector3[]{Vector3.up,Vector3.up,Vector3.up};
		mesh.uv = new Vector2[]{Vector2.zero,Vector2.zero,Vector2.zero};
		mesh.SetIndices(new int[]{0,1,2},MeshTopology.Triangles,0);
		GetComponent<MeshFilter>().mesh = mesh;
	}
}
