
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace KHFC.Editor {
	public class MaterialFinderEditorWindow : EditorWindow {
		const string TITLE_NAME = "Material Finder in Particle";
		const float NAME_MIN_WIDTH = 70f;
		const float NAME_MIN_WIDTH_PERCENTAGE = .15f;

		const string PREFAB_ROOT_PATH = "Assets/Media/";

		static Vector2 m_Size = new(340, 450);
		static float s_PreviousWindowWidth = m_Size.x;
		static GUILayoutOption s_NameWidthOption = GUILayout.Width(NAME_MIN_WIDTH);

		// SerializedObject와 SerializedProperty를 위한 필드
		SerializedObject m_SerializedObj;
		ReorderableList m_SceneReorderableList;
		ReorderableList m_PrefabReorderableList;

		GUIStyle m_TitleStyle;
		GUIStyle m_HeaderStyle;

		Vector2 m_ScrollPos;

		/// <summary> 바꾸고 싶은 머테리얼 </summary>
		static Material m_Source;
		/// <summary> <see cref="m_Source"/>에서 교체할 머테리얼 </summary>
		static Material m_Dest;

		/// <summary> 씬 내의 파티클 오브젝트들 </summary>
		[SerializeField] List<GameObject> m_ListSceneObj = new();
		/// <summary> 파티클이 붙어있는 프리팹들 </summary>
		[SerializeField] List<GameObject> m_ListPrefab = new();

		public static void ShowWindow() {
			MaterialFinderEditorWindow window = GetWindow<MaterialFinderEditorWindow>(utility: true, title: TITLE_NAME, focus: true);
			window.minSize = m_Size;
			window.FindAllParticlesUsingMaterial(m_Source);
		}
		public static void ShowWindow(Material target) {
			MaterialFinderEditorWindow window = GetWindow<MaterialFinderEditorWindow>(utility: true, title: TITLE_NAME, focus: true);
			window.minSize = m_Size;
			window.FindAllParticlesUsingMaterial(target);
		}

		void Awake() {
			m_TitleStyle = new GUIStyle(EditorStyles.label) {
				fontSize = 14,
				fontStyle = FontStyle.Bold,
				fixedHeight = 20
			};
			m_HeaderStyle = new GUIStyle(EditorStyles.label) {
				fontSize = 12,
				fontStyle = FontStyle.Bold,
				fixedHeight = 18
			};
			//environmentValueStyle = new GUIStyle(EditorStyles.label) {
			//	alignment = TextAnchor.MiddleRight
			//};
		}

		void OnEnable() {
			m_SerializedObj = new SerializedObject(this);
			InitializeReorderableList();
		}

		void OnSelectionChange() { Repaint(); }

		void OnGUI() {
			// OnGUI is called on each frame draw, so we don't want to do any unnecessary calculation if we can avoid it. So only calculate it when the width actually changed.
			if (System.Math.Abs(s_PreviousWindowWidth - position.width) > 1) {
				s_PreviousWindowWidth = position.width;
				float nameWidth = System.Math.Max(NAME_MIN_WIDTH, position.width * NAME_MIN_WIDTH_PERCENTAGE);
				s_NameWidthOption = GUILayout.Width(nameWidth);
			}

			m_SerializedObj.Update();

			using (var scrollView = new EditorGUILayout.ScrollViewScope(m_ScrollPos, false, false)) {
				m_ScrollPos = scrollView.scrollPosition;
				GUILayout.Space(5);

				DrawMaterialSelectionArea();
				GUILayout.Space(10);
				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // 구분선

				DrawSearchArea();
				GUILayout.Space(10);

				DrawReplaceArea();
			}

			m_SerializedObj.ApplyModifiedProperties();
		}

		void DrawMaterialSelectionArea() {
			EditorGUILayout.LabelField("변경 전 머테리얼", m_HeaderStyle);

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Source", s_NameWidthOption);
			Material newSource = (Material)EditorGUILayout.ObjectField(m_Source, typeof(Material), false);
			//EditorGUILayout.PropertyField(m_SourceMat);
			GUILayout.EndHorizontal();


			EditorGUILayout.LabelField("변경 후 머테리얼", m_HeaderStyle);

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Dest", s_NameWidthOption);
			m_Dest = (Material)EditorGUILayout.ObjectField(m_Dest, typeof(Material), false);
			//EditorGUILayout.PropertyField(m_DestMat);
			GUILayout.EndHorizontal();

			if (newSource != m_Source) {
				m_Source = newSource;
				FindAllParticlesUsingMaterial(newSource);
			}
		}

		void DrawSearchArea() {
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"Results (Scene: {m_ListSceneObj.Count}, Prefab: {m_ListPrefab.Count})", m_HeaderStyle);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Refresh List", GUILayout.MinWidth(80))) {
				FindAllParticlesUsingMaterial(m_Source);
			}
			GUILayout.EndHorizontal();

			using (new EditorGUILayout.VerticalScope("box")) {
				m_SceneReorderableList.DoLayoutList();
				m_PrefabReorderableList.DoLayoutList();
			}
		}

		void DrawReplaceArea() {
			bool isValid = m_Source != null && m_Dest != null && m_Source != m_Dest;
			bool hasResults = m_ListSceneObj.Count > 0 || m_ListPrefab.Count > 0;

			EditorGUI.BeginDisabledGroup(!isValid || !hasResults);

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.6f, 1f, 0.6f); // 연한 초록색 강조
			if (GUILayout.Button("Replace All Particle Material", GUILayout.MinHeight(24))) {
				if (EditorUtility.DisplayDialog("Confirm Replace",
					$"Are you sure you want to replace material?\n\nFrom: {m_Source.name}\nTo: {m_Dest.name}\n\n" +
					$"CAUTION: Changes to Prefab Assets cannot be undone!",
					"Yes, Replace", "Cancel")) {
					ReplaceAllMaterial();
				}
			}
			GUI.backgroundColor = oldColor;

			EditorGUI.EndDisabledGroup();

			if (!isValid) {
				EditorGUILayout.HelpBox("Select both Source and Destination materials to enable replacement.", MessageType.Info);
			}
		}

		void FindAllParticlesUsingMaterial(Material target) {
			m_ListSceneObj.Clear();
			m_ListPrefab.Clear();

			if (target == null) {
				Debug.LogError("MaterialFinder : Material is null");
				return;
			}

			string path = AssetDatabase.GetAssetPath(target);
			if (path == null) {
				Debug.LogError("MaterialFinder : There is no material in AssetDatabase");
				return;
			}

			m_Source = target;
			try {
				// 1. 씬 내의 오브젝트에 있는 파티클을 찾는다

				// - 씬 내 모든 ParticleSystem 컴포넌트를 찾음
				UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
				GameObject[] rootObjects = activeScene.GetRootGameObjects();
				foreach (GameObject go in rootObjects) {
					// 각 GameObject에서 모든 ParticleSystem 컴포넌트를 찾음
					ParticleSystemRenderer[] arrParticle = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
					foreach (ParticleSystemRenderer psr in arrParticle) {
						if (IsRendererUsingMaterial(psr, target)) {
							m_ListSceneObj.Add(psr.gameObject);
						}
					}
				}

				// - 컴포넌트를 찾는 다른 방법
				//ParticleSystem[] arrPS = Object.FindObjectsOfType<ParticleSystem>();
				//FindAllParticleInstance(arrPS);

				// 2. 프리팹 애셋들의 파티클을 찾는다

				string[] arrGUID = AssetDatabase.FindAssets("t:Prefab", new[] { PREFAB_ROOT_PATH });
				for (int i = 0; i < arrGUID.Length; i++) {
					if (EditorUtility.DisplayCancelableProgressBar("Searching Prefabs", $"Checking {i}/{arrGUID.Length}", (float)i / arrGUID.Length))
						break;

					string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
					GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
					if (prefab) {
						ParticleSystemRenderer[] arrParticle = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
						foreach (ParticleSystemRenderer psr in arrParticle) {
							if (IsRendererUsingMaterial(psr, target)) {
								m_ListPrefab.Add(prefab);
								break; // 프리팹 하나에 여러 파티클이 있어도 프리팹 루트 하나만 등록
							}
						}
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				Repaint();
			}
		}
		bool IsRendererUsingMaterial(ParticleSystemRenderer psr, Material target) {
			return psr.sharedMaterial == target || psr.trailMaterial == target;
		}

		void ReplaceAllMaterial() {
			int count = 0;

			// 1. 씬 오브젝트 교체
			Undo.IncrementCurrentGroup();
			Undo.SetCurrentGroupName("Replace Particle Materials");
			int undoGroupIndex = Undo.GetCurrentGroup();

			foreach (var go in m_ListSceneObj) {
				if (go == null)
					continue;

				ParticleSystemRenderer[] arrParticle = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
				foreach (ParticleSystemRenderer psr in arrParticle) {
					bool modified = false;
					Undo.RecordObject(psr, "Replace Material");

					if (psr.sharedMaterial == m_Source) {
						psr.sharedMaterial = m_Dest;
						modified = true;
					}
					if (psr.trailMaterial == m_Source) {
						psr.trailMaterial = m_Dest;
						modified = true;
					}

					if (modified) {
						EditorUtility.SetDirty(psr);
						count++;
					}
				}
			}
			Undo.CollapseUndoOperations(undoGroupIndex);

			// 2. 프리팹 교체
			foreach (var prefabRoot in m_ListPrefab) {
				if (prefabRoot == null)
					continue;

				string assetPath = AssetDatabase.GetAssetPath(prefabRoot);
				// 프리팹 내용물을 로드하여 수정
				using PrefabUtility.EditPrefabContentsScope editScope = new(assetPath);
				ParticleSystemRenderer[] arrParticle = editScope.prefabContentsRoot.GetComponentsInChildren<ParticleSystemRenderer>(true);
				bool modified = false;

				foreach (ParticleSystemRenderer psr in arrParticle) {
					if (psr.sharedMaterial == m_Source) {
						psr.sharedMaterial = m_Dest;
						modified = true;
					}
					if (psr.trailMaterial == m_Source) {
						psr.trailMaterial = m_Dest;
						modified = true;
					}
				}

				if (modified)
					count++;
			}

			AssetDatabase.SaveAssets();
			Debug.Log($"[Material Finder] Replaced material on {count} objects.");

			// 교체 완료 후 Source를 Dest로 업데이트하여 결과 확인
			m_Source = m_Dest;
			m_Dest = null;
			FindAllParticlesUsingMaterial(m_Source);
		}

		void InitializeReorderableList() {
			m_SceneReorderableList = CreateList(m_ListSceneObj, "Scene Instances");
			m_PrefabReorderableList = CreateList(m_ListPrefab, "Project Prefabs");

			ReorderableList CreateList(List<GameObject> list, string headerName) => new(list, typeof(GameObject), true, true, false, false) {
				drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, headerName, m_HeaderStyle),
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
					if (index >= list.Count)
						return;
					rect.y += 2;
					rect.height = EditorGUIUtility.singleLineHeight;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(rect, list[index], typeof(GameObject), false);
					EditorGUI.EndDisabledGroup();
				},
				onMouseUpCallback = (ReorderableList l) => {
					if (l.index >= 0 && l.index < list.Count && list[l.index] != null)
						EditorGUIUtility.PingObject(list[l.index]);
				}
			};
		}
	}
}