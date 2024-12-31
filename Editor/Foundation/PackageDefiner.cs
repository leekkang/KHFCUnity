
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace KHFC.Editor {
	/// <summary> 현재 프로젝트에서 특정 패키지를 사용하고 있는지 확인하고 디파인 심볼을 설정한다 </summary>
	[InitializeOnLoad]
	public class PackageDefiner : UnityEditor.AssetModificationProcessor {
		const string PREFIX = "KHFC_";
		/// <summary> 설치되어 있는 어셈블리 종류. 어셈블리 이름과 직접 비교한다 (이름이 동일해야함) </summary>
		static readonly HashSet<string> m_ArrAssembly = new() { "UniTask" };
		/// <summary>
		/// 설치되어 있는 패키지 이름.
		/// <see cref="UnityEditor.PackageManager.PackageInfo.displayName"/> 과 비교한다 (이름이 동일해야함)
		/// </summary>
		static readonly HashSet<string> m_ArrPackage = new() { "Addressables", "In App Purchasing" };

		/// <summary> 디파인 심볼 이름, 키는 패키지의 이름과 동일함 </summary>
		static readonly Dictionary<string, string> m_DicPackageName = new() {
			{ "UniTask", "KHFC_UNITASK" },
			{ "Addressables", "KHFC_ADDRESSABLES" },
			{ "In App Purchasing", "KHFC_IAP" },
		};
		
		static PackageDefiner() {
			// Open the Easy Save 3 window the first time ES3 is installed.
			//ES3Editor.ES3Window.OpenEditorWindowOnStart();

#if UNITY_2017_2_OR_NEWER
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			EditorApplication.playModeStateChanged += PlayModeStateChanged;
#else
			EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
			EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
#endif
		}

		/// <summary> 필요한 KHFC 디파인 심볼을 확인하고 업데이트 한다. </summary>
		[MenuItem("KHFC/Definer/Update Define Symbol", priority = 1)]
		public static void UpdateNeededDefineSymbol() {
			UpdateSymbol();
		}

		/// <summary> 현재 빌드타겟의 KHFC 디파인 심볼을 전부 제거한다 </summary>
		[MenuItem("KHFC/Definer/Remove All Package Define Symbol", priority = 2)]
		public static void RemoveAllPackageDefine() {
			RemoveSymbol(EditorUserBuildSettings.activeBuildTarget);
		}

		//[InitializeOnLoadMethod]
		static void UpdateSymbol() {
			//PlayerPrefs.SetInt
			BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
			//RemoveSymbol(target);

			BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
			List<string> list = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();

			List<string> listTarget = list.FindAll(Predicate);
			List<string> listSymbol = FindSymbolInAssemblies();
			listSymbol.AddRange(FindSymbolInPackage());

			if (listSymbol.Count != listTarget.Count) {
				//AddSymbols(listSymbol.ToArray(), group);
				list.RemoveAll(Predicate);
				list.AddRange(listSymbol);
				PlayerSettings.SetScriptingDefineSymbolsForGroup(group, list.ToArray());
			}

			static bool Predicate(string x) => x.StartsWith(PREFIX);
		}


#if UNITY_2017_2_OR_NEWER
		public static void PlayModeStateChanged(PlayModeStateChange state) {
			//if (state == PlayModeStateChange.ExitingEditMode)
			//	RefreshReferences(true);
		}
#else
	public static void PlaymodeStateChanged() {
		// Add all GameObjects and Components to the reference manager before we enter play mode.
		if (!EditorApplication.isPlaying && ES3Settings.defaultSettingsScriptableObject.autoUpdateReferences)
			RefreshReferences(true);
	}
#endif

		static List<string> FindSymbolInAssemblies() {
#if UNITY_2017_3_OR_NEWER
			UnityEditor.Compilation.Assembly[] assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();

			List<string> listName = new();

			foreach (UnityEditor.Compilation.Assembly assembly in assemblies) {
				try {
					string name = assembly.name;
					string substr = name.Length >= 5 ? name[..5] : "";

					if (substr != "Unity" && !name.StartsWith("com.uni") && !name.Contains("-Editor"))
						listName.Add(name);
				} catch { }
			}

			// Sort it alphabetically so that the order isn't constantly changing, which can affect version control.
			listName.Sort();
			List<string> listSymbol = new();
			foreach (string name in listName) {
				if (m_ArrAssembly.Contains(name))
					listSymbol.Add(m_DicPackageName[name]);
			}
			// Only update if the list has changed.
			//for (int i = 0; i < currentAssemblyNames.Length; i++) {
			//	if (currentAssemblyNames.Length != listName.Count || currentAssemblyNames[i] != listName[i]) {
			//		defaults.settings.assemblyNames = listName.ToArray();
			//		EditorUtility.SetDirty(defaults);
			//		break;
			//	}
			//}
			return listSymbol;
#else
			return null;
#endif
		}

		static List<string> FindSymbolInPackage() {
			List<string> listSymbol = new();
			//var enumerator = m_ArrPackage.GetEnumerator();
			foreach (string name in m_ArrPackage) {
				UnityEditor.PackageManager.PackageInfo info = GetPackageInfo(name);
				if (info != null)
					listSymbol.Add(m_DicPackageName[name]);
			}
			return listSymbol;
		}

		static UnityEditor.PackageManager.PackageInfo GetPackageInfo(string packageName) {
			IEnumerable<string> listPath = UnityEditor.AssetDatabase.FindAssets("package")
				.Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
				.Where(x => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(x) != null);

			List<UnityEditor.PackageManager.PackageInfo> list
				= listPath.Select(UnityEditor.PackageManager.PackageInfo.FindForAssetPath)
						  .Where(x => x != null).ToList();
			return list.Find(x => x.displayName.Contains(packageName));
		}

		static void AddSymbol(string name, BuildTargetGroup group) {
			List<string> listSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
			if (listSymbol.FindIndex(x => x == name) == -1)
				listSymbol.Add(name);

			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, listSymbol.ToArray());
			//PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", listSymbol));
		}

		static void AddSymbols(string[] names, BuildTargetGroup group) {
			UnityEngine.Debug.Log($"KHFC.PackageDefiner add symbol : {names.ToList()}");
			List<string> listSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
			// 없는 심볼이면 추가
			listSymbol.AddRange(names.Except(listSymbol));

			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, listSymbol.ToArray());
			//PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", listSymbol));
		}

		/// <summary> 심볼 추가하기 전 예전에 추가한 심볼들을 전부 제거한다. </summary>
		static void RemoveSymbol(BuildTarget target) {
			BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
			List<string> listSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
			listSymbol.RemoveAll(x => x.StartsWith(PREFIX));

			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, listSymbol.ToArray());
			//PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", listSymbol));
		}


		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			bool imported = false;
			//foreach (string path in importedAssets) {
			//	if (Path.GetExtension(path) == ".xls" || Path.GetExtension(path) == ".xlsx") {
			//		//if (cachedInfos == null) cachedInfos = FindExcelAssetInfos();
			//		cachedInfos ??= FindExcelAssetInfos();

			//		var excelName = Path.GetFileNameWithoutExtension(path);
			//		if (excelName.StartsWith("~$"))
			//			continue;

			//		ExcelAssetInfo info = cachedInfos.Find(i => i.ExcelName == excelName);

			//		if (info == null)
			//			continue;

			//		ImportExcel(path, info);
			//		imported = true;
			//	}
			//}

			if (imported) {
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
	}
}
