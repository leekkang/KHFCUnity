using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;

namespace KHFC.Editor {
	public class ResizeEditorWindow : EditorWindow {
		static int m_Width = 640;
		static int m_Height = 1136;
		static string m_ResName = "Default Resolution";

		void OnSelectionChange() { Repaint(); }

		void OnEnable() {
		}

		void OnDisable() {
		}

		public static EditorWindow GetMainGameView() {
			// 타입 찾을 때 Assembly Qualified Name을 지정해준다.
			System.Type type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			if (type == null) {
				Debug.LogError("GameView type not found");
				return null;
			}

			// Unity 에디터에서 모든 윈도우를 순회하며 GameView 타입을 찾음
			EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();

			foreach (EditorWindow window in allWindows) {
				if (window.GetType() == type) {
					return window;
				}
			}

			Debug.LogError("GameView window not found");
			return null;
		}

		void OnGUI() {
			m_Width = EditorGUILayout.IntField("X", m_Width);
			m_Height = EditorGUILayout.IntField("Y", m_Height);
			m_ResName = EditorGUILayout.TextField("Resolution Name", m_ResName);

			if (GUILayout.Button("Set")) {
				EditorWindow gameView = GetMainGameView();
				//if (gameView != null)
				//	AddCustomResolution(gameView, m_Width, m_Height, m_ResName);
				
				//Rect pos = gameView.position;
				//pos.y = pos.y - 0;
				//pos.width = m_Size.x;
				//pos.height = m_Size.y + 17;
				//gameView.position = pos;
			}
		}

		// TODO : 나중에  확인
		void AddCustomResolution(EditorWindow gameView, int width, int height, string resolutionName) {
			// 게임 뷰의 해상도 목록을 가져옴
			System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			System.Reflection.MethodInfo GetGroup = gameViewType.GetMethod("GetGroup", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

			// 현재 해상도 그룹 가져오기 (예: Standalone, iOS, Android 등)
			System.Type gameViewSizeGroupEnumType = System.Type.GetType("UnityEditor.GameViewSizeGroupType,UnityEditor");
			System.Array arrGroupEnum = System.Enum.GetValues(gameViewSizeGroupEnumType);

			foreach (var type in arrGroupEnum) {
				// Standalone 그룹 타입을 예시로 설정
				if (type.ToString() == "Standalone") {
					// 해상도 그룹 가져오기
					System.Object group = GetGroup.Invoke(null, new object[] { type });

					// 해상도 리스트 추가 메서드 호출
					System.Type gameViewSizeType = System.Type.GetType("UnityEditor.GameViewSize,UnityEditor");
					System.Object newGameViewSize = System.Activator.CreateInstance(gameViewSizeType, 1, 1, width, height, resolutionName);
					System.Type getDisplayNamesType = group.GetType();
					System.Reflection.MethodInfo addCustomSize = getDisplayNamesType.GetMethod("AddCustomSize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
					addCustomSize.Invoke(group, new object[] { newGameViewSize });
					Debug.Log($"Added and selected custom resolution: {width}x{height}, Name: {resolutionName}");

					// 해상도 선택
					System.Reflection.MethodInfo setSize = gameViewType.GetMethod("SetSize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
					setSize.Invoke(gameView, new object[] { (int)getDisplayNamesType.GetMethod("GetCount").Invoke(group, null) - 1 });

					// 게임 뷰 새로고침
					gameView.Repaint();

					Debug.Log($"Added and selected custom resolution: {width}x{height}, Name: {resolutionName}");
				}
			}

			//// GameViewSize 클래스 가져오기
			//var gameViewSizeType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSize");
			//var ctor = gameViewSizeType.GetConstructor(new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });

			//// 새 해상도 추가
			//var newSize = ctor.Invoke(new object[] { 1, width, height, resolutionName });
			//var addCustomSizeMethod = group.GetType().GetMethod("AddCustomSize");
			//addCustomSizeMethod.Invoke(group, new object[] { newSize });

			//// GameView 업데이트
			//var getDisplayNamesMethod = group.GetType().GetMethod("GetDisplayNames");
			//var displayNames = (string[])getDisplayNamesMethod.Invoke(group, null);

			//var selectedSizeIndexProp = gameViewType.GetProperty("selectedSizeIndex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			//var setSizeIndexMethod = gameView.GetType().GetMethod("SetSize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			//setSizeIndexMethod.Invoke(gameView, new object[] { displayNames.Length - 1 });
		}
	}
}