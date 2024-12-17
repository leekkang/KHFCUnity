
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace KHFC.Editor {
	/// <summary> 현재 프로젝트에서 특정 패키지를 사용하고 있는지 확인하고 디파인 심볼을 설정한다 </summary>
	[InitializeOnLoad]	
	public class PackageDefiner : UnityEditor.AssetModificationProcessor {
		const string PREFIX = "KHFC_";
		static readonly HashSet<string> m_ArrAssembly = new() { "UniTask" };
		static readonly HashSet<string> m_ArrPackage = new() { "Addressables" };
		
		static bool m_Initialized = false;
		static BuildTarget m_BuildTarget;

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

		static void UpdateSymbol() {
			if (m_Initialized)
				return;
			BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
			if (target == m_BuildTarget)
				return;

			m_BuildTarget = target;
			FindSymbolInAssemblies();
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

		static void FindSymbolInAssemblies() {
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
					listSymbol.Add(PREFIX + name.ToUpper());
			}
			// Only update if the list has changed.
			//for (int i = 0; i < currentAssemblyNames.Length; i++) {
			//	if (currentAssemblyNames.Length != listName.Count || currentAssemblyNames[i] != listName[i]) {
			//		defaults.settings.assemblyNames = listName.ToArray();
			//		EditorUtility.SetDirty(defaults);
			//		break;
			//	}
			//}
			AddSymbols(listSymbol.ToArray());
#endif
		}

		static void FindSymbolInPackage() {
			List<string> listSymbol = new();
			//var enumerator = m_ArrPackage.GetEnumerator();
			foreach (string name in m_ArrPackage) {
				UnityEditor.PackageManager.PackageInfo info = GetPackageInfo(name);
				if (info != null)
					listSymbol.Add(PREFIX + name.ToUpper());
			}
			AddSymbols(listSymbol.ToArray());
		}

		static UnityEditor.PackageManager.PackageInfo GetPackageInfo(string packageName) {
			return UnityEditor.AssetDatabase.FindAssets("package")
				.Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
					.Where(x => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(x) != null)
				.Select(UnityEditor.PackageManager.PackageInfo.FindForAssetPath)
					.Where(x => x != null)
				.First(x => x.name.Contains(packageName));
		}

		static void AddSymbol(string name) {
			BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(m_BuildTarget);
			List<string> listSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
			if(listSymbol.FindIndex(x => x == name) == -1)
				listSymbol.Add(name);

			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", listSymbol));
		}

		static void AddSymbols(string[] names) {
			BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(m_BuildTarget);
			List<string> listSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
			// 없는 심볼이면 추가
			listSymbol.AddRange(names.Except(listSymbol));

			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", listSymbol));
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
