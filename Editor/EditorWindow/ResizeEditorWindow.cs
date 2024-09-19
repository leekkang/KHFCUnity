using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;    //for Path
namespace KHFC {
	public class ResizeEditorWindow : EditorWindow {
		private static Vector2 m_Size = new Vector2(640, 1136);

		void OnSelectionChange() { Repaint(); }

		void OnEnable() {
		}

		void OnDisable() {
		}

		public static EditorWindow GetMainGameView() {
			System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");

			System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			System.Object Res = GetMainGameView.Invoke(null, null);

			return (EditorWindow)Res;
		}

		void OnGUI() {
			m_Size.x = EditorGUILayout.IntField("X", (int)m_Size.x);
			m_Size.y = EditorGUILayout.IntField("Y", (int)m_Size.y);

			if (GUILayout.Button("Set")) {
				EditorWindow gameView = GetMainGameView();
				Rect pos = gameView.position;
				pos.y = pos.y - 0;
				pos.width = m_Size.x;
				pos.height = m_Size.y + 17;
				gameView.position = pos;
			}
		}

	}
}
