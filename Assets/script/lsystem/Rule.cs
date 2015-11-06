using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rule {
	public Vector2 turn1;
	public Vector2 turn2;
	public Vector2 roll1;
	public Vector2 roll2;
	public Vector2 r1;
	public Vector2 r2;
	public Vector2 q;
	public Vector2 e;
	public Vector2 smin;
	LSystem lsys;


	public Rule (Vector2 turn1, Vector2 turn2, Vector2 roll1, Vector2 roll2, Vector2 endRadius1, Vector2 r2, Vector2 q, Vector2 e, Vector2 smin, LSystem lsys)
	{
		this.turn1 = turn1;
		this.turn2 = turn2;
		this.roll1 = roll1;
		this.roll2 = roll2;
		this.r1 = endRadius1;
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
				outList.Add (new LSElement (LSElement.LSSymbol.TURN, lsys.Eval(turn1)));
				outList.Add (new LSElement (LSElement.LSSymbol.ROLL, lsys.Eval(roll1)));

				if (lastIter) {
					outList.Add (new LSElement (LSElement.LSSymbol.LEAF_ROD, length, width));
				} else {
					outList.Add (new LSElement (LSElement.LSSymbol.APEX, length * r1, width * Mathf.Pow (q, e)));	
				}
				outList.Add (new LSElement (LSElement.LSSymbol.POP_STATE));
				outList.Add (new LSElement (LSElement.LSSymbol.PUSH_STATE));
				outList.Add (new LSElement (LSElement.LSSymbol.TURN, lsys.Eval(turn2)));
				outList.Add (new LSElement (LSElement.LSSymbol.ROLL, lsys.Eval(roll2)));

				if (lastIter) {
					outList.Add (new LSElement (LSElement.LSSymbol.LEAF_ROD, length, width));
				} else {
					outList.Add (new LSElement (LSElement.LSSymbol.APEX, length * r2, width * Mathf.Pow (1.0f - q, e)));
				}
				outList.Add (new LSElement (LSElement.LSSymbol.POP_STATE));
			} else {
				outList.Add (new LSElement (LSElement.LSSymbol.LEAF_ROD, length, width));
			}
			return true;
		} else if (elem.symbol == LSElement.LSSymbol.LEAF_ROD) {
			float length = elem.data [0];
			float width = elem.data [1];
			outList.Add (new LSElement (LSElement.LSSymbol.LEAF, length, width));
			return true;
		} else {
			outList.Add(elem);
			return false;
		}
	}
}
