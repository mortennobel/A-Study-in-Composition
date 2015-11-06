using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rule {
	
	LSystem lsys;
	public Vector2 branches;


	public Rule (Vector2 turn1, Vector2 turn2,Vector2 turn3, Vector2 roll1, Vector2 roll2,Vector2 roll3, Vector2 lengthScale1, Vector2 lengthScale2, Vector2 lengthScale3, Vector2 q, Vector2 e, Vector2 smin, Vector2 branches,LSystem lsys)
	{
		

		this.lsys = lsys;

	}
	

	public bool Apply(LSElement elem, List<LSElement> outList, bool lastIter){
		if (elem.symbol == LSElement.LSSymbol.APEX){
			float length = elem.data[0];
			float width = elem.data[1];
			if (length >= lsys.Eval(lsys.smin)) {
				outList.Add (new LSElement (LSElement.LSSymbol.WIDTH, width));
				float e = lsys.Eval (lsys.e);
				float q = lsys.Eval (lsys.q);
				Vector2[] turn = new Vector2[]{ lsys.turn1,lsys.turn2,lsys.turn3};
				Vector2[] roll = new Vector2[]{ lsys.roll1,lsys.roll2,lsys.roll3};
				float[] rs = new float[]{lsys.Eval (lsys.lengthScale1),
					lsys.Eval (lsys.lengthScale2),
					lsys.Eval (lsys.lengthScale3)};

				float[] widthScale = new float[]{ 
					Mathf.Pow (q, e),
					Mathf.Pow (1.0f - q, e),
					0.5f*(Mathf.Pow (q, e) + Mathf.Pow (1.0f - q, e))
				};

				float endScale = Mathf.Max (widthScale); // extension to give a better looking result

				outList.Add (new LSElement (LSElement.LSSymbol.DRAW, length, endScale));
				int no = Mathf.Clamp( Mathf.RoundToInt (Random.Range (lsys.branchNo.x, lsys.branchNo.y)),0,3);

				for (int i = 0; i < no	; i++) {
					outList.Add (new LSElement (LSElement.LSSymbol.PUSH_STATE));
					outList.Add (new LSElement (LSElement.LSSymbol.TURN, lsys.Eval(turn[i])));
					outList.Add (new LSElement (LSElement.LSSymbol.ROLL, lsys.Eval(roll[i])));

					if (lastIter) {
						outList.Add (new LSElement (LSElement.LSSymbol.LEAF_ROD, length, width));
					} else {
						outList.Add (new LSElement (LSElement.LSSymbol.APEX, length * rs[i], width * widthScale[i]));	
					}
					outList.Add (new LSElement (LSElement.LSSymbol.POP_STATE));	
				}
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
