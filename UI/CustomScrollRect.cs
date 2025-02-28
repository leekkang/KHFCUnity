
using UnityEngine;
using UnityEngine.UI;

namespace KHFC {
	[DisallowMultipleComponent]
	public class CustomScrollRect : ScrollRect {
		[HideInInspector] public Transform m_TR;

		static float m_Width;
		static float m_Height;

		protected override void Awake() {
			base.Awake();
			m_TR = transform;
		}

		/// <summary> Reference Resolution의 높이를 기준으로 정확한 화면 넓이를 구한다. 실제 해상도 비율이 16/9보다 작은 경우에만 해당됨 </summary>
		/// <remarks> Canvas Scaler의 영향을 받아 실제 해상도와 다른 경우가 발생한다. </remarks>
		protected float GetProperScreenWidth(bool reCalculate = false) {
			if (m_Width != 0f && !reCalculate)
				return m_Width;

			float width = 1080f;
			float ratio = (float)Screen.width / Screen.height;
			if (ratio > (9f / 16f))
				width = 1920f * ratio;

			m_Width = width;

			return width;
		}

		/// <summary> Reference Resolution의 넓이를 기준으로 정확한 화면 높이를 구한다. 실제 해상도 비율이 16/9보다 큰 경우에만 해당됨 </summary>
		/// <remarks> Canvas Scaler의 영향을 받아 실제 해상도와 다른 경우가 발생한다. </remarks>
		protected float GetProperScreenHeight(bool reCalculate = false) {
			if (m_Height != 0f && !reCalculate)
				return m_Height;

			float height = 1920f;
			float ratio = (float)Screen.height / Screen.width;
			if (ratio > (16f / 9f))
				height = 1080f * ratio;

			m_Height = height;

			return height;
		}

		/// <summary> OnClick 함수를 실행하기 전 호출하는 함수, 보통 오작동 방지를 위해 터치제한 등을 거는 용도로 사용한다 </summary>
		public virtual void OnPreClickDefault() {
			UIBase.lockTouch = true;
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