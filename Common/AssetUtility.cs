
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	public class AssetUtility {
        // Supports the following syntax:
        // 't:type' syntax (e.g 't:Texture2D' will show Texture2D objects)
        // 'l:assetlabel' syntax (e.g 'l:architecture' will show assets with AssetLabel 'architecture')
        // 'ref[:id]:path' syntax (e.g 'ref:1234' will show objects that references the object with instanceID 1234)
        // 'v:versionState' syntax (e.g 'v:modified' will show objects that are modified locally)
        // 's:softLockState' syntax (e.g 's:inprogress' will show objects that are modified by anyone (except you))
        // 'a:area' syntax (e.g 'a:all' will s search in all assets, 'a:assets' will s search in assets folder only and 'a:packages' will s search in packages folder only)
        // 'glob:path' syntax (e.g 'glob:Assets/**/*.{png|PNG}' will show objects in any subfolder with name ending by .png or .PNG)
		public static Sprite[] LoadAllSprites(string path = "") {
			string[] arrGUID;
			if (path == "")
				arrGUID = UnityEditor.AssetDatabase.FindAssets("");
			else {
				if (path[^1] != '/')
					path += '/';

				arrGUID = UnityEditor.AssetDatabase.FindAssets(/*"glob:*.{png|PNG}"*/"", new string[] { path });
			}

			List<Sprite> listSprite = new List<Sprite>(arrGUID.Length);
			for (int i = 0; i < arrGUID.Length; i++) {
				string guid = arrGUID[i];
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

				Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
				if (sprite)
					listSprite.Add(sprite);
			}

			listSprite.Capacity = listSprite.Count;
			return listSprite.ToArray();
		}

		public static GameObject[] LoadAllPrefabs(string path = "") {
			string[] arrGUID;
			if (path == "")
				arrGUID = UnityEditor.AssetDatabase.FindAssets("t:prefab");
			else {
				if (path[^1] != '/')
					path += '/';
				arrGUID = UnityEditor.AssetDatabase.FindAssets("t:prefab", new string[] { path });
			}

			List<GameObject> listPrefab = new List<GameObject>(arrGUID.Length);
			for (int i = 0; i < arrGUID.Length; i++) {
				string guid = arrGUID[i];
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

				GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				if (prefab)
					listPrefab.Add(prefab);
			}

			listPrefab.Capacity = listPrefab.Count;
			return listPrefab.ToArray();
		}

		/// <summary> Warning: Don't use for Custom Class T (Only work for built-in type) </summary>
		public static T[] LoadAllAssetsOfType<T>(string path = "") where T : Object {
			string[] arrGUID;
			if (path == "")
				arrGUID = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).ToString());
			else {
				if (path[^1] != '/')
					path += '/';

				arrGUID = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).ToString(), new string[] { path });
			}

			List<T> listSprite = new List<T>(arrGUID.Length);

			for (int i = 0; i < arrGUID.Length; i++) {
				string guid = arrGUID[i];
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				T asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
				listSprite.Add(asset);
			}

			listSprite.Capacity = listSprite.Count;

			return listSprite.ToArray();
		}
	}

	// Object[] arrAsset = AssetDatabase.LoadAllAssetsAtPath(SPRITE_PATH);
}

#endif
