using UnityEditor;
using UnityEngine;

namespace KHFC.Editor {
	public class ClipCacheKey {
		/// <summary>
		/// 현재 오브젝트의 상위 오브젝트에 <see cref="KHFC.CachedComponent"/>가 있는지 확인한 후 
		/// 해당 오브젝트 까지의 상대 경로를 클립보드에 복사한다.
		/// </summary>
		[MenuItem("GameObject/KHFC/Clip Object Path %g", priority = (int)MenuPriority.GameObject)] // 단축키 : ctrl + g
		static public void ClipObjectPath() {
			GameObject go = Selection.activeGameObject;
			if (go == null)
				return;
			
			string str = "";
			while (go.GetComponent<CachedComponent>() == null) {
				if (go.transform.parent == null) {
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

			str = '\"' + str[1..] + '\"';	// c# 8.0 ranges operator
			GUIUtility.systemCopyBuffer = str;
			Debug.Log($"copy complete : {str}");
		}

		[MenuItem("GameObject/Clip Object Path %g", true)]
		private static bool ClipObjectPathValidation() {
			// We can only copy the path in case 1 object is selected
			//return Selection.gameObjects.Length == 1 &&
			//		Selection.activeGameObject.transform is RectTransform;
			return Selection.gameObjects.Length == 1;
		}
	}
}