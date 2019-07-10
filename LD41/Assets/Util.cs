using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;
using System.Text.RegularExpressions;

// An Interpolator I is a class that maps the interval [0, 1] to itself s. t.
//		- I(0) = 0, I(1) = 1.
//		- I is monotonically increasing.
public abstract class Interpolator {
	public abstract float Interpolate (float t);
}

public class LinearInterpolator : Interpolator {
	override public float Interpolate(float t) { return t; }
}

public class LogisticInterpolator : Interpolator {
	float sigma; 	// Steepness of the curve.
	float cutoff;	// Point of transition to linear interpolation.

	public LogisticInterpolator(float sigma = 5, float cutoff = 0.1f) {
		this.sigma = sigma;
		this.cutoff = cutoff;
	}

	private static float theta(float t) { return 2 * t - 1; }

	override public float Interpolate(float t) { 
		if (t < cutoff) {
			return Util.Logistic(theta(cutoff), this.sigma) * (t / cutoff);
		}
		if (t > 1 - cutoff) {
			return Util.Logistic(theta(1-cutoff), this.sigma) +
				   (1 - Util.Logistic(theta(1-cutoff), this.sigma)) * (t - cutoff) / (1 - cutoff);
		}

		return Util.Logistic(theta(t), this.sigma);
	}
}




public static class Util {

	public static float Logistic(float x, float sigma) {
		return 1f / (1 + Mathf.Exp(-sigma * x));
	}

	public static string Obfuscate(List<int> a) {
		int prod = 111;
		foreach(int b in a) {
			prod = prod * 17 + b * 19;
		}
		string obf = Regex.Replace(prod.ToString("X4"), ".{2}", "$0 ");
		obf = obf.Trim();
		obf = Regex.Replace(obf, " ", "-");
		return obf;
	}
}
