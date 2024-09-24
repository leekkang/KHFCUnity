using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using V2R.EnumDefine;

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

		[KHFC.InspectorButton("현재 클래스 스크립트에 모든 오브젝트의 경로를 Alias enum 으로 생성한다")]
		public void MakeAliasEnum() {
			if (UnityEditor.Selection.activeObject == null || UnityEditor.Selection.activeGameObject == null)
				return;

			// 따로 컴포넌트 찾고 이런거 안하고 그냥 this로 접근해도 되는듯?
			string type = GetType().Name;
			string[] arrGUID = UnityEditor.AssetDatabase.FindAssets($"t:Script {type}");
			if (arrGUID.Length > 1) {
				Debug.Log($"왜 {type} 스크립트가 여러개..?");
				return;
			}

			string filePath = UnityEditor.AssetDatabase.GUIDToAssetPath(arrGUID[0]);
			List<string> lines = System.IO.File.ReadAllLines(filePath).ToList();

			int aliasLine = lines.FindIndex(x => x.Contains("enum Alias"));
			if (aliasLine == -1) {
				Debug.Log($"이미 Alias가 있기 때문에 진행하지 않는다.");
				return;
			}
			int classStartLine = lines.FindIndex(x => x.Contains($"public class {type}"));
			if (classStartLine == -1) {
				Debug.Log($"클래스 이름 검색 실패? 말이되나");
				return;
			}
			// 중괄호가 같은 라인에 있으면 다음 라인에 생성, 없으면 중괄호 시작지점을 찾고 그 아래줄에 생성
			if (lines[classStartLine].LastIndexOf('{') != -1)
				classStartLine += 1;
			else {
				classStartLine = lines.FindIndex(classStartLine + 1, x => x.Contains("{")) + 1;
				if (classStartLine == 0) {
					Debug.Log($"클래스 이름 뒤 중괄호 검색 실패? 말이되나");
					return;
				}
			}

			List<string> insertList = new();
			insertList.Add("\tenum Alias {");
			int count = -1;
			transform.DoRecursively(tr => {
				// 캐시 키 생성 후 enum 이름으로 만든다
				string str = "";
				while (tr.GetComponent<CachedComponent>() == null) {
					if (tr.parent == null) {
						Debug.Log("copy failed : CachedComponent is not exist in parent objects");
						return;
					}

					str = $"/{go.name}{str}";
					go = go.transform.parent.gameObject;
				}
				if (str == "") {
					Debug.Log("copy failed : CachedComponent is exist in current object");
					return;
				}

				str = '\"' + str[1..] + '\"';   // c# 8.0 ranges operator
				GUIUtility.systemCopyBuffer = str;
			});

			var startPosition = allLines.FindIndex(f => f.Contains($"enum {typeName}s"));
			var endPosition = 0;
			//이미 있는 경우 지워주기
			if (startPosition >= 0) {
				endPosition = allLines.FindIndex(startPosition, f => f.Contains("}"));
				allLines.RemoveRange(startPosition, endPosition - startPosition + 1);
			}
			//없는경우 startposition 잡아주기
			else {
				startPosition = allLines.FindIndex(f => f.Contains($"public class {target.name}")) + 2;
			}

			//모든 애들 돌면서 타입이랑 같으면 insertlist 넣어주기
			for (int j = 0; j < objectType.Length; j++) {
				//타입이랑 겹치면
				if (typeName == objectType[j].ToString()) {
					insertList.Add($"        {objectName[j]},");
				}
			}

			insertList.Add("    }");

			//뭔가 있으면 넣어주기
			if (insertList.Count > 3) {
				allLines.InsertRange(startPosition, insertList);
			}

			lines.InsertRange

			for (int i = 1; i < Enum.GetValues(typeof(ObjectType)).Length; i++) {
				string typeName = Enum.GetNames(typeof(ObjectType))[i];
				var startPosition = allLines.FindIndex(f => f.Contains($"enum {typeName}s"));
				if (startPosition >= 0) {
					var endPosition = allLines.FindIndex(startPosition, f => f.Contains("}"));

					for (int j = startPosition + 2; j < endPosition; j++) {
						string objectName = allLines[j].Replace(',', ' ').Trim();
						if (nameToIndex.TryGetValue(objectName, out int index)) {
							objectType[index] = (ObjectType)i;
						} else {
							Debug.Log($"{target.name} 스크립트에는 {objectName}가 있지만 하이어라키에는 없어요");
						}
					}
				}
			}

			System.IO.File.WriteAllLines(AssetDatabase.GUIDToAssetPath(currentScript[0]), allLines, System.Text.Encoding.UTF8);
			UnityEditor.AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(currentScript[0]));
			return;
			for (int i = 0; i < arrGUID.Length; i++) {
				string prefabPath = UnityEditor.AssetDatabase.GUIDToAssetPath(arrGUID[i]);
				string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
				string folderName = System.IO.Path.GetDirectoryName(prefabPath);

				GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
				AssetLinkData.inst.AddLink(folderName, prefabName, prefab);
			}
			UnityEditor.AssetDatabase.Refresh();
			FindCachedObject();

			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			UnityEditor.EditorUtility.SetDirty(gameObject);
		}
		private void RefreshScript() {
			var allLines = File.ReadAllLines(AssetDatabase.GUIDToAssetPath(currentScript[0])).ToList();

			var awakePosition = allLines.FindIndex(f => f.Contains("Awake()"));

			var insertList = new List<string>();

			//만약 Awake 없으면 만들어줌
			if (awakePosition < 0) {
				awakePosition = allLines.FindIndex(f => f.Contains($"public class {target.name}")) + 2;
				insertList.Add("    private void Awake()");
				insertList.Add("    {");
				insertList.Add("    }");
			}
			allLines.InsertRange(awakePosition, insertList);

			//모든 타입마다 enum 만들기 none 제외
			for (int i = 1; i < Enum.GetValues(typeof(ObjectType)).Length; i++) {

				string typeName = Enum.GetNames(typeof(ObjectType))[i];
				insertList.Clear();
				insertList.Add($"    enum {typeName}s");
				insertList.Add("    {");

				var startPosition = allLines.FindIndex(f => f.Contains($"enum {typeName}s"));
				var endPosition = 0;
				//이미 있는 경우 지워주기
				if (startPosition >= 0) {
					endPosition = allLines.FindIndex(startPosition, f => f.Contains("}"));
					allLines.RemoveRange(startPosition, endPosition - startPosition + 1);
				}
				//없는경우 startposition 잡아주기
				else {
					startPosition = allLines.FindIndex(f => f.Contains($"public class {target.name}")) + 2;
				}

				//모든 애들 돌면서 타입이랑 같으면 insertlist 넣어주기
				for (int j = 0; j < objectType.Length; j++) {
					//타입이랑 겹치면
					if (typeName == objectType[j].ToString()) {
						insertList.Add($"        {objectName[j]},");
					}
				}

				insertList.Add("    }");

				//뭔가 있으면 넣어주기
				if (insertList.Count > 3) {
					allLines.InsertRange(startPosition, insertList);
				}


				//바인드
				int bindPosition;
				string bindString;
				if (typeName == ObjectType.GameObject.ToString()) {
					bindString = $"        Bind<GameObject>(typeof({typeName}s));";
					bindPosition = allLines.FindIndex(f => f.Contains($"Bind<GameObject>(typeof({typeName}s));"));
				} else if (typeName == ObjectType.TextMesh.ToString()) {
					bindString = $"        Bind<TMPro.TextMeshProUGUI>(typeof({typeName}s));";
					bindPosition = allLines.FindIndex(f => f.Contains($"Bind<TMPro.TextMeshProUGUI>(typeof({typeName}s));"));
				} else {
					bindString = $"        Bind<UnityEngine.UI.{typeName}>(typeof({typeName}s));";
					bindPosition = allLines.FindIndex(f => f.Contains($"Bind<UnityEngine.UI.{typeName}>(typeof({typeName}s));"));
				}

				//먼저 삭제
				if (bindPosition >= 0)
					allLines.RemoveAt(bindPosition);

				//뭔가 있으면 바인드 해주기
				if (insertList.Count > 3) {
					startPosition = allLines.FindIndex(f => f.Contains("Awake()"));
					if (startPosition > 0) {
						startPosition += 2;
					}
					allLines.Insert(startPosition, bindString);
				}
			}
			File.WriteAllLines(AssetDatabase.GUIDToAssetPath(currentScript[0]), allLines, System.Text.Encoding.UTF8);
			AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(currentScript[0]));
		}

		[KHFC.InspectorButton("스크립트에 있는 Enum들만 남도록 리스트와 이름을 정리한다. Enum 번호도 제거함")]
		public void TrimBasedOnAliasEnum() {
			FindCachedObject();

			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			UnityEditor.EditorUtility.SetDirty(gameObject);
		}
#endif
	}
}