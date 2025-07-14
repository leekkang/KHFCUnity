
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 특정 액션을 메인스레드에서 실행 </summary>
public class ThreadDispatcher : MonoBehaviour {
	static readonly Queue<Action> m_QueueExecution = new();
	static ThreadDispatcher inst;

	public void Update() {
		while (m_QueueExecution.Count > 0) {
			Action action;
			lock (m_QueueExecution) {
				action = m_QueueExecution.Dequeue();
			}
			action?.Invoke();
		}
	}

	public static void Enqueue(Action action) {
		if (action == null) {
			//throw new ArgumentNullException("action");
			return;
		}

		lock (m_QueueExecution) {
			m_QueueExecution.Enqueue(action);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void Initialize() {
		if (inst == null) {
			GameObject obj = new("ThreadDispatcher");
			inst = obj.AddComponent<ThreadDispatcher>();
			DontDestroyOnLoad(obj);
		}
	}
}