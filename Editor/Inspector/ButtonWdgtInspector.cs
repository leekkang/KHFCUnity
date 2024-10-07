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

		protected override void OnEnable() {
			base.OnEnable();
			//m_Target = (ButtonWdgt)target;
			m_EnableHover = serializedObject.FindProperty("m_EnableHover");
			m_OnClickSound = serializedObject.FindProperty("m_OnClickSound");
			m_OnHoverSound = serializedObject.FindProperty("m_OnHoverSound");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_EnableHover);
			EditorGUILayout.PropertyField(m_OnClickSound);
			EditorGUILayout.PropertyField(m_OnHoverSound);
			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();

			//m_Target.m_OnClickSound = EditorGUILayout.Toggle("Enable ClickSound", m_Target.m_OnClickSound);
			//m_Target.m_OnHoverSound = EditorGUILayout.Toggle("Enable HoverSound", m_Target.m_OnHoverSound);
		}
	}
}