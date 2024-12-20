
using System.Threading;

namespace KHFC {
	/// <summary> 특정 시간대에 해당하는 Ease 값을 손쉽게 가져올 수 있도록 만든 클래스 </summary>
	public class EaseCount {
		/// <summary> 변화값 </summary>
		public float m_Value;

		/// <summary> 시작값 </summary>
		public float m_Src;
		/// <summary> 도착값 </summary>
		public float m_Dest;

		/// <summary> 카운트 진행 시간 </summary>
		public float m_Count;
		/// <summary> 값 변경 시간 </summary>
		public float m_Duration;
		/// <summary> 카운트 전 지연 시간 </summary>
		public float m_Delay;


		public delegate void Callback();

		EasingFunction.Function mFunc;

		/// <summary> 카운팅을 시작했는가? -> <see cref="m_Delay"/> 만큼 시간이 지났는가? </summary>
		public bool m_CheckStart = false;
		/// <summary> 바로 호출하지 않고 delay 이후 호출한다 </summary>
		Callback m_OnStart;
		/// <summary> 카운트 종료 이후 호출한다 </summary>
		Callback m_OnAfter;

		/// <summary> 현재 카운팅을 하고 있는가? </summary>
		public bool counting {
			get; private set;
		}
		/// <summary> 카운팅 일시 중지 </summary>
		public bool m_Pause;

		CancellationTokenSource m_Token;

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
			m_Pause = false;
			m_OnAfter?.Invoke();
		}

		public float Update(float deltaTime) {
			if (!counting)
				return m_Dest;

			if (!m_Pause) {
				m_Count += deltaTime;
				if (m_Count < m_Delay) {
					m_Value = m_Src;
				} else if (m_Count < (m_Delay + m_Duration)) {
					if (!m_CheckStart) {
						m_CheckStart = true;
						m_OnStart?.Invoke();
					}
					m_Value = mFunc(m_Src, m_Dest, (m_Count - m_Delay) / m_Duration);
				} else {
					m_Value = m_Dest;
					ForceEnd();
				}
			}
			return m_Value;
		}

		void Reset() {
			counting = true;
			m_Pause = false;
			m_Count = 0f;

			m_CheckStart = false;
			m_OnAfter = null;
			m_OnStart = null;
		}
	}
}