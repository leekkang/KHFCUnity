using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> 게임 보드 위에 표시하는 오브젝트의 렌더링 파트를 담당하는 클래스 </summary>
public class MotionBase : MonoBehaviour {
	[Serializable]
	public class MotionSprite {
		[Space(10)]
		public List<Sprite> m_ListImage;
	}
	/// <summary>
	/// 현재 렌더링 대상 오브젝트가 사용하는 모든 스프라이트들을 모아놓은 리스트
	/// </summary>
	/// <remarks> 리스트의 인덱스는 <see cref="ColorType"/> 의 값과 동일하다 </remarks>
	[SerializeField]
	protected List<MotionSprite> m_ListSprite;

	public float m_Test;

#if UNITY_EDITOR
	public virtual void UpdateSpriteList() {
		Debug.Log("UpdateSpriteList method was not overrided : MotionBase");
	}

#endif
}
