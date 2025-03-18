

namespace KHFC {
	internal partial class Util {

		/// <summary> 소수점 4자리 까지 비교 </summary>
		public static bool FloatEqual(float a, float b, float epsilon = 1.0E-4f) {
			return System.Math.Abs(a - b) < epsilon;
		}

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