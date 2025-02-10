using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KHFC {
	public delegate void DelHover(GameObject obj);

	public class TooltipWdgt : Selectable {
		// TODO : 해당 버튼을 사용하는 부모 패널을 특정지을 방법이 없을까? 현재는 클라이언트에 맞게 수정중임
		AbstractPanel m_Parent;
		DelHover m_Enter;
		DelHover m_Exit;

		[ReadOnly][FieldName("HoverEnter 버튼 연결")][SerializeField]
		AllocatedType m_EnterAllocated;
		[ReadOnly][FieldName("HoverExit 클릭 버튼 연결")][SerializeField]
		AllocatedType m_ExitAllocated;

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

			m_Enter(this.gameObject);
		}

		public override void OnPointerExit(PointerEventData eventData) {
			base.OnPointerExit(eventData);

			m_Exit(this.gameObject);
		}
	}
}
