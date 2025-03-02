
using UnityEngine;
using UnityEditor;

namespace KHFC.Editor {
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MotionBase))]
	public class MotionBaseInspector : UnityEditor.Editor {
		MotionBase m_Target;
		public void OnEnable() {
			m_Target = target as MotionBase;
		}
		bool test;

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			serializedObject.Update();

			EditorGUILayout.BeginVertical(EditorStyles.textArea);
			GUILayout.Label("TEst");
			test = EditorGUILayout.Toggle("One_MatSystem", test);
			//GUILayout.Label("★각 파티클의 Textures Sheet Animation의 X,Y값 세팅 필요★");
			//if (ParticleMgr.m_One_MatSystem) {
			//	EditorGUILayout.BeginVertical(EditorStyles.textArea);
			//	GUILayout.Label("전체 갯수");
			//	EditorGUI.indentLevel++;
			//	ParticleMgr.m_ImageTotalCnt = EditorGUILayout.IntField("ImageTotalCnt", ParticleMgr.m_ImageTotalCnt);
			//	EditorGUI.indentLevel--;
			//	EditorGUILayout.EndVertical();

			//	EditorGUILayout.BeginVertical(EditorStyles.textArea);
			//	GUILayout.Label("한묶음의 이미지 갯수, 주의!! 구현상으로 접근할 인덱스간격으로 세팅");
			//	EditorGUI.indentLevel++;
			//	ParticleMgr.m_ImageBundleCnt = EditorGUILayout.IntField("ImageBundleCnt", ParticleMgr.m_ImageBundleCnt);
			//	EditorGUI.indentLevel--;
			//	EditorGUILayout.EndVertical();

			//	EditorGUILayout.BeginVertical(EditorStyles.textArea);
			//	GUILayout.Label("파티클 하나당 이미지를 랜덤하게 출력");
			//	EditorGUI.indentLevel++;
			//	ParticleMgr.m_ImageRandom = EditorGUILayout.Toggle("ImageRandom", ParticleMgr.m_ImageRandom);
			//	if (ParticleMgr.m_ImageRandom) {
			//		EditorGUILayout.BeginVertical(EditorStyles.textArea);
			//		GUILayout.Label("랜덤범위");
			//		ParticleMgr.m_ImageRandomCnt = EditorGUILayout.IntField("ImageRandomCnt", ParticleMgr.m_ImageRandomCnt);
			//		EditorGUILayout.EndVertical();
			//	}
			//	EditorGUILayout.EndVertical();
			//}

			//if (GUILayout.Button("Update Sprite List")) {
			//	m_Source.UpdateSpriteList();
			//}
			EditorGUILayout.EndVertical();
		}
	}
}