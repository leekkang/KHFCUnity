using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KHFC {
	public delegate void DelClick(GameObject obj);

	[DisallowMultipleComponent]
	public class ButtonWdgt : UnityEngine.UI.Selectable, IPointerClickHandler, ISubmitHandler {
		UIBase m_Parent;
		DelClick m_Click;	// 얘네들 에디터 타임에 저장할 수 있는 방법은 UnityEvent 말고 없음. NGUI도 뜯어보면 런타임에 델리게이트 찾더라.
		DelHover m_Enter;
		//DelHover m_Exit;

		[FieldName("Hover 기능 사용")]
		public bool m_EnableHover = false;
		[FieldName("Click 사운드 활성화")]
		public bool m_OnClickSound = true;
		[FieldName("Hover 사운드 활성화")]
		public bool m_OnHoverSound = false;

		[SerializeField][ShowOnly("Click Method")]
		string m_ClickFuncName;
		[SerializeField][ShowOnly("HoverEnter Method")]
		string m_EnterFuncName;
		//[SerializeField][ShowOnly("HoverExit Method")]
		//string m_ExitFuncName;


		public string m_ClickSoundName = "efx_click";
		[Range(0, 1f)] public float m_ClickSoundVolume = .5f;
		public string m_HoverSoundName = "efx_hover";
		[Range(0, 1f)] public float m_HoverSoundVolume = .5f;

#if UNITY_EDITOR
		protected override void Reset() {
			base.Reset();

			Start();
			Util.Log($"m_ClickFuncName : {m_ClickFuncName}");
			Util.Log($"m_EnterFuncName : {m_EnterFuncName}");
			//Util.Log($"m_ExitFuncName : {m_ExitFuncName}");
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

			string postfix = gameObject.name.Replace("btn_", "");
			string delName = "OnClick" + postfix;
			System.Reflection.MethodInfo info = m_Parent.GetType().GetMethod(delName, flag);
			m_Click = info != null
				? (DelClick)Delegate.CreateDelegate(typeof(DelClick), m_Parent, info)
				: (DelClick)Delegate.CreateDelegate(typeof(DelClick), m_Parent, "OnClickDefault", false, false);
#if UNITY_EDITOR
			if (m_Click == null)
				Util.LogError($"Click Delegate has nullptr\nName : {delName}, Parent : {parent.name}");
			else
				m_ClickFuncName = m_Click.GetMethodInfo().ToString();
#endif

			if (m_EnableHover)
				AllocHoverFunc();
		}

		public void AllocHoverFunc() {
			if (m_Parent == null) {
				Util.LogError($"{name} doesn't have parent");
				return;
			}
			System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Instance |
													System.Reflection.BindingFlags.Public |
													System.Reflection.BindingFlags.NonPublic;

			string postfix = gameObject.name.Replace("btn_", "");
			string delName = "OnEnter" + postfix;
			System.Reflection.MethodInfo info = m_Parent.GetType().GetMethod(delName, flag);
			m_Enter = info != null
				? (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info)
				: (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnEnterDefault");
#if UNITY_EDITOR
			if (m_Enter == null)
				Util.LogWarning($"{delName} is not founded in {m_Parent.name}");
			else
				m_EnterFuncName = m_Enter.GetMethodInfo().ToString();
#endif

//			delName = "OnExit" + postfix;
//			info = m_Parent.GetType().GetMethod(delName, flag);
//			m_Exit = info != null
//				? (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, info)
//				: (DelHover)Delegate.CreateDelegate(typeof(DelHover), m_Parent, "OnExitDefault");
//#if UNITY_EDITOR
//			if (m_Exit == null)
//				Util.LogWarning($"{delName} is not founded in {parent.name}");
//			else
//				m_ExitFuncName = m_Exit.GetMethodInfo().ToString();
//#endif
		}

		void Press() {
			if (UIBase.lockTouch || m_Parent == null)
				return;
			if (!IsActive() || !IsInteractable())
				return;
			m_Parent.OnPreClickDefault();

			if (m_OnClickSound)
				SoundMgr.inst.PlayEfx(m_ClickSoundName, m_ClickSoundVolume);

			m_Click(this.gameObject);

			UIBase.lockTouch = false;
		}

		public virtual void OnPointerClick(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			Press();
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
		//	if (!m_EnableHover)
		//		return;

		//	m_Exit(this.gameObject);
		//}


		// 아래는 Button.cs 의 코드를 그대로 복사함

		public virtual void OnSubmit(BaseEventData eventData) {
			Press();

			// if we get set disabled during the press
			// don't run the coroutine.
			if (!IsActive() || !IsInteractable())
				return;

			DoStateTransition(SelectionState.Pressed, false);
			StartCoroutine(OnFinishSubmit());
		}

		System.Collections.IEnumerator OnFinishSubmit() {
			float fadeTime = colors.fadeDuration;
			float elapsedTime = 0f;

			while (elapsedTime < fadeTime) {
				elapsedTime += Time.unscaledDeltaTime;
				yield return null;
			}

			DoStateTransition(currentSelectionState, false);
		}
	}
}
