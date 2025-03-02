
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace KHFC.Editor {
	public class CachedKeyEditorMenu {
		const string PREFAB_ROOT_PATH = "Assets/Media/Prefab";

		/// <summary>
		/// <see cref="KHFC.CachedComponent"/>를 상속받은 모든 오브젝트의 캐시 키를 업데이트하는 함수
		/// </summary>
		/// <remarks> 씬 내에 인스턴스화 된 오브젝트만을 대상으로 한다. </remarks>
		[MenuItem("KHFC/Cached Key/Update All GameObject", priority = (int)MenuPriority.CacheKey)]
		static public void UpdateAllCachedGameObject() {
			UnityEngine.SceneManagement.Scene curScene = EditorSceneManager.GetActiveScene();
			GameObject[] arrObj = curScene.GetRootGameObjects();
			bool changed = false;
			foreach (var go in arrObj) {
				changed |= UpdateCacheKeyRecursive(go.transform);
				if (changed)
					EditorUtility.SetDirty(go);
			}

			if (changed)
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
		}

		/// <summary>
		/// <see cref="KHFC.CachedComponent"/>를 상속받은 모든 오브젝트의 캐시 키를 업데이트하는 함수
		/// </summary>
		/// <remarks> 폴더 내에 프리팹화 된 오브젝트만을 대상으로 한다. </remarks>
		[MenuItem("KHFC/Cached Key/Update All Prefab", priority = (int)MenuPriority.CacheKey + 1)]
		static public void UpdateAllCachedPrefab() {
			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
				UpdateCacheKeyRecursive(go.transform);
				// TODO : 애셋자체를 수정하는거라 Apply 안해도 자동 저장되는듯? 문제있으면 주석 해제
				//if (UpdateCacheKeyRecursive(go.transform))
				//PrefabUtility.SaveAsPrefabAsset(go, path);
				// ApplyPrefabInstance() 함수는 씬 내 인스턴스화 된 프리팹만 대상이 된다. 주석 해제 불필요
				//PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
			}
			AssetDatabase.Refresh();
		}


		/// <summary> <see cref="KHFC.CachedComponent"/>를 상속받은 오브젝트의 캐시 키를 업데이트하는 함수 </summary>
		/// <remarks> 씬 내에 인스턴스화 된 오브젝트만을 대상으로 한다. </remarks>
		[MenuItem("GameObject/KHFC/Update Cache Key (GO)", priority = (int)MenuPriority.GameObject)]
		private static void UpdateCachedGameObject() {
			int count = Selection.gameObjects.Length;
			if (count == 0)
				return;

			bool changed = false;
			foreach (var go in Selection.gameObjects) {
				changed |= UpdateCacheKeyRecursive(go.transform);
				if (changed)
					EditorUtility.SetDirty(go);
			}

			//if (changed)
			//	EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
		}

		/// <summary> <see cref="KHFC.CachedComponent"/>를 상속받은 오브젝트의 캐시 키를 업데이트하는 함수 </summary>
		/// <remarks> 폴더 내에 프리팹화 된 오브젝트만을 대상으로 한다. </remarks>
		[MenuItem("Assets/KHFC/Update Cache Key (P)", true)]
		private static void UpdateCachedPrefab() {
			int count = Selection.gameObjects.Length;
			if (count == 0)
				return;

			foreach (var go in Selection.gameObjects) {
				if (PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.Regular)
					continue;
				if (UpdateCacheKeyRecursive(go.transform)) {
					string path = AssetDatabase.GetAssetPath(go);
					// TODO : 애셋자체를 수정하는거라 Apply 안해도 자동 저장되는듯? 문제있으면 주석 해제
					//PrefabUtility.SaveAsPrefabAsset(go, path);
					// ApplyPrefabInstance() 함수는 씬 내 인스턴스화 된 프리팹만 대상이 된다. 주석 해제 불필요
					//PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
					Debug.Log($" Cache Key Update Complete : {go.name}");
				}
			}
			AssetDatabase.Refresh();
		}


		/// <summary> <see cref="CachedComponent"/> 가 있는 오브젝트를 찾아 캐시키를 리스트에 저장 </summary>
		/// <remarks> 부모 오브젝트에 컴포넌트가 있으면 자식 오브젝트는 건너뛴다. </remarks>
		static bool UpdateCacheKeyRecursive(Transform target) {
			bool changed = false;
			if (target.TryGetComponent(out KHFC.CachedComponent comp)) {
				changed = comp.FindCachedObject();
			} else {
				foreach (Transform child in target)
					changed |= UpdateCacheKeyRecursive(child);
			}

			return changed;
		}
	}
}