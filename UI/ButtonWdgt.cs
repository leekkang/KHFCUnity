using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KHFC {
	public delegate void DelClick(GameObject obj);

	public class ButtonWdgt : UnityEngine.UI.Button {
		UIBase m_Parent;
		DelClick m_Click;	// 얘네들 에디터 타임에 저장할 수 있는 방법은 UnityEvent 말고 없음. NGUI도 뜯어보면 런타임에 델리게이트 찾더라.
		DelHover m_Enter;
		DelHover m_Exit;

		public bool m_EnableHover = false;
		public bool m_OnClickSound = true;
		public bool m_OnHoverSound = false;

		public string m_ClickSoundName = "efx_click";
		[Range(0, 1f)] public float m_ClickSoundVolume = .5f;
		public string m_HoverSoundName = "efx_hover";
		[Range(0, 1f)] public float m_HoverSoundVolume = .5f;

		protected override void Start() {
			Transform parent = transform.parent;

			while (parent != null && !parent.TryGetComponent(out m_Parent)) {
				parent = parent.parent;
			}
			System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Instance |
													System.Reflection.BindingFlags.Public |
													System.Reflection.BindingFlags.NonPublic;

			string postfix = gameObject.name.Replace("btn_", "");
			string delName = "OnClick" + postfix;
			System.Reflection.MethodInfo info = m_Parent.GetType().GetMethod(delName, flag);
			if (info != null)
				m_Click = (DelClick)Delegate.CreateDelegate(typeof(DelClick), m_Parent, info);
			else
				m_Click = (DelClick)Delegate.CreateDelegate(typeof(DelClick), m_Parent, "OnClickDefault", false, false);

			if (m_EnableHover) {
				delName = "OnEnter" + postfix;
				info = m_Parent.GetType().GetMethod(delName, flag);
				if (info != null)
					m_Enter = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info);
				else
					m_Enter = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnEnterDefault");
			}

			//delName = "OnExit" + postfix;
			//info = m_Parent.GetType().GetMethod(delName, flag);
			//if (info != null)
			//	m_Exit = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info);
			//else
			//	m_Exit = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnExitDefault");
		}


		public override void OnPointerClick(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			if (UIBase.lockTouch)
				return;

			m_Parent.OnPreClickDefault();

			if (m_OnClickSound)
				SoundMgr.inst.PlayEfx(m_ClickSoundName, m_ClickSoundVolume);

			m_Click(this.gameObject);

			UIBase.lockTouch = false;
		}

		public override void OnPointerEnter(PointerEventData eventData) {
			base.OnPointerEnter(eventData);
			if (!m_EnableHover)
				return;

			if (m_OnHoverSound)
				SoundMgr.inst.PlayEfx(m_HoverSoundName, m_HoverSoundVolume);

			m_Enter(this.gameObject);
		}

		//public override void OnPointerExit(PointerEventData eventData) {
		//	base.OnPointerExit(eventData);

		//	m_Exit(this.gameObject);
		//}
	}
}
