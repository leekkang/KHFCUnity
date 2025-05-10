

namespace KHFC {
	internal partial class Util {
		public static int Abs(int val) => val < 0 ? -val : val;
		public static float Abs(float val) => val < 0f ? -val : val;

		/// <summary> 소수점 4자리 까지 비교 </summary>
		public static bool FloatEqual(float a, float b, float epsilon = 1.0E-4f) {
			return Abs(a - b) < epsilon;
		}

		// 삼각함수 - 가로 x, 세로 y, 사이각 theta, 빗면이 h인 직각삼각형 기준
		// Sin theta : h / y
		// Cos theta : h / x
		// Tan theta : y / x
		// Csc theta : y / h (Cosecant)
		// Sec theta : x / h (Secant)
		// Cot theta : x / y
		// Tan = Sin / Cos, Cot = C
		// Sin(a + b) = Sin(a)Cos(b) + Cos(a)Sin(b)
		// Cos(a + b) = Cos(a)Cos(b) - Sin(a)Sin(b)

		// Arc : 역함수
		// ASin : ASin(Sin theta) = theta
		// ACos : ACos(Cos theta) = theta
		// ATan : ATan(Tan theta) = theta
		// Atan2 : x, y 좌표를 받아서 정확한 각도를 출력해냄

		/// <summary> y축 기준 <paramref name="vec"/> 와의 사이각을 반환한다. </summary>
		/// <remarks> 왼손좌표계 기준 반시계 방향이 + 이다 -> 왼쪽을 보고있으면 +값, 오른쪽을 보고있으면 -값을 반환한다. </remarks>
		public static float GetLookAngle(UnityEngine.Vector2 vec) {
			return UnityEngine.Mathf.Atan2(vec.y, vec.x) * UnityEngine.Mathf.Rad2Deg;
		}

		/// <summary> Transform의 값으로 사용하기 위해 각도를 <paramref name="min"/> ~ <paramref name="min"/>+360도 범위 내 값으로 만든다 </summary>
		public static float ClampAngle(float angle, float min = 0) {
			if (angle < min) {
				while (angle < min)
					angle += 360f;
			} else if (angle > (min + 360f)) {
				while (angle > (min + 360f))
					angle -= 360f;
			}
			return angle;
		}


		/// <summary> 분산랜덤 </summary>
		/// <param name="variance">클수록 뾰족해짐! 최소값 기본값이 1</param>
		/// <param name="middleValue"> 중간값 </param>
		/// <param name="range"> 값 표현 범위, 그래프의 절반 범위 </param>
		/// <param name="variance"> 분산에 사용하는 로그의 밑(base) </param>
		public static int DistributionRandom(int middleValue, float range, int variance = 1) {
			variance = UnityEngine.Mathf.Max(1, variance);
			variance = (int)System.Math.Pow(10, variance);

			float u;
			int ret;
			do {
				u = UnityEngine.Random.Range(0.0f, 1.0f - UnityEngine.Mathf.Epsilon);
				ret = (int)(UnityEngine.Mathf.Log(u / (1 - u), variance) * range + middleValue);
			} while (ret < middleValue - range || ret > middleValue + range);
			return ret;
		}

	}
}