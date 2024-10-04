using UnityEngine;
using System.Collections;

namespace KHFC {
	public class DestroyOnFinish : MonoBehaviour {
		GameObject m_Object;

		public float m_DestroyTime = 0f;
		public bool m_IgnoreTimescale;
		public System.Action m_OnComplete = null;

		bool m_Ready = false;

		void Awake() {
			m_Object = this.gameObject;
		}

		public void StartCountDown() {
			m_Ready = true;
			StartCoroutine(CoDestroy());
		}

		void OnDisable() {
			if (m_Ready) {
				StopAllCoroutines();

				m_OnComplete?.Invoke();
				m_OnComplete = null;

				//함수 내부에서 PoolMgr.Despawn을 호출 하는데, 이펙트게임오브젝트의 부모를 바꾸다 보니 에러가 발생
				//EffectMgr.inst.ReserveDestroyEffect(m_Object);

				m_Ready = false;
			}
		}

		IEnumerator CoDestroy() {
			if (m_IgnoreTimescale)
				yield return new WaitForSecondsRealtime(m_DestroyTime);
			else
				yield return CachedYield.GetWaitForSeconds(m_DestroyTime);

			m_OnComplete?.Invoke();

			yield return CachedYield.waitForEOF;

			//EffectMgr.inst.DestroyEffect(m_Object);

			m_OnComplete = null;
			m_Ready = false;
		}
	}
}
