using UnityEditor;

namespace KHFC {
	/// <summary> 현재 프로젝트에서 특정 패키지를 사용하고 있는지 확인하고 디파인 심볼을 설정한다 </summary>
	public class PackageDefiner : AssetPostprocessor {

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
