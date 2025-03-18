

using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KHFC {
	internal partial class Util {
		public const string CONDITION_SYMBOL = "ENABLE_LOG";
		// AOS와 IOS 공통 로그 출력 개선
#if UNITY_IOS
		[System.Runtime.InteropServices.DllImport("__Internal")]
		static extern void NSLog_iOS(string log);
#endif

		public static bool isDebugBuild {
			get { return UnityEngine.Debug.isDebugBuild; }
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void Log(object message) {
#if UNITY_IOS
			NSLog_iOS(message.ToString());
#else
			UnityEngine.Debug.Log(message);
#endif
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void Log(object message, UnityEngine.Object context) {
			UnityEngine.Debug.Log(message, context);
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void LogError(object message) {
			UnityEngine.Debug.LogError(message);
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void LogError(object message, UnityEngine.Object context) {
			UnityEngine.Debug.LogError(message, context);
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void LogWarning(object message) {
			UnityEngine.Debug.LogWarning(message);
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void LogWarning(object message, UnityEngine.Object context) {
			UnityEngine.Debug.LogWarning(message, context);
		}

		public static void LogException(Exception exception) {
			UnityEngine.Debug.LogException(exception);
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void Break() {
			UnityEngine.Debug.Break();
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void DrawLine(UnityEngine.Vector3 start, UnityEngine.Vector3 end,
			UnityEngine.Color color = default, float duration = 0.0f, bool depthTest = true) {
			UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void DrawRay(UnityEngine.Vector3 start, UnityEngine.Vector3 dir,
			UnityEngine.Color color = default, float duration = 0.0f, bool depthTest = true) {
			UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
		}

		[System.Diagnostics.Conditional(CONDITION_SYMBOL)]
		public static void Assert(bool condition) {
			if (!condition)
				throw new Exception();
		}

		// 출처 : https://upbo.tistory.com/164
		//[UnityEditor.Callbacks.OnOpenAsset()]
		//static bool OnOpenDebugLog(int instance, int line) {
		//	string name = UnityEditor.EditorUtility.InstanceIDToObject(instance).name;
		//	if (!name.Equals("Debug"))
		//		return false;

		//	// 에디터 콘솔 윈도우의 인스턴스를 찾는다
		//	Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
		//	if (assembly == null)
		//		return false;

		//	Type windowType = assembly.GetType("UnityEditor.ConsoleWindow");
		//	if (windowType == null)
		//		return false;

		//	FieldInfo windowField = windowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
		//	if (windowField == null)
		//		return false;

		//	object windowInstance = windowField.GetValue(null);
		//	if (windowInstance == null)
		//		return false;
		//	if (windowInstance != (object)UnityEditor.EditorWindow.focusedWindow)
		//		return false;

		//	// 콘솔 윈도우 인스턴스의 활성화된 텍스트를 찾는다
		//	FieldInfo activeTextField = windowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
		//	if (activeTextField == null)
		//		return false;

		//	string activeTextValue = activeTextField.GetValue(windowInstance).ToString();
		//	if (string.IsNullOrEmpty(activeTextValue))
		//		return false;

		//	// 디버그 로그를 호출한 파일 경로를 찾아 편집기로 연다
		//	Match match = Regex.Match(activeTextValue, @"\(at (.+)\)");
		//	if (match.Success)
		//		match = match.NextMatch(); // stack trace의 첫번째를 건너뛴다

		//	if (!match.Success)
		//		return false;

		//	string path = match.Groups[1].Value;
		//	var split = path.Split(':');
		//	string filePath = split[0];
		//	int lineNum = Convert.ToInt32(split[1]);

		//	string dataPath = UnityEngine.Application.dataPath[..UnityEngine.Application.dataPath.LastIndexOf("Assets")];
		//	UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(dataPath + filePath, lineNum);
		//	return true;
		//}
	}
}