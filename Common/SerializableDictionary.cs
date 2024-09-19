using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유니티가 데이터 저장 시 사용하는 직렬화가 가능한 딕셔너리
/// </summary>
/// <remarks> <see cref="ISerializationCallbackReceiver"/> 인터페이스는 유니티에 정의되어있다. </remarks>
[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
	[SerializeField]
	private List<TKey> m_ListKey = new List<TKey>();
	[SerializeField]
	private List<TValue> m_ListValue = new List<TValue>();

	// save the dictionary to lists
	public void OnBeforeSerialize() {
		m_ListKey.Clear();
		m_ListValue.Clear();
		foreach (KeyValuePair<TKey, TValue> pair in this) {
			m_ListKey.Add(pair.Key);
			m_ListValue.Add(pair.Value);
		}
	}

	// load dictionary from lists
	public void OnAfterDeserialize() {
		this.Clear();

		// value가 더 많으면 생략
		bool overflow = m_ListKey.Count < m_ListValue.Count;
		if (overflow)
			Debug.LogError($"there are {m_ListKey.Count} keys and {m_ListValue.Count} values after deserialization." +
							" Make sure that both key and value types are serializable.");

		for (int i = 0; i < m_ListKey.Count; i++) {
			this.Add(m_ListKey[i], i < m_ListValue.Count ? m_ListValue[i] : default);
		}
	}
}
