using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rule {
	public Vector2 alpha1;
	public Vector2 alpha2;
	public Vector2 phi1;
	public Vector2 phi2;
	public Vector2 r1;
	public Vector2 r2;
	public Vector2 q;
	public Vector2 e;
	public Vector2 smin;
	LSystem lsys;


	public Rule (Vector2 alpha1, Vector2 alpha2, Vector2 phi1, Vector2 phi2, Vector2 r1, Vector2 r2, Vector2 q, Vector2 e, Vector2 smin, LSystem lsys)
	{
		this.alpha1 = alpha1;
		this.alpha2 = alpha2;
		this.phi1 = phi1;
		this.phi2 = phi2;
		this.r1 = r1;
		this.r2 = r2;
		this.q = q;
		this.e = e;
		this.smin = smin;
		this.lsys = lsys;
	}
	

	public bool Apply(LSElement elem, List<LSElement> outList, bool lastIter){
		if (elem.symbol == LSElement.LSSymbol.APEX){
			float length = elem.data[0];
			float width = elem.data[1];
			if (length >= lsys.Eval(smin)) {
				outList.Add (new LSElement (LSElement.LSSymbol.WIDTH, width));
				float e = lsys.Eval (this.e);
				float q = lsys.Eval (this.q);
				float r1 = lsys.Eval (this.r1);
				float r2 = lsys.Eval (this.r2);
				float endScale = Mathf.Max (Mathf.Pow (q, e), Mathf.Pow (1.0f - q, e)); // extension to give a better looking result
				outList.Add (new LSElement (LSElement.LSSymbol.DRAW, length, endScale));
				outList.Add (new LSElement (LSElement.LSSymbol.PUSH_STATE));
				outList.Add (new LSElement (LSElement.LSSymbol.TURN, lsys.Eval(alpha1)));
				outList.Add (new LSElement (LSElement.LSSymbol.ROLL, lsys.Eval(phi1)));
				outList.Add (new LSElement (LSElement.LSSymbol.APEX, length * r1, width * Mathf.Pow (q, e)));
				if (lastIter){
					outList.Add (new LSElement (LSElement.LSSymbol.LEAF, length, width));
				}
				outList.Add (new LSElement (LSElement.LSSymbol.POP_STATE));
				outList.Add (new LSElement (LSElement.LSSymbol.PUSH_STATE));
				outList.Add (new LSElement (LSElement.LSSymbol.TURN, lsys.Eval(alpha2)));
				outList.Add (new LSElement (LSElement.LSSymbol.ROLL, lsys.Eval(phi2)));
				outList.Add (new LSElement (LSElement.LSSymbol.APEX, length * r2, width * Mathf.Pow (1.0f - q, e)));
				if (lastIter){
					outList.Add (new LSElement (LSElement.LSSymbol.LEAF, length, width));
				}
				outList.Add (new LSElement (LSElement.LSSymbol.POP_STATE));
			} else {
				outList.Add (new LSElement (LSElement.LSSymbol.LEAF, length, width));
			}
		} else {
			outList.Add(elem);
		}
		return false;
	}
}
