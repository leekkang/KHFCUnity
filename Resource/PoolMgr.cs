using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Object = UnityEngine.Object;

namespace KHFC {
	public enum AssetType {
		Prefab,
		Audio,
	}

	// TODO: async 비동기 코드 추가 - 버전 전처리기 : UNITY_2018_1_OR_NEWER && (NET_4_6 || NET_STANDARD_2_0)
	// -> 코루틴도 충분히 성능이 좋아서 사용할 지는 의문, 대신 잡 시스템을 적용해보자
	// 코루틴 대신 UniTask 적용 -> 전처리기는 나중에 작업, 비동기 코드만 작성해놓자
	// Addressable을 사용하면 AssetMgr를 완전히 대체할 수 있다. 백업해놓고 일단 코드작성부터
	// 추가 필요 : 라벨로 여러 애셋 불러오는 함수
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
		public class TfInfo {
			public float[] m_Value;
			public Vector3 vector => new(m_Value[0], m_Value[1], m_Value[2]);
			public Quaternion quaternion => new(m_Value[0], m_Value[1], m_Value[2], m_Value[3]);

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
		List<Object> m_ListLoadedAsset = new List<Object>();
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

		// c# 4.0 named parameter 로 순차 접근이 필요없기 때문에 해당 함수로 모두 처리함

		/// <summary> 풀에서 관리하는 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 같은 경우 bundleName == null </remarks>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		/// <param name="pos"> 오브젝트의 로컬 위치 </param>
		/// <param name="rot"> 오브젝트의 로컬 회전값, null이면 기본값 사용 </param>
		/// <param name="scale"> 오브젝트의 로컬 크기, null이면 기본값 사용 </param>
		/// <param name="parent"> 부모 오브젝트 </param>
		/// <param name="type"> 애셋 타입, 프리팹 또는 오디오 클립 </param>
		/// <param name="activeEnable"> 생성 후 활성화 여부 </param>
		/// <param name="bundleName"> obsolete : 번들 이름 <see cref="AssetBundle"/> 시스템에서 사용함 </param>
		public GameObject SpawnGameObject(string assetName, Transform parent = null,
										  Vector3 pos = default, TfInfo rot = null, TfInfo scale = null,
										  AssetType type = AssetType.Prefab,
										  bool activeEnable = true) {
			return _SpawnGameObject(assetName, parent, pos, rot, scale, type, activeEnable);
		}


		/// <summary> 풀에서 관리하는 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 애셋 이름과 번들 이름이 같은 경우 bundleName == null </remarks>
		/// <param name="onAfter"> 비동기 작업 후 호출할 콜백 함수 </param>
		public void SpawnGameObjectAsync(string assetName, Action<GameObject> onAfter, Transform parent = null,
										 Vector3 pos = default, TfInfo rot = null, TfInfo scale = null,
										 AssetType type = AssetType.Prefab,
										 bool activeEnable = true) {
			_SpawnGameObjectAsync(assetName, onAfter, parent, pos, rot, scale, type, activeEnable);
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
			_PrespawnObject(assetName, count, Vector3.zero, null, null, type);
		}

		public void PrespawnObjectAsync(string assetName, int count = 1, Action<bool> onAfter = null, AssetType type = AssetType.Prefab) {
			_PrespawnObjectAsync(assetName, count, onAfter, Vector3.zero, null, null, type);
		}

		public IEnumerator CoPrespawnObjectAsync(string name, int count = 1, Action<bool> onAfter = null, AssetType type = AssetType.Prefab) {
			bool success = false;
			bool loaded = false;
			_PrespawnObjectAsync(name, count, (suc) => {
				success = suc;
				loaded = true;
			}, Vector3.zero, null, null, type);

			yield return new WaitUntil(() => loaded);

			onAfter?.Invoke(success);
		}

		/// <summary> 개별적으로 관리할 게임 오브젝트를 생성하는 함수 </summary>
		/// <remarks> 오브젝트 파괴 시 <see cref="Resources.UnloadAsset"/> 함수로 메모리를 해제한다. </remarks>
		public GameObject CreateGameObject(string assetName, Transform parent = null,
											Vector3 pos = default, TfInfo rot = null, TfInfo scale = null,
											AssetType type = AssetType.Prefab,
											bool activeEnable = true) {
			Object asset = CreateAsset(assetName);
			if (asset == null)
				return null;

			GameObject obj = PostProcessObject(asset, parent, pos, rot, scale,
												type, activeEnable, IsGuiObject(assetName));
			obj.name = assetName;

#if BUILD_LOCAL_BUNDLE || BUILD_REMOTE_BUNDLE || UNITY_EDITOR
			//if(unloadDependencies)
			//NFC.AssetMgr.inst.UnLoadDependencies(assetName);
#endif

			return obj;
		}

		/// <summary> 개별적으로 관리할 게임 오브젝트를 비동기로 생성하는 함수 </summary>
		/// <remarks> 오브젝트 파괴 시 <see cref="Resources.UnloadAsset"/> 함수로 메모리를 해제한다. </remarks>
		public void CreateGameObjectAsync(string assetName, Action<GameObject> onAfter, Transform parent = null,
										  Vector3 pos = default, TfInfo rot = null, TfInfo scale = null,
										  AssetType type = AssetType.Prefab,
										  bool activeEnable = true) {
			CreateAssetAsync(assetName, (asset) => {
				if (asset == null) {
					onAfter?.Invoke(null);
					return;
				}

				GameObject obj = PostProcessObject(asset, parent, pos, rot, scale,
													type, activeEnable, IsGuiObject(assetName));
				obj.name = assetName;

#if BUILD_LOCAL_BUNDLE || BUILD_REMOTE_BUNDLE || UNITY_EDITOR
				//if(unloadDependencies)
				//	NFC.AssetMgr.inst.UnLoadDependencies(assetName);
#endif
				onAfter?.Invoke(obj);
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
		/// <param name="prefix"> 제거할 애셋 이름의 패턴 </param>
		public void UnLoadPattern(string prefix) {
			List<string> listKey = new();

			// 해당 패턴의 이름을 m_ListActivated에서 찾아서 제거
			m_ListActivated.RemoveAll(item => {
				bool result = item.m_Name.StartsWith(prefix);
				if (result) {
					listKey.Add(item.m_Name);
					Destroy(item.m_Obj);
				}

				return result;
			});
			// 해당 패턴의 이름을 m_ListDeactivated에서 찾아서 제거
			m_ListDeactivated.RemoveAll(item => {
				bool result = item.m_Name.StartsWith(prefix);
				if (result) {
					listKey.Add(item.m_Name);
					Destroy(item.m_Obj);
				}

				return result;
			});

			// 해당 패턴의 이름을 애셋 풀에서 찾아서 리소스 해제
			foreach (var item in m_DicAsset) {
				if (item.Key.StartsWith(prefix)) {
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

		/// <summary> <paramref name="prefix"/>로 시작하는 오브젝트를 제외한 모든 deactive 오브젝트를 풀에서 제거한다 </summary>
		/// <param name="prefix"> 제거하지 않을 오브젝트의 이름 prefix </param>
		public void UnloadDeactivesExcept(params string[] prefix) {
			int count = m_ListDeactivated.Count;
			int exceptCount = prefix.Length;
			m_ListDeactivated.RemoveAll(item => {
				bool result = item != null;
				if (item.m_IsGui) // NGUI를 사용하는 GUI의 경우, Destroy하고 Instantiate할때 메모리릭이 발생한다.
					return false;

				for (int i = 0; i < exceptCount; i++)
					result |= item.m_Name.StartsWith(prefix[i]);

				if (!result) {
					UnLoadAsset(item.m_Name);
					Destroy(item.m_Obj);
				}

				return result;
			});

			Resources.UnloadUnusedAssets();
		}

		#endregion

		#region Asset Pool

		/// <summary> 애셋을 생성하고 풀에 넣는 함수 </summary>
		/// <remarks> 동일한 애셋이 저장되어 있으면 로드하지 않고 반환해준다. </remarks>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		/// <param name="location"> 애셋을 로드하는 위치 </param>
		/// <param name="bundleName"> 번들 이름 </param>
		public Object SpawnAsset(string assetName) {
			// TryGetValue는 ContainsKey()로 찾은 값을 반환한다. 자주 사용하자
			if (m_DicAsset.TryGetValue(assetName, out Object asset))
				return asset;

			asset = CreateAsset(assetName);
			if (asset != null) {
				m_DicAsset.Add(assetName, asset);
#if UNITY_EDITOR
				m_ListLoadedAsset.Add(asset);
#endif
			}

			return asset;
		}

		/// <summary> 비동기로 애셋을 생성하고 풀에 넣는 함수 </summary>
		/// <param name="onAfter"> 애셋 생성 이후 호출하는 콜백함수, 실패할 경우 파라미터에 null이 들어간다 </param>
		public void SpawnAssetAsync(string assetName, Action<Object> onAfter) {
			if (m_DicAsset.TryGetValue(assetName, out Object asset)) {
				onAfter?.Invoke(asset);
				return;
			}

			CreateAssetAsync(assetName, (loadedAsset) => {
				if (!m_DicAsset.ContainsKey(assetName)) {
					m_DicAsset.Add(assetName, loadedAsset);
#if UNITY_EDITOR
					m_ListLoadedAsset.Add(loadedAsset);
#endif
				}

				onAfter?.Invoke(loadedAsset);
			});
		}

		/// <summary> 개별적으로 관리할 애셋을 생성하는 함수 </summary>
		public T CreateAsset<T>(string assetName) where T : Object {
			Object asset = _LoadFromAddressable(assetName);
			if (asset == null)
				Debug.LogWarning("There is no resource, name : " + assetName);

			return (T)asset;
		}

		/// <summary> 개별적으로 관리할 애셋을 생성하는 함수 </summary>
		Object CreateAsset(string assetName) {
			Object asset = _LoadFromAddressable(assetName);
			if (asset == null)
				Debug.LogWarning("There is no resource, name : " + assetName);

			return asset;
		}

		/// <summary> 개별적으로 관리할 애셋을 비동기로 생성하는 함수 </summary>
		/// <param name="onAfter"> 로드 한 이후에 호출할 함수 </param>
		public void CreateAssetAsync(string assetName, Action<Object> onAfter) {
			// 동일한 애셋의 생성 요청이 여러번 들어올 경우 중복 생성을 방지하기 위해 콜백함수만 임시로 저장해놓는다
			// -> 애셋은 한번만, 콜백에서 진행하는 오브젝트 생성은 여러번
			lock(_lockObj) {
				if (m_DicAssetAfterAction.ContainsKey(assetName)) {
					m_DicAssetAfterAction[assetName].Add(onAfter);
					return;
				} else {
					m_DicAssetAfterAction.Add(assetName, new List<Action<Object>>() { onAfter });
				}
			}

			_LoadFromAddressableAsync(assetName, (asset) => {
				if (asset == null) {
					Debug.LogWarning("There is no resource, name : " + assetName);
					onAfter?.Invoke(null);
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
			});
		}

		/// <summary> 애셋 풀에서 해당 애셋을 찾아 제거해주는 함수 </summary>
		/// <param name="assetName"> 애셋 이름 (== 상대 경로) </param>
		/// <param name="unloadDependencies"> <c>true</c>인 경우 AssetBundle을 이용 할때 Dependencies도 언로드 한다 </param>
		public void UnLoadAsset(string assetName, bool unloadDependencies = true) {
			if (!m_DicAsset.TryGetValue(assetName, out Object obj))
				return;

			m_DicAsset.Remove(assetName);
			ReleaseAddressable(obj);

#if UNITY_EDITOR
			m_ListLoadedAsset.RemoveBySwap(obj);
#endif
			obj = null;
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
			ClearAddressable();

			Resources.UnloadUnusedAssets();
		}





		public void ClearAddressable() {
			foreach (var pair in m_DicAsset) {
				Addressables.Release(pair.Value);
			}
			m_DicAsset.Clear();
		}

		public void ReleaseAddressable(Object asset) {
			Addressables.Release(asset);
		}
		public void ReleaseAddressable(string assetName) {
			if (m_DicAsset.TryGetValue(assetName, out Object asset)) {
				Addressables.Release(asset);
				m_DicAsset.Remove(assetName);
			}
		}


		void LoadAssetPostProcess(ref AsyncOperationHandle<Object> handle, ref string assetName, System.Action<Object> onAfter) {
			if (handle.Status == AsyncOperationStatus.Succeeded) {
				//Debug.Log($"Async operation succeeded : {assetName}");
				onAfter?.Invoke(handle.Result);
			} else if (handle.Status == AsyncOperationStatus.Failed) {
				//Debug.LogError("Async operation failed");
				Addressables.Release(handle);
				onAfter?.Invoke(null);
			}
		}

		Object _LoadFromAddressable(string assetName) {
			if (m_DicAsset.TryGetValue(assetName, out Object asset))
				return asset;

			AsyncOperationHandle<Object> handle = Addressables.LoadAssetAsync<Object>(assetName);
			if (!handle.IsValid()) {
				Debug.LogError($"Invalid AsyncOperationHandle name : {assetName}");
				Addressables.Release(handle);
				return null;
			}

			handle.WaitForCompletion();
			LoadAssetPostProcess(ref handle, ref assetName, (obj) => asset = obj);
			return asset;
		}
		async UniTask<Object> _LoadFromAddressableAsync(string assetName) {
			if (m_DicAsset.TryGetValue(assetName, out Object asset)) {
				return asset;
			}

			AsyncOperationHandle<Object> handle = Addressables.LoadAssetAsync<Object>(assetName);
			if (!handle.IsValid()) {
				Debug.LogError($"Invalid AsyncOperationHandle name : {assetName}");
				Addressables.Release(handle);
				return null;
			}

			if (handle.IsDone)
				LoadAssetPostProcess(ref handle, ref assetName, (obj) => asset = obj);
			else
				Debug.Log("Async operation still in progress");

			Object obj = await handle.ToUniTask();
			m_DicAsset.Add(assetName, obj);
			Debug.Log($"LoadFromAddressable Complete : {obj.name}");
			return obj;
		}

		void _LoadFromAddressableAsync(string assetName, System.Action<Object> onAfter) {
			//AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(assetName);
			if (m_DicAsset.TryGetValue(assetName, out Object asset)) {
				onAfter(asset);
				return;
			}
			AsyncOperationHandle<Object> handle = Addressables.LoadAssetAsync<Object>(assetName);
			if (!handle.IsValid()) {
				Debug.LogError($"Invalid AsyncOperationHandle name : {assetName}");
				Addressables.Release(handle);
				onAfter(null);
				return;
			}
			//AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(key); ;
			//sizeHandle.Completed += (op) => {
			//	long result = op.Result;
			//	Debug.Log($"{key} size : {result}");
			//};

			if (handle.IsDone)
				LoadAssetPostProcess(ref handle, ref assetName, onAfter);
			//else
			//	Debug.Log("Async operation still in progress");

			handle.Completed += (operation) => {
				LoadAssetPostProcess(ref operation, ref assetName, onAfter);
				//Debug.Log($"_LoadFromAddressable Complete : {operation.Result.name}");
			};
		}

		GameObject _SpawnGameObject(string assetName, Transform parent,
									Vector3 pos, TfInfo rot, TfInfo scale,
									AssetType type, bool activeEnable) {
			if (TryGetRecycledItem(assetName, out PoolItem resultItem)) {
				resultItem.m_Obj = PostProcess(resultItem.m_Obj, parent, pos, rot, scale,
												activeEnable, resultItem.m_IsGui);
			} else {
				Object asset = SpawnAsset(assetName);
				if (asset == null)
					return null;

				resultItem.m_Obj = PostProcessObject(asset, parent, pos, rot, scale,
													type, activeEnable, resultItem.m_IsGui);
				resultItem.m_Obj.name = assetName;
			}
			m_ListActivated.Add(resultItem);

			return resultItem.m_Obj;
		}

		void _SpawnGameObjectAsync(string assetName, Action<GameObject> onAfter,
									Transform parent, Vector3 pos, TfInfo rot, TfInfo scale,
									AssetType type, bool activeEnable) {
			if (TryGetRecycledItem(assetName, out PoolItem resultItem)) {
				resultItem.m_Obj = PostProcess(resultItem.m_Obj, parent, pos, rot, scale,
												activeEnable, resultItem.m_IsGui);
				m_ListActivated.Add(resultItem);

				onAfter?.Invoke(resultItem.m_Obj);
			} else {
				SpawnAssetAsync(assetName, (asset) => {
					if (asset == null) {
						onAfter?.Invoke(null);
						return;
					}

					resultItem.m_Obj = PostProcessObject(asset, parent, pos, rot, scale,
														 type, activeEnable, resultItem.m_IsGui);
					resultItem.m_Obj.name = assetName;
					m_ListActivated.Add(resultItem);

					onAfter?.Invoke(resultItem.m_Obj);
				});
			}
		}

		void _PrespawnObject(string assetName, int count,
							Vector3 pos, TfInfo rot, TfInfo scale, AssetType type) {
			// 애셋을 먼저 생성한 뒤 반복
			Object asset = SpawnAsset(assetName);
			if (asset == null)
				return;

			for (int i = 0; i < count; ++i) {
				if (TryGetRecycledItem(assetName, out PoolItem resultItem)) {
					resultItem.m_Obj = PostProcess(resultItem.m_Obj, transform, pos, rot, scale,
													false, resultItem.m_IsGui);
					m_ListDeactivated.Add(resultItem);
				} else {
					resultItem.m_Obj = PostProcessObject(asset, transform, pos, rot, scale,
														 type, false, resultItem.m_IsGui);
					resultItem.m_Obj.name = assetName;
					PostPrespawnProcess(resultItem);
				}
			}
		}

		void _PrespawnObjectAsync(string assetName, int count, Action<bool> onAfter,
								  Vector3 pos, TfInfo rot, TfInfo scale, AssetType type) {
			// 애셋을 먼저 생성한 뒤 반복
			SpawnAssetAsync(assetName, (asset) => {
				if (!asset) {
					onAfter?.Invoke(false);
					return;
				}

				for (int i = 0; i < count; ++i) {
					if (TryGetRecycledItem(assetName, out PoolItem resultItem)) {
						resultItem.m_Obj = PostProcess(resultItem.m_Obj, transform, pos, rot, scale,
														false, resultItem.m_IsGui);
						m_ListDeactivated.Add(resultItem);
					} else {
						resultItem.m_Obj = PostProcessObject(asset, transform, pos, rot, scale,
															 type, false, resultItem.m_IsGui);
						resultItem.m_Obj.name = assetName;
						PostPrespawnProcess(resultItem);
					}
				}

				onAfter?.Invoke(asset != null);
			});

		}

		/// <summary> <see cref="AssetType"/>에 맞게 게임 오브젝트를 설정하는 함수 </summary>
		/// <param name="parent"> 부모로 설정 할 Transform </param>
		/// <param name="rot"> null이 아닌 경우 트랜스폼 값 재설정 </param>
		/// <param name="scale"> null이 아닌 경우 트랜스폼 값 재설정 </param>
		/// <param name="activeEnable"> 생성 후 활성화 여부 </param>
		/// <param name="isGui"> ui의 경우 추가정보를 처리하기 위한 플래그 </param>
		GameObject PostProcessObject(Object asset, Transform parent,
									 Vector3 pos, TfInfo rot, TfInfo scale,
									 AssetType type, bool activeEnable, bool isGui = false) {
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

			return PostProcess(createdObj, parent, pos, rot, scale, activeEnable, isGui);
		}
		GameObject PostProcess(GameObject obj, Transform parent, 
								Vector3 pos, TfInfo rot, TfInfo scale,
								bool activeEnable, bool isGui = false) {
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

//#if BUILD_LOCAL_BUNDLE || BUILD_REMOTE_BUNDLE || UNITY_EDITOR
//			if (AssetMgr.inst.m_LocationType == AssetLocation.Resources)
//				return;

//			// 이펙트의 쉐이더를 찾아준다
//			if (item.m_Obj.name.StartsWith("eff_")) {
//				item.m_Obj.transform.DoRecursively(x => {
//					if (x.TryGetComponent<ParticleSystemRenderer>(out var particleRenderer)) {
//						Material material = particleRenderer.sharedMaterial;

//						if (material != null) {
//							string shaderName = material.shader.name;
//							particleRenderer.sharedMaterial.shader = Shader.Find(shaderName);
//						}
//					}
//				});
//			}
//#endif
		}

		bool TryGetRecycledItem(string assetName, out PoolItem poolItem) {
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
		IEnumerator _CoDespawn(GameObject obj, float delay) {
			yield return CachedYield.GetWFS(delay);

			_Despawn(obj);
		}

		/// <summary>
		/// 사용하지 않는 Object를 m_ListDeactivated로 옮기는 함수
		/// </summary>
		void _Despawn(GameObject obj) {
			if (obj == null)
				return;

			_Despawn(m_ListActivated.Find(item => item.m_Obj == obj));
		}
		void _Despawn(PoolItem poolItem) {
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
		void ChangeParent(GameObject obj, bool isGui = false) {
			if (isGui)  //GUI 오브젝트들은 다른것들 처럼 PoolMgr로 부모변경을 수행 하면 안된다. m_OnAfterDespawnGuiObj에서 수행 하자.
				return;

			obj.transform.SetParent(transform);
		}

		/// <summary>
		/// Object가 GuiObject인지를 판단하기 위한 함수.
		/// </summary>
		/// <returns><c>true</c> if this instance is GUI object the specified name; otherwise, <c>false</c>.</returns>
		/// <param name="name"> 판단을 원하는 Object명 </param>
		bool IsGuiObject(string name) {
			//if (name.StartsWith("wdgt_") == true ||
			//	name.StartsWith("pnl_") == true ||
			//	name.StartsWith("stage_") == true) {
			//	return true;
			//}

			return false;
		}

	}
}