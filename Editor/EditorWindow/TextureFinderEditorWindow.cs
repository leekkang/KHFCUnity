
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using UnityEditorInternal;

namespace KHFC.Editor {
	public class TextureFinderEditorWindow : EditorWindow {
		const string TITLE_NAME = "Texture Finder in Material, UI.Image";
		const float NAME_MIN_WIDTH = 70f;
		const float NAME_MIN_WIDTH_PERCENTAGE = .15f;

		const string PREFAB_ROOT_PATH = "Assets/Media/";
		const string MATERIAL_ROOT_PATH = "Assets/Media/";

		static Vector2 m_Size = new(320, 400);
		static float s_PreviousWindowWidth = m_Size.x;
		static GUILayoutOption s_NameWidthOption = GUILayout.Width(NAME_MIN_WIDTH);

		// SerializedObject와 SerializedProperty를 위한 필드
		SerializedObject m_SerializedObj;

		GUIStyle titleLabelStyle;
		GUIStyle headerLabelStyle;

		Vector2 m_ScrollPos;

		SerializedProperty m_SourceMat;
		SerializedProperty m_DestMat;
		ReorderableList m_MaterialReorderableList;
		ReorderableList m_SceneReorderableList;
		ReorderableList m_PrefabReorderableList;

		/// <summary> 바꾸고 싶은 텍스쳐 </summary>
		static Texture2D m_Source;
		//[SerializeField] Texture2D m_Source;	// SerializedProperty는 static field에는 사용하지 못한다
		/// <summary> <see cref="m_Source"/>에서 교체할 텍스쳐 </summary>
		static Texture2D m_Dest;
		//[SerializeField] Texture2D m_Dest;

		/// <summary> 대상 텍스쳐를 메인으로 사용하는 머테리얼 리스트 </summary>
		List<Material> m_ListMaterial;
		/// <summary> 씬 내에서 해당 텍스쳐를 사용하는 오브젝트들 </summary>
		List<UnityEngine.UI.Image> m_ListSceneImage;
		/// <summary> 이미지 오브젝트가 붙어있는 프리팹들 </summary>
		[SerializeField] List<GameObject> m_ListPrefab;
		List<UnityEngine.UI.Image> m_ListPrefabImage;

		public static void ShowWindow() {
			TextureFinderEditorWindow window = GetWindow<TextureFinderEditorWindow>(utility: true, title: TITLE_NAME, focus: true);
			window.minSize = m_Size;
			window.GetAllMaterialsUseTexture(m_Source);
		}
		public static void ShowWindow(Texture2D target) {
			TextureFinderEditorWindow window = GetWindow<TextureFinderEditorWindow>(utility: true, title: TITLE_NAME, focus: true);
			window.minSize = m_Size;
			window.GetAllMaterialsUseTexture(target);
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

				EditorGUILayout.LabelField("변경 전 텍스쳐", headerLabelStyle);

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Source", s_NameWidthOption);
				Texture2D tmp = null;
				tmp = (Texture2D)EditorGUILayout.ObjectField(m_Source, typeof(Texture2D), false);
				//EditorGUILayout.PropertyField(m_SourceMat);
				GUILayout.EndHorizontal();


				EditorGUILayout.LabelField("변경 후 텍스쳐", headerLabelStyle);
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Dest", s_NameWidthOption);
				m_Dest = (Texture2D)EditorGUILayout.ObjectField(m_Dest, typeof(Texture2D), false);
				//EditorGUILayout.PropertyField(m_DestMat);
				GUILayout.EndHorizontal();

				GUILayout.Space(12);
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Texture Parent List", headerLabelStyle);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Refresh List", GUILayout.MinWidth(80))) {
					GetAllMaterialsUseTexture(tmp);
				}
				GUILayout.EndHorizontal();
				using (new EditorGUILayout.VerticalScope("box")) {
					m_MaterialReorderableList.DoLayoutList();
					m_SceneReorderableList.DoLayoutList();
					m_PrefabReorderableList.DoLayoutList();
				}

				if (tmp != m_Source) {
					m_Source = tmp;
					GetAllMaterialsUseTexture(tmp);
				}

				GUI.enabled = m_Source != null && m_Dest != null
					&& m_Source != m_Dest;
				if (GUILayout.Button("Replace All Parent Object's Texture", GUILayout.MinHeight(24))) {
					ReplaceAllTexture();
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

		void GetAllMaterialsUseTexture(Texture2D tex) {
			if (tex == null) {
				Debug.LogError("TextureFinder : Texture is null");
				return;
			}

			string path = AssetDatabase.GetAssetPath(tex);
			if (path == null) {
				Debug.LogError("TextureFinder : There is no texture in AssetDatabase");
				return;
			}

			string guid = AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets);
			m_Source = tex;
			m_ListMaterial = new List<Material>();
			m_ListSceneImage = new List<UnityEngine.UI.Image>();
			m_ListPrefab = new List<GameObject>();
			m_ListPrefabImage = new List<UnityEngine.UI.Image>();
			InitializeReorderableList();

			//AssetDatabase.InstanceIDsToGUIDs

			List<ParticleSystemRenderer> listPS = new();
			List<int> listInstID = new();

			// 1. 해당 텍스쳐를 사용하는 머테리얼 애셋들을 찾는다
			string[] arrGUID = AssetDatabase.FindAssets("t:material", new string[] { MATERIAL_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				Material mat = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Material)) as Material;
				if (mat.mainTexture != null) {
					listInstID.Add(mat.mainTexture.GetInstanceID());
					m_ListMaterial.Add(mat);
				}
			}

			// 2. 씬 내의 이미지 오브젝트에 있는 텍스쳐를 찾는다

			// - 씬 내 모든 ParticleSystem 컴포넌트를 찾음
			UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			GameObject[] rootObjects = activeScene.GetRootGameObjects();
			foreach (GameObject go in rootObjects) {
				UnityEngine.UI.Image[] arrImage = go.GetComponentsInChildren<UnityEngine.UI.Image>(true);
				for (int i = 0, len = arrImage.Length; i < len; ++i) {
					if (arrImage[i].mainTexture != null) {
						listInstID.Add(arrImage[i].mainTexture.GetInstanceID());
						m_ListSceneImage.Add(arrImage[i]);
					}
				}
			}

			// - 컴포넌트를 찾는 다른 방법
			//UnityEngine.UI.Image[] arrPS = Object.FindObjectsOfType<UnityEngine.UI.Image>();
			//FindAllParticleInstance(arrPS);

			// 3. 프리팹 애셋들 내 이미지 오브젝트에 있는 텍스쳐를 찾는다

			arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
				UnityEngine.UI.Image[] arrImage = go.GetComponentsInChildren<UnityEngine.UI.Image>(true);
				for (int j = 0, len = arrImage.Length; j < len; ++j) {
					if (arrImage[j].mainTexture != null) {
						listInstID.Add(arrImage[j].mainTexture.GetInstanceID());
						m_ListPrefab.Add(go);
						m_ListPrefabImage.Add(arrImage[j]);
					}
				}
			}

			// 변환 결과 출력

			NativeArray<GUID> arrGuid = new(listInstID.Count, Allocator.Temp);
			//for (int i = 0; i < listMatInstID.Count; ++i)
			//	arrGuid[i] = new GUID();
			// InstanceID 배열을 GUID 배열로 변환
			AssetDatabase.InstanceIDsToGUIDs(new NativeArray<int>(listInstID.ToArray(), Allocator.Temp), arrGuid);

			int count = 0, idx = 0;
			int start = 0;
			int end = m_ListMaterial.Count;
			for (int len = end; idx < len; ++idx) {
				if (guid == arrGuid[idx].ToString()) {
					m_ListMaterial[count++] = m_ListMaterial[idx];
				}
			}
			m_ListMaterial.RemoveRange(count, end - count);

			count = 0;
			start = end;
			end += m_ListSceneImage.Count;
			for (int len = end; idx < len; ++idx) {
				if (guid == arrGuid[idx].ToString()) {
					m_ListSceneImage[count++] = m_ListSceneImage[idx - start];
				}
			}
			m_ListSceneImage.RemoveRange(count, m_ListSceneImage.Count - count);

			count = 0;
			start = end;
			end += m_ListPrefabImage.Count;
			for (int len = end; idx < len; ++idx) {
				if (guid == arrGuid[idx].ToString()) {
					m_ListPrefab[count] = m_ListPrefab[idx - start];
					m_ListPrefabImage[count++] = m_ListPrefabImage[idx - start];
				}
			}
			m_ListPrefab.RemoveRange(count, m_ListPrefab.Count - count);
			m_ListPrefabImage.RemoveRange(count, m_ListPrefabImage.Count - count);

		}

		void ReplaceAllTexture() {
			for (int i = 0; i < m_ListMaterial.Count; ++i) {
				m_ListMaterial[i].mainTexture = m_Dest;
			}

			for (int i = 0; i < m_ListSceneImage.Count; ++i) {
				m_ListSceneImage[i].sprite = Sprite.Create(m_Dest, new Rect(0, 0, m_Dest.width, m_Dest.height), new Vector2(0.5f, 0.5f));
				EditorUtility.SetDirty(m_ListSceneImage[i]);
			}
			for (int i = 0; i < m_ListPrefabImage.Count; ++i) {
				m_ListPrefabImage[i].sprite = Sprite.Create(m_Dest, new Rect(0, 0, m_Dest.width, m_Dest.height), new Vector2(0.5f, 0.5f));
			}

			m_Source = m_Dest;
			m_Dest = null;

			GetAllMaterialsUseTexture(m_Source);
			AssetDatabase.Refresh();
		}

		void InitializeReorderableList() {
			m_MaterialReorderableList = new(m_ListMaterial, typeof(Material), true, true, false, false) {
				drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField(rect, "Material List Used Texture", headerLabelStyle);
				},
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
					Material element = (Material)m_MaterialReorderableList.list[index];
					rect.y += 2;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(
						new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						element, typeof(Material), false);
					EditorGUI.EndDisabledGroup();
				}
			};

			m_SceneReorderableList = new(m_ListSceneImage, typeof(UnityEngine.UI.Image), true, true, false, false) {
				drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField(rect, "Parent Image List in Scene", headerLabelStyle);
				},
				drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
					UnityEngine.UI.Image element = (UnityEngine.UI.Image)m_SceneReorderableList.list[index];
					rect.y += 2;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(
						new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						element, typeof(UnityEngine.UI.Image), false);
					EditorGUI.EndDisabledGroup();
				}
			};
			m_PrefabReorderableList = new(m_ListPrefab, typeof(GameObject), true, true, false, false) {
				drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField(rect, "Root GameObject List of Prefab", headerLabelStyle);
				},
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