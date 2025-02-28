using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KHFC {
	[CustomEditor(typeof(TooltipWdgt), true)]
	[CanEditMultipleObjects]
	public class TooltipWdgtWdgtInspector : UnityEditor.UI.ButtonEditor {
		//ButtonWdgt m_Target;
		SerializedProperty m_OnHoverSound;
		SerializedProperty m_EnterAllocated;
		SerializedProperty m_ExitAllocated;

		SerializedProperty m_HoverSoundName;
		SerializedProperty m_HoverSoundVolume;

		protected override void OnEnable() {
			base.OnEnable();
			//m_Target = (ButtonWdgt)target;
			m_OnHoverSound = serializedObject.FindProperty("m_OnHoverSound");
			m_EnterAllocated = serializedObject.FindProperty("m_EnterAllocated");
			m_ExitAllocated = serializedObject.FindProperty("m_ExitAllocated");

			m_HoverSoundName = serializedObject.FindProperty("m_HoverSoundName");
			m_HoverSoundVolume = serializedObject.FindProperty("m_HoverSoundVolume");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_OnHoverSound);
			EditorGUILayout.PropertyField(m_EnterAllocated);
			EditorGUILayout.PropertyField(m_ExitAllocated);
			if (m_OnHoverSound.boolValue) {
				EditorGUILayout.PropertyField(m_HoverSoundName);
				EditorGUILayout.PropertyField(m_HoverSoundVolume);
			}
			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();

			//m_Target.m_OnClickSound = EditorGUILayout.Toggle("Enable ClickSound", m_Target.m_OnClickSound);
			//m_Target.m_OnHoverSound = EditorGUILayout.Toggle("Enable HoverSound", m_Target.m_OnHoverSound);
		}
	}
}