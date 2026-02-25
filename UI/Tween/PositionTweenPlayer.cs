
using UnityEngine;
using PrimeTween;

namespace KHFC {
	public class PositionTweenPlayer : BaseRandomTweenPlayer {
		public Transform[] m_ArrTarget;
		public TweenSettings<Vector3> m_Setting;

		public bool m_UseRandomEndValue = false;
		public Vector3 m_MinEndValue;
		public Vector3 m_MaxEndValue;

		Vector3[] m_ArrDefaultValue;

		protected override int GetTargetCount() {
			return m_ArrTarget != null ? m_ArrTarget.Length : 0;
		}

		protected override Tween CreateSingleTweenForTarget(int index) {
			Transform target = m_ArrTarget[index];
			if (target == null)
				return default;

			TweenSettings<Vector3> finalSettings = GetFinalSettings(m_Setting);

			if (m_UseRandomEndValue) {
				finalSettings.endValue = new Vector3(
					Random.Range(m_MinEndValue.x, m_MaxEndValue.x),
					Random.Range(m_MinEndValue.y, m_MaxEndValue.y),
					Random.Range(m_MinEndValue.z, m_MaxEndValue.z)
				);
			}

			return Tween.LocalPosition(target, finalSettings);
		}

		protected override void SaveDefaultValue() {
			if (m_ArrTarget == null)
				return;

			m_ArrDefaultValue = new Vector3[m_ArrTarget.Length];
			for (int i = 0; i < m_ArrTarget.Length; i++) {
				if (m_ArrTarget[i] != null)
					m_ArrDefaultValue[i] = m_ArrTarget[i].localPosition;
			}
		}

		protected override void RestoreDefaultValue() {
			if (m_ArrTarget == null || m_ArrDefaultValue == null)
				return;

			for (int i = 0; i < m_ArrTarget.Length; i++) {
				if (m_ArrTarget[i] != null && i < m_ArrDefaultValue.Length)
					m_ArrTarget[i].localPosition = m_ArrDefaultValue[i];
			}
		}
	}
}