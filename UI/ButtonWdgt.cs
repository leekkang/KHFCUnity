using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KHFC {
	public delegate void DelClick(GameObject obj);

	public enum AllocatedType {
		None = 0,
		Custom,
		Default,
	}

	public class ButtonWdgt : Button {
		// TODO : 해당 버튼을 사용하는 부모 패널을 특정지을 방법이 없을까? 현재는 클라이언트에 맞게 수정중임
		AbstractPanel m_Parent;
		DelClick m_Click;	// 얘네들 에디터 타임에 저장할 수 있는 방법은 UnityEvent 말고 없음. NGUI도 뜯어보면 런타임에 델리게이트 찾더라.
		DelHover m_Enter;
		DelHover m_Exit;

		[FieldName("Hover 기능 사용")]
		public bool m_EnableHover = false;
		[FieldName("Click 사운드 활성화")]
		public bool m_OnClickSound = true;
		[FieldName("Hover 사운드 활성화")]
		public bool m_OnHoverSound = false;

		[ReadOnly][FieldName("Click 버튼 연결")][SerializeField]
		AllocatedType m_ClickAllocated;
		[ReadOnly][FieldName("HoverEnter 버튼 연결")][SerializeField]
		AllocatedType m_EnterAllocated;
		[ReadOnly][FieldName("HoverExit 클릭 버튼 연결")][SerializeField]
		AllocatedType m_ExitAllocated;

		protected override void Start() {
			Transform parent = transform.parent;

			while (parent != null && !parent.TryGetComponent(out m_Parent)) {
				parent = parent.parent;
			}
			m_ClickAllocated = AllocatedType.None;
			m_EnterAllocated = AllocatedType.None;
			m_ExitAllocated = AllocatedType.None;
			System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Instance |
													System.Reflection.BindingFlags.Public |
													System.Reflection.BindingFlags.NonPublic;

			string postfix = gameObject.name.Replace("btn_", "");
			string delName = "OnClick" + postfix;
			System.Reflection.MethodInfo info = m_Parent.GetType().GetMethod(delName, flag);
			if (info != null) {
				m_Click = (DelClick)Delegate.CreateDelegate(typeof(DelClick), m_Parent, info);
				m_ClickAllocated = AllocatedType.Custom;
			} else {
				m_Click = (DelClick)Delegate.CreateDelegate(typeof(DelClick), m_Parent, "OnClickDefault", false, false);
				m_ClickAllocated = AllocatedType.Default;
			}

			if (m_EnableHover) {
				delName = "OnEnter" + postfix;
				info = m_Parent.GetType().GetMethod(delName, flag);
				if (info != null) {
					m_Enter = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info);
					m_EnterAllocated = AllocatedType.Custom;
				} else {
					m_Enter = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnEnterDefault");
					m_EnterAllocated = AllocatedType.Default;
				}
			}

			//delName = "OnExit" + postfix;
			//info = m_Parent.GetType().GetMethod(delName, flag);
			//if (info != null) {
			//	m_Exit = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info);
			//	m_ExitAllocated = AllocatedType.Custom;
			//} else {
			//	m_Exit = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnExitDefault");
			//	m_ExitAllocated = AllocatedType.Default;
			//}
		}


		public override void OnPointerClick(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			if (UIBase.lockTouch)
				return;

			m_Parent.OnPreClickDefault();

			if (m_OnClickSound)
				SoundMgr.inst.PlayEfx(GlobalConst.SOUND_EFX_BUTTON_OK, GlobalConst.SOUND_BUTTON_VOLUME);

			m_Click(this.gameObject);

			UIBase.lockTouch = false;
		}

		public override void OnPointerEnter(PointerEventData eventData) {
			base.OnPointerEnter(eventData);
			if (!m_EnableHover)
				return;

			if (m_OnHoverSound)
				SoundMgr.inst.PlayEfx(GlobalConst.SOUND_EFX_BUTTON_HOVER, GlobalConst.SOUND_BUTTON_VOLUME);

			m_Enter(this.gameObject);
		}

		//public override void OnPointerExit(PointerEventData eventData) {
		//	base.OnPointerExit(eventData);

		//	m_Exit(this.gameObject);
		//}
	}
}
