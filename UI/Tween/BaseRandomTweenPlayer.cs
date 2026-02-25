
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KHFC {
	public abstract class BaseRandomTweenPlayer : MonoBehaviour {
		[FieldName("루프 활성화")]
		public bool m_IsLoop = true;
		[FieldName("루프 시 다음 재생 사이 랜덤 딜레이 사용")]
		public bool m_UseRandomDelay = true;
		public float m_MinDelay = 0.5f;
		public float m_MaxDelay = 2.0f;

		[FieldName("랜덤 트윈 길이 사용")]
		public bool m_UseRandomDuration = false;
		public float m_MinDuration = 0.5f;
		public float m_MaxDuration = 1.5f;

		[Header("Random Ease Curves (Optional)")]
		[Tooltip("여기에 커브를 넣으면 기존 Ease를 무시하고 무작위로 적용됩니다.")]
		public AnimationCurve[] m_ArrRandomCurve;

		[FieldName("트윈 정지 시 원래 값으로 원복")]
		public bool m_RestoreOnStop = true;

		// 런타임 독립 루프 관리를 위한 시퀀스 배열
		protected Sequence[] m_ArrRuntimeSequence;
		// 에디터 스크러빙을 위한 단일 통합 시퀀스
		protected Sequence m_EditorScrubSequence;

		protected bool m_IsPlayingInfinite;
		bool m_IsDefaultSaved = false;

		class TweenLoopState {
			public BaseRandomTweenPlayer instance;
			public int index;
		}
		TweenLoopState[] m_ArrLoopState;

		void OnDisable() {
			StopAllTweens();	// 비활성화 시 자동 정지 및 원복
		}

		public void Play() {
			// 최초 실행 시 초기값 캐싱
			if (!m_IsDefaultSaved) {
				SaveDefaultValue();
				m_IsDefaultSaved = true;
			}
			StopAllTweens();

			int count = GetTargetCount();
			if (count == 0)
				return;

			if (m_IsLoop) {
				m_IsPlayingInfinite = true;
				m_ArrRuntimeSequence = new Sequence[count];
				m_ArrLoopState = new TweenLoopState[count];
				for (int i = 0; i < count; i++) {
					m_ArrLoopState[i] = new TweenLoopState { instance = this, index = i };
					PlayIndependentCycle(i);	// 각 타겟별로 독립된 루프 실행
				}
			} else {
				// 단발성 재생은 에디터처럼 한 번에 그룹핑해서 실행
				m_EditorScrubSequence = Sequence.Create();
				for (int i = 0; i < count; i++) {
					Tween t = CreateSingleTweenForTarget(i);
					if (t.isAlive)
						m_EditorScrubSequence.Group(t);
				}
			}
		}

		public void StopAllTweens() {
			m_IsPlayingInfinite = false;
			if (m_EditorScrubSequence.isAlive)
				m_EditorScrubSequence.Stop();

			if (m_ArrRuntimeSequence != null) {
				for (int i = 0; i < m_ArrRuntimeSequence.Length; i++) {
					if (m_ArrRuntimeSequence[i].isAlive)
						m_ArrRuntimeSequence[i].Stop();
				}
			}

			if (m_RestoreOnStop && m_IsDefaultSaved)
				RestoreDefaultValue();
		}

		void PlayIndependentCycle(int index) {
			if (!m_IsPlayingInfinite)
				return;

			Tween t = CreateSingleTweenForTarget(index);
			if (!t.isAlive)
				return;

			Sequence seq = Sequence.Create(t);
			if (m_UseRandomDelay)
				seq.ChainDelay(Random.Range(m_MinDelay, m_MaxDelay));

			// 자기 자신(index)의 루프만 다시 호출 (다른 타겟들을 기다리지 않음!)
			seq.OnComplete(m_ArrLoopState[index], static state => state.instance.PlayIndependentCycle(state.index));

			m_ArrRuntimeSequence[index] = seq;
		}

#if UNITY_EDITOR
		/// <summary> 현재 트윈들의 1회 재생 </summary>
		/// <param name="pause"> 슬라이드로 시간 별 트윈 상태 지정이 가능한 스크럽 모드를 사용하기 위해 트윈을 정지상태로 만듦 </param>
		public void PlayOneCycle(bool pause) {
			if (!m_IsDefaultSaved) {
				SaveDefaultValue();
				m_IsDefaultSaved = true;
			}
			StopAllTweens();

			int count = GetTargetCount();
			m_EditorScrubSequence = Sequence.Create();
			for (int i = 0; i < count; i++) {
				Tween t = CreateSingleTweenForTarget(i);
				if (t.isAlive)
					m_EditorScrubSequence.Group(t);
			}
			m_EditorScrubSequence.isPaused = pause;
		}

		public bool IsEditorSequenceAlive() => m_EditorScrubSequence.isAlive;
		public float GetEditorSequenceProgress() => m_EditorScrubSequence.progress;
		public void SetEditorSequenceProgress(float p) => m_EditorScrubSequence.progress = p;
#endif

		protected TweenSettings<T> GetFinalSettings<T>(TweenSettings<T> originalSettings) where T : struct {
			TweenSettings<T> s = originalSettings;
			s.settings.cycles = 1; // 래퍼가 루프를 제어하므로 1로 강제

			if (m_UseRandomDuration)
				s.settings.duration = Random.Range(m_MinDuration, m_MaxDuration);

			// 커브가 있으면 ease 대신 적용
			if (m_ArrRandomCurve != null && m_ArrRandomCurve.Length > 0) {
				s.settings.ease = Ease.Custom;
				s.settings.customEase = m_ArrRandomCurve[Random.Range(0, m_ArrRandomCurve.Length)];
			}
			return s;
		}

		protected abstract int GetTargetCount();
		protected abstract Tween CreateSingleTweenForTarget(int index);
		protected abstract void SaveDefaultValue();
		protected abstract void RestoreDefaultValue();
	}


#if UNITY_EDITOR

// true 플래그를 통해 BaseRandomTweenPlayer를 상속받은 모든 클래스에 적용!
	[CustomEditor(typeof(BaseRandomTweenPlayer), true)]
	public class BaseRandomTweenPlayerEditor : Editor {
		float sliderProgress = 0f;

		readonly HashSet<string> basePropertyNames = new HashSet<string> {
			"m_IsLoop",
			"m_UseRandomDelay", "m_MinDelay", "m_MaxDelay",
			"m_UseRandomDuration", "m_MinDuration", "m_MaxDuration",
			"m_ArrRandomCurve",
			"m_RestoreOnStop"
		};

		public override void OnInspectorGUI() {
			serializedObject.Update(); // 직렬화된 객체 상태 갱신

			BaseRandomTweenPlayer script = (BaseRandomTweenPlayer)target;

			// (선택사항) 맨 위에 스크립트 레퍼런스 필드는 비활성화 상태로 그려줌
			SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");
			if (scriptProp != null) {
				using (new EditorGUI.DisabledScope(true)) {
					EditorGUILayout.PropertyField(scriptProp);
				}
			}

			GUILayout.Space(8);
			EditorGUILayout.LabelField(" 트윈 타겟 및 고유 설정", EditorStyles.boldLabel);

			// [PASS 1] 구현체(자식) 클래스의 고유 변수들 먼저 그리기
			SerializedProperty iterator = serializedObject.GetIterator();
			bool enterChildren = true;

			while (iterator.NextVisible(enterChildren)) {
				enterChildren = false;
				string propName = iterator.name;

				// m_Script 이거나 Base 클래스의 변수라면 이번 패스에서는 건너뜀
				if (propName == "m_Script" || basePropertyNames.Contains(propName))
					continue;

				// 구현체의 랜덤 EndValue 조건부 숨김 로직
				if (propName == "m_MinEndValue" || propName == "m_MaxEndValue") {
					SerializedProperty useRandomEndValueProp = serializedObject.FindProperty("m_UseRandomEndValue");
					if (useRandomEndValueProp != null && !useRandomEndValueProp.boolValue)
						continue;

					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(iterator, true);
					EditorGUI.indentLevel--;
				} else {
					EditorGUILayout.PropertyField(iterator, true);
				}
			}

			GUILayout.Space(12);
			EditorGUILayout.LabelField(" 공통 랜덤 및 재생 제어", EditorStyles.boldLabel);

			iterator = serializedObject.GetIterator();
			enterChildren = true;

			while (iterator.NextVisible(enterChildren)) {
				enterChildren = false;
				string propName = iterator.name;

				// Base 클래스의 변수가 아니면 건너뜀
				if (!basePropertyNames.Contains(propName))
					continue;

				if (propName == "m_MinDelay" || propName == "m_MaxDelay") {
					if (!serializedObject.FindProperty("m_UseRandomDelay").boolValue)
						continue;
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(iterator, true);
					EditorGUI.indentLevel--;
					continue;
				}
				if (propName == "m_MinDuration" || propName == "m_MaxDuration") {
					if (!serializedObject.FindProperty("m_UseRandomDuration").boolValue)
						continue;
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(iterator, true);
					EditorGUI.indentLevel--;
					continue;
				}
				if (propName == "m_ArrRandomCurve") {
					EditorGUILayout.PropertyField(iterator, true);

					GUILayout.BeginHorizontal();
					GUILayout.Space(EditorGUI.indentLevel * 15); // 들여쓰기 맞춤
					if (GUILayout.Button("모든 커브 Time을 0~1로 자동 정렬", EditorStyles.miniButton)) {
						NormalizeCurvesTime((BaseRandomTweenPlayer)target);
					}
					GUILayout.EndHorizontal();
					continue;
				}

				EditorGUILayout.PropertyField(iterator, true);
			}

			serializedObject.ApplyModifiedProperties(); // 변경된 값 적용

			// ---------------------------------------------------------
			// 이하 기존 버튼 및 타임라인 로직 동일
			// ---------------------------------------------------------
			GUILayout.Space(8);
			Rect rect = EditorGUILayout.GetControlRect(false, 1);
			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
			GUILayout.Space(12);

			// 테스트 컨트롤러
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("▶ 1사이클 재생", GUILayout.Height(30))) {
				script.PlayOneCycle(false);
			}
			if (GUILayout.Button("⏸ 1사이클 스크럽", GUILayout.Height(30))) {
				script.PlayOneCycle(true);
				sliderProgress = 0f;
			}
			if (GUILayout.Button("🔁 무한 랜덤 재생", GUILayout.Height(30))) {
				script.Play();
			}
			GUILayout.EndHorizontal();

			if (script.IsEditorSequenceAlive()) {
				if (GUILayout.Button("⏹ 정지 및 원복", GUILayout.Height(25))) {
					script.StopAllTweens();
				}
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();
			float currentProgress = script.IsEditorSequenceAlive() ? script.GetEditorSequenceProgress() : 0f;
			sliderProgress = EditorGUILayout.Slider("현재 타임라인", currentProgress, 0f, 1f);

			if (EditorGUI.EndChangeCheck()) {
				if (script.IsEditorSequenceAlive()) {
					script.SetEditorSequenceProgress(sliderProgress);
					SceneView.RepaintAll();
				}
			}
		}

		public override bool RequiresConstantRepaint() {
			BaseRandomTweenPlayer script = (BaseRandomTweenPlayer)target;
			return script != null && script.IsEditorSequenceAlive();
		}

		void OnEnable() { EditorApplication.update += OnEditorUpdate; }
		void OnDisable() { EditorApplication.update -= OnEditorUpdate; }

		void OnEditorUpdate() {
			BaseRandomTweenPlayer script = (BaseRandomTweenPlayer)target;
			if (script != null && script.IsEditorSequenceAlive()) {
				SceneView.RepaintAll();
			}
		}

		void NormalizeCurvesTime(BaseRandomTweenPlayer script) {
			if (script.m_ArrRandomCurve == null)
				return;

			Undo.RecordObject(script, "Normalize Curves Time");

			bool isModified = false;
			foreach (AnimationCurve curve in script.m_ArrRandomCurve) {
				if (curve == null || curve.keys.Length < 2)
					continue;

				Keyframe[] keys = curve.keys;

				// 첫 번째 키의 시간을 무조건 0으로
				if (keys[0].time != 0f) {
					keys[0].time = 0f;
					isModified = true;
				}

				// 마지막 키의 시간을 무조건 1로
				int lastIdx = keys.Length - 1;
				if (keys[lastIdx].time != 1f) {
					keys[lastIdx].time = 1f;
					isModified = true;
				}

				if (isModified)
					curve.keys = keys;
			}

			if (isModified) {
				EditorUtility.SetDirty(script);
				UnityEngine.Debug.Log("모든 커브의 Time이 0~1로 정렬되었습니다.");
			}
		}
	}
#endif
}