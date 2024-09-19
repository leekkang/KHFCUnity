using UnityEngine;
using System.Collections.Generic;

namespace KHFC {
	/// <summary> Yield Instruction중 자주 사용하는 것들을 캐싱 </summary>
	public class CachedYield {
		/// <summary> WaitForSeconds를 캐싱, time값이 Key가 된다 </summary>
		private static Dictionary<float, WaitForSeconds> DicCachedWFS = new();

		private static WaitForEndOfFrame CachedEOF = new(); // c# 9.0: Target-typed new expressions
		public static WaitForEndOfFrame waitForEOF {
			get {
				//if (CachedEOF == null)
				//	CachedEOF = new WaitForEndOfFrame();
				CachedEOF ??= new WaitForEndOfFrame();

				return CachedEOF;
			}
		}

		/// <summary> 캐시되어 있는 WaitForSeconds를 반환 </summary>
		public static WaitForSeconds GetWaitForSeconds(float time) {
			if (time <= 0f)
				return null;

			if (!DicCachedWFS.TryGetValue(time, out WaitForSeconds res)) {
				res = new WaitForSeconds(time);
				DicCachedWFS.Add(time, res);
			}
			return res;
		}

		/// <summary> 캐시되어 있는 WaitForSeconds를 정리 </summary>
		public static void ClearCachedWaitForSeconds() {
			DicCachedWFS.Clear();
		}
	}
}
