
using UnityEngine;

namespace KHFC {
	public abstract class AbstractPanel : CachedComponent {
		[HideInInspector] public Transform m_TR;

		public virtual void Awake() {
			m_TR = transform;
		}


		public virtual void OnEnterDefault(GameObject obj) {
#if UNITY_EDITOR
			Debug.Log($"{GetType()} OnEnter is not registered : {obj.name}");
#endif
		}
		public virtual void OnExitDefault(GameObject obj) {
#if UNITY_EDITOR
			Debug.Log($"{GetType()} OnExit is not registered : {obj.name}");
#endif
		}
		public void OnClickDefault(GameObject obj) {
#if UNITY_EDITOR
			Debug.Log($"{GetType()} OnClick is not registered : {obj.name}");
#endif
		}
	}
}
