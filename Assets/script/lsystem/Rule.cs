using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rule {
	public float alpha1;
	public float alpha2;
	public float phi1;
	public float phi2;
	public float r1;
	public float r2;
	public float q;
	public float e;
	public float smin;


	public Rule (float alpha1, float alpha2, float phi1, float phi2, float r1, float r2, float q, float e, float smin)
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
	}
	

	public bool Apply(LSElement elem, List<LSElement> outList, bool lastIter){
		if (elem.symbol == LSElement.LSSymbol.APEX){
			float length = elem.data[0];
			float width = elem.data[1];
			if (length >= smin) {
#if SOLUTION_CODE
				outList.Add (new LSElement (LSElement.LSSymbol.WIDTH, width));
				float endScale = Mathf.Max (Mathf.Pow (q, e), Mathf.Pow (1.0f - q, e)); // extension to give a better looking result
				outList.Add (new LSElement (LSElement.LSSymbol.DRAW, length, endScale));
				outList.Add (new LSElement (LSElement.LSSymbol.PUSH_STATE));
				outList.Add (new LSElement (LSElement.LSSymbol.TURN, alpha1));
				outList.Add (new LSElement (LSElement.LSSymbol.ROLL, phi1));
				outList.Add (new LSElement (LSElement.LSSymbol.APEX, length * r1, width * Mathf.Pow (q, e)));
				if (lastIter){
					outList.Add (new LSElement (LSElement.LSSymbol.LEAF, length, width));
				}
				outList.Add (new LSElement (LSElement.LSSymbol.POP_STATE));
				outList.Add (new LSElement (LSElement.LSSymbol.PUSH_STATE));
				outList.Add (new LSElement (LSElement.LSSymbol.TURN, alpha2));
				outList.Add (new LSElement (LSElement.LSSymbol.ROLL, phi2));
				outList.Add (new LSElement (LSElement.LSSymbol.APEX, length * r2, width * Mathf.Pow (1.0f - q, e)));
				if (lastIter){
					outList.Add (new LSElement (LSElement.LSSymbol.LEAF, length, width));
				}
				outList.Add (new LSElement (LSElement.LSSymbol.POP_STATE));
#else
				outList.Add(new LSElement(LSElement.LSSymbol.DRAW, length, 1));
				outList.Add(new LSElement(LSElement.LSSymbol.A, length*r1, width*Mathf.Pow(q,e)));
#endif

			} else {
				outList.Add (new LSElement (LSElement.LSSymbol.LEAF, length, width));
			}
		} else {
			outList.Add(elem);
		}
		return false;
	}
}
