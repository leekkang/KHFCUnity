using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> 유니티 레거시 애니메이션을 사용하는 오브젝트 </summary>
public class AnimationMotion : MotionBase {
	private Animator m_Animator;

	private void Awake() {
		if (m_Animator == null)
			m_Animator = GetComponent<Animator>();


	}
}