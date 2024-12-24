
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace KHFC {
	/// <summary> 정확하게 시간을 측정할 수 있는 컴포넌트 </summary>
	[DisallowMultipleComponent]
	public class TimeChecker : MonoBehaviour {
		public static TimeChecker inst;
		readonly static DateTime epochTime = new(1970, 1, 1);

		/// <summary> 현재 시간, 스레드에서 계속 갱신함 </summary>
		public static DateTime utcNow = DateTime.UtcNow;
		/// <summary> 현재 시간 타임스탬프, 스레드에서 계속 갱신함, 단위는 millisecond </summary>
		public static long timeStamp;
		public static int deltaMilliSec;
		public static int deltaSec;

		static readonly object LockObj = new();

		class DelGroup {
			public int hashCode;
			/// <summary> 반복 횟수, 1이면 한 번 호출, -1이면 취소할 때 까지 계속 호출 </summary>
			public int loopCount;
			/// <summary> 반복 시간 간격, 단위는 millisecond </summary>
			public int interval;
			int delta;
			public System.Action callback;

			/// <summary> 루프를 종료해야 하면 false 리턴 </summary>
			public bool Call(int millisec) {
				if (interval > 0) {
					delta += millisec;
					if (delta < interval)
						return true;
					delta -= interval;
				}

				if (loopCount > 0)
					--loopCount;

				callback();
				return loopCount != 0;
			}
		}

		/// <summary> 유니티 스케일의 영향을 받지 않는 콜백 그룹 </summary>
		System.Collections.Generic.List<DelGroup> m_ListFixed;
		/// <summary> 유니티 스케일의 영향을 받는 콜백 그룹 </summary>
		System.Collections.Generic.List<DelGroup> m_ListScaled;

		DateTime m_StartTime;

		/// <summary> 스레드에서 루프 간 시간 간격, 단위는 millisecond 이다. </summary>
		/// <remarks> 기본값 50, 0.02초마다 1번씩 루프를 호출한다. -> 1초에 50번 호출 </remarks>
		public int m_Interval = 50;
		CancellationTokenSource m_Token;

		/// <summary> 고정 스레드를 퍼즈 때 멈추게 할 것인가? 스케일 타이머는 값 상관없이 무조건 멈춤 </summary>
		public bool m_StopOnPause = true;
		bool m_Pause = false;

		/// <summary> 특정 시간에 호출할 함수를 등록한다. 타임스케일 영향을 받지 않는다 </summary>
		/// <param name="millisec"> 호출 간격, 0이면 매번 호출 </param>
		/// <param name="exeCount"> 반복 횟수 </param>
		public static void AddFixed(System.Action onFixedUpdate, int millisec, int exeCount = 1) {
			if (onFixedUpdate == null)
				return;
			inst.ReserveFixed(onFixedUpdate, millisec, exeCount);
		}
		public static void RemoveFixed(System.Action onFixedUpdate) {
			if (onFixedUpdate == null)
				return;
			inst.ReleaseFixed(onFixedUpdate);
		}

		/// <summary> 특정 시간에 호출할 함수를 등록한다 </summary>
		/// <param name="millisec"> 호출 간격, 0이면 매번 호출 </param>
		/// <param name="exeCount"> 반복 횟수 </param>
		public static void AddScaled(System.Action onScaledUpdate, int millisec, int exeCount = 1) {
			if (onScaledUpdate == null)
				return;
			inst.ReserveScaled(onScaledUpdate, millisec, exeCount);
		}
		public static void RemoveScaled(System.Action onScaledUpdate) {
			if (onScaledUpdate == null)
				return;
			inst.ReleaseScaled(onScaledUpdate);
		}

		public void Init() {
			m_StartTime = DateTime.UtcNow;
			m_Token?.Cancel();
			m_Token = new CancellationTokenSource();
			Task.Run(Loop);
		}

		void Awake() {
			inst = this;
			m_ListFixed = new System.Collections.Generic.List<DelGroup>();
			m_ListScaled = new System.Collections.Generic.List<DelGroup>();
		}

		void OnDestroy() {
			m_Token?.Cancel();
		}

		void FixedUpdate() {
			int milliSec = (int)(Time.fixedDeltaTime * 1000);
			for (int i = m_ListScaled.Count - 1; i > -1; --i) {
				if (!m_ListScaled[i].Call(milliSec))
					ReleaseScaled(m_ListScaled[i].callback);
			}
		}

		void OnApplicationPause(bool pause) {
			if (!m_StopOnPause)
				return;
			m_Pause = pause;
		}

		public void ReserveFixed(System.Action onFixed, int millisec, int repeatCount) {
			int hashCode = onFixed.GetHashCode();

			if (!m_ListFixed.TryGetValue(out DelGroup group, (x) => x.hashCode == hashCode)) {
				lock (LockObj) {
					m_ListFixed.Add(new DelGroup() {
						hashCode = hashCode,
						interval = millisec,
						loopCount = repeatCount,
						callback = onFixed
					});
				}
			}
		}

		public void ReleaseFixed(System.Action onFixed) {
			int hashCode = onFixed.GetHashCode();
			if (m_ListFixed.TryGetValue(out DelGroup group, (x) => x.hashCode == hashCode)) {
				lock (LockObj) {
					m_ListFixed.RemoveBySwap(group);
				}
			}
		}

		public void ReserveScaled(System.Action onScaled, int millisec, int repeatCount) {
			int hashCode = onScaled.GetHashCode();

			if (!m_ListScaled.TryGetValue(out DelGroup group, (x) => x.hashCode == hashCode)) {
				lock (LockObj) {
					m_ListScaled.Add(new DelGroup() {
						hashCode = hashCode,
						interval = millisec,
						loopCount = repeatCount,
						callback = onScaled
					});
				}
			}
		}

		public void ReleaseScaled(System.Action onScaled) {
			int hashCode = onScaled.GetHashCode();
			if (m_ListScaled.TryGetValue(out DelGroup group, (x) => x.hashCode == hashCode)) {
				lock (LockObj) {
					m_ListScaled.RemoveBySwap(group);
				}
			}
		}

		async Task Loop() {
			long prev = (m_StartTime - epochTime).Ticks / 10000;
			while (!m_Token.IsCancellationRequested) {
				await Task.Delay(m_Interval, cancellationToken: m_Token.Token);

				utcNow = DateTime.UtcNow;
				timeStamp = (utcNow - epochTime).Ticks / 10000;
				deltaMilliSec = (int)(timeStamp - prev);
				deltaSec = deltaMilliSec / 1000;
				prev = timeStamp;

				if (m_Pause)
					continue;

				for (int i = m_ListFixed.Count - 1; i > -1; --i) {
					if (!m_ListFixed[i].Call(deltaMilliSec))
						ReleaseFixed(m_ListFixed[i].callback);
				}
			}
		}
	}
}