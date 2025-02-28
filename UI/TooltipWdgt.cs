using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KHFC {
	public delegate void DelHover(GameObject obj);

	public class TooltipWdgt : UnityEngine.UI.Selectable {
		UIBase m_Parent;
		DelHover m_Enter;
		DelHover m_Exit;

		[SerializeField][ReadOnly][FieldName("HoverEnter Method 연결")]
		AllocatedType m_EnterAllocated;
		[SerializeField][ReadOnly][FieldName("HoverExit Method 연결")]
		AllocatedType m_ExitAllocated;

		public bool m_EnableHover = false;
		public bool m_OnClickSound = true;
		public bool m_OnHoverSound = false;

		public string m_HoverSoundName = "efx_hover";
		[Range(0, 1f)] public float m_HoverSoundVolume = .5f;

		protected override void Start() {
			base.Awake();

			Transform parent = transform.parent;

			while (parent != null && !parent.TryGetComponent(out m_Parent)) {
				parent = parent.parent;
			}
			System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Instance |
													System.Reflection.BindingFlags.Public |
													System.Reflection.BindingFlags.NonPublic;
			string delName = "OnEnter" + gameObject.name;
			System.Reflection.MethodInfo info = m_Parent.GetType().GetMethod(delName, flag);
			if (info != null) {
				m_Enter = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info);
				m_EnterAllocated = AllocatedType.Custom;
			} else {
				m_Enter = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnEnterDefault");
				m_EnterAllocated = AllocatedType.Default;
			}

			delName = "OnExit" + gameObject.name;
			info = m_Parent.GetType().GetMethod(delName, flag);
			if (info != null) {
				m_Exit = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info);
				m_ExitAllocated = AllocatedType.Custom;
			} else {
				m_Exit = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnExitDefault");
				m_ExitAllocated = AllocatedType.Default;
			}
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
