
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KHFC {
	public class AssetLinkDataEditor {
		const string PREFAB_ROOT_PATH = "Assets/02.Prefabs";

		//public static Dictionary<ItemType, string> dicItem = new Dictionary<ItemType, string>() {
		//	{ ItemType.Normal, "NormalItem" },
		//	{ ItemType.Line_X, "LineXItem" },
		//	{ ItemType.Line_Y, "LineYItem" },
		//	{ ItemType.Line_C, "LineCItem" },
		//	{ ItemType.Butterfly, "ButterflyItem" },
		//	{ ItemType.Bomb, "BombItem" },
		//	{ ItemType.Rainbow, "RainbowItem" },
		//	{ ItemType.Donut, "DonutItem" },
		//	{ ItemType.Spiral, "SpiralItem" },
		//	{ ItemType.JellyBear, "JellyBearItem" },
		//	{ ItemType.TimeBomb, "TimeBombItem" },
		//	{ ItemType.Misson_Food1, "Food1Item" },
		//	{ ItemType.Misson_Food2, "Food2Item" },
		//	{ ItemType.Misson_Food3, "Food3Item" },
		//	{ ItemType.Misson_Food4, "Food4Item" },
		//	{ ItemType.Misson_Food5, "Food5Item" },
		//	{ ItemType.Misson_Food6, "Food6Item" },
		//	{ ItemType.BonusCross, "BonusCrossItem" },
		//	{ ItemType.BonusBomb, "BonusBombItem" },
		//	{ ItemType.Mystery, "MysteryItem" },
		//	{ ItemType.Chameleon, "ChameleonItem" },
		//	{ ItemType.JellyMon, "JellyMonItem" },
		//	{ ItemType.Ghost, "GhostItem" },
		//	{ ItemType.Key, "KeyItem" },
		//	{ ItemType.Lizard, "LizardItem" },
		//	{ ItemType.Foot, "FootItem" },
		//	{ ItemType.Fish, "FishItem" },
		//};


		public static void SetAllData() {
			AssetLinkData.inst.ResetAllList();

			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
				string folderName = System.IO.Path.GetDirectoryName(prefabPath);
		//		bool found = false;
		//		foreach (var pair in dicItem) {
		//			if (dicItem[pair.Key] == prefabName) {
		//				GameObject item = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
		//				AssetLinkData.inst.AddItem(pair.Key, item);
		//				found = true;
		//			}
		//		}
		//		if (found)
		//			continue;
		//		foreach (var pair in dicPanel) {
		//			if (dicPanel[pair.Key] == prefabName) {
		//				GameObject panel = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
		//				AssetLinkData.inst.AddPanel(pair.Key, panel);
		//				found = true;
		//			}
		//		}
		//		if (found)
		//			continue;
		//		foreach (var pair in dicEffect) {
		//			if (dicEffect[pair.Key] == prefabName) {
		//				GameObject effect = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		//				AssetLinkData.inst.AddEffect(pair.Key, effect);
		//				found = true;
		//			}
		//		}
			}
			AssetDatabase.Refresh();
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