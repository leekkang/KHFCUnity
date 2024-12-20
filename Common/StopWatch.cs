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
		public static void CheckTimeStart(string msg) {
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

#if UNITY_EDITOR
			if (!mDicWatch.ContainsKey(msg))
				mDicWatch.Add(msg, watch);

			watch.Reset();
			watch.Start();

			string resultMsg = $"StopWatch : {msg} START";
			UnityEngine.Debug.Log(resultMsg);

			if (!mUseLog)
				return;

			mLog ??= new System.Text.StringBuilder();
			mLog.AppendLine(resultMsg);
#endif
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void CheckTimeEnd(string msg) {
#if UNITY_EDITOR
			if (!mDicWatch.ContainsKey(msg))
				return;

			System.Diagnostics.Stopwatch watch = mDicWatch[msg];

			watch.Stop();

			string resultMsg = $"StopWatch : {msg} End, Time : {watch.Elapsed.TotalSeconds}";
			UnityEngine.Debug.Log(resultMsg);

			if (mUseLog)
				mLog.AppendLine(resultMsg);
#endif
		}
	}
}