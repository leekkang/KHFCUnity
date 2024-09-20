
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using KHFC;

// priority 순서 : 상단 메뉴바는 값이 클수록 위쪽에 배치, 나머지 오브젝트, 애셋 우클릭 메뉴는 값이 작을수록 위쪽에 배치
public class KHFCEditorMenu {
	const string NAME_ASSET_LINK_OBJ = "AssetLinkData";

	/// <summary>
	/// 실제 게임에서 사용하는 프리팹 경로를 <see cref="AssetLinkData"/> 오브젝트에 저장하는 함수
	/// </summary>
	[MenuItem("KHFC/Fill Asset Link Data")]
	static public void FillAssetLinkData() {
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
	[MenuItem("KHFC/Fill Asset Link Data", isValidateFunction:true)]
	static bool ValidateFillAssetLinkData() {
#if UNITY_EDITOR
		UnityEngine.SceneManagement.Scene curScene = EditorSceneManager.GetActiveScene();
		bool isTarget = curScene.name == "Threematch" || curScene.name == "EditorMode";
		//if (!isTarget)
			//EditorUtility.DisplayDialog("Error", "현재 씬이 ThreeMatch가 아닙니다.", "close");

		return isTarget;
#else
		return false;
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
		KHFC.ScreenCapture.TakeScreenCapture();
	}

	[MenuItem("KHFC/Shortcut/SetActive %e")]
	public static void SetActive() {
		foreach (GameObject obj in Selection.objects) {
			obj.SetActive(!obj.activeSelf);
		}
	}


	/// <summary> 자식 오브젝트에 붙어있는 컴포넌트에 따라 접두사를 붙이는 함수 </summary>
	/// <remarks> 씬 내에 인스턴스화 된 오브젝트만을 대상으로 한다. </remarks>
	[MenuItem("GameObject/KHFC/Prefixing by Component Type (GO)")]
	private static void UpdateCachedGameObject() {
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
}
