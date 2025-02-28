using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KHFC {
	public delegate void DelClick(GameObject obj);
	public enum AllocatedType {
		None = 0,
		Custom,
		Default,
	}

	public class ButtonWdgt : UnityEngine.UI.Button {
		UIBase m_Parent;
		DelClick m_Click;	// 얘네들 에디터 타임에 저장할 수 있는 방법은 UnityEvent 말고 없음. NGUI도 뜯어보면 런타임에 델리게이트 찾더라.
		DelHover m_Enter;
		DelHover m_Exit;

		[FieldName("Hover 기능 사용")]
		public bool m_EnableHover = false;
		[FieldName("Click 사운드 활성화")]
		public bool m_OnClickSound = true;
		[FieldName("Hover 사운드 활성화")]
		public bool m_OnHoverSound = false;

		[SerializeField][ReadOnly][FieldName("Click Method 연결")]
		AllocatedType m_ClickAllocated;
		[SerializeField][ReadOnly][FieldName("HoverEnter Method 연결")]
		AllocatedType m_EnterAllocated;
		[SerializeField][ReadOnly][FieldName("HoverExit Method 연결")]
		AllocatedType m_ExitAllocated;


		public string m_ClickSoundName = "efx_click";
		[Range(0, 1f)] public float m_ClickSoundVolume = .5f;
		public string m_HoverSoundName = "efx_hover";
		[Range(0, 1f)] public float m_HoverSoundVolume = .5f;

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

			if (m_Click == null)
				Debug.LogError($"Click Delegate has nullptr\nName : {delName}, Parent : {(parent != null ? parent.name : "")}");

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
