
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;

namespace KHFC.Editor {
	// priority 순서 : 값이 작을수록 위쪽에 배치, 기본값은 1000이다.
	enum MenuPriority {
		Window = 0,
		Definer = 10,
		ETC = 100,
		UGUI = 800,
		CacheKey = 900,
		Shortcut = 1010,
		Others = 1100,

		GameObject = -100,
		Assets = 1000,
	}

	public class KHFCEditorMenu {
		const string NAME_ASSET_LINK_OBJ = "AssetLinkData";

		/// <summary>
		/// 실제 게임에서 사용하는 프리팹 경로를 <see cref="AssetLinkData"/> 오브젝트에 저장하는 함수
		/// </summary>
		[MenuItem("KHFC/Create and Fill Asset Link Data",
			priority = (int)MenuPriority.Others)]
		public static void FillAssetLinkData() {
			UnityEngine.SceneManagement.Scene curScene = EditorSceneManager.GetActiveScene();
			GameObject[] arrObj = curScene.GetRootGameObjects();
			foreach (var obj in arrObj) {
				GameObject target = obj.FindRecursively(NAME_ASSET_LINK_OBJ);
				if (target != null && target.TryGetComponent(out AssetLinkData comp)) {
					comp.Awake();
					EditorLinkData.SetAllData();
					return;
				}
			}

			// AssetLinkData 오브젝트가 없다. 생성
			GameObject linkObj = new GameObject(NAME_ASSET_LINK_OBJ);
			AssetLinkData component = linkObj.AddComponent<AssetLinkData>();
			component.Awake();
			EditorLinkData.SetAllData();
		}
		//	[MenuItem("KHFC/Fill Asset Link Data", isValidateFunction:true)]
		//	static bool ValidateFillAssetLinkData() {
		//#if UNITY_EDITOR
		//		UnityEngine.SceneManagement.Scene curScene = EditorSceneManager.GetActiveScene();
		//		bool isTarget = curScene.name == "Threematch" || curScene.name == "EditorMode";
		//		//if (!isTarget)
		//			//EditorUtility.DisplayDialog("Error", "현재 씬이 ThreeMatch가 아닙니다.", "close");

		//		return isTarget;
		//#else
		//		return false;
		//#endif
		//	}

		[MenuItem("KHFC/Add X 90 degree", priority = (int)MenuPriority.Others + 10)]
		public static void AddXdegree() {
			GameObject root = GameObject.Find("TestPlane");
			if (root == null) {
				Debug.LogError("testplane is not exist");
				return;
			}

			Transform tr = root.transform;
			for (int i = 1, count = tr.childCount; i < count; ++i) {
				Transform target = tr.GetChild(i).GetChild(0).GetChild(0).GetChild(0);
				Vector3 angle = target.localEulerAngles;
				target.localScale *= .2f;
				angle.y = 180f;
				target.localEulerAngles = angle;
			}
		}

		[MenuItem("KHFC/Show All Assembly", priority = (int)MenuPriority.Others + 1)]
		static void ShowAllAssembly() {
			UnityEditor.Compilation.Assembly[] assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();

			List<string> listName = new();
			foreach (UnityEditor.Compilation.Assembly assembly in assemblies) {
				try {
					string name = assembly.name;
					string substr = name.Length >= 5 ? name[..5] : "";

					if (substr != "Unity" && !name.StartsWith("com.uni") && !name.Contains("-Editor"))
						listName.Add(name);
				} catch { }
			}

			StringBuilder sb = new();
			for (int i = 0; i < listName.Count; ++i) {
				sb.AppendLine(listName[i]);
			}
			Debug.Log(sb);
			EditorUtility.DisplayDialog("All Assemblies", sb.ToString(), "close");
		}


		/// <summary> 씬 내의 모든 버튼 컴포넌트를 버튼위젯 컴포넌트로 변경한다. </summary>
		[MenuItem("KHFC/UGUI/Change UI.Button To KHFC.ButtonWdgt",
			priority = (int)MenuPriority.UGUI)]
		public static void ChangeButtonToButtonWdgt() {
			UnityEngine.SceneManagement.Scene curScene = EditorSceneManager.GetActiveScene();
			GameObject[] arrObj = curScene.GetRootGameObjects();
			foreach (var obj in arrObj) {
				obj.transform.DoRecursively(tr => {
					if (tr.TryGetComponent(out UnityEngine.UI.Button comp)) {
						tr.gameObject.RemoveComponent<UnityEngine.UI.Button>();
						tr.gameObject.SafeAddComponent<ButtonWdgt>();
					}
				});
			}
		}

		/// <summary> 특정 머테리얼의 모든 링크를 찾아주는 윈도우를 연다 </summary>
		[MenuItem("KHFC/Window/Open Material Finder Window", priority = (int)MenuPriority.Window)]
		static public void OpenMaterialFinderEditorWindow() {
			Material mat = null;
			if (Selection.activeObject is Material)
				mat = Selection.activeObject as Material;
			if (mat != null)
				MaterialFinderEditorWindow.ShowWindow(mat);
			else
				MaterialFinderEditorWindow.ShowWindow();
		}

		/// <summary> 특정 텍스쳐의 모든 링크를 찾아주는 윈도우를 연다 </summary>
		[MenuItem("KHFC/Window/Open Texture Finder Window", priority = (int)MenuPriority.Window + 1)]
		static public void OpenTextureFinderEditorWindow() {
			Texture2D tex = null;
			if (Selection.activeObject is Texture2D)
				tex = Selection.activeObject as Texture2D;
			if (tex != null)
				TextureFinderEditorWindow.ShowWindow(tex);
			else
				TextureFinderEditorWindow.ShowWindow();
		}

		/// <summary> 게임창의 사이즈를 변경한다 </summary>
		[MenuItem("KHFC/Window/Open Resize Editor Window", priority = (int)MenuPriority.Window + 2)]
		static public void OpenResizeEditorWindow() {
			EditorWindow.GetWindow<ResizeEditorWindow>(false, "GameView Size", true);
		}

		/// <summary> 사용하지 않는 에셋을 할당 해제한다 </summary>
		[MenuItem("KHFC/ETC/ClearProfilerMemory", priority = (int)MenuPriority.ETC)]
		public static void ClearMemory() {
			Resources.UnloadUnusedAssets();
			EditorUtility.UnloadUnusedAssetsImmediate(true);
		}

		/// <summary> 현재 화면을 png로 애셋 폴더에 저장한다 </summary>
		[MenuItem("KHFC/ETC/Capture Current Screen #0", priority = (int)MenuPriority.ETC + 1)] // 단축키 : shift + 0
		public static void CaptureCurrentScreen() {
			KHFC.Editor.ScreenCapture.TakeScreenCapture();
		}

		/// <summary> 선택한 게임오브젝트의 활성화 상태를 변경한다 </summary>
		[MenuItem("KHFC/Shortcut/SetActive %e", priority = (int)MenuPriority.Shortcut)]
		public static void SetActive() {
			foreach (GameObject obj in Selection.objects) {
				obj.SetActive(!obj.activeSelf);
				EditorUtility.SetDirty(obj);
			}
		}
		/// <summary> 현재 선택한 오브젝트들의 부모 오브젝트들을 저장한다. </summary>
		[MenuItem("KHFC/Shortcut/SavePrefab %q", priority = (int)MenuPriority.Shortcut + 1)]
		public static void SavePrefab() {
			List<GameObject> listParent = new();
			foreach (GameObject obj in Selection.objects) {
				GameObject targetPrefab = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
				if (targetPrefab != null
					&& PrefabUtility.IsPartOfPrefabInstance(targetPrefab)
					&& !listParent.Contains(targetPrefab)) {
					listParent.Add(targetPrefab);
				}
				//PrefabUtility.ApplyPrefabInstance(targetPrefab, InteractionMode.UserAction);
				PrefabUtility.ApplyPrefabInstances(listParent.ToArray(), InteractionMode.UserAction);
			}
		}



		/// <summary> 캔버스 아래의 모든 <see cref="MaskableGraphic"/>의 RaycastTarget 옵션을 끈다. </summary>
		[MenuItem("GameObject/KHFC/Disable Raycast Under Canvas", priority = (int)MenuPriority.GameObject)]
		public static void DisableRaycast() {
			GameObject go = Selection.activeGameObject;

			if (go == null)
				return;
			if (!(go.TryGetComponent(out Canvas canvas)
				|| go.TryGetComponent(out CanvasGroup group)
				|| go.TryGetComponent(out CanvasRenderer renderer)))
				return;

			go.transform.DoRecursively(tr => {
				if (tr.GetComponent<Button>())
					return;

				if (tr.TryGetComponent(out MaskableGraphic graphic)) {
					graphic.raycastTarget = false;
					graphic.maskable = false;
				}
			});

			Debug.Log($"raycast disable complete");
		}

		[MenuItem("GameObject/KHFC/Disable Raycast Under Canvas", true)]
		static bool DisableRaycastValidation() {
			// We can only copy the path in case 1 object is selected
			return Selection.gameObjects.Length == 1 &&
					(Selection.activeGameObject.TryGetComponent(out Canvas canvas) ||
					Selection.activeGameObject.TryGetComponent(out CanvasGroup group) ||
					Selection.activeGameObject.TryGetComponent(out CanvasRenderer renderer));
		}

		/// <summary> 자식 오브젝트에 붙어있는 컴포넌트에 따라 접두사를 붙이는 함수 </summary>
		/// <remarks> 씬 내에 인스턴스화 된 오브젝트만을 대상으로 한다. </remarks>
		[MenuItem("GameObject/KHFC/Prefixing by Component Type (GO)", priority = (int)MenuPriority.GameObject + 5)]
		static void UpdateCachedGameObject() {
			int count = Selection.gameObjects.Length;
			if (count == 0)
				return;

			//bool changed = false;
			foreach (var go in Selection.gameObjects) {
				go.transform.DoRecursively((tr) => {
					if (tr.GetComponent<ParticleSystem>() != null) {
						if (!tr.name.StartsWith("eff_"))
							tr.name = "eff_" + tr.name;
					} else if (tr.GetComponent<SpriteRenderer>() != null
							 //|| tr.GetComponent<UISprite>() != null	// NGUI 전용
							 ) {
						if (!tr.name.StartsWith("spr_"))
							tr.name = "spr_" + tr.name;
					}
				});
				EditorUtility.SetDirty(go);
			}

			//if (changed)
			//	EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
		}

		[MenuItem("Assets/KHFC/Create KHFCSetting Scriptable Object", priority = (int)MenuPriority.Assets)]
		public static void CreateMyAsset() {
			KHFCSetting asset = ScriptableObject.CreateInstance<KHFCSetting>();
			string[] arrGUID = AssetDatabase.FindAssets(string.Format("{0} t:script", "KHFCSetting"));

			string assetPath = AssetDatabase.GUIDToAssetPath(arrGUID[0]).Replace("KHFCSetting.cs", "Resources/KHFCSetting.asset");
			KHFC.Util.CreateDir(System.IO.Path.GetDirectoryName(assetPath));
			AssetDatabase.CreateAsset(asset, assetPath);
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset;
		}
		[MenuItem("Assets/KHFC/Create KHFCSetting Scriptable Object", isValidateFunction: true)]
		static bool ValidateCreateMyAsset() {
			string[] arrGUID = AssetDatabase.FindAssets(string.Format("{0} t:script", "KHFCSetting"));
			return Selection.gameObjects.Length == 1 
				&& (arrGUID != null && arrGUID.Length > 0);
		}

		/// <summary>
		/// 선택한 머테리얼을 사용하는 모든 파티클을 찾는 함수
		/// </summary>
		[MenuItem("Assets/KHFC/Find All Particles That Use This", priority = (int)MenuPriority.Assets)]
		public static void FindAllParticlesThatUseThisMaterial() {
			MaterialFinderEditorWindow.ShowWindow(Selection.activeObject as Material);
		}
		[MenuItem("Assets/KHFC/Find All Particles That Use This", isValidateFunction: true)]
		static bool ValidateFindAllParticlesThatUseThisMaterial() {
			return Selection.activeObject is Material;
		}

		/// <summary>
		/// 선택한 텍스쳐를 사용하는 모든 오브젝트를 찾는 함수
		/// </summary>
		[MenuItem("Assets/KHFC/Find All Materials That Use This", priority = (int)MenuPriority.Assets)]
		public static void FindAllObjectsThatUseThisTexture() {
			TextureFinderEditorWindow.ShowWindow(Selection.activeObject as Texture2D);
		}
		[MenuItem("Assets/KHFC/Find All Materials That Use This", isValidateFunction: true)]
		static bool ValidateFindAllObjectsThatUseThisTexture() {
			return Selection.activeObject is Texture2D;
		}

		/// <summary>
		/// 배치 제작용. 3DMerge 프로젝트에서만 사용
		/// </summary>
		//		[MenuItem("KHFC/Make Model To Object")]
		//		static public void MakeModelToObject() {
		//#if UNITY_EDITOR
		//			// base gameobject
		//			GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Media/Prefab/Empty.prefab");
		//			//string folderPath = "Assets/3D Props - Adorable Items/Adorable 3D Items";
		//			string folderPath = "Assets/Media/Mesh/LowpolyHats";
		//			string[] arrGUID = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });
		//			CreateAssetFromGUID(arrGUID, 15f);  // 1 / 모델 스케일 팩터 -> 0.01이면 100

		//			void CreateAssetFromGUID(string[] arrGUID, float scale) {
		//				for (int i = 0; i < arrGUID.Length; i++) {
		//					string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
		//					string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);

		//					//AssetDatabase.CopyAsset( )
		//					GameObject go = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
		//					PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

		//					GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		//					prefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		//					PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
		//					prefab.RemoveComponent<Animator>();
		//					prefab.SafeAddComponent<cakeslice.Outline>();

		//					MeshCollider col = prefab.SafeAddComponent<MeshCollider>();
		//					col.convex = true;
		//					col.isTrigger = false;
		//					col.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation
		//										| MeshColliderCookingOptions.EnableMeshCleaning
		//										| MeshColliderCookingOptions.WeldColocatedVertices
		//										| MeshColliderCookingOptions.UseFastMidphase;

		//					Transform parent = go.transform.GetChild(0).GetChild(0);
		//					prefab.transform.position = Vector3.zero;
		//					//prefab.transform.rotation = Quaternion.identity;
		//					prefab.transform.localEulerAngles = new Vector3(0f, 180, 0f);
		//					prefab.transform.localScale *= scale;
		//					prefab.transform.SetParent(parent, false);

		//					go.ChangeLayerRecursivley(LayerMask.NameToLayer("Object"));
		//					PrefabUtility.SaveAsPrefabAsset(go, $"Assets/Media/Prefab/CreatedByEditor/{prefab.name}.prefab");
		//					GameObject.DestroyImmediate(go);

		//					//AssetDatabase.CreateAsset
		//				}
		//			}
		//			AssetDatabase.Refresh();
		//#endif
		//		}
	}
}