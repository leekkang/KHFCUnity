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

			//int aliasIndex = Convert.ToInt32(alias);
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
				//if (m_ListCachedObject.TryGetValue(aliasIndex, out GameObject obj))
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
			//int count = -1;
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

				// 별명은 현재 이름 + 부모 이름으로 표시 (너무 길 수도 있어서, 어차피 딕셔너리에 들어감)
				string alias = tr.parent != null ? $"{tr.parent.name}_{tr.name}" : $"{tr.name}";

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
