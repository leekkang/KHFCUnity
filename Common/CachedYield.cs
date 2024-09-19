using UnityEngine;
using System.Collections.Generic;

namespace KHFC {
	/// <summary> Yield Instruction중 자주 사용하는 것들을 캐싱 </summary>
	public class CachedYield {
		/// <summary> WaitForSeconds를 캐싱, time값이 Key가 된다 </summary>
		private static Dictionary<float, WaitForSeconds> m_DicCachedWFS = new();

		private static WaitForEndOfFrame m_CachedEOF = new(); // c# 9.0: Target-typed new expressions
		public static WaitForEndOfFrame WaitForEOF {
			get {
				//if (m_CachedEOF == null)
				//	m_CachedEOF = new WaitForEndOfFrame();
				m_CachedEOF ??= new WaitForEndOfFrame();

				return m_CachedEOF;
			}
		}

		/// <summary> 캐시되어 있는 WaitForSeconds를 반환 </summary>
		public static WaitForSeconds GetWaitForSeconds(float time) {
			if (time == 0f)
				return null;

			if (!m_DicCachedWFS.TryGetValue(time, out WaitForSeconds res)) {
				res = new WaitForSeconds(time);
				m_DicCachedWFS.Add(time, res);
			}
			return res;
		}
		/// <summary> 캐시되어 있는 WaitForSeconds를 정리 </summary>
		public static void ClearCachedWaitForSeconds() {
			m_DicCachedWFS.Clear();
		}
	}
}
