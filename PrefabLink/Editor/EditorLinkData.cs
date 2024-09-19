
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KHFC {
	public class EditorLinkData {
		const string PREFAB_ROOT_PATH = "Assets/Media/Prefab";

		public static Dictionary<ItemType, string> dicItem = new Dictionary<ItemType, string>() {
			{ ItemType.Normal, "NormalItem" },
			{ ItemType.Line_X, "LineXItem" },
			{ ItemType.Line_Y, "LineYItem" },
			{ ItemType.Line_C, "LineCItem" },
			{ ItemType.Butterfly, "ButterflyItem" },
			{ ItemType.Bomb, "BombItem" },
			{ ItemType.Rainbow, "RainbowItem" },
			{ ItemType.Donut, "DonutItem" },
			{ ItemType.Spiral, "SpiralItem" },
			{ ItemType.JellyBear, "JellyBearItem" },
			{ ItemType.TimeBomb, "TimeBombItem" },
			{ ItemType.Misson_Food1, "Food1Item" },
			{ ItemType.Misson_Food2, "Food2Item" },
			{ ItemType.Misson_Food3, "Food3Item" },
			{ ItemType.Misson_Food4, "Food4Item" },
			{ ItemType.Misson_Food5, "Food5Item" },
			{ ItemType.Misson_Food6, "Food6Item" },
			{ ItemType.BonusCross, "BonusCrossItem" },
			{ ItemType.BonusBomb, "BonusBombItem" },
			{ ItemType.Mystery, "MysteryItem" },
			{ ItemType.Chameleon, "ChameleonItem" },
			{ ItemType.JellyMon, "JellyMonItem" },
			{ ItemType.Ghost, "GhostItem" },
			{ ItemType.Key, "KeyItem" },
			{ ItemType.Lizard, "LizardItem" },
			{ ItemType.Foot, "FootItem" },
			{ ItemType.Fish, "FishItem" },
		};

		public static Dictionary<PanelType, string> dicPanel = new Dictionary<PanelType, string>() {
			{ PanelType.Default_Empty, "DefaultEmptyPanel" },
			{ PanelType.Default_Full, "DefaultFullPanel" },
			{ PanelType.Creator_Empty, "CreatorEmptyPanel" },
			{ PanelType.Wafer_floor, "WaferPanel" },
			{ PanelType.ConveyerBelt, "ConveyerBelt" },
			{ PanelType.IceCream_Block, "IceCreamPanel" },
			{ PanelType.IceCream_Creator, "IceCreamCreaterPanel" },
			{ PanelType.Bread_Block, "BreadPanel" },
			{ PanelType.Fixed_Block, "FixedPanel" },
			{ PanelType.Lolly_Cage, "LollyCagePanel" },
			{ PanelType.Ice_Cage, "IceCagePanel" },
			{ PanelType.Warp_In, "WarpInPanel" },
			{ PanelType.Warp_Out, "WarpOutPanel" },
			{ PanelType.FoodArrive, "FoodArrive" },
			{ PanelType.JellyBearStart, "JellyBearStart" },
			{ PanelType.Creator_Food, "CreatorFood" },
			{ PanelType.Creator_Sprial, "CreatorSpiral" },
			{ PanelType.Creator_TimeBomb, "CreatorTimeBomb" },
			{ PanelType.Creator_Food_Sprial, "CreatorFoodSpiral" },
			{ PanelType.Creator_Food_TimeBomb, "CreatorFoodTimeBomb" },
			{ PanelType.Creator_Sprial_TimeBomb, "CreatorSprialTimeBomb" },
			{ PanelType.Cake_A, "CakeAPanel" },
			{ PanelType.Cake_B, "CakeBPanel" },
			{ PanelType.Cake_C, "CakeCPanel" },
			{ PanelType.Cake_D, "CakeDPanel" },
			{ PanelType.Jam, "JamPanel" },
			{ PanelType.Cracker, "CrackerPanel" },
			{ PanelType.Bottle_Cage, "BottleCagePanel" },
			{ PanelType.Creator_Key, "CreatorKey" },
			{ PanelType.Creator_Key_Food, "CreatorKeyFood" },
			{ PanelType.Creator_Key_TimeBomb, "CreatorKeyTimeBomb" },
			{ PanelType.S_Tree, "S_TreePanel" },
			{ PanelType.MagicColor, "MagicColorPanel" },
			{ PanelType.Ring, "RingPanel" },
			{ PanelType.JewelTree_A, "JewelTreeAPanel" },
			{ PanelType.JewelTree_B, "JewelTreeAPanel" },
			{ PanelType.JewelTree_C, "JewelTreeAPanel" },
			{ PanelType.JewelTree_D, "JewelTreeAPanel" },
			{ PanelType.Stele, "StelePanel" },
			{ PanelType.Stele_Hide, "SteleHidePanel" },
			{ PanelType.LizardLoad, "LizardLoad" },
			{ PanelType.LizardArrive, "LizardArrive" },
			{ PanelType.FishCage, "FishCagePanel" },
			{ PanelType.Crow, "CrowPanel" },
		};

		public static Dictionary<EffectType, string> dicEffect = new Dictionary<EffectType, string>() {
			{ EffectType.DefaultPiece1, "PieceEffect" },
			{ EffectType.DefaultPiece2, "PieceEffect2" },
			{ EffectType.Bomb, "BombEffect" },
			{ EffectType.BombNoDelay, "BombEffectNoDelay" },
			{ EffectType.BombBig1, "BombBigEffect" },
			{ EffectType.BombBig2, "BigRREffect" },
			{ EffectType.BonusBomb, "BonusBombEffect" },
			{ EffectType.Spiral, "SpiralEffect" },
			{ EffectType.Donut, "DonubEffect" },
			{ EffectType.BombCircle, "BombBrust" },
			{ EffectType.TimeBomb1, "TimeBombEffect" },
			{ EffectType.TimeBomb2, "TimeBombEffect2" },
			{ EffectType.Lightning1, "LightningEffect" },
			{ EffectType.Lightning2, "LightningEffect2" },
			{ EffectType.Rainbow, "fxPRainbow" },
			{ EffectType.Owl1, "OwlEffect_pidgeon" },
			{ EffectType.Owl2, "OwlEffect2" },
			{ EffectType.Smoke, "SmokeEffect" },
			{ EffectType.Butterfly, "ButterflyEffect" },
			{ EffectType.Gas1, "GasEffect" },
			{ EffectType.Gas2, "GasEffect2" },
			{ EffectType.Jellymon, "JellymonEffect" },
			{ EffectType.RainBowX2, "RainbowX2Brust" },
			{ EffectType.Chameleon, "ChameleonPieceEffect" },
			{ EffectType.Hammer, "HammerEffect" },
			{ EffectType.RainbowCircle, "fx_RainbowCircle" },
			{ EffectType.JellyBear, "JellyBearEffect" },

			{ EffectType.WaferAllInOne, "WaferEffect" },
			{ EffectType.Wafer, "WaferEffect 1" },
			{ EffectType.Wafer1, "WaferEffect 2" },
			{ EffectType.Wafer2, "WaferEffect 3" },
			{ EffectType.Bread1, "BreadEffect1" },
			{ EffectType.Bread2, "BreadEffect2" },
			{ EffectType.LollyCage, "LollyCageEffect" },
			{ EffectType.LollyCage2, "LollyCageEffect2" },
			{ EffectType.LollyCage3, "LollyCageEffect3" },
			{ EffectType.IceCage, "IceCageEffect" },
			{ EffectType.IceCream, "IceCreamEffect1" },
			{ EffectType.IceCream2, "IceCreamEffect2" },
			{ EffectType.IceCream3, "IceCreamEffect3" },
			{ EffectType.Cake, "CakeEffect" },
			{ EffectType.Cake2, "CakeEffect2" },
			{ EffectType.CakeBurst, "CakeBurstEffect" },
			{ EffectType.Jam, "JamEffect" },
			{ EffectType.IceCreamCreater, "IceCreamCreaterEffect" },
			{ EffectType.Cracker, "CrackerEffect" },
			{ EffectType.Mystery, "MysteryEffect" },
			{ EffectType.Jellybear, "JellybearEffect" },
			{ EffectType.BottleDef6, "BottleEffectDef6" },
			{ EffectType.Bottle2, "BottleEffect2Piece" },
			{ EffectType.Bottle4, "BottleEffect4Piece" },
			{ EffectType.BottleShine, "BottleShine" },
			{ EffectType.MagicColor, "MagicColorEffect" },
			{ EffectType.S_TreeDef2, "S_TreeEffectDef2" },
			{ EffectType.S_TreeDef1, "S_TreeEffectDef1" },
			{ EffectType.S_TreeDef0, "S_TreeEffectDef0" },
			{ EffectType.JewelTree, "JewelTreeEffect" },
			{ EffectType.SteleHide, "SteleHideEffect" },
			{ EffectType.Foot, "FootEffect" },
			{ EffectType.Fish1, "FishEffect" },
			{ EffectType.FishCageDef0, "FishCageEffectDef0" },
			{ EffectType.FishCageDef1, "FishCageEffectDef1" },
			{ EffectType.FishCageDef2, "FishCageEffectDef2" },
			{ EffectType.Crow, "CrowEffectAllBrust" },
			{ EffectType.CrowHit, "CrowEffectHit" },
			{ EffectType.Line, "LineEffect" },
			{ EffectType.LineBomb, "LineBombEffect" },
			{ EffectType.BigShot, "BigShotEffect" },

			{ EffectType.ItemChange, "ItemChangeEffect" },
			{ EffectType.ItemChange2, "ItemChangeEffect2" },
			{ EffectType.ItemChange3, "ItemChangeEffect3" },
			{ EffectType.Sign, "SignEffect" },
			{ EffectType.BigBrust, "TimeBombBrust" },
			{ EffectType.Block, "BlockEffect" },
			{ EffectType.Switch, "SwitchEffect" },
			{ EffectType.Switch1, "SwitchEffect1" },
			{ EffectType.Panel, "Block_PanelEffect" },
			{ EffectType.LizardLoad, "LizardLoadEffect" },
			{ EffectType.BigLineShot, "BigLineShot" },
			{ EffectType.FishRun, "FishRunEffect" },
			{ EffectType.GhostDestory, "GhostDestroyEffect" },
			{ EffectType.BigLineShotEff, "BigLineShotEffect" },
			{ EffectType.BlackWater, "BlackWaterEffect" },
			{ EffectType.WaterSpread, "WaterSpreadEffect" },
			{ EffectType.FishCageWater, "FishCageWaterEffect" },
		};


		public static void SetAllData() {
			AssetLinkData.inst.ResetAllList();

			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
				bool found = false;
				foreach (var pair in dicItem) {
					if (dicItem[pair.Key] == prefabName) {
						GameObject item = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
						AssetLinkData.inst.AddItem(pair.Key, item);
						found = true;
					}
				}
				if (found)
					continue;
				foreach (var pair in dicPanel) {
					if (dicPanel[pair.Key] == prefabName) {
						GameObject panel = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
						AssetLinkData.inst.AddPanel(pair.Key, panel);
						found = true;
					}
				}
				if (found)
					continue;
				foreach (var pair in dicEffect) {
					if (dicEffect[pair.Key] == prefabName) {
						GameObject effect = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
						AssetLinkData.inst.AddEffect(pair.Key, effect);
						found = true;
					}
				}
			}
			AssetDatabase.Refresh();
		}

		public static void SetAllItemToData() {
			AssetLinkData.inst.ResetItemList();

			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
				foreach (var pair in dicItem) {
					if (dicItem[pair.Key] == prefabName) {
						GameObject item = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

						AssetLinkData.inst.AddItem(pair.Key, item);
					}
				}
			}
			AssetDatabase.Refresh();
		}

		public static void SetAllPanelToData() {
			AssetLinkData.inst.ResetPanelList();

			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
				foreach (var pair in dicPanel) {
					if (dicPanel[pair.Key] == prefabName) {
						GameObject panel = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

						AssetLinkData.inst.AddPanel(pair.Key, panel);
					}
				}
			}
			AssetDatabase.Refresh();
		}

		public static void SetAllEffectToData() {
			AssetLinkData.inst.ResetEffectList();

			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
				foreach (var pair in dicEffect) {
					if (dicEffect[pair.Key] == prefabName) {
						GameObject effect = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

						AssetLinkData.inst.AddEffect(pair.Key, effect);
					}
				}
			}
			AssetDatabase.Refresh();
		}

		public static GameObject SetItem(ItemType type) {
			string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { PREFAB_ROOT_PATH });
			string targetName = dicItem[type];
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				if (targetName == System.IO.Path.GetFileNameWithoutExtension(prefabPath)) {
					GameObject item = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

					AssetLinkData.inst.AddItem(type, item);
					return item;
				}
			}
			Debug.LogError($"Target Item is not found : {type}, {targetName}");

			return null;
		}
	};
}
#endif
