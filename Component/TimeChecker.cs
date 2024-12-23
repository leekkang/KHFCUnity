
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace KHFC {
	/// <summary> 정확하게 시간을 측정할 수 있는 컴포넌트 </summary>
	[DisallowMultipleComponent]
	public class TimeChecker : MonoBehaviour {
		readonly static DateTime epochTime = new(1970, 1, 1);

		public static DateTime utcNow = DateTime.UtcNow;
		public static TimeChecker inst;

		System.Action<float> m_OnFixed;

		/// <summary> 현재 시간, 스레드에서 계속 갱신함, 단위는 millisecond </summary>
		[ReadOnly]
		[SerializeField] long m_CurTimeStamp;

		/// <summary> 현재 시간, 스레드에서 계속 갱신함, 단위는 millisecond </summary>
		public long now => m_CurTimeStamp;

		DateTime m_StartTime;

		/// <summary> 스레드에서 루프 간 시간 간격, 단위는 millisecond 이다. </summary>
		/// <remarks> 기본값 50, 0.02초마다 1번씩 루프를 호출한다. -> 1초에 50번 호출 </remarks>
		public int m_Interval = 50;
		CancellationTokenSource m_Token;

		/// <summary> 한번에 돌릴 각도 </summary>
		[KHFC.FieldName("한번에 돌릴 각도")]
		[SerializeField] float m_Angle = -30f;

		/// <summary> 각도가 변경될 때 마다 호출하는 콜백함수 </summary>
		System.Action m_OnChangeRotation;

		public static void AddFixed(System.Action<float> onFixedUpdate) => inst.AddFixed(onFixedUpdate);
		public static void RemoveFixed(System.Action<float> onFixedUpdate) => inst.AddFixed(onFixedUpdate);

		public void AddFixed(System.Action<float> onFixedUpdate) {
			if (onFixedUpdate != null)
				m_OnFixed += onFixedUpdate;

			if (m_DicMultiObjEventMap.ContainsKey(eventType)) {
				m_DicMultiObjEventMap[eventType] -= observer;
				if (null == m_DicMultiObjEventMap[eventType]) {
					m_DicMultiObjEventMap.Remove(eventType);
				}
			}
		}

		public void RemoveFixed(System.Action<float> onFixedUpdate) {
			if (onFixedUpdate != null)
				m_OnFixed -= onFixedUpdate;
		}

		public void Init() {
			m_StartTime = DateTime.UtcNow;
			m_Token?.Cancel();
			m_Token = new CancellationTokenSource();
			Task.Run(Loop);
		}

		async Task Loop() {
			while (!m_Token.IsCancellationRequested) {
				m_CurTimeStamp = Interop.Sys.GetSystemTimeAsTicks()

				await Task.Delay(m_Interval, cancellationToken:m_Token.Token);
			}
		}

		void Awake() {
			inst = this;
		}

		void OnDestroy() {
			m_Token?.Cancel();
		}

		void Update() {
			if (!m_Play)
				return;

			float deltaTime = Time.deltaTime;
			if (m_ChangeAngle) {
				Vector3 angle = m_TR.localEulerAngles;
				m_TmpAngle += m_DeltaAngle * deltaTime;

				if ((m_Angle > 0f && m_TmpAngle > m_DestAngle) ||
					(m_Angle < 0f && m_TmpAngle < m_DestAngle)) {
					m_TmpAngle = m_DestAngle;
					m_ChangeAngle = false;
				}


				angle.z = KHFC.Util.ClampAngle(m_TmpAngle, 0);
				//angle.z = KHFC.Util.ClampAngle(angle.z, 0);
				//m_TR.localRotation = Quaternion.Euler(angle);
				m_TR.localEulerAngles = angle;
			}
			m_Count -= deltaTime;
			if (m_Count > 0)
				return;

			// 각도 변경시간이 너무 짧으면 일반 회전과 동일하게 처리한다.
			if (m_AngleChangeTime > 0.01f) {
				m_ChangeAngle = true;
				m_TmpAngle = m_TR.localEulerAngles.z;
				m_DestAngle = m_TR.localEulerAngles.z + m_Angle;
			} else {
				//m_Angle = KHFC.Util.ClampAngle(m_Angle, 0);
				m_TR.Rotate(new Vector3(0, 0, m_Angle));
			}
			m_OnChangeRotation?.Invoke();
			m_Count = m_Interval;
		}
	}
}