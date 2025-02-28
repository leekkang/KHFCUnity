using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
//using System.Reflection;

namespace KHFC {
	[System.Serializable]
	public class PublishOption {
		public string m_PublishName;		//Publish 이름 ( BUILD에선 결과물의 이름, DefineSymbol로 사용된다)
		public string m_PrebuildFunc;
		public string m_PostbuildFunc;
		public string m_ProductName		= "Solocar";
		public string m_AppIdentifier	= "com.khfc.solocar";
	}

	[System.Serializable]
	public class BundleProcess {
		public string m_AssetPrefix;
		public string m_BundleProcessor;
		/// <summary>패치의 대상이긴 하지만, 에셋번들로 만들진 않겠다</summary>
		public bool m_DontZipAssetbundle;
		/// <summary>통합 Manifest를 사용하지 않고, 개별 Manifest를 사용하겠다. 다른 번들과 Dependecy가 없는 것만 사용 할 수 있다.</summary>
		public bool m_UseSingly;
	}

	/// <summary>
	/// 클라이언트의 번들 관리에 연관된 변수와 기능을 제공하는 클래스
	/// </summary>
	/// <remarks> 싱글톤 클래스이다. </remarks>
	public class KHFCSetting : ScriptableObject {

		/// <summary> Inspector에서 Info항목에 보여지길 원하는 변수에 적용해 준다. </summary>
		public class InfoAttribute : System.Attribute {
			public string m_Msg;

			public InfoAttribute(string msg) {
				m_Msg = msg;
			}
		}

		static KHFCSetting m_Instance;
		public static KHFCSetting inst {
			get {
				if (!m_Instance) {
					m_Instance = (KHFCSetting)Resources.Load("KHFCSetting");

					if (m_Instance == null) {
						Debug.LogError("KHFCSetting.asset이 없습니다");
					}
				}
				return m_Instance;
			}
		}
		//public static KHFCSetting inst {
		//	get {
		//		m_Instance = NullableInstance;

		//		if (m_Instance == null) {
		//			// Create KHFCSetting.asset inside the folder in which KHFCSetting.cs reside.
		//			m_Instance = ScriptableObject.CreateInstance<KHFCSetting>();
		//			var guids = AssetDatabase.FindAssets(string.Format("{0} t:script", "KHFCSetting"));
		//			if (guids == null || guids.Length <= 0) {
		//				return m_Instance;
		//			}
		//			var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]).Replace("KHFCSetting.cs", "KHFCSetting.asset");
		//			AssetDatabase.CreateAsset(m_Instance, assetPath);
		//		}

		//		return m_Instance;
		//	}
		//}
		//public static KHFCSetting NullableInstance {
		//	get {
		//		if (m_Instance == null) {
		//			var guids = AssetDatabase.FindAssets(string.Format("{0} t:ScriptableObject", "KHFCSetting"));
		//			if (guids == null || guids.Length <= 0) {
		//				return m_Instance;
		//			}
		//			var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
		//			m_Instance = (KHFCSetting)AssetDatabase.LoadAssetAtPath(assetPath, typeof(KHFCSetting));
		//		}

		//		return m_Instance;
		//	}
		//}

		const string m_NameAssembly = "Assembly-CSharp";
		const string m_NameAssemblyEditor = "Assembly-CSharp-Editor";

		#region Build

		public PublishOption m_SelectedPubOpt; // 빌드시 선택된 옵션이 저장되는 변수
		public PublishOption m_DefaultPubOpt;

		[Info("PrebuildFunc의 기본 실행 함수")]
		public readonly string m_DefaultPrebuildFuncName = "KHFC.BuildBaseEdtWnd.PreBuild";
		[Info("PostbuildFunc의 기본 실행 함수")]
		public readonly string m_DefaultPostbuildFuncName = "KHFC.BuildBaseEdtWnd.PostBuild";

		public string m_CodeName = "solocar";
		public List<PublishOption> m_ListPublishOption;
		public string m_PrebuildFuncName;
		public string m_PostbuildFuncName;

		#region Required Version

		/// <summary>Unity Version</summary>
		public string m_UnityVersion;
		/// <summary>Android SDK Plaform Version</summary>
		public string m_AosPlatformVersion;
		/// <summary>Android SDK Build-tools Version</summary>
		public string m_AosBuildtoolsVersion;
		/// <summary>XCode Version</summary>
		public string m_IosXcodeVersion;

		#endregion

		[Info("기본적으로 있어야 하는 Application의 시작 Scene Path")]
		public const string BOOTSTRAP_SCENE_PATH = "Assets/Client/Scenes/BootStrap.unity";

		#endregion

		#region AssetBundle

		public List<BundleProcess> m_ListBundleProcess;

		#endregion

		#region DefaultFolderPaths

		public const string ASSET_RESOURCES_PATH = "Assets/Resources";
		[Info("클라에서 공유하게 될 스크립트가 위치할 폴더")]
		public const string CLIENT_SCRIPT_PATH = "Assets/Client/Script/Global/";
		//[Info("ExportAsset을 할 때 결과물들이 위치할 폴더")]
		//public const string EXPORT_ASSET_PATH = "Assets/Client/@ExportedAssets/Resources/";
		[Info("현재 프로젝트의 Root 폴더")]
		public static string m_PrjRootPath;
		//폴더 구조 만들 리스트
		List<string> m_ListFolderPath;

		#endregion

		public string assetLocationKey {
			get { return m_CodeName + "AssetLoadLocation"; }
		}

		public void SetDefaultSetting() {
			m_DefaultPubOpt = new PublishOption {
				m_PublishName = "DEV_SOLOCAR",
				m_PrebuildFunc = "KHFC.BuildBaseEdtWnd.DefaultPreBuild",
				m_PostbuildFunc = "KHFC.BuildBaseEdtWnd.DefaultPostBuild",
				m_ProductName = "Solocar",
				m_AppIdentifier = "com.PolyHighTech.solocar",
			};
		}

		//Call Method(classname.methodname)
		public static void StringCallMethod(string funcName, params object[] funcParameters) {
			if (string.IsNullOrEmpty(funcName) || !funcName.Contains(".")) {
				Debug.LogError("String value isn't Correct");
				return;
			}

			//[1] namespace.classname or classname
			//[2] methodname
			// must use static func to call
			int dotIndex = funcName.LastIndexOf(".");
			string className = funcName.Substring(0, dotIndex);
			string methodName = funcName.Substring(dotIndex + 1);

			// Editor에서 요청할때는 ", assembly"가 필요하기 때문에 예외처리
			System.Type classType = System.Type.GetType(className + ", " + m_NameAssembly);
			if (classType == null) {
				classType = System.Type.GetType(className + ", " + m_NameAssemblyEditor);
				if (classType == null) {
					Debug.LogError("class name or method name is wrong, " + className + ", " + methodName);
					return;
				}
			}
			System.Reflection.MethodInfo info = classType.GetMethod(methodName);
			if (info == null) {
				Debug.LogError("method name is wrong");
				return;
			}

			// 함수가 받아야하는 매개변수가 있을경우
			if (funcParameters.Length != 0)
				info.Invoke(null, funcParameters);
			else
				info.Invoke(null, null);
		}

		//Ensure Directory and Debug
		public void CheckPredefinedPath() {
			if (m_ListFolderPath != null)
				m_ListFolderPath.Clear();

			m_ListFolderPath = new List<string>() {
				ASSET_RESOURCES_PATH,
				CLIENT_SCRIPT_PATH,
			};

			for (int i = 0; i < m_ListFolderPath.Count; i++) {
				Util.CreateDir(m_ListFolderPath[i]);
			}
		}

		/// <summary>
		/// Puboption을 저장하는 함수
		/// </summary>
		public void CopyPublishOption(PublishOption option) {
			m_SelectedPubOpt = new PublishOption {
				m_PublishName = option.m_PublishName,
				m_PrebuildFunc = option.m_PrebuildFunc,
				m_PostbuildFunc = option.m_PostbuildFunc,
				m_ProductName = option.m_ProductName,
				m_AppIdentifier = option.m_AppIdentifier
			};
		}
		
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void OnProjectLoadedInEditor() {
			m_PrjRootPath = Path.GetDirectoryName(Path.GetDirectoryName(Application.dataPath));
			Debug.Log("Current Project Root is : " + m_PrjRootPath);
		}
#endif
	}
}
