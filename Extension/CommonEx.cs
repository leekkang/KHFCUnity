
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class CommonEx {
	public static void SetAlpha(this Graphic graphic, float alpha) {
		Color color = graphic.color;
		color.a = alpha;
		graphic.color = color;
	}

	public static int Sum(this IEnumerable<int> array) {
		int sum = 0;
		foreach (int nums in array)
			sum += nums;
		return sum;
	}
	public static float Sum(this IEnumerable<float> array) {
		float sum = 0;
		foreach (float nums in array)
			sum += nums;
		return sum;
	}
	public static double Sum(this IEnumerable<double> array) {
		double sum = 0;
		foreach (double nums in array)
			sum += nums;
		return sum;
	}
	public static float Average(this IEnumerable<int> array) {
		int sum = 0;
		int count = 0;
		foreach (int nums in array) {
			sum += nums;
			count++;
		}
		return (float)sum / count;
	}
	public static float Average(this IEnumerable<float> array) {
		float sum = 0;
		int count = 0;
		foreach (float nums in array) {
			sum += nums;
			count++;
		}
		return sum / count;
	}
	public static double Average(this IEnumerable<double> array) {
		double sum = 0;
		int count = 0;
		foreach (double nums in array) {
			sum += nums;
			count++;
		}
		return sum / count;
	}

	public static void PlayAllParticle(this ParticleSystem particle) {
		ParticleSystem[] particles = particle.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem part in particles) {
			part.Play();
		}
	}
	public static void StopAllParticle(this ParticleSystem particle) {
		ParticleSystem[] particles = particle.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem part in particles) {
			part.Stop();
		}
	}
}
