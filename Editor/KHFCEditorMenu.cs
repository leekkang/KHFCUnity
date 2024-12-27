
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace KHFC.Editor {
	// priority 순서 : 상단 메뉴바는 값이 클수록 위쪽에 배치, 나머지 오브젝트, 애셋 우클릭 메뉴는 값이 작을수록 위쪽에 배치
	public class KHFCEditorMenu {
		const string NAME_ASSET_LINK_OBJ = "AssetLinkData";

		/// <summary>
		/// 실제 게임에서 사용하는 프리팹 경로를 <see cref="AssetLinkData"/> 오브젝트에 저장하는 함수
		/// </summary>
		[MenuItem("KHFC/Fill Asset Link Data")]
		public static void FillAssetLinkData() {
#if UNITY_EDITOR
			UnityEngine.SceneManagement.Scene curScene = EditorSceneManager.GetActiveScene();
			GameObject[] arrObj = curScene.GetRootGameObjects();
			foreach (var obj in arrObj) {
				GameObject target = obj.FindRecursively(NAME_ASSET_LINK_OBJ);
				if (target != null && target.TryGetComponent(out AssetLinkData comp)) {
					comp.Awake();
					AssetLinkDataEditor.SetAllData();
					return;
				}
			}

			// AssetLinkData 오브젝트가 없다. 생성
			GameObject linkObj = new GameObject(NAME_ASSET_LINK_OBJ);
			AssetLinkData component = linkObj.AddComponent<AssetLinkData>();
			component.Awake();
			AssetLinkDataEditor.SetAllData();
#endif
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

		/// <summary>
		/// 배치 제작용. 3DMerge 프로젝트에서만 사용
		/// </summary>
		//	[MenuItem("KHFC/Make Model To Object")]
		//	static public void MakeModelToObject() {
		//#if UNITY_EDITOR
		//		// base gameobject
		//		GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Media/Prefab/Empty.prefab");
		//		//string folderPath = "Assets/3D Props - Adorable Items/Adorable 3D Items";
		//		string folderPath = "Assets/Media/Mesh/LowpolyHats";
		//		string[] arrGUID = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });
		//		CreateAssetFromGUID(arrGUID, 15f);	// 1 / 모델 스케일 팩터 -> 0.01이면 100

		//		void CreateAssetFromGUID(string[] arrGUID, float scale) {
		//			for (int i = 0; i < arrGUID.Length; i++) {
		//				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
		//				string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);

		//				//AssetDatabase.CopyAsset( )
		//				GameObject go = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
		//				PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

		//				GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		//				prefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		//				PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
		//				prefab.RemoveComponent<Animator>();
		//				prefab.SafeAddComponent<cakeslice.Outline>();

		//				MeshCollider col = prefab.SafeAddComponent<MeshCollider>();
		//				col.convex = true;
		//				col.isTrigger = false;
		//				col.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation
		//									| MeshColliderCookingOptions.EnableMeshCleaning
		//									| MeshColliderCookingOptions.WeldColocatedVertices
		//									| MeshColliderCookingOptions.UseFastMidphase;

		//				Transform parent = go.transform.GetChild(0).GetChild(0);
		//				prefab.transform.position = Vector3.zero;
		//				//prefab.transform.rotation = Quaternion.identity;
		//				prefab.transform.localEulerAngles = new Vector3(0f, 180, 0f);
		//				prefab.transform.localScale *= scale;
		//				prefab.transform.SetParent(parent, false);

		//				go.ChangeLayerRecursivley(LayerMask.NameToLayer("Object"));
		//				PrefabUtility.SaveAsPrefabAsset(go, $"Assets/Media/Prefab/CreatedByEditor/{prefab.name}.prefab");
		//				GameObject.DestroyImmediate(go);

		//				//AssetDatabase.CreateAsset
		//			}
		//		}
		//		AssetDatabase.Refresh();
		//#endif
		//	}

		[MenuItem("KHFC/Add X 90 degree")]
		public static void AddXdegree() {
#if UNITY_EDITOR
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
#endif
		}

		[MenuItem("KHFC/Show All Assembly")]
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
		[MenuItem("KHFC/UGUI/Change UI.Button To KHFC.ButtonWdgt")]
		public static void ChangeButtonToButtonWdgt() {
#if UNITY_EDITOR
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
#endif
		}


		/// <summary> 사용하지 않는 에셋을 할당 해제한다 </summary>
		[MenuItem("KHFC/ETC/ClearProfilerMemory")]
		public static void ClearMemory() {
			Resources.UnloadUnusedAssets();
			EditorUtility.UnloadUnusedAssetsImmediate(true);
		}

		/// <summary> 게임창의 사이즈를 변경한다 </summary>
		[MenuItem("KHFC/ETC/Open Resize Editor Window", false, 20)]
		static public void ResizeEditorWindow() {
			EditorWindow.GetWindow<KHFC.ResizeEditorWindow>(false, "GameView Size", true);
		}

		/// <summary> 현재 화면을 png로 애셋 폴더에 저장한다 </summary>
		[MenuItem("KHFC/ETC/Capture Current Screen #0")] // 단축키 : shift + 0
		public static void CaptureCurrentScreen() {
			KHFC.Editor.ScreenCapture.TakeScreenCapture();
		}

		[MenuItem("KHFC/Shortcut/SetActive %e")]
		public static void SetActive() {
			foreach (GameObject obj in Selection.objects) {
				obj.SetActive(!obj.activeSelf);
				EditorUtility.SetDirty(obj);
			}
		}


		/// <summary> 자식 오브젝트에 붙어있는 컴포넌트에 따라 접두사를 붙이는 함수 </summary>
		/// <remarks> 씬 내에 인스턴스화 된 오브젝트만을 대상으로 한다. </remarks>
		[MenuItem("GameObject/KHFC/Prefixing by Component Type (GO)")]
		static void UpdateCachedGameObject() {
#if UNITY_EDITOR
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
#endif
		}

		/// <summary> 캔버스 아래의 모든 <see cref="MaskableGraphic"/>의 RaycastTarget 옵션을 끈다. </summary>
		[MenuItem("GameObject/KHFC/Disable Raycast Under Canvas", false, -99)]
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

		[MenuItem("Assets/KHFC/Create KHFCSetting Scriptable Object")]
		public static void CreateMyAsset() {
			KHFCSetting asset = ScriptableObject.CreateInstance<KHFCSetting>();
			string[] arrGUID = AssetDatabase.FindAssets(string.Format("{0} t:script", "KHFCSetting"));
			if (arrGUID == null || arrGUID.Length <= 0) {
				Debug.Log("KHFCSetting script is not found");
				return;
			}
			string assetPath = AssetDatabase.GUIDToAssetPath(arrGUID[0]).Replace("KHFCSetting.cs", "Resources/KHFCSetting.asset");
			KHFC.Util.CreateDir(System.IO.Path.GetDirectoryName(assetPath));
			AssetDatabase.CreateAsset(asset, assetPath);
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset;
		}
	}
}