using UnityEngine;

namespace KHFC {
	/// <summary> 특정 시간대에 해당하는 Ease 값을 손쉽게 가져올 수 있도록 만든 클래스 </summary>
	public class EaseCount {
		public float m_Duration;
		public float m_Src;
		public float m_Dest;
		public float m_Count;
		public float m_Delay;

		public float value {
			get; private set;
		}

		public delegate void Callback();

		EasingFunction.Function mFunc;

		bool m_CheckStart = false;
		/// <summary> 바로 호출하지 않고 delay 이후 호출한다 </summary>
		Callback m_OnStart;
		Callback m_OnAfter;

		public bool counting {
			get; private set;
		}
		public bool pause {
			get; set;
		}

		public EaseCount(EasingFunction.Ease type) {
			mFunc = EasingFunction.GetEasingFunction(type);
			counting = false;
		}

		/// <summary> 카운트를 세팅한다. 파라미터를 제외한 다른 모든 기능들도 초기화한다. </summary>
		public EaseCount Set(float src, float dest, float duration, float delay = 0f) {
			m_Src = src;
			m_Dest = dest;
			m_Duration = duration;
			m_Delay = delay;

			Reset();

			return this;
		}

		public void Reset() {
			counting = true;
			pause = false;
			m_Count = 0f;

			m_CheckStart = false;
			m_OnAfter = null;
			m_OnStart = null;
		}

		public EaseCount SetEase(EasingFunction.Ease type) {
			mFunc = EasingFunction.GetEasingFunction(type);
			return this;
		}

		public EaseCount SetDelay(float delay) {
			m_Delay = delay;
			return this;
		}

		public EaseCount OnStart(Callback callback) {
			m_OnStart = callback;
			return this;
		}

		public EaseCount OnComplete(Callback callback) {
			m_OnAfter = callback;
			return this;
		}

		public void ForceEnd() {
			counting = false;
			pause = false;
			m_OnAfter?.Invoke();
		}

		public float Update() {
			if (!counting)
				return m_Dest;

			if (!pause) {
				m_Count += Time.deltaTime;
				if (m_Count < m_Delay) {
					value = m_Src;
				} else if (m_Count < (m_Delay + m_Duration)) {
					if (!m_CheckStart) {
						m_CheckStart = true;
						m_OnStart?.Invoke();
					}
					value = mFunc(m_Src, m_Dest, (m_Count - m_Delay) / m_Duration);
				} else {
					value = m_Dest;
					ForceEnd();
				}
			}
			return value;
		}
	}
}