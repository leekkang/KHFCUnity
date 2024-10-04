using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Object = UnityEngine.Object;

namespace KHFC {
	public enum AssetType {
		Prefab,
		Audio,
	}

	public interface IGameObjectPool {
		/// <summary> 애셋 이름으로 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 구현 시  </remarks>
		/// <param name="assetName"> 생성하려는 오브젝트의 애셋 이름 (== 상대 경로) </param>
		/// <param name="bundleName"> 대상 애셋의 번들 이름 </param>
		/// <param name="type"> 애셋의 타입 </param>
		/// <param name="v"> 위치 값 </param>
		/// <param name="q"> 회전 값 </param>
		/// <param name="parent"> 부모가 될 transform </param>
		/// <param name="activeEnable"> GameObject를 화면에 보이게 할지 말지 결정(default = true) </param>
		/// <param name="setRotation"> GameObject의 회전 값을 새로 설정 할지 말지 결정(default = true) </param>
		GameObject SpawnGameObject(string assetName, string bundleName, AssetType type,
								   Vector3 pos, Quaternion rot, Transform parent = null,
								   bool activeEnable = true, bool setRotation = true);
		void PreSpawnObject(string assetName, int count = 1, AssetType type = AssetType.Prefab);
		void DespawnObject(GameObject obj, float delay = 0f);
		Object SpawnAsset(string assetName, string bundleName);
		void UnLoadAsset(string assetName, bool unloadDependencies = true);
		void UnLoadObject(string assetName);
	}

	// TODO: NGUI 의존적인 코드를 일반 UI 대응방식으로 변경
	// TODO: async 비동기 코드 추가 - 버전 전처리기 : UNITY_2018_1_OR_NEWER && (NET_4_6 || NET_STANDARD_2_0)
	// -> 코루틴도 충분히 성능이 좋아서 사용할 지는 의문, 대신 잡 시스템을 적용해보자
	public class PoolMgr : Singleton<PoolMgr> {
		bool m_OnInitialized = false;
		public bool initialized => m_OnInitialized;

		// GameObject.name을 호출 할 때마다 GC가 발생하기 때문에, name을 cache하기 위해 구조체선언
		[System.Serializable]
		public class PoolItem {
			public string		m_Name;
			public GameObject	m_Obj;
			public bool			m_IsGui;
		}

		/// <summary> 트랜스폼의 값 변경여부를 확인하기 위해 만든 클래스 </summary>
		class TfInfo {
			public float[] m_Value;
			public Vector3 vector => new Vector3(m_Value[0], m_Value[1], m_Value[2]);
			public Quaternion quaternion => new Quaternion(m_Value[0], m_Value[1], m_Value[2], m_Value[3]);

			public TfInfo(Vector3 val) {
				m_Value = new float[3];
				m_Value[0] = val.x;
				m_Value[1] = val.y;
				m_Value[2] = val.z;
			}
			public TfInfo(Quaternion val) {
				m_Value = new float[4];
				m_Value[0] = val.x;
				m_Value[1] = val.y;
				m_Value[2] = val.z;
				m_Value[2] = val.w;
			}

			public static bool operator true(TfInfo info) {
				return info != null;
			}
			public static bool operator false(TfInfo info) {
				return info == null;
			}
		}

		public System.Action<GameObject> m_OnAfterLoadGuiObj = null;
		public System.Action<GameObject> m_OnAfterDespawnGuiObj	= null;

		/// <summary> 현재 씬에서 사용중인 오브젝트들 </summary>
		[SerializeField]
		List<PoolItem> m_ListActivated = new List<PoolItem>();
		/// <summary> 로드했지만 사용하지 않는 오브젝트들. 매니저의 자식으로 붙어있다. </summary>
		[SerializeField]
		List<PoolItem> m_ListDeactivated = new List<PoolItem>();

		/// <summary> 현재 메모리에 올라와 있는 모든 애셋들, 키는 애셋 이름 </summary>
		Dictionary<string, Object> m_DicAsset = new Dictionary<string, Object>();

		/// <summary> 비동기 로드 시 안전을 위한 락 오브젝트 </summary>
		object _lockObj = new object();
		/// <summary> 동일 애셋을 여러개 생성할 때 비동기 처리를 위해 애프터 함수를 임시 저장하는 딕셔너리, 키는 애셋 이름 </summary>
		Dictionary<string, List<Action<Object>>> m_DicAssetAfterAction = new Dictionary<string, List<Action<Object>>>();

#if UNITY_EDITOR
		/// <summary> 디버그용 </summary>
		[SerializeField]
		private List<Object> m_ListLoadedAsset = new List<Object>();
#endif

		public void Open() {
			//if (m_OnInitialized)
			//	return;

			m_OnInitialized = true;
		}
		

		public Object GetAsset(string assetName) {
			m_DicAsset.TryGetValue(assetName, out Object asset);
			return asset;
		}

		#region GameObject

		/// <summary> 풀에서 관리하는 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 동일할 경우 사용 </remarks>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		/// <param name="pos"> 오브젝트의 로컬 위치 </param>
		/// <param name="parent"> 부모 오브젝트 </param>
		/// <param name="type"> 애셋 타입, 프리팹 또는 오디오 클립 </param>
		/// <param name="activeEnable"> 생성 후 활성화 여부 </param>
		public GameObject SpawnGameObject(string assetName,
										  Vector3 pos = default, Transform parent = null,
										  AssetType type = AssetType.Prefab, bool activeEnable = true) {
			return _SpawnGameObject(assetName, assetName, pos, null, null,
									parent, type, activeEnable);
		}
		/// <summary> 풀에서 관리하는 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 동일할 경우 사용 </remarks>
		/// <param name="rot"> 오브젝트의 로컬 회전값 </param>
		public GameObject SpawnGameObject(string assetName, Quaternion rot,
										  Vector3 pos = default, Transform parent = null,
										  AssetType type = AssetType.Prefab, bool activeEnable = true) {
			return _SpawnGameObject(assetName, assetName, pos, new TfInfo(rot), null,
									parent, type, activeEnable);
		}
		/// <summary> 풀에서 관리하는 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 동일할 경우 사용 </remarks>
		/// <param name="scale"> 오브젝트의 로컬 크기 </param>
		public GameObject SpawnGameObject(string assetName, Vector3 scale,
										  Vector3 pos = default, Transform parent = null,
										  AssetType type = AssetType.Prefab, bool activeEnable = true) {
			return _SpawnGameObject(assetName, assetName, pos, null, new TfInfo(scale),
									parent, type, activeEnable);
		}

		/// <summary> 풀에서 관리하는 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 다를 경우 사용 </remarks>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		/// <param name="bundleName"> 번들 이름 </param>
		/// <param name="pos"> 오브젝트의 로컬 위치 </param>
		/// <param name="parent"> 부모 오브젝트 </param>
		/// <param name="type"> 애셋 타입, 프리팹 또는 오디오 클립 </param>
		/// <param name="activeEnable"> 생성 후 활성화 여부 </param>
		public GameObject SpawnGameObject(string assetName, string bundleName,
										  Vector3 pos = default, Transform parent = null,
										  AssetType type = AssetType.Prefab, bool activeEnable = true) {
			return _SpawnGameObject(assetName, bundleName, pos, null, null,
									parent, type, activeEnable);
		}
		/// <summary> 풀에서 관리하는 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 다를 경우 사용 </remarks>
		/// <param name="rot"> 오브젝트의 로컬 회전값 </param>
		public GameObject SpawnGameObject(string assetName, string bundleName, Quaternion rot,
										  Vector3 pos = default, Transform parent = null,
										  AssetType type = AssetType.Prefab, bool activeEnable = true) {
			return _SpawnGameObject(assetName, bundleName, pos, new TfInfo(rot), null,
									parent, type, activeEnable);
		}
		/// <summary> 풀에서 관리하는 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 다를 경우 사용 </remarks>
		/// <param name="scale"> 오브젝트의 로컬 크기 </param>
		public GameObject SpawnGameObject(string assetName, string bundleName, Vector3 scale,
										  Vector3 pos = default, Transform parent = null,
										  AssetType type = AssetType.Prefab, bool activeEnable = true) {
			return _SpawnGameObject(assetName, bundleName, pos, null, new TfInfo(scale),
									parent, type, activeEnable);
		}


		/// <summary> 풀에서 관리하는 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 동일할 경우 사용 </remarks>
		/// <param name="actOnAfter"> 비동기 작업 후 호출할 콜백 함수 </param>
		public void SpawnGameObjectAsync(string assetName, Action<GameObject> actOnAfter,
										 Vector3 pos = default, Transform parent = null,
										 AssetType type = AssetType.Prefab, bool activeEnable = true) {
			_SpawnGameObjectAsync(assetName, assetName, actOnAfter,
								  pos, null, null, parent, type, activeEnable);
		}

		/// <summary> 풀에서 관리하는 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 동일할 경우 사용 </remarks>
		/// <param name="rot"> 오브젝트의 로컬 회전값 </param>
		public void SpawnGameObjectAsync(string assetName, Quaternion rot, Action<GameObject> actOnAfter,
										 Vector3 pos = default, Transform parent = null,
										 AssetType type = AssetType.Prefab, bool activeEnable = true) {
			_SpawnGameObjectAsync(assetName, assetName, actOnAfter,
								  pos, new TfInfo(rot), null, parent, type, activeEnable);
		}
		/// <summary> 풀에서 관리하는 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 동일할 경우 사용 </remarks>
		/// <param name="scale"> 오브젝트의 로컬 크기 </param>
		public void SpawnGameObjectAsync(string assetName, Vector3 scale, Action<GameObject> actOnAfter,
										 Vector3 pos = default, Transform parent = null,
										 AssetType type = AssetType.Prefab, bool activeEnable = true) {
			_SpawnGameObjectAsync(assetName, assetName, actOnAfter,
								  pos, null, new TfInfo(scale), parent, type, activeEnable);
		}

		/// <summary> 풀에서 관리하는 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 다를 경우 사용 </remarks>
		/// <param name="bundleName"> 번들 이름 </param>
		/// <param name="actOnAfter"> 비동기 작업 후 호출할 콜백 함수 </param>
		public void SpawnGameObjectAsync(string assetName, string bundleName, Action<GameObject> actOnAfter,
										 Vector3 pos = default, Transform parent = null,
										 AssetType type = AssetType.Prefab, bool activeEnable = true) {
			_SpawnGameObjectAsync(assetName, bundleName, actOnAfter,
								  pos, null, null, parent, type, activeEnable);
		}
		/// <summary> 풀에서 관리하는 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 다를 경우 사용 </remarks>
		/// <param name="rot"> 오브젝트의 로컬 회전값 </param>
		public void SpawnGameObjectAsync(string assetName, string bundleName, Quaternion rot, Action<GameObject> actOnAfter,
										 Vector3 pos = default, Transform parent = null,
										 AssetType type = AssetType.Prefab, bool activeEnable = true) {
			_SpawnGameObjectAsync(assetName, assetName, actOnAfter,
								  pos, new TfInfo(rot), null, parent, type, activeEnable);
		}
		/// <summary> 풀에서 관리하는 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 다를 경우 사용 </remarks>
		/// <param name="scale"> 오브젝트의 로컬 크기 </param>
		public void SpawnGameObjectAsync(string assetName, string bundleName, Vector3 scale, Action<GameObject> actOnAfter,
											   Vector3 pos = default, Transform parent = null,
											   AssetType type = AssetType.Prefab, bool activeEnable = true) {
			_SpawnGameObjectAsync(assetName, assetName, actOnAfter,
								  pos, null, new TfInfo(scale), parent, type, activeEnable);
		}


		/// <summary> 게임 오브젝트를 풀에 되돌리는 기본 함수 </summary>
		/// <param name="obj"> 비활성화 할 오브젝트 </param>
		/// <param name="delay"> 지연 시간, 해당 시간이 지난 후 디스폰한다. </param>
		public void DespawnObject(GameObject obj, float delay = 0f) {
			if (delay > 0)
				StartCoroutine(_CoDespawn(obj, delay));
			else
				_Despawn(obj);
		}


		/// <summary> 게임 오브젝트를 미리 풀에 생성해놓는 함수 </summary>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		/// <param name="count"> 만들 오브젝트의 개수 </param>
		/// <param name="type"> 애셋 타입, 프리팹 또는 오디오 클립 </param>
		public void PrespawnObject(string assetName, int count = 1, AssetType type = AssetType.Prefab) {
			_PrespawnObject(assetName, count, type, Vector3.zero, null, null);
		}

		public void PrespawnObjectAsync(string assetName, int count = 1,
										Action<bool> actOnAfter = null, AssetType type = AssetType.Prefab) {
			_PrespawnObjectAsync(assetName, count, actOnAfter, type, Vector3.zero, null, null);
		}

		public IEnumerator CoPrespawnObjectAsync(string name, int count = 1,
												Action<bool> actOnAfter = null, AssetType type = AssetType.Prefab) {
			bool success = false;
			bool loaded = false;
			_PrespawnObjectAsync(name, count, (suc) => {
				success = suc;
				loaded = true;
			}, type, Vector3.zero, null, null);

			yield return new WaitUntil(() => loaded);

			actOnAfter?.Invoke(success);
		}

		/// <summary> 개별적으로 관리할 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 오브젝트 파괴 시 <see cref="Resources.UnloadAsset"/> 함수로 메모리를 해제한다. </remarks>
		public GameObject CreateGameObject(string assetName, string bundleName,
										   Vector3 pos = default, Transform parent = null, AssetType type = AssetType.Prefab,
										   bool activeEnable = true, bool unloadDependencies = true) {
			Object asset = CreateAsset(assetName, bundleName);

			GameObject obj = PostProcessObject(asset, pos, null, null,
												parent, type, activeEnable, IsGuiObject(assetName));
			obj.name = assetName;

#if BUILD_LOCAL_BUNDLE || BUILD_REMOTE_BUNDLE || UNITY_EDITOR
			//if(unloadDependencies)
			//NFC.AssetMgr.inst.UnLoadDependencies(assetName);
#endif

			return obj;
		}

		/// <summary> 개별적으로 관리할 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 오브젝트 파괴 시 <see cref="Resources.UnloadAsset"/> 함수로 메모리를 해제한다. </remarks>
		public void CreateGameObjectAsync(string assetName, string bundleName, Action<GameObject> onAfterCreate,
										  Vector3 pos = default, Transform parent = null, AssetType type = AssetType.Prefab,
										  bool activeEnable = true, bool unloadDependencies = true) {
			CreateAssetAsync(assetName, bundleName, (asset) => {
				GameObject obj = PostProcessObject(asset, pos, null, null,
													parent, type, activeEnable, IsGuiObject(assetName));
				obj.name = assetName;

#if BUILD_LOCAL_BUNDLE || BUILD_REMOTE_BUNDLE || UNITY_EDITOR
				//if(unloadDependencies)
				//	NFC.AssetMgr.inst.UnLoadDependencies(assetName);
#endif
				onAfterCreate?.Invoke(obj);
			});
		}


		/// <summary> 특정 애셋과 게임 오브젝트들을 모든 풀에서 제거하는 함수 </summary>
		public void UnLoadObject(string assetName) {
			// Find Active List
			m_ListActivated.RemoveAll(item => {
				bool result = item.m_Name == assetName;
				if (result)
					Destroy(item.m_Obj);

				return result;
			});

			// Find Deactive List
			m_ListDeactivated.RemoveAll(item => {
				bool result = item.m_Name == assetName;
				if (result)
					Destroy(item.m_Obj);

				return result;
			});

			UnLoadAsset(assetName);
		}

		/// <summary> 이름에 <paramref name="pattern"/>을 포함하는 애셋과 게임 오브젝트들을 모든 풀에서 제거하는 함수 </summary>
		/// <param name="pattern"> 제거할 애셋 이름의 패턴 </param>
		public void UnLoadPattern(string pattern) {
			List<string> listKey = new List<string>();

			// 해당 패턴의 이름을 m_ListActivated에서 찾아서 제거
			m_ListActivated.RemoveAll(item => {
				bool result = item.m_Name.StartsWith(pattern);
				if (result) {
					listKey.Add(item.m_Name);
					Destroy(item.m_Obj);
				}

				return result;
			});
			// 해당 패턴의 이름을 m_ListDeactivated에서 찾아서 제거
			m_ListDeactivated.RemoveAll(item => {
				bool result = item.m_Name.StartsWith(pattern);
				if (result) {
					listKey.Add(item.m_Name);
					Destroy(item.m_Obj);
				}

				return result;
			});

			// 해당 패턴의 이름을 애셋 풀에서 찾아서 리소스 해제
			foreach (var item in m_DicAsset) {
				if (item.Key.StartsWith(pattern)) {
					Resources.UnloadAsset(item.Value);  // 리소스만 제거, 키 값은 존재함
					listKey.Add(item.Key);
				}
			}

			// 애셋 풀에 있는 모든 지워진 애셋들을 제거
			foreach (string key in listKey)
				UnLoadAsset(key);

			Resources.UnloadUnusedAssets();

			// Tools/Engine/ETC/ClearProfilerMemory 메뉴를 대신 사용하자.
#if UNITY_EDITOR
			// 에디터에서 지워지지 않는 메모리 애셋을 해제해준다.
			// 파라미터를 false로 할 경우 스크립트 프로퍼티와 정적 변수에 연결된 애셋들도 전부 해제한다.
			// 해당 메서드는 비동기가 아니다 -> 끝날때까지 기다린다.
			//EditorUtility.UnloadUnusedAssetsImmediate(true);
#endif
		}

		/// <summary> 현재 사용하지 않는 게임 오브젝트의 애셋을 풀에서 제거하는 함수 </summary>
		public void UnLoadDeactives() {
			int count = m_ListDeactivated.Count;
			m_ListDeactivated.RemoveAll(item => {
				bool result = item != null;
				if (item.m_IsGui) // NGUI를 사용하는 GUI의 경우, Destroy하고 Instantiate할때 메모리릭이 발생한다.
					return false;

				UnLoadAsset(item.m_Name);
				Destroy(item.m_Obj);

				return result;
			});

			Resources.UnloadUnusedAssets();
		}

		#endregion

		#region Asset Pool

		/// <summary> 애셋을 생성하고 풀에 넣는 함수 </summary>
		/// <remarks> 동일한 애셋이 저장되어 있으면 로드하지 않고 반환해준다.
		/// <para> 애셋 이름과 번들 이름이 동일한 경우 사용 </para></remarks>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		public Object SpawnAsset(string assetName) {
			return SpawnAsset(assetName , assetName);
		}
		/// <summary> 애셋을 생성하고 풀에 넣는 함수 </summary>
		/// <remarks> 동일한 애셋이 저장되어 있으면 로드하지 않고 반환해준다. </remarks>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		/// <param name="bundleName"> 번들 이름 </param>
		public Object SpawnAsset(string assetName, string bundleName) {
			// TryGetValue는 ContainsKey()로 찾은 값을 반환한다. 자주 사용하자
			if (m_DicAsset.TryGetValue(assetName, out Object asset))
				return asset;

			asset = CreateAsset(assetName, bundleName);
			if (asset != null) {
				m_DicAsset.Add(assetName, asset);
#if UNITY_EDITOR
				m_ListLoadedAsset.Add(asset);
#endif
			}

			return asset;
		}

		/// <summary> 비동기로 애셋을 생성하고 풀에 넣는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 동일한 경우 사용 </remarks>
		/// <param name="actOnLoad"> 생성에 성공할 경우 호출하는 함수 </param>
		/// <param name="actOnFail"> 생성에 실패할 경우 호출하는 함수 </param>
		public void SpawnAssetAsync(string assetName, Action<Object> actOnLoad, Action actOnFail = null) {
			SpawnAssetAsync(assetName, assetName, actOnLoad, actOnFail);
		}
		/// <summary> 비동기로 애셋을 생성하고 풀에 넣는 함수 </summary>
		/// <param name="actOnLoad"> 생성에 성공할 경우 호출하는 함수 </param>
		/// <param name="actOnFail"> 생성에 실패할 경우 호출하는 함수 </param>
		public void SpawnAssetAsync(string assetName, string bundleName, Action<Object> actOnLoad, Action actOnFail = null) {
			if (m_DicAsset.TryGetValue(assetName, out Object asset)) {
				actOnLoad?.Invoke(asset);
				return;
			}

			CreateAssetAsync(assetName, bundleName, (loadedAsset) => {
				if (!m_DicAsset.ContainsKey(assetName))
					m_DicAsset.Add(assetName, loadedAsset);
#if UNITY_EDITOR
				m_ListLoadedAsset.Add(loadedAsset);
#endif

				actOnLoad?.Invoke(loadedAsset);
			}, actOnFail);
		}

		/// <summary> 개별적으로 관리할 애셋을 생성하는 함수 </summary>
		public Object CreateAsset(string assetName, string bundleName) {
			Object asset = AssetMgr.inst.GetAsset(assetName, bundleName);
			if (asset == null)
				Debug.LogWarning("There is no resource, name : " + assetName);

			return asset;
		}

		/// <summary> 개별적으로 관리할 애셋을 비동기로 생성하는 함수 </summary>
		/// <param name="actOnLoad"> 로드 한 이후에 호출할 함수 </param>
		public void CreateAssetAsync(string assetName, string bundleName, Action<Object> actOnLoad, Action actOnFail = null) {
			// 동일한 애셋의 생성 요청이 여러번 들어올 경우 중복 생성을 방지하기 위해 콜백함수만 임시로 저장해놓는다
			// -> 애셋은 한번만, 콜백에서 진행하는 오브젝트 생성은 여러번
			lock(_lockObj) {
				if (m_DicAssetAfterAction.ContainsKey(assetName)) {
					m_DicAssetAfterAction[assetName].Add(actOnLoad);
					return;
				} else {
					m_DicAssetAfterAction.Add(assetName, new List<Action<Object>>());
				}
			}

			AssetMgr.inst.GetAssetAsync(assetName, bundleName, (asset) => {
				if (asset == null) {
					Debug.LogWarning("There is no resource, name : " + assetName);
					actOnFail?.Invoke();
					return;
				}

				lock (_lockObj) {
					if (m_DicAssetAfterAction.ContainsKey(assetName)) {
						List<System.Action<Object>> listAction = m_DicAssetAfterAction[assetName];
						for (int i = 0; i < listAction.Count; ++i) {
							listAction[i]?.Invoke(asset);
						}

						m_DicAssetAfterAction.Remove(assetName);
					}
				}

				actOnLoad?.Invoke(asset);
			});
		}

		/// <summary> 애셋 풀에서 해당 애셋을 찾아 제거해주는 함수 </summary>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		/// <param name="unloadDependencies"> <c>true</c>인 경우 AssetBundle을 이용 할때 Dependencies도 언로드 한다 </param>
		public void UnLoadAsset(string assetName, bool unloadDependencies = true) {
			if (!m_DicAsset.TryGetValue(assetName, out Object obj))
				return;

			m_DicAsset.Remove(assetName);

#if UNITY_EDITOR
			m_ListLoadedAsset.RemoveBySwap(obj);
#endif
			obj = null;

#if BUILD_LOCAL_BUNDLE || BUILD_REMOTE_BUNDLE || UNITY_EDITOR
			AssetMgr.inst.UnLoadMainAssetBundle(assetName);
			//if (unloadDependencies)
			//	AssetMgr.inst.UnLoadDependencies(assetName);
#endif
		}

		#endregion


		/// <summary> 모든 게임 오브젝트, 애셋을 제거하는 함수 </summary>
		public void ClearAll() {
			int count = m_ListActivated.Count;
			for (int i = 0; i < count; ++i) {
				Destroy(m_ListActivated[i].m_Obj);
			}
			m_ListActivated.Clear();

			count = m_ListDeactivated.Count;
			for (int i = 0; i < count; ++i) {
				Destroy(m_ListDeactivated[i].m_Obj);
			}
			m_ListDeactivated.Clear();

#if UNITY_EDITOR
			m_ListLoadedAsset.Clear();
#endif

			m_DicAsset.Clear();

			Resources.UnloadUnusedAssets();
		}






		GameObject _SpawnGameObject(string assetName, string bundleName,
									Vector3 pos, TfInfo rot, TfInfo scale,
									Transform parent, AssetType type, bool activeEnable) {
			if (TryGetRecycledItem(assetName, out PoolItem resultItem)) {
				resultItem.m_Obj = PostProcess(resultItem.m_Obj, pos, rot, scale,
												parent, activeEnable, resultItem.m_IsGui);
			} else {
				Object asset = SpawnAsset(assetName, bundleName);
				if (asset == null)
					return null;

				resultItem.m_Obj = PostProcessObject(asset, pos, rot, scale,
													 parent, type, activeEnable, resultItem.m_IsGui);
				resultItem.m_Obj.name = assetName;
			}
			m_ListActivated.Add(resultItem);

			return resultItem.m_Obj;
		}

		void _SpawnGameObjectAsync(string assetName, string bundleName, Action<GameObject> actOnAfter,
									Vector3 pos, TfInfo rot, TfInfo scale,
									Transform parent, AssetType type, bool activeEnable) {
			if (TryGetRecycledItem(assetName, out PoolItem resultItem)) {
				resultItem.m_Obj = PostProcess(resultItem.m_Obj, pos, rot, scale,
												parent, activeEnable, resultItem.m_IsGui);
				m_ListActivated.Add(resultItem);

				actOnAfter?.Invoke(resultItem.m_Obj);
			} else {
				SpawnAssetAsync(assetName, bundleName, (asset) => {
					resultItem.m_Obj = PostProcessObject(asset, pos, rot, scale,
														 parent, type, activeEnable, resultItem.m_IsGui);
					resultItem.m_Obj.name = assetName;
					m_ListActivated.Add(resultItem);

					actOnAfter?.Invoke(resultItem.m_Obj);
				}, () => actOnAfter?.Invoke(null));
			}
		}

		void _PrespawnObject(string assetName, int count, AssetType type,
							Vector3 pos, TfInfo rot, TfInfo scale) {
			// 애셋을 먼저 생성한 뒤 반복
			Object asset = SpawnAsset(assetName, assetName);
			if (asset == null)
				return;

			for (int i = 0; i < count; ++i) {
				if (TryGetRecycledItem(assetName, out PoolItem resultItem)) {
					resultItem.m_Obj = PostProcess(resultItem.m_Obj, pos, rot, scale,
													transform, false, resultItem.m_IsGui);
					m_ListDeactivated.Add(resultItem);
				} else {
					resultItem.m_Obj = PostProcessObject(asset, pos, rot, scale,
														 transform, type, false, resultItem.m_IsGui);
					resultItem.m_Obj.name = assetName;
					PostPrespawnProcess(resultItem);
				}
			}
		}

		void _PrespawnObjectAsync(string assetName, int count, Action<bool> actOnAfter, AssetType type,
								  Vector3 pos, TfInfo rot, TfInfo scale) {
			// 애셋을 먼저 생성한 뒤 반복
			SpawnAssetAsync(assetName, assetName, (asset) => {
				if (!asset) {
					actOnAfter?.Invoke(false);
					return;
				}

				for (int i = 0; i < count; ++i) {
					if (TryGetRecycledItem(assetName, out PoolItem resultItem)) {
						resultItem.m_Obj = PostProcess(resultItem.m_Obj, pos, rot, scale,
														transform, false, resultItem.m_IsGui);
						m_ListDeactivated.Add(resultItem);
					} else {
						resultItem.m_Obj = PostProcessObject(asset, pos, rot, scale,
															 transform, type, false, resultItem.m_IsGui);
						resultItem.m_Obj.name = assetName;
						PostPrespawnProcess(resultItem);
					}
				}

				actOnAfter?.Invoke(true);
			}, () => actOnAfter?.Invoke(false));

		}

		/// <summary> <see cref="AssetType"/>에 맞게 게임 오브젝트를 설정하는 함수 </summary>
		/// <param name="parent"> 부모로 설정 할 Transform </param>
		/// <param name="rot"> null이 아닌 경우 트랜스폼 값 재설정 </param>
		/// <param name="scale"> null이 아닌 경우 트랜스폼 값 재설정 </param>
		/// <param name="activeEnable"> 생성 후 활성화 여부 </param>
		/// <param name="isGui"> ui의 경우 추가정보를 처리하기 위한 플래그 </param>
		GameObject PostProcessObject(Object asset, Vector3 pos, TfInfo rot, TfInfo scale,
									 Transform parent, AssetType type, bool activeEnable, bool isGui = false) {
			GameObject createdObj = null;

			if (type == AssetType.Prefab) {
				createdObj = Instantiate((GameObject)asset);
			} else if (type == AssetType.Audio) {
				createdObj = new GameObject();
				AudioSource audio = createdObj.AddComponent<AudioSource>();
				audio.playOnAwake = false;
				audio.clip = (AudioClip)asset;
			}
			createdObj.name = asset.name;

			return PostProcess(createdObj, pos, rot, scale, parent, activeEnable, isGui);
		}
		GameObject PostProcess(GameObject obj, Vector3 pos, TfInfo rot, TfInfo scale,
							   Transform parent, bool activeEnable, bool isGui = false) {
			obj.SetActive(activeEnable);
			
			obj.transform.SetParent(parent ? parent : transform);

			obj.transform.localPosition = pos;
			if (rot)
				obj.transform.localRotation = rot.quaternion;
			if (scale)
				obj.transform.localScale = scale.vector;

			// 예외상황. 확인을 위해 넣어놓음
			if (obj.transform.parent == null)
				DontDestroyOnLoad(obj);

			if (isGui) {
				obj.transform.localScale = Vector3.one;
				//NGUITools.MarkParentAsChanged(obj);
				m_OnAfterLoadGuiObj?.Invoke(obj);
			}

			return obj;
		}

		void PostPrespawnProcess(PoolItem item) {
			m_ListDeactivated.Add(item);
			if (item.m_IsGui)
				m_OnAfterDespawnGuiObj?.Invoke(item.m_Obj);


#if BUILD_LOCAL_BUNDLE || BUILD_REMOTE_BUNDLE || UNITY_EDITOR
			if (AssetMgr.inst.m_LocationType == AssetLocation.Resources)
				return;

			// 이펙트의 쉐이더를 찾아준다
			if (item.m_Obj.name.StartsWith("eff_")) {
				item.m_Obj.transform.DoRecursively(x => {
					if (x.TryGetComponent<ParticleSystemRenderer>(out var particleRenderer)) {
						Material material = particleRenderer.sharedMaterial;

						if (material != null) {
							string shaderName = material.shader.name;
							particleRenderer.sharedMaterial.shader = Shader.Find(shaderName);
						}
					}
				});
			}
#endif
		}

		private bool TryGetRecycledItem(string assetName, out PoolItem poolItem) {
			int index = m_ListDeactivated.FindIndex(item => item.m_Name == assetName);
			bool result = index != -1;
			if (result) {
				poolItem = m_ListDeactivated[index];
				m_ListDeactivated.RemoveAtBySwap(index);
			} else {
				poolItem = new PoolItem {
					m_Name = assetName,
					m_IsGui = IsGuiObject(assetName)
				};
			}

			return result && poolItem.m_Obj != null;
		}

		/// <summary>
		/// 일정 시간 이후에 Despawn을 하기 위한 함수
		/// </summary>
		/// <returns>The despawn.</returns>
		/// <param name="obj"> l_active에서 내릴 Object명 </param>
		/// <param name="delay"> 지연 시킬 시간 </param>
		private IEnumerator _CoDespawn(GameObject obj, float delay) {
			yield return CachedYield.GetWaitForSeconds(delay);

			_Despawn(obj);
		}

		/// <summary>
		/// 사용하지 않는 Object를 m_ListDeactivated로 옮기는 함수
		/// </summary>
		private void _Despawn(GameObject obj) {
			if (obj == null)
				return;

			_Despawn(m_ListActivated.Find(item => item.m_Obj == obj));
		}
		private void _Despawn(PoolItem poolItem) {
			if (poolItem == null)
				return;

			m_ListActivated.RemoveBySwap(poolItem);
			GameObject obj = poolItem.m_Obj;
			if (poolItem.m_Obj == null)
				return;

			m_ListDeactivated.Add(poolItem);

			ChangeParent(obj, poolItem.m_IsGui);

			if (poolItem.m_IsGui)
				m_OnAfterDespawnGuiObj?.Invoke(obj);

			//obj.transform.localScale = Vector3.one;
			obj.SetActive(false);
		}

		// ngui object는 부모들중 UIPanel을 가지고 있어야 한다.
		private void ChangeParent(GameObject obj, bool isGui = false) {
			if (isGui)  //GUI 오브젝트들은 다른것들 처럼 PoolMgr로 부모변경을 수행 하면 안된다. m_OnAfterDespawnGuiObj에서 수행 하자.
				return;

			obj.transform.SetParent(transform);
		}

		/// <summary>
		/// Object가 GuiObject인지를 판단하기 위한 함수.
		/// </summary>
		/// <returns><c>true</c> if this instance is GUI object the specified name; otherwise, <c>false</c>.</returns>
		/// <param name="name"> 판단을 원하는 Object명 </param>
		private bool IsGuiObject(string name) {
			//if (name.StartsWith("wdgt_") == true ||
			//	name.StartsWith("pnl_") == true ||
			//	name.StartsWith("stage_") == true) {
			//	return true;
			//}

			return false;
		}

	}
}
