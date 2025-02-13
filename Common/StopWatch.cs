using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace KHFC {
	public static class StopWatch {
#if UNITY_EDITOR
		static Dictionary<string, System.Diagnostics.Stopwatch> mDicWatch = new Dictionary<string, System.Diagnostics.Stopwatch>();
#endif
		static bool mUseLog = false;
		static System.Text.StringBuilder mLog;

		public static System.Text.StringBuilder GetLog() {
			mUseLog = false;
			return mLog;
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void InitLog() {
			mUseLog = true;
			mLog = new System.Text.StringBuilder();
			mLog.Remove(0, mLog.Length);
		}
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void CheckTimeStart(string timerName) {
			System.Diagnostics.Stopwatch watch;

#if UNITY_EDITOR
			if (!mDicWatch.ContainsKey(timerName))
				mDicWatch.Add(timerName, new System.Diagnostics.Stopwatch());

			watch = mDicWatch[timerName];
			watch.Reset();
			watch.Start();

            string resultMsg = $"StopWatch : {timerName} START";
			UnityEngine.Debug.Log(resultMsg);

			if (!mUseLog)
				return;

			if (mLog == null)
				mLog = new System.Text.StringBuilder();

			mLog.AppendLine(resultMsg);
#endif
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void CheckTimeEnd(string timerName) {
#if UNITY_EDITOR
			if (!mDicWatch.ContainsKey(timerName))
				return;

			System.Diagnostics.Stopwatch watch = mDicWatch[timerName];

			watch.Stop();

            string resultMsg = $"StopWatch : {timerName} End, Time : {watch.Elapsed.TotalSeconds}";
			UnityEngine.Debug.Log(resultMsg);

			if (mUseLog)
				mLog.AppendLine(resultMsg);
#endif
		}
	}
}