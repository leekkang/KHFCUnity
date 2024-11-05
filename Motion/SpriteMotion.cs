using KHFC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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
		/// <summary> 스프라이트 순서 </summary>
		public List<SpriteFrame> m_ListFrame;

		/// <summary> 현재 재생중인지 확인 </summary>
		public bool m_Playing;
		/// <summary> 반복 여부 </summary>
		public bool m_Loop;
		/// <summary> 역재생 여부 </summary>
		public bool m_Reverse;
		/// <summary> 타임스케일을 무시할 것인지 </summary>
		public bool m_IgnoreTimeScale = false;
		/// <summary> 애니메이션 총 길이 (sec) </summary>
		public float m_Length;
		/// <summary> 루프 시작 프레임, 첫 플레이 시 0에서 시작, 끝까지 가면 해당 프레임 부터 다시 시작함 </summary>
		public int m_LoopStartFrame = -1;

		public bool isPlayable => m_ListFrame != null && m_ListFrame.Count > 0;

		public void SetFrameTime(float time) {
			for (int i = 0, count = m_ListFrame.Count; i < count; ++i) {
				m_ListFrame[i].m_Time = time;
			}
		}

		public void MakeFrame(int count) {
			float time = m_Length / count;
			m_ListFrame ??= new List<SpriteFrame>();
			for (int i = 0; i < count; ++i) {
				m_ListFrame.Add(new SpriteFrame() { m_Index = i, m_Time = time });
			}
		}

		/// <summary> 시작 프레임을 반환한다. </summary>
		public int GetStartFrame() {
			return m_Reverse ? m_ListFrame.Count - 1 : 0;
		}

		/// <summary> 다음 프레임 인덱스를 반환한다. 멈춰야 하면 -1을 반환한다. </summary>
		public int GetNextFrame(int curIndex) {
			if (m_Reverse) {
				--curIndex;
				if (curIndex < 0)
					curIndex = m_Loop ? (m_LoopStartFrame != -1 ? m_LoopStartFrame : m_ListFrame.Count - 1) : -1;
			} else {
				++curIndex;
				if (curIndex >= m_ListFrame.Count)
					curIndex = m_Loop ? (m_LoopStartFrame != -1 ? m_LoopStartFrame : 0) : -1;
			}
			return curIndex;
		}
	};
	
	/// <summary> 스프라이트 리스트 </summary>
	[KHFC.FieldName("스프라이트들")] public UnityEngine.Sprite[] m_ArrSprite;

	/// <summary> 스프라이트 애니메이션을 사용하는 오브젝트 </summary>
	[SerializeField] List<SpriteSequence> m_ListSeq;

	/// <summary> 현재 재생에 사용하는 시퀀스 인덱스 </summary>
	[ReadOnly] public int m_CurSeqIndex = 0;
	/// <summary> 현재 재생하는 시퀀스의 <see cref="SpriteFrame"/> 리스트 인덱스 </summary>
	[ReadOnly] public int m_CurFrame = 0;


	public UnityEngine.SpriteRenderer m_SpriteRenderer;
	public UnityEngine.UI.Image m_UGUIRenderer;

	public bool isPlaying { get => enabled && m_ListSeq[m_CurSeqIndex].m_Playing; }

	float m_UpdateTime = 0f;


#if UNITY_EDITOR
	/// <summary> 현재 지정되어 있는 스프라이트들로 디폴트 시퀀스를 하나 생성한다. 시간은 1초이다. </summary>
	[InspectorButton("1초짜리 기본 시퀀스 생성 (스프라이트가 배열에 존재해야 함)")]
	public void MakeDefaultSequence() {
		if (m_ArrSprite.Length > 0) {
			m_ListSeq.Add(new SpriteSequence());
			m_ListSeq[^1].MakeFrame(m_ArrSprite.Length);
			SetPlayTime(1f, m_ListSeq.Count - 1);
		}
	}
#endif

	public void Awake() {
		m_ListSeq = new List<SpriteSequence>();
	}

	void Start() {
		Play();
	}

	void Update() {
		if (m_CurSeqIndex >= m_ListSeq.Count)
			return;

		SpriteSequence seq = m_ListSeq[m_CurSeqIndex];
		if (!seq.isPlayable || seq.m_Length <= 0f) {
			enabled = seq.m_Playing = false;
			return;
		}

		float time = seq.m_IgnoreTimeScale ? Time.unscaledTime : Time.time;
		if (m_UpdateTime < time) {
			m_UpdateTime = time;
			int newIndex = seq.GetNextFrame(m_CurFrame);
			if (newIndex == -1) {
				enabled = seq.m_Playing = false;
				return;
			}

			m_CurFrame = newIndex;
			UpdateSprite();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="type"> 색 구분이 없는 스프라이트의 경우 디폴트 값을 사용 </param>
	public void Play(Color color) {

	}

	/// <summary>
	/// Continue playing the animation. If the animation has reached the end, it will restart from beginning
	/// </summary>
	public void Play(int seqIndex = 0) {
		if (m_ListSeq.Count >= seqIndex)
			return;

		bool anotherSequence = seqIndex != m_CurSeqIndex;
		if (anotherSequence) {
			m_ListSeq[m_CurSeqIndex].m_Playing = false;
			m_CurSeqIndex = seqIndex;
		}

		SpriteSequence seq = m_ListSeq[m_CurSeqIndex];
		if (seq.isPlayable) {
			if (anotherSequence)
				m_CurFrame = seq.GetStartFrame();
			else if (!enabled && !seq.m_Loop) {		// 멈춰있었으면 다시 재생
				m_CurFrame = seq.GetNextFrame(m_CurFrame);
				if (m_CurFrame == -1)
					m_CurFrame = seq.GetStartFrame();
			}

			enabled = seq.m_Playing = true;
			UpdateSprite();
		}
	}

	/// <summary>
	/// Pause the animation.
	/// </summary>
	public void Pause() {
		m_ListSeq[m_CurSeqIndex].m_Playing = false;
		enabled = false;
	}

	/// <summary> 시퀀스의 전체 플레이타임을 지정한다. 각 프레임 별 시간도 동일한 값으로 재설정한다. (frame) </summary>
	public void SetPlayTime(int frame, int seqIndex = -1) {
		if (seqIndex == -1)
			seqIndex = m_CurSeqIndex;
		m_ListSeq[seqIndex].m_Reverse = frame < 0;
		float frameTime = 1f / Math.Abs(frame);
		m_ListSeq[seqIndex].SetFrameTime(frameTime);
		m_ListSeq[seqIndex].m_Length = frameTime * m_ListSeq[seqIndex].m_ListFrame.Count;
	}

	/// <summary> 시퀀스의 전체 플레이타임을 지정한다. 각 프레임 별 시간도 동일한 값으로 재설정한다. (sec) </summary>
	/// <param name="seqIndex"> 특정 시퀀스를 지정하고 싶을 때 값 추가, -1이면 <see cref="m_CurSeqIndex"/> 를 사용한다. </param>
	public void SetPlayTime(float playTime, int seqIndex = -1) {
		if (seqIndex == -1)
			seqIndex = m_CurSeqIndex;
		m_ListSeq[seqIndex].m_Reverse = playTime < 0f;
		playTime = Mathf.Abs(playTime);
		float frameTime = playTime / m_ListSeq[seqIndex].m_ListFrame.Count;
		m_ListSeq[seqIndex].SetFrameTime(frameTime);
		m_ListSeq[seqIndex].m_Length = playTime;
	}

	/// <summary>
	/// Reset the animation to the beginning.
	/// </summary>
	public void ResetToBeginning() {
		m_CurFrame = m_ListSeq[m_CurSeqIndex].GetStartFrame();
		UpdateSprite();
	}

	void UpdateSprite() {
		if (m_SpriteRenderer == null && m_UGUIRenderer == null) {
			m_SpriteRenderer = GetComponent<UnityEngine.SpriteRenderer>();
			m_UGUIRenderer = GetComponent<UnityEngine.UI.Image>();

			if (m_SpriteRenderer == null && m_UGUIRenderer == null) {
				enabled = false;
				return;
			}
		}

		float time = m_ListSeq[m_CurSeqIndex].m_IgnoreTimeScale ? Time.unscaledTime : Time.time;
		SpriteFrame frame = m_ListSeq[m_CurSeqIndex].m_ListFrame[m_CurFrame];
		if (m_ListSeq[m_CurSeqIndex].isPlayable)
			m_UpdateTime = time + frame.m_Time;

		if (m_SpriteRenderer != null)
			m_SpriteRenderer.sprite = m_ArrSprite[frame.m_Index];
		else if (m_UGUIRenderer != null)
			m_UGUIRenderer.sprite = m_ArrSprite[frame.m_Index];
	}
}