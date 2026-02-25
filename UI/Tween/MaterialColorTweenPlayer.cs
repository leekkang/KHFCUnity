
using UnityEngine;
using PrimeTween;

namespace KHFC {
	public class MaterialColorTweenPlayer : BaseRandomTweenPlayer {
		public Renderer[] m_ArrTarget;
		public TweenSettings<Color> m_Setting;

		public string m_MaterialPropertyName = "_BaseColor";

		public bool m_UseRandomEndValue = false;
		public Color m_MinEndValue;
		public Color m_MaxEndValue;

		Color[] m_ArrDefaultValue;
		int m_PropertyID;


		protected override int GetTargetCount() {
			return m_ArrTarget != null ? m_ArrTarget.Length : 0;
		}

		protected override Tween CreateSingleTweenForTarget(int index) {
			Renderer target = m_ArrTarget[index];
			if (target == null)
				return default;

			TweenSettings<Color> finalSettings = GetFinalSettings(m_Setting);

			if (m_UseRandomEndValue) {
				finalSettings.endValue = new Color(
					Random.Range(m_MinEndValue.r, m_MaxEndValue.r),
					Random.Range(m_MinEndValue.g, m_MaxEndValue.g),
					Random.Range(m_MinEndValue.b, m_MaxEndValue.b),
					Random.Range(m_MinEndValue.a, m_MaxEndValue.a)
				);
			}

			return Tween.MaterialColor(target.sharedMaterial, m_PropertyID, finalSettings);
		}

		protected override void SaveDefaultValue() {
			if (m_ArrTarget == null)
				return;

			m_PropertyID = Shader.PropertyToID(m_MaterialPropertyName);
			m_ArrDefaultValue = new Color[m_ArrTarget.Length];
			for (int i = 0; i < m_ArrTarget.Length; i++) {
				if (m_ArrTarget[i] != null)
					m_ArrDefaultValue[i] = m_ArrTarget[i].sharedMaterial.GetColor(m_PropertyID);
			}
		}

		protected override void RestoreDefaultValue() {
			if (m_ArrTarget == null || m_ArrDefaultValue == null)
				return;

			for (int i = 0; i < m_ArrTarget.Length; i++) {
				if (m_ArrTarget[i] != null && i < m_ArrDefaultValue.Length)
					m_ArrTarget[i].sharedMaterial.SetColor(m_PropertyID, m_ArrDefaultValue[i]);
			}
		}
	}
}