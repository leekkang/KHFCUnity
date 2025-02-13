using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KHFC {
	[CustomEditor(typeof(ButtonWdgt), true)]
	[CanEditMultipleObjects]
	public class ButtonWdgtInspector : UnityEditor.UI.ButtonEditor {
		//ButtonWdgt m_Target;
		SerializedProperty m_EnableHover;
		SerializedProperty m_OnClickSound;
		SerializedProperty m_OnHoverSound;

		SerializedProperty m_ClickAllocated;
		SerializedProperty m_EnterAllocated;
		SerializedProperty m_ExitAllocated;

		protected override void OnEnable() {
			base.OnEnable();
			//m_Target = (ButtonWdgt)target;
			m_EnableHover = serializedObject.FindProperty("m_EnableHover");
			m_OnClickSound = serializedObject.FindProperty("m_OnClickSound");
			m_OnHoverSound = serializedObject.FindProperty("m_OnHoverSound");
			m_ClickAllocated = serializedObject.FindProperty("m_ClickAllocated");
			m_EnterAllocated = serializedObject.FindProperty("m_EnterAllocated");
			m_ExitAllocated = serializedObject.FindProperty("m_ExitAllocated");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_EnableHover);
			EditorGUILayout.PropertyField(m_OnClickSound);
			EditorGUILayout.PropertyField(m_OnHoverSound);
			EditorGUILayout.PropertyField(m_ClickAllocated);
			EditorGUILayout.PropertyField(m_EnterAllocated);
			EditorGUILayout.PropertyField(m_ExitAllocated);
			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();

			//m_Target.m_OnClickSound = EditorGUILayout.Toggle("Enable ClickSound", m_Target.m_OnClickSound);
			//m_Target.m_OnHoverSound = EditorGUILayout.Toggle("Enable HoverSound", m_Target.m_OnHoverSound);
		}
	}
}