using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Face : System.IEquatable<Face> {
	public Halfedge halfedge;
	HMesh hmesh;

	public Face(HMesh hmesh){
		this.hmesh = hmesh;
	}

	public int NoEdges(){
		int count = 1;
		Halfedge current = halfedge.next;
		while (current != halfedge){
			count++;
			current = current.next;
		}
		return count;
	}

	public List<Halfedge> Circulate(){
		List<Halfedge> res = new List<Halfedge>();
		Halfedge iter = halfedge;
		bool first = true;
		while (iter != halfedge || first){
			res.Add(iter);
			first = false;
			iter = iter.next;
		}
		return res;
	}

	public void ReassignFaceToEdgeLoop(){
		foreach (var he in Circulate()){
			he.Link(this);
		}
	}
	
	// split the face in the center
	// return the new Vertex
	public Vertex Split(){
		Vertex v = hmesh.CreateVertex();

		List<Halfedge> newHalfedges = new List<Halfedge>();

		foreach (var heIter in Circulate()){
			v.position += heIter.vert.position;
			v.uv1 += heIter.vert.uv1;
			v.uv2 += heIter.vert.uv2;

			Halfedge toNewVertex = hmesh.CreateHalfedge();
			Halfedge fromNewVertex = hmesh.CreateHalfedge();
			toNewVertex.Glue(fromNewVertex);
			newHalfedges.Add(toNewVertex);
		}
		int count = 0;

		bool first = true;
		// second iteration - link everything together
		foreach (var heIter in Circulate()){
			Halfedge prevNewEdge = newHalfedges[(count-1+newHalfedges.Count)%newHalfedges.Count];
			Halfedge newEdge = newHalfedges[count];
			// link halfedges
			newEdge.opp.Link(heIter.next);
			heIter.Link(newEdge);
			newEdge.Link(prevNewEdge.opp);

			// link vertices
			newEdge.opp.Link(heIter.vert);
			newEdge.Link(v);

			if (!first){
				Face newFace = hmesh.CreateFace();
				newFace.halfedge = heIter;
				newFace.ReassignFaceToEdgeLoop();
			}

			Halfedge toNewVertex = hmesh.CreateHalfedge();
			Halfedge fromNewVertex = hmesh.CreateHalfedge();
			toNewVertex.Glue(fromNewVertex);
			newHalfedges.Add(toNewVertex);
			
			first = false;
			count++;
		}


		v.position = v.position * (1.0f/count); // set average position
		v.uv1 = v.uv1 * (1.0f/count);
		v.uv2 = v.uv2 * (1.0f/count);

		return v;
	}

	bool System.IEquatable<Face>.Equals(Face obj){
		return obj == this;
	}

	public override bool Equals (object obj)
	{
		return this == obj;
	}
	
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}

	public bool IsValid(){
		bool valid = true;
		if (halfedge == null){
			Debug.LogWarning("Halfedge is null");
			valid = false;
		}
		foreach (var he in Circulate()){
			if (he.face != this){
				Debug.LogWarning("Halfedge.face is not correct");
				valid = false;
			}
		}
		return valid;
	}
}
