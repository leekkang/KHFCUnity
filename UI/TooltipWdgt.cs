using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KHFC {
	public delegate void DelHover(GameObject obj);

	[DisallowMultipleComponent]
	public class TooltipWdgt : UnityEngine.UI.Selectable {
		UIBase m_Parent;
		DelHover m_Enter;
		DelHover m_Exit;

		[SerializeField][ShowOnly("HoverEnter Method")]
		string m_EnterFuncName;
		[SerializeField][ShowOnly("HoverExit Method")]
		string m_ExitFuncName;

		public bool m_EnableHover = false;
		public bool m_OnClickSound = true;
		public bool m_OnHoverSound = false;

		public string m_HoverSoundName = "efx_hover";
		[Range(0, 1f)] public float m_HoverSoundVolume = .5f;

#if UNITY_EDITOR
		protected override void Reset() {
			base.Reset();
			Start();
			Util.Log($"m_EnterFuncName : {m_EnterFuncName}");
			Util.Log($"m_ExitFuncName : {m_ExitFuncName}");
		}
#endif

		protected override void Start() {
			Transform parent = transform.parent;

			while (parent != null && !parent.TryGetComponent(out m_Parent)) {
				parent = parent.parent;
			}
			if (parent == null) {
				Util.LogError($"{name} doesn't have parent");
				return;
			}
			System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Instance |
													System.Reflection.BindingFlags.Public |
													System.Reflection.BindingFlags.NonPublic;
			string delName = "OnEnter" + gameObject.name;
			System.Reflection.MethodInfo info = m_Parent.GetType().GetMethod(delName, flag);
			m_Enter = info != null
				? (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info)
				: (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnEnterDefault");
#if UNITY_EDITOR
			if (m_Enter == null)
				Util.LogWarning($"{delName} is not founded in {parent.name}");
			else
				m_EnterFuncName = m_Enter.GetMethodInfo().ToString();
#endif

			delName = "OnExit" + gameObject.name;
			info = m_Parent.GetType().GetMethod(delName, flag);
			m_Exit = info != null
				? (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info)
				: (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnExitDefault");
#if UNITY_EDITOR
			if (m_Exit == null)
				Util.LogWarning($"{delName} is not founded in {parent.name}");
			else
				m_ExitFuncName = m_Exit.GetMethodInfo().ToString();
#endif
		}

		public override void OnPointerEnter(PointerEventData eventData) {
			base.OnPointerEnter(eventData);

			if (m_OnHoverSound)
				SoundMgr.inst.PlayEfx(m_HoverSoundName, m_HoverSoundVolume);

			m_Enter(this.gameObject);
		}

		public override void OnPointerExit(PointerEventData eventData) {
			base.OnPointerExit(eventData);

			m_Exit(this.gameObject);
		}
	}
}