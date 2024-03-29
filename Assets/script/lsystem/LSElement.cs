﻿using UnityEngine;
using System.Collections;

public class LSElement {
	public enum LSSymbol {TURN = '+', ROLL = '/', WIDTH='!', APEX = 'A', PUSH_STATE = '[', POP_STATE=']', DRAW='F', LEAF_ROD = 'R', LEAF = 'L', GRAVITY = 'G'};

	public LSSymbol symbol;
	public float[] data;


	public LSElement(LSSymbol symbol, params float[] data) {
		this.symbol = symbol;
		this.data = data;
	}

	public override string ToString() {
		string res = ""+(char)symbol;
		if (data.Length > 0){
			res += "(";
			for (int i=0;i<data.Length;i++){
				if (i>0){
					res += ", ";
				}
				res += data[i];
			}
			res += ")";
		}
		return res;
	}
}
