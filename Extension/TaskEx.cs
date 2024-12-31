
using System;
#if KHFC_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

public static class TaskEx {
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