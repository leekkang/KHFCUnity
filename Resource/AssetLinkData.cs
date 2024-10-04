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
		//[SerializeField] List<LinkList> m_ListLink = new();

		// TODO : 아래 리스트, 딕셔너리는 현재 사용중이지만 serialized dictionary 도입 후 모두 제거

		// Awake에서 딕셔너리로 값을 옮기기 위해 에디터에서 저장하는 값
		[SerializeField] List<UnityEngine.Object> m_ListLink;
		[SerializeField] List<string> m_ListName;
		// O(1)의 접근시간을 위해 Awake에서 값을 만들어줌 프리팹 링크 이름, 인덱스
		Dictionary<string, int> m_DicIndex;

		public override void Awake() {
			base.Awake();

		}

		void MakeDictionary() {
			// TODO : serialized dictionary 도입 후 모두 제거
			if (m_ListLink != null) {
				m_DicIndex = new(m_ListLink.Count);
				for (int i = m_ListLink.Count - 1; i >= 0; --i) {
					m_DicIndex[m_ListName[i]] = i;
				}
			}
		}

		public T GetLink<T>(string name) where T : UnityEngine.Object {
			if (m_DicIndex == null) {
				MakeDictionary();
				if (m_DicIndex == null)
					return null;
			}
			if (m_DicIndex.TryGetValue(name, out int index))
				return (T)m_ListLink[index];
			return null;
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
			m_ListLink ??= new();
			m_ListLink?.Clear();
			m_ListName ??= new();
			m_ListName?.Clear();
			
			EditorUtility.SetDirty(gameObject);
		}

		public void AddLink<T>(string folderName, string prefabName, T obj) where T : UnityEngine.Object {
			//if (m_ListLink.TryGetValue(out GameObject prefab, x => x.name == prefabName)) {
			//	Debug.Log($"Replace Asset : {prefab.name}");
			//} else {
				m_ListLink.Add(obj);
				m_ListName.Add(prefabName);
				Debug.Log($"Create Link : {prefabName}");
			//}
			EditorUtility.SetDirty(gameObject);
		}

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
