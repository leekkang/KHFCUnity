using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KHFC.Editor {
	[CustomEditor(typeof(TooltipWdgt), true)]
	[CanEditMultipleObjects]
	public class TooltipWdgtInspector : UnityEditor.UI.SelectableEditor {
		//ButtonWdgt m_Source;
		SerializedProperty m_EnterFuncName;
		SerializedProperty m_ExitFuncName;

		SerializedProperty m_OnHoverSound;
		SerializedProperty m_HoverSoundName;
		SerializedProperty m_HoverSoundVolume;

		protected override void OnEnable() {
			base.OnEnable();
			//m_Source = (ButtonWdgt)target;
			m_EnterFuncName = serializedObject.FindProperty("m_EnterFuncName");
			m_ExitFuncName = serializedObject.FindProperty("m_ExitFuncName");

			m_OnHoverSound = serializedObject.FindProperty("m_OnHoverSound");
			m_HoverSoundName = serializedObject.FindProperty("m_HoverSoundName");
			m_HoverSoundVolume = serializedObject.FindProperty("m_HoverSoundVolume");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_EnterFuncName);
			EditorGUILayout.PropertyField(m_ExitFuncName);

			EditorGUILayout.PropertyField(m_OnHoverSound);
			if (m_OnHoverSound.boolValue) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_HoverSoundName);
				EditorGUILayout.PropertyField(m_HoverSoundVolume);
				--EditorGUI.indentLevel;
			}
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space(12f);
			base.OnInspectorGUI();

			//m_Source.m_OnClickSound = EditorGUILayout.Toggle("Enable ClickSound", m_Source.m_OnClickSound);
			//m_Source.m_OnHoverSound = EditorGUILayout.Toggle("Enable HoverSound", m_Source.m_OnHoverSound);
		}
	}
}