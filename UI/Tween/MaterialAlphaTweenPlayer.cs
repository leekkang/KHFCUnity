
using UnityEngine;
using PrimeTween;

namespace KHFC {
	public class MaterialAlphaTweenPlayer : BaseRandomTweenPlayer {
		public Renderer[] m_ArrTarget;
		public TweenSettings<float> m_Setting;

		public string m_MaterialPropertyName = "_BaseColor";

		public bool m_UseRandomEndValue = false;
		public float m_MinEndValue;
		public float m_MaxEndValue;

		float[] m_ArrDefaultValue;
		int m_PropertyID;


		protected override int GetTargetCount() {
			return m_ArrTarget != null ? m_ArrTarget.Length : 0;
		}

		protected override Tween CreateSingleTweenForTarget(int index) {
			Renderer target = m_ArrTarget[index];
			if (target == null)
				return default;

			TweenSettings<float> finalSettings = GetFinalSettings(m_Setting);

			if (m_UseRandomEndValue) {
				finalSettings.endValue = Random.Range(m_MinEndValue, m_MaxEndValue);
			}

			return Tween.MaterialProperty(target.sharedMaterial, m_PropertyID, finalSettings);
		}

		protected override void SaveDefaultValue() {
			if (m_ArrTarget == null)
				return;

			m_PropertyID = Shader.PropertyToID(m_MaterialPropertyName);
			m_ArrDefaultValue = new float[m_ArrTarget.Length];
			for (int i = 0; i < m_ArrTarget.Length; i++) {
				if (m_ArrTarget[i] != null)
					m_ArrDefaultValue[i] = m_ArrTarget[i].sharedMaterial.GetFloat(m_PropertyID);
			}
		}

		protected override void RestoreDefaultValue() {
			if (m_ArrTarget == null || m_ArrDefaultValue == null)
				return;

			for (int i = 0; i < m_ArrTarget.Length; i++) {
				if (m_ArrTarget[i] != null && i < m_ArrDefaultValue.Length)
					m_ArrTarget[i].sharedMaterial.SetFloat(m_PropertyID, m_ArrDefaultValue[i]);
			}
		}
	}
}