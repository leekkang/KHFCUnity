using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

//TODO : Dependency 관련 코드 모두 제거. 시간나면 추가하자
namespace KHFC {
	public enum AssetLocation {
		Resources,
		LocalAssetBundle,
		RemoteAssetBundle,
	}

	public class AssetMgr : Singleton<AssetMgr> {
		public const string BUNDLE_EXT = ".unity3d";

		bool m_OnInitialized = false;
		public bool initialized => m_OnInitialized;

		System.Text.StringBuilder m_SB;  // 스트링 빌더 캐시

		public AssetLocation m_LocationType;
		string m_LocationPath;

#if UNITY_EDITOR
		/// <summary>에디터에서 현재 로드 된 에셋번들을 확인 하기 위함</summary>
		public List<AssetBundle> m_ListLoadedbundle = new List<AssetBundle>();
#endif
		class AssetBundleInfo {
			public bool m_Loaded = false;
			public AssetBundle m_Bundle;
			public List<string> m_ListDependency = new List<string>();

			public void Load(AssetBundle loadedBundle) {
				m_Loaded = true;
				m_Bundle = loadedBundle;

#if UNITY_EDITOR
				if (!AssetMgr.inst.m_ListLoadedbundle.Contains(loadedBundle))
					AssetMgr.inst.m_ListLoadedbundle.Add(loadedBundle);
#endif
			}

			public void Unload(bool fullUnload = true) {
				m_Loaded = false;
				if (m_Bundle != null)
					m_Bundle.Unload(fullUnload);

#if UNITY_EDITOR
				AssetMgr.inst.m_ListLoadedbundle.Remove(m_Bundle);
#endif
			}
		}

		Dictionary<string, AssetBundleInfo> m_DicAssetbundle = null;

		public void Open() {
			if (m_OnInitialized)
				return;

#if UNITY_EDITOR
			string locType = EditorPrefs.GetString(KHFCSetting.inst.assetLocationKey, AssetLocation.Resources.ToString());
			m_LocationType = (AssetLocation)System.Enum.Parse(typeof(AssetLocation), locType);
#else
#if BUILD_RESOURCES
			m_LocationType = AssetLocation.Resources;
#elif BUILD_LOCAL_BUNDLE
			m_LocationType = AssetLocation.LocalAssetBundle;
#elif BUILD_REMOTE_BUNDLE
			m_LocationType = AssetLocation.RemoteAssetBundle;
#endif
#endif
			Debug.Log("PatchMode : " + m_LocationType);

			m_LocationPath = GetLocationPath();

			m_OnInitialized = true;
		}

		//Get Asset by location Type
		public Object GetAsset(string assetName, string bundleName) {
			Object resultObj = null;

			bundleName = bundleName.ToLower();

			if (m_LocationType == AssetLocation.Resources) {
				resultObj = LoadFromAssetLink(assetName);
				//resultObj = LoadFromResources(assetName);
			} else {
				string prefix = bundleName.Split('_')[0];
				BundleProcess processor = KHFCSetting.inst.m_ListBundleProcess.Find(unit => unit.m_AssetPrefix == prefix);
				if (processor == null || !processor.m_DontZipAssetbundle) {
					resultObj = LoadFromAssetBundle(assetName, bundleName, processor.m_UseSingly);
				} else {
					resultObj = LoadFromFile(assetName, bundleName, processor);
				}
			}

			if (resultObj == null) {
				Debug.LogWarning("Can't Load Asset, AssetName : " + assetName + "  BundleName : " + bundleName);
			}

			return resultObj;
		}

		public void GetAssetAsync(string assetName, string bundleName, System.Action<Object> actOnAfter) {
			if (m_LocationType == AssetLocation.Resources) {
				Object resultObj = LoadFromAssetLink(assetName);
				actOnAfter(resultObj);
				//LoadFromResourcesAsync(assetName, asset => actOnAfter(asset));
			} else {
				bundleName = bundleName.ToLower();
				string prefix = bundleName.Split('_')[0];
				BundleProcess processor = KHFCSetting.inst.m_ListBundleProcess.Find(unit => unit.m_AssetPrefix == prefix);
				if (processor == null || !processor.m_DontZipAssetbundle) {
					LoadFromAssetBundleAsync(assetName, bundleName, (asset) => actOnAfter(asset), processor.m_UseSingly);
				} else {
					LoadFromFileAsync(assetName, bundleName, processor, (asset) => actOnAfter(asset));
				}
			}
		}

		public async Task<Object> GetAssetAsync(string assetName, string bundleName) {
			if (m_LocationType == AssetLocation.Resources) {
				return LoadFromAssetLink(assetName);
				//LoadFromResourcesAsync(assetName, asset => actOnAfter(asset));
			} else {
				//bundleName = bundleName.ToLower();
				//string prefix = bundleName.Split('_')[0];
				//BundleProcess processor = KHFCSetting.inst.m_ListBundleProcess.Find(unit => unit.m_AssetPrefix == prefix);
				//if (processor == null || !processor.m_DontZipAssetbundle) {
				//	LoadFromAssetBundleAsync(assetName, bundleName, (asset) => actOnAfter(asset), processor.m_UseSingly);
				//} else {
				//	LoadFromFileAsync(assetName, bundleName, processor, (asset) => actOnAfter(asset));
				//}
			}
			return null;
		}


		private Object LoadFromAssetLink(string assetName) {
			return AssetLinkData.inst.GetLink<Object>(assetName);
		}

		private Object LoadFromResources(string assetName) {
			return Resources.Load(assetName);
		}

		private void LoadFromResourcesAsync(string assetName, System.Action<Object> actOnAfter) {
			StartCoroutine(CoLoadFromResourcesAsync(assetName, actOnAfter));
		}
		private IEnumerator CoLoadFromResourcesAsync(string assetName, System.Action<Object> actOnAfter) {
			ResourceRequest req = Resources.LoadAsync(assetName);
			yield return req;

			actOnAfter(req.asset);
		}

		private async Task<Object> LoadFromResourcesAsync(string assetName) {
			ResourceRequest req = Resources.LoadAsync(assetName);
			await req;
			return req.asset;
		}


		private Object LoadFromAssetBundle(string assetName, string bundleName, bool isSingly = false) {
			if (m_SB == null)
				m_SB = new System.Text.StringBuilder();

			if (m_DicAssetbundle == null)
				LoadRoot();

			m_SB.Remove(0, m_SB.Length);

			bundleName += BUNDLE_EXT;
			string prefix = bundleName.Split('_')[0];

			string finalBundleName = bundleName;

			if (!isSingly) {
				m_SB.AppendFormat("{0}/{1}", prefix, bundleName);

				//"cha_anubis.unity3d" -> "cha/cha_anubis.unity3d"
				finalBundleName = m_SB.ToString();
				if (!m_DicAssetbundle.ContainsKey(finalBundleName))
					return null;
			} else {
				if (!m_DicAssetbundle.ContainsKey(finalBundleName)) {
					m_DicAssetbundle.Add(finalBundleName, new AssetBundleInfo());
				}
			}

			AssetBundleInfo bundleInfo = m_DicAssetbundle[finalBundleName];
			AssetBundle loadedBundle = bundleInfo.m_Bundle != null ? bundleInfo.m_Bundle : AssetBundle.LoadFromFile(m_LocationPath + bundleName);

			if (loadedBundle == null)
				return null;

			//if (loadedBundle != null && bundleInfo.m_ListDependencies.Count > 0)
			//	LoadDependencies(bundleInfo.m_ListDependencies, bundleName);

			Object resultObj = loadedBundle.LoadAsset(assetName);
			//사운드 클립 관련해서 아래 에러가 발생 해서 아래와 같이 해결함
			//Error: Cannot create FMOD::Sound instance for resource archive:{path}, (File not found. )
			//https://forum.unity3d.com/threads/fmod-sound-instance-for-resource-error-with-asset-bundles.431456/
			if (resultObj is AudioClip) {
				AudioClip clip = resultObj as AudioClip;

				if (clip.loadState == AudioDataLoadState.Unloaded ||
					clip.loadState == AudioDataLoadState.Failed) {
					clip.LoadAudioData();
				}
			}

			// TODO: loadedBundle.Unload 할지 여부를 판단하는 조건문을 다른 방식으로 대체 필요
			if (prefix == "dat" || prefix == "pnl" || prefix == "wdgt" || prefix == "eff") {
				loadedBundle.Unload(false);
			} else {
				bundleInfo.Load(loadedBundle);
			}

			return resultObj;
		}

		private void LoadFromAssetBundleAsync(string assetName, string bundleName, System.Action<Object> actOnAfter, bool isSingly = false) {
			StartCoroutine(CoLoadFromAssetBundleAsync(assetName, bundleName, actOnAfter, isSingly));
		}
		private IEnumerator CoLoadFromAssetBundleAsync(string assetName, string bundleName, System.Action<Object> actOnAfter, bool isSingly = false) {
			if (m_SB == null)
				m_SB = new System.Text.StringBuilder();

			if (m_DicAssetbundle == null) {
				LoadRoot();
			}

			m_SB.Remove(0, m_SB.Length);

			string prefix = bundleName.Split('_')[0];
			bundleName = bundleName + BUNDLE_EXT;
			string finalBundleName = string.Empty;
			AssetBundleInfo bundleInfo = null;

			if (!isSingly) {
				m_SB.AppendFormat("{0}/{1}", prefix, bundleName);
				finalBundleName = m_SB.ToString();

				if (!m_DicAssetbundle.ContainsKey(finalBundleName)) {
					actOnAfter(null);
					yield break;
				}

				bundleInfo = m_DicAssetbundle[finalBundleName];
				//if (bundleInfo.m_ListDependency.Count > 0) {
				//	//"cha_anubis.unity3d" -> "cha/cha_anubis.unity3d"
				//	yield return StartCoroutine(CoLoadDependenciesAsync(bundleInfo.m_ListDependency, finalBundleName));
				//}
			} else {
				finalBundleName = bundleName;

				if (!m_DicAssetbundle.ContainsKey(finalBundleName)) {
					m_DicAssetbundle.Add(finalBundleName, new AssetBundleInfo());
				}

				bundleInfo = m_DicAssetbundle[finalBundleName];
			}

			AssetBundle loadedBundle = bundleInfo.m_Bundle;

			if (loadedBundle == null) {
				AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(m_LocationPath + bundleName);
				yield return abcr;

				loadedBundle = abcr.assetBundle;
			}

			if (loadedBundle == null) {
				actOnAfter(null);
				yield break;
			}

			AssetBundleRequest abr = loadedBundle.LoadAssetAsync(assetName);
			abr.priority = 2000;
			yield return abr;

			Object resultObj = abr.asset;
			//사운드 클립 관련해서 아래 에러가 발생 해서 아래와 같이 해결함
			//Error: Cannot create FMOD::Sound instance for resource archive:{path}, (File not found. )
			//https://forum.unity3d.com/threads/fmod-sound-instance-for-resource-error-with-asset-bundles.431456/
			if (resultObj is AudioClip) {
				AudioClip clip = resultObj as AudioClip;

				if (clip.loadState == AudioDataLoadState.Unloaded || clip.loadState == AudioDataLoadState.Failed) {
					clip.LoadAudioData();
				}
			}

			// TODO: loadedBundle.Unload 할지 여부를 판단하는 조건문을 다른 방식으로 대체 필요
			if (prefix == "dat" || prefix == "pnl" || prefix == "wdgt" || prefix == "eff") {
				loadedBundle.Unload(false);
			} else {
				bundleInfo.Load(loadedBundle);
			}

			actOnAfter(resultObj);
		}

		private Object LoadFromFile(string assetName, string bundleName, BundleProcess bundleProcessor) {
			string file_extension = string.Empty;

			// TODO: 확장자 선정 방식 변경 필요
			if (bundleProcessor.m_AssetPrefix == "snd") {
				if (assetName.StartsWith("snd_bgm")) {
					file_extension = ".ogg";
				} else {
					file_extension = ".wav";
				}
			}

			string filePath = "file://" + System.IO.Path.Combine(m_LocationPath, assetName + file_extension);
			WWW wwwLoadFile = new WWW(filePath);

			while (!wwwLoadFile.isDone)
				;

			if (!string.IsNullOrEmpty(wwwLoadFile.error)) {
				Debug.LogError("Can't Load File : " + filePath + ", ERROR : " + wwwLoadFile.error);
				return null;
			}

			return (Object)wwwLoadFile.GetAudioClip();
		}

		private void LoadFromFileAsync(string assetName, string bundleName, BundleProcess bundleProcessor, System.Action<Object> actOnAfter) {
			StartCoroutine(CoLoadFromFileAsync(assetName, bundleName, bundleProcessor, actOnAfter));
		}
		private IEnumerator CoLoadFromFileAsync(string assetName, string bundleName, BundleProcess bundleProcessor, System.Action<Object> actOnAfter) {
			string file_extension = string.Empty;

			// TODO: 확장자 선정 방식 변경 필요
			if (bundleProcessor.m_AssetPrefix == "snd") {
				if (assetName.StartsWith("snd_bgm")) {
					file_extension = ".ogg";
				} else {
					file_extension = ".wav";
				}
			}

			string filePath = "file://" + System.IO.Path.Combine(m_LocationPath, assetName + file_extension);
			Object resultObj = null;

			using (WWW wwwLoadFile = new WWW(filePath)) {

				yield return wwwLoadFile;

				if (!string.IsNullOrEmpty(wwwLoadFile.error)) {
					Debug.LogError("Can't Load File : " + filePath + ", ERROR : " + wwwLoadFile.error);
					actOnAfter(null);
				}

				if (resultObj is AudioClip) {
					AudioClip clip = resultObj as AudioClip;

					if (clip.loadState == AudioDataLoadState.Unloaded || clip.loadState == AudioDataLoadState.Failed) {
						clip.LoadAudioData();
					}
				}

				actOnAfter(resultObj);
			}
		}

		public void UnLoadMainAssetBundle(string assetName, bool fullUnload = true) {
			if (m_LocationType == AssetLocation.Resources)
				return;

			m_SB.Remove(0, m_SB.Length);

			string prefix = assetName.Split('_')[0];
			string bundleName = assetName + BUNDLE_EXT;
			BundleProcess processor = KHFCSetting.inst.m_ListBundleProcess.Find(item => item.m_AssetPrefix == prefix);

			if (!processor.m_UseSingly) {
				m_SB.AppendFormat("{0}/{1}", prefix, bundleName);
				bundleName = m_SB.ToString();
			}

			if (m_DicAssetbundle.ContainsKey(bundleName)) {
				m_DicAssetbundle[bundleName].Unload(fullUnload);
			}
		}


		private void LoadRoot() {
			//			string manifest_name = "";

			//#if UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN
			//			                manifest_name = "StandaloneWindows64";
			//#elif UNITY_IOS
			//			                manifest_name = "iOS";
			//#elif UNITY_ANDROID
			//			            manifest_name = "Android";
			//#elif UNITY_EDITOR_OSX
			//			                manifest_name = EditorPrefs.GetString(KHFCSetting.inst.m_patch_type_key, "StandaloneOSX");
			//#elif UNITY_STANDALONE_LINUX
			//						manifest_name = "StandaloneLinux64";
			//#else
			//			manifest_name = EditorPrefs.GetString(KHFCSetting.inst.m_patch_type_key, "StandaloneWindows64");
			//#endif

			//			string root_dna_text = System.IO.File.ReadAllText(m_str_location_path + manifest_name + DNAFILE_EXT);
			//			RootDna dna = (RootDna)JsonUtility.FromJson(root_dna_text, typeof(RootDna));

			m_DicAssetbundle = new Dictionary<string, AssetBundleInfo>();

			//int count = dna.m_l_assets.Count;

			//for (int i = 0; i < count; i++) {
			//	RootDnaInfo dna_info = dna.m_l_assets[i];

			//	if (m_DicAssetbundle.ContainsKey(dna_info.asset_name))
			//		continue;

			//	AssetBundleInfo bundleInfo = new AssetBundleInfo();
			//	bundleInfo.m_Loaded = false;
			//	//UI인경우, UIFont에셋번들 종속성으로 되어 있는것들은 제거 하자.
			//	string dynamic_font = dna_info.m_ListDependency.Find(dep => dep.StartsWith("fnt/fnt_dyn_"));
			//	if (!string.IsNullOrEmpty(dynamic_font)) {
			//		dna_info.m_ListDependency.Remove(dynamic_font);
			//	}

			//	bundleInfo.m_DicAssetbundle.AddRange(dna_info.m_ListDependency);

			//	m_d_assetbundles[dna_info.asset_name] = bundleInfo;
			//}
		}

		private string GetLocationPath() {
			switch (m_LocationType) {
				case AssetLocation.Resources:
					return "";
				case AssetLocation.LocalAssetBundle:
#if UNITY_EDITOR || UNITY_STANDALONE
					return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar;
#else
				return Application.persistentDataPath + "/";
#endif
				case AssetLocation.RemoteAssetBundle:
					return Application.persistentDataPath + "/";
				default:
					return "NONE";
			}
		}
	}
}
