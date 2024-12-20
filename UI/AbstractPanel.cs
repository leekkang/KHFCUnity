
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KHFC {
	public abstract class AbstractPanel : UIBase {
		public const float DEFAULT_WIDTH = 1080f;
		public const float DEFAULT_HEIGHT = 1920f;

		static float m_Width;
		static float m_Height;

		[NonSerialized] public Canvas m_Canvas;
		[NonSerialized] public CanvasScaler m_Scaler;

		public override void Init() {
			base.Init();

			m_Canvas = gameObject.SafeAddComponent<Canvas>();
			m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;

			m_Scaler = gameObject.SafeAddComponent<CanvasScaler>();
			m_Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			m_Scaler.referenceResolution = new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT);
			m_Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
		}

		/// <summary> 패널 히스토리에 추가하기 전 호출. 액티브, 초기 연출을 관리 </summary>
		public virtual void Open(bool needActivate) {
			if (needActivate)
				gameObject.SetActive(true);
		}

		/// <summary> 패널 히스토리에서 제거하기 전 호출 </summary>
		/// <param name="removeOnly"> 팝업 연출 없이 제거하고 싶을 때 사용 </param>
		public virtual void Close(bool removeOnly) {
			gameObject.SetActive(false);
		}

		public void SetSortOrder(int order) {
			m_Canvas.sortingOrder = order;
		}

		/// <summary> 다음 패널에 없는 리소스들을 언로드한다. </summary>
		/// <remarks> Addressable을 사용하거나 규모가 작으면 사용하지 않아도 괜찮음 </remarks>
		public virtual void UnloadAsset(AbstractPanel nextPanel = null) { }

		/// <summary> Reference Resolution의 높이를 기준으로 정확한 화면 넓이를 구한다. 실제 해상도 비율이 16/9보다 작은 경우에만 해당됨 </summary>
		/// <remarks> Canvas Scaler의 영향을 받아 실제 해상도와 다른 경우가 발생한다. </remarks>
		protected float GetProperScreenWidth(bool reCalculate = false) {
			if (m_Width != 0f && !reCalculate)
				return m_Width;

			Vector2 resolution = m_Scaler.referenceResolution;

			float width = resolution.x;
			float ratio = (float)Screen.width / Screen.height;
			if (ratio > (resolution.x / resolution.y))
				width = resolution.y * ratio;

			m_Width = width;

			return width;
		}

		/// <summary> Reference Resolution의 넓이를 기준으로 정확한 화면 높이를 구한다. 실제 해상도 비율이 16/9보다 큰 경우에만 해당됨 </summary>
		/// <remarks> Canvas Scaler의 영향을 받아 실제 해상도와 다른 경우가 발생한다. </remarks>
		protected float GetProperScreenHeight(bool reCalculate = false) {
			if (m_Height != 0f && !reCalculate)
				return m_Height;

			Vector2 resolution = m_Scaler.referenceResolution;

			float height = resolution.y;
			float ratio = (float)Screen.height / Screen.width;
			if (ratio > (resolution.y / resolution.x))
				height = resolution.x * ratio;

			m_Height = height;

			return height;
		}
	}
}