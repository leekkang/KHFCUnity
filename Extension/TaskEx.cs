
using System;
#if KHFC_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading;
using System.Threading.Tasks;
#endif

internal static class TaskEx {
	public static void CancelAndDispose(this CancellationTokenSource cts) {
		if (cts == null)
			return;
		try { cts.Cancel(); } catch { }
		cts.Dispose();
	}

	/// <summary> 특정 시간 후 자동 취소 설정 (CancelAfter 대체용) </summary>
	public static void CancelAfterSafe(this CancellationTokenSource cts, int milliseconds) {
		if (cts == null)
			return;
		if (milliseconds <= 0) {
			cts.Cancel();
		} else {
			cts.CancelAfter(milliseconds);
		}
	}

	/// <summary> 토큰이 취소되었는지 확인 </summary>
	public static bool IsCancelled(this CancellationTokenSource cts) {
		return cts != null && cts.IsCancellationRequested;
	}

#if KHFC_UNITASK
	public async static void OnComplete<T>(this UniTask<T> task, Action action) {
		await task;
		action.Invoke();
	}
	public async static void OnComplete<T>(this UniTask<T> task, Action<T> action) {
		T ret = await task;
		action.Invoke(ret);
	}
#else
	public async static void OnComplete<T>(this Task<T> task, Action action) {
		await task;
		action.Invoke();
	}
	public async static void OnComplete<T>(this Task<T> task, Action<T> action) {
		T ret = await task;
		action.Invoke(ret);
	}
	public static void Forget(this Task task) { }
	public static void Forget<T>(this Task<T> task) { }
#endif
}