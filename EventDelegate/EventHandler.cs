using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	public enum EventType {
		UpdateGold,
		ResetDay,
	}

	public delegate void EventCallback(params object[] data);

	public class EventHandler {
		static readonly EventHandler inst = new();
		readonly Dictionary<EventType, EventCallback> m_DicEvent = new();
		/// <summary> <paramref name="eventType"/>의 델리게이트 실행 </summary>
		public static void Notify(EventType eventType, params object[] data) {
			ShowLog("Event Notify::" + eventType, data);
			inst.NotifyToObservers(eventType, data);
		}

		/// <summary> <paramref name="eventType"/>에 델리게이트 등록 </summary>
		public static void Register(EventType eventType, EventCallback observer) {
			inst.RegisterObserver(eventType, observer);
		}

		/// <summary> <paramref name="eventType"/>의 <paramref name="observer"/> 델리게이트 제거 </summary>
		public static void Remove(EventType eventType, EventCallback observer) {
			inst.RemoveObserver(eventType, observer);
		}

		/// <summary> <paramref name="eventType"/>의 모든 델리게이트 제거 </summary>
		public static void Remove(EventType eventType) {
			inst.RemoveEvent(eventType);
		}

		/// <summary> <paramref name="obj"/> 를 타겟으로 하는 모든 델리게이트 제거 </summary>
		public static void Remove(object obj) {
			inst.RemoveObserverObject(obj);
		}

		/// <summary> 델리게이트 실행 </summary>
		/// <param name="data"> 델리게이트 파라미터 </param>
		void NotifyToObservers(EventType eventType, params object[] data) {
			if (m_DicEvent.ContainsKey(eventType))
				m_DicEvent[eventType](data);
		}

		/// <summary> 델리게이트 등록 </summary>
		public void RegisterObserver(EventType eventType, EventCallback observer) {
			if (m_DicEvent.ContainsKey(eventType))
				m_DicEvent[eventType] += observer;
			else
				m_DicEvent.Add(eventType, observer);
		}

		/// <summary> <paramref name="obj"/> 를 타겟으로 하는 모든 델리게이트 제거 </summary>
		public void RemoveObserverObject(object obj) {
			List<EventType> keys = new(m_DicEvent.Keys);

			for (int i = 0; i < keys.Count; i++) {
				EventType key = keys[i];
				EventCallback callBack = m_DicEvent[key];
				Delegate[] arrDel = callBack.GetInvocationList();
				foreach (Delegate del in arrDel) {
					if (del.Target != obj)
						continue;
					m_DicEvent[key] -= callBack;
					if (m_DicEvent[key] == null)
						m_DicEvent.Remove(key);
				}
			}
		}

		/// <summary> 델리게이트 제거 </summary>
		void RemoveObserver(EventType eventType, EventCallback observer) {
			if (m_DicEvent.ContainsKey(eventType)) {
				m_DicEvent[eventType] -= observer;
				if (m_DicEvent[eventType] == null)
					m_DicEvent.Remove(eventType);
			}
		}

		/// <summary> <paramref name="eventType"/>의 모든 델리게이트 제거 </summary>
		void RemoveEvent(EventType eventType) {
			if (m_DicEvent.ContainsKey(eventType))
				m_DicEvent.Remove(eventType);
		}

		static void ShowLog(string message, object[] data = null) {
			if (data != null) {
				for (int i = 0; i < data.Length; i++) {
					message += ", " + data[i].ToString();
				}
			}
			UnityEngine.Debug.Log(message);
		}
	}
}