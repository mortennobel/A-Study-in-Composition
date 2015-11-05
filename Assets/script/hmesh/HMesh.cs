using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HMesh {

	HashSet<Vertex> vertices = new HashSet<Vertex>();
	HashSet<Face> faces = new HashSet<Face>();
	HashSet<Halfedge> halfedges = new HashSet<Halfedge>();

	public HashSet<Vertex> GetVertices(){
		return new HashSet<Vertex>(vertices);
	}

	public HashSet<Face> GetFaces(){
		return new HashSet<Face>(faces);
	}

	public HashSet<Halfedge> GetHalfedges(){
		return new HashSet<Halfedge>(halfedges);
	}

	public HMesh(){
	}

	public void Build(Mesh mesh){
		if (mesh.subMeshCount != 1){
			Debug.LogError("Invalid mesh.subMeshCount. Must be 1.");
		}
		if (mesh.GetTopology(0) != MeshTopology.Triangles){
			Debug.LogError("Only triangles supported.");
		}
		List<Vertex> vertexList = new List<Vertex>();
		Dictionary<IntPair, Halfedge> halfedgeByVertexID = new Dictionary<IntPair, Halfedge>();
		for (int i=0;i<mesh.vertices.Length;i++){
			var newV = CreateVertex();
			newV.position = mesh.vertices[i];
			if (mesh.uv != null && mesh.uv.Length>i){
				newV.uv1 = mesh.uv[i];
			}
			if (mesh.uv2 != null && mesh.uv2.Length>i){
				newV.uv2 = mesh.uv2[i];
			}
			vertexList.Add(newV);

		}
		for (int i=0;i<mesh.triangles.Length;i+=3){
			int[] idx = new int[]{
				mesh.triangles[i],
				mesh.triangles[i+1],
				mesh.triangles[i+2]};
			Halfedge[] edges = new Halfedge[3];
			Face face = CreateFace();
			for (int j=0;j<3;j++){
				Halfedge edge = CreateHalfedge();
				edge.Link(face);
				edges[j] = edge;
			}
			for (int j=0;j<3;j++){
				int from = idx[j];
				int to = idx[(j+1)%3];
				edges[j].Link(edges[(j+1)%3]);
				edges[j].Link(vertexList[to]);
				halfedgeByVertexID.Add(new IntPair(from, to), edges[j]);
			}
		}

		// glue all opposite half edges
		foreach (var keyValue in halfedgeByVertexID){
			if (keyValue.Key.first < keyValue.Key.second){
				continue; // skip half of the halfedges (this avoids unneeded glue)
			}
			var otherKey = new IntPair(keyValue.Key.second, keyValue.Key.first);
			Halfedge otherEdge;
			if (halfedgeByVertexID.TryGetValue(otherKey, out otherEdge)){
				keyValue.Value.Glue(otherEdge);
			}
		}
	}

	public Mesh Export(){
		List<Vertex> vertexList = new List<Vertex>(vertices);
		Mesh res = new Mesh();
		Vector3[] vertexArray = new Vector3[vertexList.Count];
		Vector2[] uv1 = new Vector2[vertexList.Count];
		Vector2[] uv2 = new Vector2[vertexList.Count];
		for (int i=0;i<vertexArray.Length;i++){
			vertexArray[i] = vertexList[i].position;
			uv1[i] = vertexList[i].uv1;
			uv2[i] = vertexList[i].uv2;
		}
		res.vertices = vertexArray;
		res.uv = uv1;
		res.uv2 = uv2;
		List<int> triangles = new List<int>();
		foreach (var face in faces){
			if (face.NoEdges() != 3){
				Debug.LogError("Only triangles supported");
				continue;
			}
			var he = face.halfedge;
			bool first = true;
			while (he != face.halfedge || first){
				int indexOfVertex = vertexList.IndexOf(he.vert);
				triangles.Add(indexOfVertex);
				he = he.next;
				first = false;
			}
		}

		string s = "";
		foreach (var i in triangles.ToArray()){
			s+= i+", ";
		}
		Debug.Log("Exporting triangles "+s+" count "+triangles.Count);

		Debug.Log("Vertices "+vertexList.Count);
		res.SetTriangles(triangles.ToArray(),0);
		return res;
	}

	public void Clear(){
		foreach (var v in new List<Vertex>(vertices)){
			Destroy(v);
		}
		foreach (var f in new List<Face>(faces)){
			Destroy(f);
		}
		foreach (var h in new List<Halfedge>(halfedges)){
			Destroy(h);
		}
	}

	public Vertex CreateVertex(){
		var res = new Vertex(this);
		vertices.Add(res);
		return res;
	}

	public Face CreateFace(){
		var res = new Face(this);
		faces.Add(res);
		return res;
	}

	public Halfedge CreateHalfedge(){
		var res = new Halfedge(this);
		halfedges.Add(res);
		return res;
	}

	public bool Destroy(Face face){
		bool res = faces.Remove(face);
		if (res){
			face.halfedge = null;
		}
		return res;
	}

	public bool Destroy(Halfedge halfedge){
		bool res = halfedges.Remove(halfedge);
		if (res){
			halfedge.face = null;
			halfedge.next = null;
			halfedge.opp = null;
			halfedge.prev = null;
			halfedge.vert = null;
		}
		return res;
	}

	public bool Destroy(Vertex vertex){
		bool res = vertices.Remove(vertex);
		if (res){
			vertex.halfedge = null;
			vertex.position = new Vector3(Mathf.Infinity,Mathf.Infinity,Mathf.Infinity);
		}
		return res;
	}

}
