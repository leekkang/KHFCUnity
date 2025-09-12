using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KHFC {
	/// <summary>
	/// 지정한 자식 오브젝트들의 경로로 대상을 캐싱하는 컴포넌트
	/// </summary>
	public abstract class CachedComponent : MonoBehaviour {
		/// <summary> 현재 오브젝트가 사용하는 게임 오브젝트들의 리스트 </summary>
		[SerializeField]
		protected List<GameObject> m_ListCachedObject;

		/// <summary> GetComponent의 오버헤드를 줄이기 위해 컴포넌트를 캐싱하는 딕셔너리 </summary>
		[NonSerialized]
		Dictionary<ObjectTypeKey, Component> m_DicCachedComponent = new();

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

#if UNITY_EDITOR
		/// <summary> <see cref="System.Enum"/> 타입을 인덱스로 사용해서 캐싱된 오브젝트를 찾는 함수 </summary>
		/// <remarks> <see cref="LoadCacheKeys()"/> 함수에서 필요한 오브젝트의 경로를 지정한다. </remarks>
		public GameObject GetCachedObject<T>(T alias) where T : System.Enum {
			if (m_ListCachedObject == null) {
				Util.LogError($"Cached Object List is null!");
				return null;
			}

			//int aliasIndex = Convert.ToInt32(alias);
			int index = (int)(object)alias;

			if (m_ListCachedObject.Count > index)
				return m_ListCachedObject[index];
			else {
				Util.LogError($"Cached Object is not found : {alias}");
				return null;
			}
		}
#else
		public GameObject GetCachedObject<T>(T alias) where T : System.Enum {
			int index = (int)(object)alias;
			if (m_ListCachedObject.Count > index)
				return m_ListCachedObject[index];
			return null;
		}
#endif

#if UNITY_EDITOR
		/// <summary> 인덱스로 캐싱된 오브젝트를 찾는 함수 </summary>
		/// <remarks> <see cref="LoadCacheKeys()"/> 함수에서 필요한 오브젝트의 경로를 지정한다. </remarks>
		public GameObject GetCachedObject(int index) {
			if (m_ListCachedObject == null) {
				Util.LogError($"Cached Object List is null!");
				return null;
			}

			//if (m_ListCachedObject.TryGetValue(aliasIndex, out GameObject obj))
			if (m_ListCachedObject.Count > index)
				return m_ListCachedObject[index];
			else {
				Util.LogError($"Cached Object is not found : {index}");
				return null;
			}
		}
#else
		public GameObject GetCachedObject(int index)
			=> m_ListCachedObject.Count > index ? m_ListCachedObject[index] : null;
#endif

		/// <summary>
		/// <see cref="System.Enum"/> 타입을 인덱스로 사용해서 캐싱된 오브젝트에 지정한 컴포넌트를 찾아 반환하는 함수
		/// </summary>
		/// <remarks> <see cref="LoadCacheKeys()"/> 함수에서 필요한 오브젝트의 경로를 지정한다. </remarks>
		public T GetCachedObject<T>(System.Enum alias) where T : Component {
			GameObject cachedGo = GetCachedObject(alias);
			if (cachedGo == null)
				return null;
#if UNITY_EDITOR
			if (typeof(T) == typeof(GameObject))
				Util.LogWarning($"Use Component Type, current type : {typeof(T).Name}");
#endif
			return GetCachedComponent<T>(cachedGo);
		}

		/// <summary> 인덱스로 캐싱된 오브젝트에 지정한 컴포넌트를 찾아 반환하는 함수 </summary>
		/// <remarks> <see cref="LoadCacheKeys()"/> 함수에서 필요한 오브젝트의 경로를 지정한다. </remarks>
		public T GetCachedObject<T>(int index) where T : Component {
			GameObject cachedGo = GetCachedObject(index);
			if (cachedGo == null)
				return null;
#if UNITY_EDITOR
			if (typeof(T) == typeof(GameObject))
				Util.LogWarning($"Use Component Type, current type : {typeof(T).Name}");
#endif
			return GetCachedComponent<T>(cachedGo);
		}


		readonly struct ObjectTypeKey : IEquatable<ObjectTypeKey> {
			public readonly GameObject Go;
			public readonly Type Type;

			public ObjectTypeKey(GameObject go, Type type) {
				Go = go;
				Type = type;
			}

			public bool Equals(ObjectTypeKey other) =>
				Go == other.Go && Type == other.Type;

			public override bool Equals(object obj) =>
				obj is ObjectTypeKey other && Equals(other);

			public override int GetHashCode() {
				unchecked {
					return ((Go != null ? Go.GetHashCode() : 0) * 397) ^ (Type != null ? Type.GetHashCode() : 0);
				}
			}
		}

		T GetCachedComponent<T>(GameObject go) where T : Component {
			ObjectTypeKey key = new(go, typeof(T));
			if (!m_DicCachedComponent.TryGetValue(key, out Component comp)) {
				comp = go.GetComponent<T>();
				m_DicCachedComponent[key] = comp;
			}
			return (T)comp;
		}

		/// <summary> 오브젝트가 파괴되거나 더 이상 필요 없을 때 캐시에서 제거 </summary>
		void RemoveCache(GameObject go) {
			List<ObjectTypeKey> list = new();

			foreach (var key in m_DicCachedComponent.Keys) {
				if (key.Go == go)
					list.Add(key);
			}

			foreach (var key in list) {
				m_DicCachedComponent.Remove(key);
			}
		}

		void ClearCache() {
			m_DicCachedComponent.Clear();
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
				//Transform tf = transform.GetTransformByFullName(key.Key);
				Transform tf = transform.Find(key.Key);

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

		/// <summary> 애셋 리스트에서 현재 오브젝트의 유효성 검사를 수행, 파일경로를 리턴 </summary>
		bool TryGetValidateScript(ref string filePath) {
			if (UnityEditor.Selection.activeObject == null || UnityEditor.Selection.activeGameObject == null)
				return false;

			// 따로 컴포넌트 찾고 이런거 안하고 그냥 this로 접근해도 되는듯?
			string type = GetType().Name;
			string[] arrGUID = UnityEditor.AssetDatabase.FindAssets($"t:Script {type}");
			if (arrGUID.Length > 1) {
				Debug.Log($"왜 {type} 스크립트가 여러개..?");
				return false;
			}

			filePath = UnityEditor.AssetDatabase.GUIDToAssetPath(arrGUID[0]);
			return true;
		}

		int FindBraceIndex(List<string> lines, int startIndex) {
			if (lines[startIndex].LastIndexOf('{') != -1)
				return startIndex + 1;

			int index = lines.FindIndex(startIndex + 1, x => x.Contains('{')) + 1;
			if (index == 0) {
				Debug.Log($"{startIndex}번 라인부터 뒤에 중괄호가 없음");
				return -1;
			}
			return index;
		}

		[KHFC.InspectorButton("현재 클래스 스크립트에 모든 오브젝트의 경로를 Alias enum 으로 생성한다")]
		public void MakeAliasEnum() {
			string filePath = "";
			if (!TryGetValidateScript(ref filePath))
				return;

			List<string> lines = System.IO.File.ReadAllLines(filePath).ToList();
			if (lines.Exists(x => x.Contains("enum Alias"))) {
				Debug.Log($"이미 Alias가 있기 때문에 진행하지 않는다.");
				return;
			}
			if (lines.Exists(x => x.Contains("public override void LoadCacheKeys()"))) {
				Debug.Log($"이미 LoadCacheKeys()가 있기 때문에 진행하지 않는다.");
				return;
			}

			int insertIndex = lines.FindIndex(x => x.Contains($"public class {GetType().Name}"));
			if (insertIndex == -1) {
				Debug.Log($"클래스 이름 검색 실패? 말이되나");
				return;
			}
			// 중괄호가 같은 라인에 있으면 다음 라인에 생성, 없으면 중괄호 시작지점을 찾고 그 아래줄에 생성
			insertIndex = FindBraceIndex(lines, insertIndex);
			if (insertIndex == -1)
				return;

			List<string> insertList = new() { "\tenum Alias {" };
			List<string> cacheKeyList = new();

			// 캐시 키 생성 후 enum 이름으로 만든다
			transform.DoRecursively(tr => {
				if (tr == transform)
					return;

				string str = "";
				Transform root = tr;
				while (root.GetComponent<CachedComponent>() == null) {
					str = $"/{root.name}{str}";
					root = root.parent;
				}

				// 자식 중 CachedComponent가 존재하는 경우 해당 자식은 무시한다
				if (root != transform)
					return;

				str = str[1..]; // c# 8.0 ranges operator

				// 일단 전체 경로를 표시한다. 생성 이후 이름을 수정하는게 찾는것보다 빠른듯
				string alias = str.Replace('/', '_');

				//// 별명은 현재 이름 + 부모 이름으로 표시 (너무 길 수도 있어서, 어차피 딕셔너리에 들어감)
				//string alias = tr.parent != null ? $"{tr.parent.name}_{tr.name}" : $"{tr.name}";

				insertList.Add($"\t\t{alias},");
				//insertList.Add($"\t\t{alias} = {++count},");
				cacheKeyList.Add($"\t\t\t{{\"{str}\", (int)Alias.{alias}}},");
			});
			insertList.Add("\t}\n");

			insertList.Add("#if UNITY_EDITOR");
			insertList.Add("\t// 1대1 매칭하기 때문에 alias의 개수와 항상 동일해야 한다");
			insertList.Add("\tpublic override void LoadCacheKeys() {");
			insertList.Add($"\t\t{nameof(m_DicCacheKey)} = new System.Collections.Generic.Dictionary<string, int> {{");
			insertList.AddRange(cacheKeyList);
			insertList.Add("\t\t};\n\t}\n#endif");

			lines.InsertRange(insertIndex, insertList);

			System.IO.File.WriteAllLines(filePath, lines, System.Text.Encoding.UTF8);
			UnityEditor.AssetDatabase.ImportAsset(filePath);
		}
#endif
	}
}