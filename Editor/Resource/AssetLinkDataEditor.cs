
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KHFC.Editor {
	// TODO : AssetLinkData를 제네릭하게 만들면 해당 코드 전부 AssetLinkData로 옮겨도 될 듯
	// TODO : Addressables 도입으로 필요없어짐. Addressables를 사용하지 않는 시스템에서만 사용
	public class AssetLinkDataEditor {
		// TODO : Addressable 도입으로 전부 번들로 빠짐, 스프라이트는 아틀라스에서 복사하기 때문에 강한 참조 사용 제외
		//const string PREFAB_ROOT_PATH = "Assets/Media/Prefab";
		//const string SOUND_ROOT_PATH = "Assets/Media/Sound";
		const string SPRITE_ITEM_ROOT_PATH = "Assets/Media/Texture/UI/Item";

		public static void SetAllData() {
			AssetLinkData.inst.ResetAllList();

			// Sprite
			string[] arrGUID = AssetDatabase.FindAssets("t:Sprite", new string[] { SPRITE_ITEM_ROOT_PATH });
			LoadAssetFromGUID<Sprite>(arrGUID);

			// Prefab
			//arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			//LoadAssetFromGUID<GameObject>(arrGUID);

			// Sound
			//arrGUID = AssetDatabase.FindAssets("t:AudioClip", new string[] { SOUND_ROOT_PATH });
			//LoadAssetFromGUID<AudioClip>(arrGUID);

			AssetDatabase.Refresh();

			void LoadAssetFromGUID<T>(string[] arrGUID) where T : UnityEngine.Object {
				for (int i = 0; i < arrGUID.Length; i++) {
					string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
					string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
					string folderName = System.IO.Path.GetDirectoryName(prefabPath);

					T prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(T)) as T;
					AssetLinkData.inst.AddLink(folderName, prefabName, prefab);
				}
			}
		}

		//public static void SetAllItemToData() {
		//	AssetLinkData.inst.ResetItemList();

		//	string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
		//	for (int i = 0; i < arrGUID.Length; i++) {
		//		string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
		//		string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
		//		foreach (var pair in dicItem) {
		//			if (dicItem[pair.Key] == prefabName) {
		//				GameObject item = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

		//				AssetLinkData.inst.AddItem(pair.Key, item);
		//			}
		//		}
		//	}
		//	AssetDatabase.Refresh();
		//}

		//public static void SetAllPanelToData() {
		//	AssetLinkData.inst.ResetPanelList();

		//	string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
		//	for (int i = 0; i < arrGUID.Length; i++) {
		//		string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
		//		string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
		//		foreach (var pair in dicPanel) {
		//			if (dicPanel[pair.Key] == prefabName) {
		//				GameObject panel = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

		//				AssetLinkData.inst.AddPanel(pair.Key, panel);
		//			}
		//		}
		//	}
		//	AssetDatabase.Refresh();
		//}

		//public static void SetAllEffectToData() {
		//	AssetLinkData.inst.ResetEffectList();

		//	string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
		//	for (int i = 0; i < arrGUID.Length; i++) {
		//		string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
		//		string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
		//		foreach (var pair in dicEffect) {
		//			if (dicEffect[pair.Key] == prefabName) {
		//				GameObject effect = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

		//				AssetLinkData.inst.AddEffect(pair.Key, effect);
		//			}
		//		}
		//	}
		//	AssetDatabase.Refresh();
		//}

		//public static GameObject SetItem(ItemType type) {
		//	string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
		//	string targetName = dicItem[type];
		//	for (int i = 0; i < arrGUID.Length; i++) {
		//		string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
		//		if (targetName == System.IO.Path.GetFileNameWithoutExtension(prefabPath)) {
		//			GameObject item = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

		//			AssetLinkData.inst.AddItem(type, item);
		//			return item;
		//		}
		//	}
		//	Debug.LogError($"Target Item is not found : {type}, {targetName}");

		//	return null;
		//}
	}
}
#endif
