
using UnityEngine;

/// <summary> 시계바늘처럼 움직이고 싶을 때 사용하는 회전 컴포넌트 </summary>
[DisallowMultipleComponent]
[ExecuteInEditMode]
//[UnityEditor.CanEditMultipleObjects]
public class RotateClock : MonoBehaviour {
	/// <summary> 현재 회전을 하고 있는지 확인 </summary>
	[ReadOnly]
	[SerializeField] bool m_Play = true;
	/// <summary> 시작할 때 바로 회전을 적용할 것인가 인터벌 이후 적용할 것인가 </summary>
	[SerializeField] bool m_RotateImmediately = false;


	/// <summary> 한번에 돌릴 각도 </summary>
	[KHFC.FieldName("한번에 돌릴 각도")] 
	[SerializeField] float m_Angle = -30f;
	/// <summary> 각도 변경 시간 간격 </summary>
	[KHFC.FieldName("각도 변경 시간 간격")]
	[SerializeField] float m_Interval = 1f;

	/// <summary> 각도 변경에 걸리는 시간 </summary>
	[KHFC.FieldName("각도 변경에 걸리는 시간")]
	[SerializeField] float m_AngleChangeTime = .05f;

	/// <summary> 각도가 변경될 때 마다 호출하는 콜백함수 </summary>
	System.Action m_OnChangeRotation;

	float m_Count;

	Transform m_TR;				// 캐싱용

	bool m_ChangeAngle = false;
	float m_DestAngle;			// 각도 변경 시 필요한 임시 각도
	float m_DeltaAngle;			// 각도 변경 시 시간에 따른 변경값
	float m_TmpAngle;			// 각도 변경 시 임시 저장값

	public bool isPlay => m_Play;
	public float angle {
		get => m_Angle;
		set {
			m_Angle = value;
			if (KHFC.Utility.FloatEqual(m_Angle, 0f))
				m_Play = false;
			m_DeltaAngle = m_Angle / m_AngleChangeTime;
		}
	}
	public float interval {
		get => m_Interval;
		set {
			m_Interval = value;
			if (KHFC.Utility.FloatEqual(m_Interval, 0f))
				m_Play = false;
		}
	}
	public bool rotateClockwise => m_Angle < 0;
	public System.Action onChange {
		private get => m_OnChangeRotation;
		set { m_OnChangeRotation = value; }
	}

	public void Play() {
		m_Play = true;
	}

	public void Stop() {
		m_Play = false;
		m_Count = m_RotateImmediately ? 0f : m_Interval;
	}

	private void Awake() {
		m_TR = transform;
		m_DeltaAngle = m_Angle / m_AngleChangeTime;
	}

	void Update() {
		if (!m_Play)
			return;

		float deltaTime = Time.deltaTime;
		if (m_ChangeAngle) {
			Vector3 angle = m_TR.localEulerAngles;
			m_TmpAngle += m_DeltaAngle * deltaTime;

			if ((m_Angle > 0f && m_TmpAngle > m_DestAngle) ||
				(m_Angle < 0f && m_TmpAngle < m_DestAngle)) {
				m_TmpAngle = m_DestAngle;
				m_ChangeAngle = false;
			}

			
			angle.z = KHFC.Utility.ClampAngle(m_TmpAngle, 0);
			//angle.z = KHFC.Utility.ClampAngle(angle.z, 0);
			//m_TR.localRotation = Quaternion.Euler(angle);
			m_TR.localEulerAngles = angle;
		}
		m_Count -= deltaTime;
		if (m_Count > 0)
			return;

		// 각도 변경시간이 너무 짧으면 일반 회전과 동일하게 처리한다.
		if (m_AngleChangeTime > 0.01f) {
			m_ChangeAngle = true;
			m_TmpAngle = m_TR.localEulerAngles.z;
			m_DestAngle = m_TR.localEulerAngles.z + m_Angle;
		} else {
			//m_Angle = KHFC.Utility.ClampAngle(m_Angle, 0);
			m_TR.Rotate(new Vector3(0, 0, m_Angle));
		}
		m_OnChangeRotation?.Invoke();
		m_Count = m_Interval;
	}
}
