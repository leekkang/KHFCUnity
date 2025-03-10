using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using UnityEditorInternal;

namespace KHFC.Editor {
	public class MaterialFinderEditorWindow : EditorWindow {
		const string TITLE_NAME = "Material Finder in Particle";
		const float NAME_MIN_WIDTH = 70f;
		const float NAME_MIN_WIDTH_PERCENTAGE = .15f;

		const string PREFAB_ROOT_PATH = "Assets/Media/";

		static Vector2 m_Size = new(320, 400);
		static float s_PreviousWindowWidth = m_Size.x;
		static GUILayoutOption s_NameWidthOption = GUILayout.Width(NAME_MIN_WIDTH);

		// SerializedObject와 SerializedProperty를 위한 필드
		SerializedObject m_SerializedObj;

		SerializedProperty m_SourceMat;
		SerializedProperty m_DestMat;
		ReorderableList m_SceneReorderableList;
		ReorderableList m_PrefabReorderableList;

		GUIStyle titleLabelStyle;
		GUIStyle headerLabelStyle;

		Vector2 m_ScrollPos;

		/// <summary> 바꾸고 싶은 머테리얼 </summary>
		static Material m_Source;
		//[SerializeField] Material m_Source;	// SerializedProperty는 static field에는 사용하지 못한다
		/// <summary> <see cref="m_Source"/>에서 교체할 머테리얼 </summary>
		static Material m_Dest;
		//[SerializeField] Material m_Dest;

		/// <summary> 씬 내의 파티클 오브젝트들 </summary>
		[SerializeField] List<GameObject> m_ListSceneObj;
		List<ParticleSystemRenderer> m_ListSceneParticle;
		/// <summary> 파티클이 붙어있는 프리팹들 </summary>
		[SerializeField] List<GameObject> m_ListPrefab;
		List<ParticleSystemRenderer> m_ListPrefabParticle;

		public static void ShowWindow() {
			MaterialFinderEditorWindow window = GetWindow<MaterialFinderEditorWindow>(utility: true, title: TITLE_NAME, focus: true);
			window.minSize = m_Size;
			window.GetAllParticlesUseMaterial(m_Source);
		}
		public static void ShowWindow(Material target) {
			MaterialFinderEditorWindow window = GetWindow<MaterialFinderEditorWindow>(utility: true, title: TITLE_NAME, focus: true);
			window.minSize = m_Size;
			window.GetAllParticlesUseMaterial(target);
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
			//m_SourceMat = m_SerializedObj.FindProperty("m_Source");
			//m_DestMat = m_SerializedObj.FindProperty("m_Dest");

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

				EditorGUILayout.LabelField("변경 전 머테리얼", headerLabelStyle);

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Source", s_NameWidthOption);
				Material tmp = null;
				tmp = (Material)EditorGUILayout.ObjectField(m_Source, typeof(Material), false);
				//EditorGUILayout.PropertyField(m_SourceMat);
				GUILayout.EndHorizontal();


				EditorGUILayout.LabelField("변경 후 머테리얼", headerLabelStyle);
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Dest", s_NameWidthOption);
				m_Dest = (Material)EditorGUILayout.ObjectField(m_Dest, typeof(Material), false);
				//EditorGUILayout.PropertyField(m_DestMat);
				GUILayout.EndHorizontal();

				GUILayout.Space(12);
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Particle Parent List", headerLabelStyle);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Refresh List", GUILayout.MinWidth(80))) {
					GetAllParticlesUseMaterial(tmp);
				}
				GUILayout.EndHorizontal();
				using (new EditorGUILayout.VerticalScope("box")) {
					m_SceneReorderableList.DoLayoutList();
					m_PrefabReorderableList.DoLayoutList();
				}

				if (tmp != m_Source) {
					m_Source = tmp;
					GetAllParticlesUseMaterial(tmp);
				}

				GUI.enabled = m_Source != null && m_Dest != null
					&& m_Source != m_Dest;
				if (GUILayout.Button("Replace All Particle Material", GUILayout.MinHeight(24))) {
					ReplaceAllMaterial();
				}
				GUI.enabled = true;
			}


			m_SerializedObj.ApplyModifiedProperties();
		}


		void CalculateFieldWidth() {
			float currentWidth = position.width;

			float nameWidth = System.Math.Max(NAME_MIN_WIDTH, currentWidth * NAME_MIN_WIDTH_PERCENTAGE);
			s_NameWidthOption = GUILayout.Width(nameWidth);
		}

		void GetAllParticlesUseMaterial(Material mat) {
			if (mat == null) {
				Debug.LogError("MaterialFinder : Material is null");
				return;
			}

			string path = AssetDatabase.GetAssetPath(mat);
			if (path == null) {
				Debug.LogError("MaterialFinder : There is no material in AssetDatabase");
				return;
			}
			string guid = AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets);
			m_Source = mat;
			m_ListSceneObj = new List<GameObject>();
			m_ListSceneParticle = new List<ParticleSystemRenderer>();
			m_ListPrefab = new List<GameObject>();
			m_ListPrefabParticle = new List<ParticleSystemRenderer>();
			InitializeReorderableList();

			//AssetDatabase.InstanceIDsToGUIDs

			List<ParticleSystemRenderer> listPS = new();
			List<int> listMatInstID = new();

			// 1. 씬 내의 오브젝트에 있는 파티클을 찾는다

			// - 씬 내 모든 ParticleSystem 컴포넌트를 찾음
			UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			GameObject[] rootObjects = activeScene.GetRootGameObjects();
			foreach (GameObject go in rootObjects) {
				// 각 GameObject에서 모든 ParticleSystem 컴포넌트를 찾음
				ParticleSystem[] arrParticle = go.GetComponentsInChildren<ParticleSystem>(true);
				FindAllParticleInstance(go, arrParticle, false);
			}

			// - 컴포넌트를 찾는 다른 방법
			//ParticleSystem[] arrPS = Object.FindObjectsOfType<ParticleSystem>();
			//FindAllParticleInstance(arrPS);

			// 2. 프리팹 애셋들의 파티클을 찾는다

			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
				ParticleSystem[] arrParticle = go.GetComponentsInChildren<ParticleSystem>(true);
				FindAllParticleInstance(go, arrParticle, true);
			}

			// 변환 결과 출력

			NativeArray<GUID> arrGuid = new(listMatInstID.Count, Allocator.Temp);
			//for (int i = 0; i < listMatInstID.Count; ++i)
			//	arrGuid[i] = new GUID();
			// InstanceID 배열을 GUID 배열로 변환
			AssetDatabase.InstanceIDsToGUIDs(new NativeArray<int>(listMatInstID.ToArray(), Allocator.Temp), arrGuid);

			int count = 0;
			int sceneCount = m_ListSceneObj.Count;
			for (int i = 0, len = sceneCount; i < len; ++i) {
				if (guid == arrGuid[i].ToString()) {
					m_ListSceneParticle.Add(listPS[i]);
					m_ListSceneObj[count++] = m_ListSceneObj[i];
				}
			}
			m_ListSceneObj.RemoveRange(count, sceneCount - count);

			count = 0;
			for (int i = sceneCount, len = arrGuid.Length; i < len; ++i) {
				if (guid == arrGuid[i].ToString()) {
					m_ListPrefabParticle.Add(listPS[i]);
					m_ListPrefab[count++] = m_ListPrefab[i - sceneCount];
				}
			}
			m_ListPrefab.RemoveRange(count, m_ListPrefab.Count - count);

			//for (int i = 0; i < arrGuid.Length; i++) {
			//	Debug.Log($"Material GUID: {arrGuid[i]}");
			//}

			void FindAllParticleInstance(GameObject parent,
				ParticleSystem[] arrParticle,
				bool isPrefab) {
				for (int i = 0, len = arrParticle.Length; i < len; ++i) {
					ParticleSystemRenderer psRenderer = arrParticle[i].GetComponent<ParticleSystemRenderer>();
					if (psRenderer == null || psRenderer.sharedMaterial == null)
						continue;

					listPS.Add(psRenderer);
					listMatInstID.Add(psRenderer.sharedMaterial.GetInstanceID());
					if (isPrefab)
						m_ListPrefab.Add(parent);
					else
						m_ListSceneObj.Add(psRenderer.gameObject);
				}
			}
		}

		void ReplaceAllMaterial() {
			for (int i = 0; i < m_ListSceneParticle.Count; ++i) {
				m_ListSceneParticle[i].sharedMaterial = m_Dest;
				EditorUtility.SetDirty(m_ListSceneParticle[i]);
			}
			for (int i = 0; i < m_ListPrefabParticle.Count; ++i) {
				m_ListPrefabParticle[i].sharedMaterial = m_Dest;
			}
			m_Source = m_Dest;
			m_Dest = null;

			GetAllParticlesUseMaterial(m_Source);
			AssetDatabase.Refresh();
		}

		void InitializeReorderableList() {
			m_SceneReorderableList = new(m_ListSceneObj, typeof(GameObject), true, true, false, false) {
				drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField(rect, "Particle Parent List in Scene", headerLabelStyle);
				}
					,
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
					GameObject element = (GameObject)m_SceneReorderableList.list[index];
					rect.y += 2;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(
						new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						element, typeof(GameObject), false);
					EditorGUI.EndDisabledGroup();
				}
			};
			m_PrefabReorderableList = new(m_ListPrefab, typeof(GameObject), true, true, false, false) {
				drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField(rect, "Particle Root Object List of Prefab", headerLabelStyle);
				}
					,
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
					GameObject element = (GameObject)m_PrefabReorderableList.list[index];
					rect.y += 2;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(
						new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						element, typeof(GameObject), false);
					EditorGUI.EndDisabledGroup();
				}
			};
		}
	}
}