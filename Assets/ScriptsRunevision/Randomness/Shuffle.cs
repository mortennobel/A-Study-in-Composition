using UnityEngine;
using System.Collections.Generic;

namespace Runevision.Structures {

public static class Shuffle {
	public static void ShuffleList<E> (IList<E> list, System.Random rand) {
		if (list.Count > 1) {
			for (int i = list.Count - 1; i >= 0; i--) {
				E tmp = list[i];
				int randomIndex = rand.Next (i + 1);
				
				//Swap elements
				list[i] = list[randomIndex];
				list[randomIndex] = tmp;
			}
		}
	}
}

}
