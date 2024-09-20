using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	/// <summary>
	/// 지정한 자식 오브젝트들의 경로로 대상을 캐싱하는 컴포넌트
	/// </summary>
	public abstract class CachedComponent : MonoBehaviour {
		/// <summary> 현재 오브젝트가 사용하는 게임 오브젝트들의 리스트 </summary>
		[SerializeField]
		protected List<GameObject> m_ListCachedObject;

#if UNITY_EDITOR
		/// <summary> 해당 스크립트에서 사용할 모든 게임 오브젝트의 경로 </summary>
		/// <remarks> key는 경로, value는 함수에서 가져오도록 등록할 이름 </remarks>
		public Dictionary<string, int> m_DicCacheKey;

		public virtual void LoadCacheKeys() { }

		/// <summary>
		/// 스크립트 컴파일이 완료되면 유니티가 호출하는 함수. 컴파일할 때 마다 호출하기 때문에
		/// 값을 이미 다 채운 상태라면 매크로를 false로 변경
		/// </summary>
		/// <remarks> Obsolete : 인스펙터 버튼으로 위치 이동 </remarks>
		//public void OnValidate() {
		//}
#endif

		/// <summary> <see cref="System.Enum"/> 타입을 인덱스로 사용해서 캐싱된 오브젝트를 찾는 함수 </summary>
		/// <remarks> <see cref="LoadCacheKeys()"/> 함수에서 필요한 오브젝트의 경로를 지정한다. </remarks>
		public GameObject GetCachedObject<T>(T alias) where T : System.Enum {
			if (m_ListCachedObject == null) {
				Debug.LogError($"Cached Object List is null!");
				return null;
			}

			//int index = Convert.ToInt32(alias);
			int index = (int)((object)alias);

			if (m_ListCachedObject.Count > index)
				return m_ListCachedObject[index];
			else {
				Debug.LogError($"Cached Object is not found : {alias}");
				return null;
			}
		}

		/// <summary> 인덱스로 캐싱된 오브젝트를 찾는 함수 </summary>
		/// <remarks> <see cref="LoadCacheKeys()"/> 함수에서 필요한 오브젝트의 경로를 지정한다. </remarks>
		public GameObject GetCachedObject(int index) {
			if (m_ListCachedObject == null) {
				Debug.LogError($"Cached Object List is null!");
				return null;
			}

			if (m_ListCachedObject.Count > index)
				//if (m_ListCachedObject.TryGetValue(index, out GameObject obj))
				return m_ListCachedObject[index];
			else {
				Debug.LogError($"Cached Object is not found : {index}");
				return null;
			}
		}

		/// <summary>
		/// <see cref="System.Enum"/> 타입을 인덱스로 사용해서 캐싱된 오브젝트에 지정한 컴포넌트를 찾아 반환하는 함수
		/// </summary>
		/// <remarks> <see cref="LoadCacheKeys()"/> 함수에서 필요한 오브젝트의 경로를 지정한다. </remarks>
		public T GetCachedObject<T>(System.Enum alias) where T : Component {
			GameObject cachedGo = GetCachedObject(alias);
			if (cachedGo == null)
				return null;

			if (typeof(T) == typeof(GameObject))
				Debug.LogWarning($"Use Component Type, current type : {typeof(T).Name}");

			return (T)cachedGo.GetComponent<T>();
		}

		/// <summary> 인덱스로 캐싱된 오브젝트에 지정한 컴포넌트를 찾아 반환하는 함수 </summary>
		/// <remarks> <see cref="LoadCacheKeys()"/> 함수에서 필요한 오브젝트의 경로를 지정한다. </remarks>
		public T GetCachedObject<T>(int index) where T : Component {
			GameObject cachedGo = GetCachedObject(index);
			if (cachedGo == null)
				return null;

			if (typeof(T) == typeof(GameObject))
				Debug.LogWarning($"Use Component Type, current type : {typeof(T).Name}");

			return (T)cachedGo.GetComponent<T>();
		}

#if UNITY_EDITOR
		/// <summary>
		/// <see cref="m_DicCacheKey"/> 에 경로 데이터를 로드한 후 <see cref="m_ListCachedObject"/> 에 저장하는 함수
		/// </summary>
		/// <remarks> 시작 전 에디터에서 미리 로드해야 게임 내에서 경로 사용 가능 </remarks>
		public bool FindCachedObject() {
			LoadCacheKeys();
			if (m_DicCacheKey == null || m_DicCacheKey.Count == 0)
				return false;

			m_ListCachedObject = new List<GameObject>();
			m_ListCachedObject.Resize(m_DicCacheKey.Count);
			foreach (var key in m_DicCacheKey) {
				Transform tf = transform.GetTransformByFullName(key.Key);

				if (tf == null) {
					Debug.LogError($"Can't Find Transfrom by FullName : {key}, Panel : {GetType().Name}");
					return false;
				}

				m_ListCachedObject[key.Value] = tf.gameObject;
			}

			return true;
		}

		[KHFC.InspectorButton("LoadCacheKeys() 에서 정의한 캐시 경로로 오브젝트를 찾아 저장")]
		public void UpdateCacheKey() {
			FindCachedObject();

			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			UnityEditor.EditorUtility.SetDirty(gameObject);
		}
#endif
	}
}