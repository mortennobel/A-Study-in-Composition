using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MengerSponge : MonoBehaviour {

	#if SOLUTION_CODE
		
		private List<Vector3> vertices = new List<Vector3>();
		private List<Vector3> normals = new List<Vector3>();
		private List<int> indices = new List<int>();
		
		
		private void addTriangle(Vector3 p1, Vector3 p2, Vector3 p3){
			vertices.Add(p1);
			vertices.Add(p2);
			vertices.Add(p3);
			indices.Add(indices.Count);
			indices.Add(indices.Count);
			indices.Add(indices.Count);
		}
		
		private void addQuad(Vector3 p3, Vector3 p2, Vector3 p1, Vector3 p4){
			// add first triangle
			int p1Index = vertices.Count;
			indices.Add(vertices.Count);
			vertices.Add(p1);
			
			indices.Add(vertices.Count);
			vertices.Add(p2);
			int p3Index = vertices.Count;
			indices.Add(vertices.Count);
			vertices.Add(p3);
			
			// Add second triangle
			indices.Add(p1Index); // reuse vertex from triangle above
			indices.Add(p3Index); // reuse vertex from triangle above
			indices.Add(vertices.Count);
			vertices.Add(p4);

			Vector3 normal = Vector3.Cross(p3-p1, p1-p2).normalized;
			normals.Add(normal);
			normals.Add(normal);
			normals.Add(normal);
			normals.Add(normal);
		}
		
		private void addCube(Vector3 pStart, Vector3 pEnd){
			float length = pEnd.x-pStart.x;
			Vector3 p1 = pStart;
			Vector3 p2 = pStart+Vector3.right*length;
			Vector3 p3 = pStart+Vector3.forward*length+Vector3.right*length;
			Vector3 p4 = pStart+Vector3.forward*length;
			
			Vector3 p5 = pEnd-Vector3.forward*length-Vector3.right*length;
			Vector3 p6 = pEnd-Vector3.forward*length;
			Vector3 p7 = pEnd;
			Vector3 p8 = pEnd-Vector3.right*length;
			
			addQuad(p1,p4,p3,p2);
			addQuad(p7,p6,p2,p3);
			addQuad(p7,p3,p4,p8);
			addQuad(p1,p5,p8,p4);
			addQuad(p1,p2,p6,p5);
			addQuad(p7,p8,p5,p6);
			return;
		}
		
		private void generateMengerSponge(Vector3 pStart, Vector3 pEnd, int depth){
			if (depth == 0){
				addCube(pStart, pEnd);
				return;
			}
			depth --;
			float length = pEnd.x-pStart.x;
			
			Vector3 endOffset = Vector3.one*length/3.0f;
			
			for (int x=0;x<3;x++){
				for (int y=0;y<3;y++){
					for (int z=0;z<3;z++){
						if ((x==1&&y==1) || (x==1 && z==1) || (y==1 && z==1)){
							continue;
						}
						Vector3 newStart = pStart+Vector3.right*length*(x/3.0f)+Vector3.forward*length*(z/3.0f)+Vector3.up*length*(y/3.0f);
						generateMengerSponge(newStart, newStart+endOffset, depth);
					}
				}
			}
			
		}
#endif
	public int subdivisions;

	Mesh CreateMengerSponge(int subdivisions){
#if SOLUTION_CODE
		vertices.Clear();
		indices.Clear();
		generateMengerSponge(Vector3.zero, Vector3.one, subdivisions);
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.normals = normals.ToArray();
		mesh.RecalculateBounds();
		mesh.uv = new Vector2[vertices.Count];

		mesh.SetIndices(indices.ToArray(),MeshTopology.Triangles,0);

#else
		// todo implement
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[]{Vector3.zero,Vector3.up,Vector3.right};
		mesh.normals = new Vector3[]{Vector3.up,Vector3.up,Vector3.up};
		mesh.uv = new Vector2[]{Vector2.zero,Vector2.zero,Vector2.zero};
		
		mesh.SetIndices(new int[]{0,1,2},MeshTopology.Triangles,0);
#endif
		return mesh;

	}

	void Start () {
		GetComponent<MeshFilter>().mesh = CreateMengerSponge(subdivisions);
	}
}
