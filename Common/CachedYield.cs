
using UnityEngine;
using System.Collections.Generic;

namespace KHFC {
	/// <summary> Yield Instruction중 자주 사용하는 것들을 캐싱 </summary>
	public class CachedYield {
		/// <summary> WaitForSeconds를 캐싱, time값이 Key가 된다 </summary>
		readonly static Dictionary<float, WaitForSeconds> DicCachedWFS = new();
		readonly static Dictionary<float, WaitForSecondsRealtime> DicCachedWFSR = new();

		public static readonly WaitForEndOfFrame waitForEOF = new(); // c# 9.0: Target-typed new expressions
		public static readonly WaitForFixedUpdate waitFixed = new();

		/// <summary> 캐시되어 있는 <see cref="WaitForSeconds"/>를 반환 </summary>
		public static WaitForSeconds GetWFS(float time) {
			if (time <= 0f)
				return null;

			if (DicCachedWFS.TryGetValue(time, out WaitForSeconds wfs))
				return wfs;
			return DicCachedWFS[time] = new WaitForSeconds(time);
		}

		/// <summary> 캐시되어 있는 <see cref="WaitForSecondsRealtime"/>를 반환 </summary>
		public static WaitForSecondsRealtime GetWFSR(float time) {
			if (time <= 0f)
				return null;

			if (DicCachedWFSR.TryGetValue(time, out WaitForSecondsRealtime wfsr))
				return wfsr;
			return DicCachedWFSR[time] = new WaitForSecondsRealtime(time);
		}

		/// <summary> 캐시되어 있는 <see cref="WaitForSeconds"/> 딕셔너리를 정리 </summary>
		public static void ClearWaitForSeconds() {
			DicCachedWFS.Clear();
		}

		/// <summary> 캐시되어 있는 <see cref="WaitForSecondsRealtime"/> 딕셔너리를 정리 </summary>
		public static void ClearWaitForSecondsRealtime() {
			DicCachedWFSR.Clear();
		}
	}
}