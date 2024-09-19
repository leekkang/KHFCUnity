using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 스프라이트 변경을 통해 애니메이션 흉내를 내는 오브젝트 </summary>
public class SpriteMotion : MotionBase {
	/// <summary> 스프라이트 애니메이션의 한 장면 </summary>
	[Serializable]
	protected class SpriteFrame {
		/// <summary> 현재 장면의 스프라이트 인덱스 </summary>
		public int m_Index;
		public float m_Time;
	}

	/// <summary> 스프라이트 애니메이션 </summary>
	[Serializable]
	protected class SpriteSequence {
		/// <summary> 스프라이트 이름 포맷 </summary>
		public string m_Format;
		/// <summary> 스프라이트를 변경할 대상 </summary>
		public UnityEngine.SpriteRenderer m_Renderer;
		/// <summary> 스프라이트 순서 </summary>
		public List<SpriteFrame> m_ListFrame;

		/// <summary> 반복 여부 </summary>
		public bool m_Loop;
		/// <summary> 현재 재생중인지 확인 </summary>
		public bool m_Playing;
		/// <summary> 타임스케일을 무시할 것인지 </summary>
		public bool m_IgnoreTimeScale = false;
		/// <summary> 애니메이션 총 길이 (sec) </summary>
		public float m_Length;
	};

	/// <summary> 스프라이트 애니메이션을 사용하는 오브젝트 </summary>
	[SerializeField]
	List<SpriteSequence> m_ListSeq;

	public void Awake() {
		m_ListSeq = new List<SpriteSequence>();
	}

	public int m_CurIndex = 0;

	public UnityEngine.Sprite[] frames;

	UnityEngine.SpriteRenderer mUnitySprite;
	//float mUpdate = 0f;

	public bool isPlaying { get { return enabled; } }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="type"> 색 구분이 없는 스프라이트의 경우 디폴트 값을 사용 </param>
	public void Play(ColorType type = ColorType.None) {
		
	}

	///// <summary>
	///// Continue playing the animation. If the animation has reached the end, it will restart from beginning
	///// </summary>
	//public void Play() {
	//	if (frames != null && frames.Length > 0) {
	//		if (!enabled && !loop) {
	//			int newIndex = framerate > 0 ? frameIndex + 1 : frameIndex - 1;
	//			if (newIndex < 0 || newIndex >= frames.Length)
	//				frameIndex = framerate < 0 ? frames.Length - 1 : 0;
	//		}

	//		enabled = true;
	//		UpdateSprite();
	//	}
	//}

	///// <summary>
	///// Pause the animation.
	///// </summary>

	//public void Pause() { enabled = false; }

	///// <summary>
	///// Reset the animation to the beginning.
	///// </summary>

	//public void ResetToBeginning() {
	//	frameIndex = framerate < 0 ? frames.Length - 1 : 0;
	//	UpdateSprite();
	//}

	///// <summary>
	///// Start playing the animation right away.
	///// </summary>

	//void Start() { Play(); }

	///// <summary>
	///// Advance the animation as necessary.
	///// </summary>

	//void Update() {
	//	if (frames == null || frames.Length == 0) {
	//		enabled = false;
	//	} else if (framerate != 0) {
	//		float time = ignoreTimeScale ? RealTime.time : Time.time;

	//		if (mUpdate < time) {
	//			mUpdate = time;
	//			int newIndex = framerate > 0 ? frameIndex + 1 : frameIndex - 1;

	//			if (!loop && (newIndex < 0 || newIndex >= frames.Length)) {
	//				enabled = false;
	//				return;
	//			}

	//			frameIndex = NGUIMath.RepeatIndex(newIndex, frames.Length);
	//			UpdateSprite();
	//		}
	//	}
	//}

	//void UpdateSprite() {
	//	if (mUnitySprite == null && mNguiSprite == null) {
	//		mUnitySprite = GetComponent<UnityEngine.SpriteRenderer>();
	//		mNguiSprite = GetComponent<UI2DSprite>();

	//		if (mUnitySprite == null && mNguiSprite == null) {
	//			enabled = false;
	//			return;
	//		}
	//	}

	//	float time = ignoreTimeScale ? RealTime.time : Time.time;
	//	if (framerate != 0)
	//		mUpdate = time + Mathf.Abs(1f / framerate);

	//	if (mUnitySprite != null) {
	//		mUnitySprite.sprite = frames[frameIndex];
	//	} else if (mNguiSprite != null) {
	//		mNguiSprite.nextSprite = frames[frameIndex];
	//	}
	//}
}
