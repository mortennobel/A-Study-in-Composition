using UnityEngine;

namespace Runevision.Structures {

public class SimplexNoise {
	
	private static SimplexNoise instance = new SimplexNoise ();

	private int i, j, k;
	private int[] A = {0, 0, 0};
	private float u, v, w, s;
	private float onethird = 0.333333333f;
	private float onesixth = 0.166666667f;
	private static int[] T = {0x15, 0x38, 0x32, 0x2c, 0x0d, 0x13, 0x07, 0x2a};

	public static float Noise (Vector3 position) {
		return instance.GetNoise (position.x, position.y, position.z);
	}

	public static float Noise (float x, float y, float z) {
		return instance.GetNoise (x, y, z);
	}

	public float GetNoise (Vector3 position) {
		return GetNoise (position.x, position.y, position.z);
	}
	
	public float GetNoise (float x, float y, float z) {
		// Skew input space to relative coordinate in simplex cell
		s = (x + y + z) * onethird;
		i = FastFloor (x+s);
		j = FastFloor (y+s);
		k = FastFloor (z+s);
		
		// Unskew cell origin back to (x, y, z) space
		s = (i + j + k) * onesixth;
		u = x - i + s;
		v = y - j + s;
		w = z - k + s;
		
		A[0] = A[1] = A[2] = 0;
		
		// For 3D case, the simplex shape is a slightly irregular tetrahedron.
		// Determine which simplex we're in
		int hi = u >= w ? u >= v ? 0 : 1 : v >= w ? 1 : 2;
		int lo = u < w ? u < v ? 0 : 1 : v < w ? 1 : 2;
		
		return Mathf.Clamp ((K (hi) + K (3 - hi - lo) + K (lo) + K (0)) * 3, -1, 1);
	}
	
	private float K (int a) {
		s = (A[0] + A[1] + A[2]) * onesixth;
		float x = u - A[0] + s;
		float y = v - A[1] + s;
		float z = w - A[2] + s;
		float t = 0.6f - x * x - y * y - z * z;
		int h = Shuffle (i + A[0], j + A[1], k + A[2]);
		A[a]++;
		if (t < 0)
			return 0;
		int b5 = h >> 5 & 1;
		int b4 = h >> 4 & 1;
		int b3 = h >> 3 & 1;
		int b2 = h >> 2 & 1;
		int b = h & 3;
		float p = b == 1 ? x : b == 2 ? y : z;
		float q = b == 1 ? y : b == 2 ? z : x;
		float r = b == 1 ? z : b == 2 ? x : y;
		p = b5 == b3 ? -p : p;
		q = b5 == b4 ? -q: q;
		r = b5 != (b4^b3) ? -r : r;
		t *= t;
		return 8 * t * t * (p + (b == 0 ? q + r : b2 == 0 ? q : r));
	}

	private static int FastFloor (float n) {
		return n > 0 ? (int) n : (int) n - 1;
	}

	private static int Shuffle (int i, int j, int k) {
		return B (i, j, k, 0) + B (j, k, i, 1) + B (k, i, j, 2) + B (i, j, k, 3) +
			   B (j, k, i, 4) + B (k, i, j, 5) + B (i, j, k, 6) + B (j, k, i, 7);
	}
	
	private static int B (int i, int j, int k, int b) {
		return T[B (i, b) << 2 | B (j, b) << 1 | B (k, b)];
	}
	
	private static int B (int n, int b) {
		return n >> b & 1;
	}
}

}
