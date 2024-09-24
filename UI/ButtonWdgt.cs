using KHFC;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void DelClick(GameObject obj);

public class ButtonWdgt : Button {
	// TODO : 해당 버튼을 사용하는 부모 패널을 특정지을 방법이 없을까? 현재는 클라이언트에 맞게 수정중임
	BasePanel m_Parent;
	DelClick m_Click;	// 얘네들 에디터 타임에 저장할 수 있는 방법은 UnityEvent 말고 없음. NGUI도 뜯어보면 런타임에 델리게이트 찾더라.
	DelHover m_Enter;
	DelHover m_Exit;

	[SerializeField] public bool m_OnClickSound = true;
	[SerializeField] public bool m_OnHoverSound = true;

	protected override void Start() {
		Transform parent = transform.parent;
		
		while (parent != null && !parent.TryGetComponent(out m_Parent)) {
			parent = parent.parent;
		}

		string delName = "OnClick" + gameObject.name;
		System.Reflection.MethodInfo info = m_Parent.GetType().GetMethod(delName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (info != null)
			m_Click = (DelClick)Delegate.CreateDelegate(typeof(DelClick), m_Parent, info);
		else
			m_Click = (DelClick)Delegate.CreateDelegate(typeof(DelClick), m_Parent, "OnClickDefault", false, false);

		delName = "OnEnter" + gameObject.name;
		info = m_Parent.GetType().GetMethod(delName, BindingFlags.Instance | BindingFlags.Public  | BindingFlags.NonPublic);
		if (info != null)
			m_Enter = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info);
		else
			m_Enter = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnEnterDefault");

		//delName = "OnExit_" + gameObject.name;
		//info = m_Parent.GetType().GetMethod(delName, BindingFlags.Instance | BindingFlags.Public  | BindingFlags.NonPublic);
		//if (info != null)
		//	m_Exit = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info);
		//else
		//	m_Exit = (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnExit_Default");
	}


	public override void OnPointerClick(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Left)
			return;
		if (KHFC.UIBase.lockTouch)
			return;

		KHFC.UIBase.lockTouch = true;

		if (m_OnClickSound)
			SoundMgr.inst.PlayEfx(GlobalConst.SOUND_EFX_BUTTON_OK, GlobalConst.SOUND_BUTTON_VOLUME);
		m_Click(this.gameObject);

		KHFC.UIBase.lockTouch = false;
	}

	public override void OnPointerEnter(PointerEventData eventData) {
		base.OnPointerEnter(eventData);

		if (m_OnHoverSound)
			SoundMgr.inst.PlayEfx(GlobalConst.SOUND_EFX_BUTTON_HOVER, GlobalConst.SOUND_BUTTON_VOLUME);
		m_Enter(this.gameObject);
	}

	//public override void OnPointerExit(PointerEventData eventData) {
	//	base.OnPointerExit(eventData);

	//	m_Exit(this.gameObject);
	//}
}