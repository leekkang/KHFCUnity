
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace KHFC.Editor {
	public class ScriptFinderEditorWindow : EditorWindow {
		const string TITLE_NAME = "Script Finder in Object";
		const float NAME_MIN_WIDTH = 70f;
		const float NAME_MIN_WIDTH_PERCENTAGE = .15f;

		const string PREFAB_ROOT_PATH = "Assets/Media/";

		static Vector2 m_Size = new(320, 400);
		static float s_PreviousWindowWidth = m_Size.x;
		static GUILayoutOption s_NameWidthOption = GUILayout.Width(NAME_MIN_WIDTH);

		// SerializedObject와 SerializedProperty를 위한 필드
		SerializedObject m_SerializedObj;
		ReorderableList m_SceneReorderableList;
		ReorderableList m_PrefabReorderableList;

		GUIStyle titleLabelStyle;
		GUIStyle headerLabelStyle;

		Vector2 m_ScrollPos;

		/// <summary> 찾고 싶은 스크립트 파일 </summary>
		static MonoScript m_Target;

		/// <summary> 씬 내에서 해당 스크립트를 가진 오브젝트들 </summary>
		[SerializeField] List<GameObject> m_ListSceneObj;
		/// <summary> 해당 스크립트가 붙어있는 프리팹들 </summary>
		[SerializeField] List<GameObject> m_ListPrefab;

		public static void ShowWindow() {
			ScriptFinderEditorWindow window = GetWindow<ScriptFinderEditorWindow>(utility: true, title: TITLE_NAME, focus: true);
			window.minSize = m_Size;
			window.FindAllObjectsWithScript(m_Target);
		}
		public static void ShowWindow(MonoScript target) {
			ScriptFinderEditorWindow window = GetWindow<ScriptFinderEditorWindow>(utility: true, title: TITLE_NAME, focus: true);
			window.minSize = m_Size;
			window.FindAllObjectsWithScript(target);
		}

		void Awake() {
			titleLabelStyle = new GUIStyle(EditorStyles.label) {
				fontSize = 14,
				fontStyle = FontStyle.Bold,
				fixedHeight = 20
			};
			headerLabelStyle = new GUIStyle(EditorStyles.label) {
				fontSize = 12,
				fontStyle = FontStyle.Bold,
				fixedHeight = 18
			};
			//environmentValueStyle = new GUIStyle(EditorStyles.label) {
			//	alignment = TextAnchor.MiddleRight
			//};
		}

		void OnEnable() {
			// SerializedObject 초기화
			m_SerializedObj = new SerializedObject(this);
			InitializeReorderableList();
		}

		void OnSelectionChange() { Repaint(); }

		void OnGUI() {
			// OnGUI is called on each frame draw, so we don't want to do any unnecessary calculation if we can avoid it. So only calculate it when the width actually changed.
			if (System.Math.Abs(s_PreviousWindowWidth - position.width) > 1) {
				s_PreviousWindowWidth = position.width;
				CalculateFieldWidth();
			}

			m_SerializedObj.Update();

			using (var scrollView = new EditorGUILayout.ScrollViewScope(m_ScrollPos, false, false)) {
				m_ScrollPos = scrollView.scrollPosition;

				GUILayout.Space(5);

				EditorGUILayout.LabelField("검색 대상 스크립트", headerLabelStyle);

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Script", s_NameWidthOption);

				MonoScript tmp = (MonoScript)EditorGUILayout.ObjectField(m_Target, typeof(MonoScript), false);
				GUILayout.EndHorizontal();

				GUILayout.Space(12);

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Search Result", headerLabelStyle);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Refresh List", GUILayout.MinWidth(80))) {
					FindAllObjectsWithScript(tmp);
				}
				GUILayout.EndHorizontal();
				using (new EditorGUILayout.VerticalScope("box")) {
					m_SceneReorderableList.DoLayoutList();
					m_PrefabReorderableList.DoLayoutList();
				}

				if (tmp != m_Target) {
					m_Target = tmp;
					FindAllObjectsWithScript(tmp);
				}
			}

			m_SerializedObj.ApplyModifiedProperties();
		}


		void CalculateFieldWidth() {
			float currentWidth = position.width;

			float nameWidth = System.Math.Max(NAME_MIN_WIDTH, currentWidth * NAME_MIN_WIDTH_PERCENTAGE);
			s_NameWidthOption = GUILayout.Width(nameWidth);
		}

		/// <summary> 실제 검색 로직을 수행하는 함수 </summary>
		void FindAllObjectsWithScript(MonoScript script) {
			if (script == null) {
				Debug.LogError("ScriptFinder : Script is null");
				return;
			}

			System.Type targetType = script.GetClass();
			if (targetType == null) {
				Debug.LogError("ScriptFinder : There is no valid type in script");
				return;
			}

			m_Target = script;
			m_ListSceneObj = new List<GameObject>();
			m_ListPrefab = new List<GameObject>();
			InitializeReorderableList();

			// 1. 씬 내의 오브젝트를 찾는다

			// - 씬 내 모든 ParticleSystem 컴포넌트를 찾음
			UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			GameObject[] rootObjects = activeScene.GetRootGameObjects();
			foreach (GameObject go in rootObjects) {
				// 각 GameObject에서 모든 컴포넌트를 검색
				Component[] arrComponent = go.GetComponentsInChildren(targetType, true);
				foreach (var component in arrComponent) {
					m_ListSceneObj.Add(component.gameObject);
				}
			}

			// - 컴포넌트를 찾는 다른 방법
			//ParticleSystem[] arrPS = Object.FindObjectsOfType<ParticleSystem>();
			//FindAllParticleInstance(arrPS);

			// 2. 프리팹 애셋들을 찾는다

			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });

			try {
				for (int i = 0; i < arrGUID.Length; i++) {
					if (EditorUtility.DisplayCancelableProgressBar("Searching Prefabs", $"Checking {i}/{arrGUID.Length}...", (float)i / arrGUID.Length))
						break;

					string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
					GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					if (go != null) {
						Component[] arrComponent = go.GetComponentsInChildren(targetType, true);
						if (arrComponent.Length > 0)
							m_ListPrefab.Add(go);
						//foreach (var component in arrComponent) {
						//	m_ListPrefab.Add(component.gameObject);
						//}
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
			}
		}

		void InitializeReorderableList() {
			m_SceneReorderableList = new(m_ListSceneObj, typeof(GameObject), true, true, false, false) {
				drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField(rect, $"Scene Instance: {m_ListSceneObj.Count}", headerLabelStyle);
				},
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
					GameObject element = (GameObject)m_SceneReorderableList.list[index];
					rect.y += 2;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
										  element, typeof(GameObject), false);
					EditorGUI.EndDisabledGroup();
				},
				onMouseUpCallback = (ReorderableList list) => {
					if (list.index >= 0 && list.index < m_ListSceneObj.Count) {
						EditorGUIUtility.PingObject(m_ListSceneObj[list.index]);
					}
				}
			};
			m_PrefabReorderableList = new(m_ListPrefab, typeof(GameObject), true, true, false, false) {
				drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField(rect, $"Project Prefabs: {m_ListPrefab.Count}", headerLabelStyle);
				},
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
					GameObject element = (GameObject)m_PrefabReorderableList.list[index];
					rect.y += 2;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
										  element, typeof(GameObject), false);
					EditorGUI.EndDisabledGroup();
				},
				onMouseUpCallback = (ReorderableList list) => {
					if (list.index >= 0 && list.index < m_ListPrefab.Count) {
						EditorGUIUtility.PingObject(m_ListPrefab[list.index]);
					}
				}
			};
		}
	}
}