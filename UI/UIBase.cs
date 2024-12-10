using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	public abstract class UIBase : CachedComponent {
		/// <summary> 모든 UI의 <see cref="Graphics"/>가 사용하는 아틀라스들의 리스트 </summary>
		static RefList<UnityEngine.U2D.SpriteAtlas> mListLoadedAtlas;

		static readonly object LockTouchObj = new();
		static int LockTouch = 0;
		public static bool lockTouch {
			get { lock (LockTouchObj) { return LockTouch > 0; } }
			set {
				lock (LockTouchObj) {
					LockTouch += value ? 1 : -1;
					if (LockTouch < 0)
						LockTouch = 0;
				}
			}
		}
		public static void ClearLock() {
			lock (LockTouchObj) {
				LockTouch = 0;
			}
		}

		protected Transform m_CachedTransform;
		public RectTransform rectTransform {
			get {
				if (m_CachedTransform == null)
					m_CachedTransform = GetComponent<RectTransform>();
				return m_CachedTransform as RectTransform;
			}
		}

		bool m_OnInitialized;
		public bool initialized => m_OnInitialized;


		/// <summary> 해당 UI의 초기화에 필요한 정보를 로드하는 함수 </summary>
		public virtual void Init() {
			m_ListCachedObject = new List<GameObject>();

			m_OnInitialized = true;
		}

		void OnEnable() {
			if (!m_OnInitialized)
				return;

			OnEnableProcess();
		}
		void OnDisable() {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode &&
				UnityEditor.EditorApplication.isPlaying)
				return;
#endif

			if (!m_OnInitialized)
				return;

			OnDisableProcess();
		}

		public virtual void OnEnableProcess() { }
		public virtual void OnDisableProcess() { }


		public static bool TryGetAtlas(string name, out UnityEngine.U2D.SpriteAtlas atlas) {
			if (mListLoadedAtlas == null) {
				atlas = null;
				return false;
			}
			atlas = mListLoadedAtlas.Find(x => x.name == name);
			return atlas != null;
		}
		public static void AddAtlas(UnityEngine.U2D.SpriteAtlas atlas) {
			mListLoadedAtlas ??= new RefList<UnityEngine.U2D.SpriteAtlas>();
			mListLoadedAtlas.Add(atlas);
		}
		/// <summary> 리스트에서 제거되면 true, 아니면 false </summary>
		public static bool RemoveAtlas(UnityEngine.U2D.SpriteAtlas atlas) {
			return mListLoadedAtlas != null && mListLoadedAtlas.Remove(atlas);
		}
		/// <summary> 리스트에서 제거되면 true, 아니면 false </summary>
		public static bool RemoveAtlas(string name) {
			return mListLoadedAtlas != null && mListLoadedAtlas.Remove(x => x.name == name);
		}
	}
}
