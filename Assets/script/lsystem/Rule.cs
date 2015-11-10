using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rule {
	
	LSystem lsys;

	public Rule (LSystem lsys)
	{
		this.lsys = lsys;
	}

	public bool Apply(LSElement elem, List<LSElement> outList, bool lastIter){
		if (elem.symbol == LSElement.LSSymbol.APEX){
			float length = elem.data[0];
			float width = elem.data[1];

			if (width < lsys.minTerminalWidthRatio * Mathf.Min (lsys.initialWidth.x, lsys.initialWidth.y))
				lastIter = true;

			if (length <= lsys.Eval (lsys.smin))
				lastIter = true;

			if (lastIter) {
				outList.Add (new LSElement (LSElement.LSSymbol.LEAF_ROD, length, width));
				return true;
			}

			outList.Add (new LSElement (LSElement.LSSymbol.WIDTH, width));
			float e = lsys.Eval (lsys.e);
			Vector2[] turn = new Vector2[]{ lsys.turn1,lsys.turn2,lsys.turn3};
			Vector2[] roll = new Vector2[]{ lsys.roll1,lsys.roll2,lsys.roll3};
			float[] rs = new float[]{lsys.Eval (lsys.lengthScale1),
				lsys.Eval (lsys.lengthScale2),
				lsys.Eval (lsys.lengthScale3)};

			// Calculate number of branches.
			int no = Mathf.Clamp( Mathf.RoundToInt (Random.Range (lsys.branchNo.x, lsys.branchNo.y)),0,3);

			// Calculate angles and calculate widths based on that.
			float[] angles = new float[no];
			float[] crossSectionRatios = new float[no];
			float smallBranchBias = lsys.Eval (lsys.smallBranchBias);

			float smallestCrossSection = Mathf.Infinity;
			for (int i = 0; i < no; i++) {
				angles[i] = lsys.Eval (turn[i]);
				// 0 at 90 degrees or greater, 1 at straight (0 degrees).
				crossSectionRatios[i] = Mathf.InverseLerp (90, 0, Mathf.Abs (angles[i]));

				if (crossSectionRatios[i] < smallestCrossSection)
					smallestCrossSection = crossSectionRatios[i];
			}

			// Subtract smallest ratio from all so smallest branch has ratio 0.
			for (int i = 0; i < no; i++)
				crossSectionRatios[i] -= smallestCrossSection * smallBranchBias;

			// Calculate sum.
			float crossSectionRatiosSum = 0;
			for (int i = 0; i < no; i++)
				crossSectionRatiosSum += crossSectionRatios[i];

			// Increase ratios by smallest allowed ratio.
			for (int i = 0; i < no; i++) {
				crossSectionRatios[i] += lsys.minBranchRatio * crossSectionRatiosSum;
			}

			// Recalculate sum.
			crossSectionRatiosSum = 0;
			for (int i = 0; i < no; i++)
				crossSectionRatiosSum += crossSectionRatios[i];

			// Make sure cross section ratios add up to one,
			// And calculate widths from it.
			float[] widthScale = new float[no];
			for (int i = 0; i < no; i++) {
				crossSectionRatios[i] /= crossSectionRatiosSum;
				widthScale[i] = Mathf.Pow (crossSectionRatios[i], e);
			}

			float endScale = Mathf.Max (widthScale); // extension to give a better looking result

			outList.Add (new LSElement (LSElement.LSSymbol.DRAW, length, endScale));

			for (int i = 0; i < no; i++) {
				outList.Add(new LSElement (LSElement.LSSymbol.PUSH_STATE));
				outList.Add(new LSElement (LSElement.LSSymbol.TURN, angles[i]));
				outList.Add(new LSElement (LSElement.LSSymbol.ROLL, lsys.Eval (roll [i])));
				float thinBranch = Mathf.Pow (1.0f - width / lsys.Eval (lsys.initialWidth), 3.0f);
				outList.Add(new LSElement (LSElement.LSSymbol.GRAVITY, thinBranch * lsys.gravity));
				outList.Add(new LSElement (LSElement.LSSymbol.APEX, length * rs [i], width * widthScale [i]));	
				outList.Add(new LSElement (LSElement.LSSymbol.POP_STATE));	
			}
			return true;
		}
		else if (elem.symbol == LSElement.LSSymbol.LEAF_ROD) {
			float length = elem.data [0];
			float width = elem.data [1];
			outList.Add (new LSElement (LSElement.LSSymbol.LEAF, length, width));
			return true;
		}
		else {
			outList.Add(elem);
			return false;
		}
	}
}
