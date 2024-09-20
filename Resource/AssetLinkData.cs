using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KHFC {
	/// <summary>
	/// 애셋 데이터베이스에 있는 애셋들의 링크를 관리하는 클래스
	/// </summary>
	public class AssetLinkData : SingleGOComponent<AssetLinkData> {
		[Serializable]
		class LinkList {
			[SerializeField] public List<GameObject> m_List;
		}

		[SerializeField]
		List<LinkList> m_ListLink = new();

		//[System.Serializable]
		//class ItemLinkData {
		//	public ItemType m_Type;
		//	public GameObject m_Obj;

		//	public ItemLinkData(ItemType type, GameObject obj) {
		//		m_Type = type;
		//		m_Obj = obj;
		//	}
		//}

		//[SerializeField]
		//List<ItemLinkData> m_ListItem;

		//Dictionary<int, GameObject> m_DicItem;

		public override void Awake() {
			base.Awake();

			//if (m_ListItem != null) {
			//	m_DicItem = new Dictionary<int, GameObject>();
			//	foreach (ItemLinkData item in m_ListItem) {
			//		m_DicItem[(int)item.m_Type] = item.m_Obj;
			//	}
			//	//m_ListItem.Clear();
			//	//m_ListItem = null;
			//}
		}

		//public GameObject GetItemObj(ItemType type) {
		//	return m_DicItem.TryGetValue((int)type, out GameObject go) ? go : null;
		//}
		//public Item GetItem(ItemType type) {
		//	return m_DicItem.TryGetValue((int)type, out GameObject go) ? go.GetComponent<Item>() : null;
		//}
		//public bool TryGetItem<T>(PanelType type, out T item) where T : Item {
		//	if (m_DicItem.TryGetValue((int)type, out GameObject go))
		//		item = go.GetComponent<T>();
		//	else
		//		item = null;

		//	return item != null;
		//}

		//public GameObject GetPanelObj(PanelType type) {
		//	return m_DicPanel.TryGetValue((int)type, out GameObject go) ? go : null;
		//}
		//public Panel GetPanel(PanelType type) {
		//	return m_DicPanel.TryGetValue((int)type, out GameObject go) ? go.GetComponent<Panel>() : null;
		//}
		//public bool TryGetPanel<T>(PanelType type, out T panel) where T : Panel {
		//	if (m_DicPanel.TryGetValue((int)type, out GameObject go))
		//		panel = go.GetComponent<T>();
		//	else
		//		panel = null;

		//	return panel != null;
		//}

		//public GameObject GetEffect(EffectType type) {
		//	return m_DicEffect.TryGetValue((int)type, out GameObject go) ? go : null;
		//}

#if UNITY_EDITOR
		public void ResetAllList() {
			//m_ListItem ??= new();
			//m_ListItem?.Clear();
			EditorUtility.SetDirty(gameObject);
		}

		//public void AddItem(ItemType type, GameObject obj) {
		//	if (m_ListItem.TryGetValue(out ItemLinkData item, x => x.m_Type == type)) {
		//		item.m_Obj = obj;
		//		Debug.Log($"Replace Asset : {type}");
		//	} else {
		//		item = new ItemLinkData(type, obj);
		//		m_ListItem.Add(item);
		//		Debug.Log($"Create Asset : {type}");
		//	}
		//	EditorUtility.SetDirty(gameObject);
		//}

		//public void ResetItemList() {
		//	m_ListItem ??= new();
		//	m_ListItem.Clear();
		//	EditorUtility.SetDirty(gameObject);
		//}

		//public void AddPanel(PanelType type, GameObject obj) {
		//	if (m_ListPanel.TryGetValue(out PanelLinkData panel, x => x.m_Type == type)) {
		//		panel.m_Obj = obj;
		//		Debug.Log($"Replace Asset : {type}");
		//	} else {
		//		panel = new PanelLinkData(type, obj);
		//		m_ListPanel.Add(panel);
		//		Debug.Log($"Create Asset : {type}");
		//	}
		//	EditorUtility.SetDirty(gameObject);
		//}

		//public void ResetPanelList() {
		//	m_ListPanel ??= new();
		//	m_ListPanel.Clear();
		//	EditorUtility.SetDirty(gameObject);
		//}

		//public void AddEffect(EffectType type, GameObject obj) {
		//	if (m_ListEffect.TryGetValue(out EffectLinkData panel, x => x.m_Type == type)) {
		//		panel.m_Obj = obj;
		//		Debug.Log($"Replace Asset : {type}");
		//	} else {
		//		panel = new EffectLinkData(type, obj);
		//		m_ListEffect.Add(panel);
		//		Debug.Log($"Create Asset : {type}");
		//	}
		//	EditorUtility.SetDirty(gameObject);
		//}

		//public void ResetEffectList() {
		//	m_ListEffect ??= new();
		//	m_ListEffect.Clear();
		//	EditorUtility.SetDirty(gameObject);
		//}
#endif
	}
}
