using UnityEngine;
using System.Collections.Generic;

namespace KHFC {
	/// <summary> Yield Instruction중 자주 사용하는 것들을 캐싱 </summary>
	public class CachedYield {
		/// <summary> WaitForSeconds를 캐싱, time값이 Key가 된다 </summary>
		private readonly static Dictionary<float, WaitForSeconds> DicCachedWFS = new();

		public static WaitForEndOfFrame waitForEOF = new(); // c# 9.0: Target-typed new expressions
		public static WaitForFixedUpdate waitFixed = new();

		/// <summary> 캐시되어 있는 WaitForSeconds를 반환 </summary>
		public static WaitForSeconds GetWaitForSeconds(float time) {
			if (time <= 0f)
				return null;

			if (DicCachedWFS.TryGetValue(time, out WaitForSeconds wfs))
				return wfs;

			return DicCachedWFS[time] = new WaitForSeconds(time);
		}

		/// <summary> 캐시되어 있는 WaitForSeconds를 정리 </summary>
		public static void ClearCachedWaitForSeconds() {
			DicCachedWFS.Clear();
		}
	}
}
