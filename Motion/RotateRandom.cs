
using System;
using System.Collections;
using UnityEngine;

/// <summary> 속도를 랜덤으로 받는 회전 컴포넌트 </summary>
[DisallowMultipleComponent]
//[UnityEditor.CanEditMultipleObjects]
public class RotateRandom : MonoBehaviour {
	/// <summary> 현재 회전을 하고 있는지 확인, 주의 : 부드러운 정지 시 속력이 있으면 true임 </summary>
	[ReadOnly]
	[SerializeField] bool m_Play;
	//[ShowOnly("현재 속력")]
	[KHFC.FieldName("현재 속력")]
	[SerializeField] float m_Speed;

	[KHFC.FieldName("최소 속력")]
	[SerializeField] float m_Min = 45f;
	[KHFC.FieldName("최대 속력")]
	[SerializeField] float m_Max = 60f;

	/// <summary> 반대로 도는 것을 포함할 것인가 </summary>
	[Tooltip("반대 방향으로 도는 것을 포함할 수 있다.")]
	[KHFC.FieldName("반대 방향 허용")]
	[SerializeField] bool m_AllowReverse = true;

	/// <summary> 속력을 변화시킬 때 변화 시간의 최소 간격 </summary>
	[Space(12)]
	[Header("부드러운 속력 변화에 필요한 값")]
	[KHFC.FieldName("최소 변화 시간")]
	[SerializeField] float m_MinInterval = .1f;

	Coroutine m_SoftStop;

	public bool isPlay => m_Play;
	public float speed => m_Speed;
	public float min => m_Min;
	public float max => m_Max;
	public bool rotateClockwise => m_Speed < 0;
	public bool allowReverse { get => m_AllowReverse; set => m_AllowReverse = value; }

	/// <summary> <paramref name="delay"/>를 지정하면 속력이 부드럽게 변화한다. </summary>
	public void SetSpeed(float speed, float delay = 0f, EasingFunction.Ease easeType = EasingFunction.Ease.Linear) {
		m_Min = speed;
		m_Max = speed;
		SetSpeedInternal(speed, delay, easeType);
	}

	/// <summary> <paramref name="delay"/>를 지정하면 속력이 부드럽게 변화한다. </summary>
	public void SetMinMax(float min, float max, float delay = 0f, EasingFunction.Ease easeType = EasingFunction.Ease.Linear) {
		m_Min = min;
		m_Max = max;
		float speed = GetRandomSpeed();
		SetSpeedInternal(speed, delay, easeType);
	}

	public void Stop(float delay = 0f, EasingFunction.Ease easeType = EasingFunction.Ease.Linear) {
		SetSpeedInternal(0, delay, easeType);
	}

	/// <summary> 지정한 최대 최소값을 기준으로 랜덤하게 속력을 변경한다. </summary>
	public void ChangeSpeedRandom() {
		float speed = GetRandomSpeed();
		SetSpeedInternal(speed, 0f, EasingFunction.Ease.Linear);
	}


	void Awake() {
		m_Speed = GetRandomSpeed();
		m_Play = !KHFC.Utility.FloatEqual(m_Speed, 0f);
		m_SoftStop = null;
	}

	void Update() {
		if (!m_Play)
			return;

		transform.Rotate(m_Speed * Time.deltaTime * new Vector3(0, 0, 1));
	}

	void SetSpeedInternal(float speed, float delay, EasingFunction.Ease easeType) {
		if (m_SoftStop != null)
			StopCoroutine(m_SoftStop);

		// 멈췄다 다시 돌아가기 시작할 때를 위해 켜는 부분만 따로 지정
		if (!KHFC.Utility.FloatEqual(speed, 0f))
			m_Play = true;

		if (!gameObject.activeInHierarchy || delay < m_MinInterval) {
			m_Speed = speed;
			return;
		}

		m_SoftStop = StartCoroutine(CoSoftChangeSpeed(speed, delay, easeType));
	}

	/// <summary> 시간을 기준으로 천천히 속력이 변화하는 모션을 수행 </summary>
	IEnumerator CoSoftChangeSpeed(float endSpeed, float delay, EasingFunction.Ease easeType) {
		int count = Mathf.CeilToInt(delay / m_MinInterval);
		//float deltaSpeed = (endSpeed - m_Speed) / (count + 1);
		float deltaTime = 1f / (count + 1);

		float accTime = 0f;
		float startSpeed = m_Speed;
		EasingFunction.Function func = EasingFunction.GetEasingFunction(easeType);
		while (delay > 0f) {
			accTime += deltaTime;
			m_Speed = func(startSpeed, endSpeed, accTime);
			delay -= m_MinInterval;
			yield return new WaitForSeconds(m_MinInterval);
		}

		m_Speed = endSpeed;
		m_Play = !KHFC.Utility.FloatEqual(m_Speed, 0f);
	}


	float GetRandomSpeed() {
		int sign = m_AllowReverse ? (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1) : 1;

		if (KHFC.Utility.FloatEqual(m_Min, m_Max))
			return sign * m_Min;

		ValidateMinMax();
		return sign * UnityEngine.Random.Range(m_Min, m_Max);
		//return sign * AbilityManager.inst.m_Rand.Next(m_Min, m_Max);
	}

	void ValidateMinMax() {
		if (m_Min > m_Max)
			(m_Min, m_Max) = (m_Max, m_Min);
	}
}
