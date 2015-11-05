using UnityEngine;
using System.Collections;

public class Halfedge  : System.IEquatable<Halfedge>  {
	public Halfedge next;
	public Halfedge prev;
	public Halfedge opp;
	public Vertex vert;
	public Face face;

	HMesh hmesh;

	public Halfedge(HMesh hmesh){
		this.hmesh = hmesh;
	}

	public Vertex Collapse(bool center = false){
#if SOLUTION_CODE
		if (center){
			vert.position = (vert.position + prev.vert.position)*0.5f;
			vert.uv1 = (vert.uv1 + prev.vert.uv1)*0.5f;
			vert.uv2 = (vert.uv2 + prev.vert.uv2)*0.5f;
		}
		CollapseInternal(false);
		if (opp != null){
			opp.CollapseInternal(true);
			hmesh.Destroy(opp);
		}
		hmesh.Destroy(this);

		return vert;
#else
		Debug.Log("Implement Collapse");
		return null;
#endif
	}

#if SOLUTION_CODE
	void CollapseInternal(bool opp){

		if (!opp) {
			prev.vert.ReplaceVertex(vert);
			hmesh.Destroy(prev.vert);
		} else {
			vert.ReplaceVertex(prev.vert);
			hmesh.Destroy(vert);
		}
		prev.opp.Glue(next.opp);
		foreach (var he in face.Circulate()){
			hmesh.Destroy(he);
		}
		hmesh.Destroy(face);
	}
#endif

	public void Flip(){
#if SOLUTION_CODE
		if (IsBoundary()){
			Debug.LogError("Cannot flip boundary edge");
			return;
		}
		if (face.NoEdges() != 3 || opp.face.NoEdges() != 3){
			Debug.LogError("Can only flip edge between two triangles");
		}
		Halfedge oldNext = next;
		Halfedge oldPrev = prev;
		Halfedge oldOppNext = opp.next;
		Halfedge oldOppPrev = opp.prev;

		Vertex thisVert = vert;
		Vertex oppVert = opp.vert;
		Vertex thisOppositeVert = next.vert;
		Vertex oppOppositeVert = opp.next.vert;

		// flip halfedges
		this.Link(oldPrev);
		oldNext.Link(opp);
		opp.Link(oldOppPrev);
		oldOppNext.Link(this);

		oldOppPrev.Link(oldNext);
		oldPrev.Link (oldOppNext);

		// reassign vertices
		this.Link(thisOppositeVert);
		opp.Link(oppOppositeVert);

		face.ReassignFaceToEdgeLoop();
		opp.face.ReassignFaceToEdgeLoop();
#else 
		Debug.Log("Implement this");
#endif
	}

	public bool IsBoundary(){
		return opp==null;
	}

	public void Link(Halfedge nextEdge){
		if (this == nextEdge){
			Debug.LogWarning("Link of self");
		}
		next = nextEdge;
		nextEdge.prev = this;
	}

	public void Link(Face face){
		this.face = face;
		face.halfedge = this;
	}

	public void Link(Vertex vertex){
		if (next == null){
			Debug.LogWarning("next pointer is null");
		}
		vertex.halfedge = next;
		vert = vertex;
	}

	public Vertex Split(){
#if SOLUTION_CODE
		Vertex vertex = hmesh.CreateVertex();
		vertex.position = (vert.position+prev.vert.position)*0.5f;
		vertex.uv1 = (vert.uv1+prev.vert.uv1)*0.5f;
		vertex.uv2 = (vert.uv2+prev.vert.uv2)*0.5f;
		var newHE = SplitInternal(vertex);
		if (opp != null){
			var newOppHE = opp.SplitInternal(vertex);
			newHE.Glue(opp);
			this.Glue(newOppHE);
			newOppHE.IsValid();
		}
		newHE.IsValid();
		return vertex;
#else
		Debug.Log("Implement this");
		return null;
#endif
	}

	// glue two halfedges together
	public void Glue(Halfedge oppEdge){
		if (oppEdge == this){
			Debug.LogWarning("Glue to self");
		}
		opp = oppEdge;
		oppEdge.opp = this;
	}

	public override bool Equals (object obj)
	{
		return this == obj;
	}

	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}

	bool System.IEquatable<Halfedge>.Equals(Halfedge obj){
		return obj == this;
	}

#if SOLUTION_CODE
	Halfedge SplitInternal(Vertex vertex){
		if (face.NoEdges() != 3){
			Debug.LogError("Can only Split edge between two triangles");
		}
		Halfedge oldNext = next;
		Halfedge oldPrev = prev;
		
		Vertex oppositeVert = next.vert;
		
		Halfedge newPrev = hmesh.CreateHalfedge();
		Halfedge splitEdge1 = hmesh.CreateHalfedge();
		Halfedge splitEdge2 = hmesh.CreateHalfedge();
		splitEdge1.Glue(splitEdge2);

		// Link halfedges
		oldPrev.Link(newPrev);
		newPrev.Link(splitEdge1);
		splitEdge1.Link(oldPrev);

		oldNext.Link(splitEdge2);
		splitEdge2.Link(this);

		// Link vertices
		splitEdge1.Link (oppositeVert);
		splitEdge2.Link (vertex);
		newPrev.Link(vertex);
		oldPrev.Link(oldPrev.vert); // set correct vertex link

		Link(face);
		face.ReassignFaceToEdgeLoop();
		Face newFace = hmesh.CreateFace();
		newPrev.Link(newFace);
		newFace.ReassignFaceToEdgeLoop();

		face.IsValid();
		newFace.IsValid();
		oppositeVert.IsValid();
		vertex.IsValid();
		splitEdge1.IsValid();
		splitEdge2.IsValid();

		return newPrev;
	}
#endif

	public bool IsValid(){
		bool valid = true;
		if (opp != null && opp.opp != this){
			Debug.LogWarning("opp is different from this or null");
			valid = false;
		}
		if (prev.next != this){
			Debug.LogWarning("prev.next is different from this");
			valid = false;
		}
		if (next.prev != this){
			Debug.LogWarning("next.prev is different from this");
			valid = false;
		}

		return valid;
	}
}
