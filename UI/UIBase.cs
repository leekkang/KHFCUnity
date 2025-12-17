
using System;
using UnityEngine;

namespace KHFC {
	public abstract class UIBase : CachedComponent {
		/// <summary> 모든 UI의 <see cref="Graphics"/>가 사용하는 아틀라스들의 리스트 </summary>
		static RefList<UnityEngine.U2D.SpriteAtlas> sListLoadedAtlas;

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

		public RectTransform rt => transform as RectTransform;

		bool m_OnInitialized;
		public bool initialized => m_OnInitialized;


		/// <summary> 해당 UI의 초기화에 필요한 정보를 로드하는 함수 </summary>
		public virtual void Init() {
			m_OnInitialized = true;
		}

		void OnEnable() {
			if (!m_OnInitialized)
				return;

			OnEnableProcess();
		}
		void OnDisable() {
//#if UNITY_EDITOR
//			if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !UnityEditor.EditorApplication.isPlaying)
//				return;
//#endif
			if (!m_OnInitialized)
				return;

			OnDisableProcess();
		}

		/// <summary> <see cref="Init"/> 호출 이후 동작하도록 <see cref="OnEnable"/> 을 래핑하는 함수 </summary>
		public virtual void OnEnableProcess() { }
		/// <summary> <see cref="Init"/> 호출 이후 동작하도록 <see cref="OnDisable"/> 을 래핑하는 함수 </summary>
		public virtual void OnDisableProcess() { }
		public virtual void OnDestroyProcess() { }


		protected GameObject Get(int index) => GetCachedObject(index);
		protected GameObject Get<T>(T alias) where T : System.Enum => GetCachedObject(alias);
		protected T Get<T>(Enum alias) where T : Component => GetCachedObject<T>(alias);
		protected T Get<T>(int index) where T : Component => GetCachedObject<T>(index);


		/// <summary> OnClick 함수를 실행하기 전 호출하는 함수, 보통 오작동 방지를 위해 터치제한 등을 거는 용도로 사용한다 </summary>
		public virtual void OnPreClickDefault() {
			lockTouch = true;
		}

		/// <summary> 버튼에 OnClick- 함수가 없을 때 기본적으로 호출하는 함수 </summary>
		/// <param name="obj"> 호출한 버튼 오브젝트 </param>
		public void OnClickDefault(GameObject obj) {
#if UNITY_EDITOR
			Debug.Log($"{GetType()} OnClick is not registered : {obj.name}");
#endif
		}

		/// <summary> 버튼에 OnEnter- 함수가 없을 때 기본적으로 호출하는 함수 </summary>
		/// <param name="obj"> 호출한 버튼 오브젝트 </param>
		public virtual void OnEnterDefault(GameObject obj) {
#if UNITY_EDITOR
			Debug.Log($"{GetType()} OnEnter is not registered : {obj.name}");
#endif
		}
		/// <summary> 버튼에 OnExit- 함수가 없을 때 기본적으로 호출하는 함수 </summary>
		/// <param name="obj"> 호출한 버튼 오브젝트 </param>
		public virtual void OnExitDefault(GameObject obj) {
#if UNITY_EDITOR
			Debug.Log($"{GetType()} OnExit is not registered : {obj.name}");
#endif
		}


		public static bool TryGetAtlas(string name, out UnityEngine.U2D.SpriteAtlas atlas) {
			if (sListLoadedAtlas == null) {
				atlas = null;
				return false;
			}
			atlas = sListLoadedAtlas.Find(x => x.name == name);
			return atlas != null;
		}
		public static void AddAtlas(UnityEngine.U2D.SpriteAtlas atlas) {
			sListLoadedAtlas ??= new RefList<UnityEngine.U2D.SpriteAtlas>();
			sListLoadedAtlas.Add(atlas);
		}
		/// <summary> 리스트에서 제거되면 true, 아니면 false </summary>
		public static bool RemoveAtlas(UnityEngine.U2D.SpriteAtlas atlas) {
			return sListLoadedAtlas != null && sListLoadedAtlas.Remove(atlas);
		}
		/// <summary> 리스트에서 제거되면 true, 아니면 false </summary>
		public static bool RemoveAtlas(string name) {
			return sListLoadedAtlas != null && sListLoadedAtlas.Remove(x => x.name == name);
		}
	}
}