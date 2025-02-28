using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KHFC {
	[CustomEditor(typeof(ButtonWdgt), true)]
	[CanEditMultipleObjects]
	public class ButtonWdgtInspector : UnityEditor.UI.SelectableEditor {
		//ButtonWdgt m_Target;
		SerializedProperty m_EnableHover;
		SerializedProperty m_OnClickSound;
		SerializedProperty m_OnHoverSound;

		SerializedProperty m_ClickFuncName;
		SerializedProperty m_EnterFuncName;
		//SerializedProperty m_ExitFuncName;

		SerializedProperty m_ClickSoundName;
		SerializedProperty m_ClickSoundVolume;
		SerializedProperty m_HoverSoundName;
		SerializedProperty m_HoverSoundVolume;

		bool m_HoverState;

		protected override void OnEnable() {
			base.OnEnable();
			//m_Target = (ButtonWdgt)target;
			m_EnableHover = serializedObject.FindProperty("m_EnableHover");
			m_OnClickSound = serializedObject.FindProperty("m_OnClickSound");
			m_OnHoverSound = serializedObject.FindProperty("m_OnHoverSound");
			m_ClickFuncName = serializedObject.FindProperty("m_ClickFuncName");
			m_EnterFuncName = serializedObject.FindProperty("m_EnterFuncName");
			//m_ExitFuncName = serializedObject.FindProperty("m_ExitFuncName");

			m_ClickSoundName = serializedObject.FindProperty("m_ClickSoundName");
			m_ClickSoundVolume = serializedObject.FindProperty("m_ClickSoundVolume");
			m_HoverSoundName = serializedObject.FindProperty("m_HoverSoundName");
			m_HoverSoundVolume = serializedObject.FindProperty("m_HoverSoundVolume");

			m_HoverState = m_EnableHover.boolValue;
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_ClickFuncName);
			EditorGUILayout.PropertyField(m_OnClickSound);
			if (m_OnClickSound.boolValue) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_ClickSoundName);
				EditorGUILayout.PropertyField(m_ClickSoundVolume);
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.PropertyField(m_EnableHover);
			bool hoverState = m_EnableHover.boolValue;
			if (hoverState) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_EnterFuncName);
				//EditorGUILayout.PropertyField(m_ExitFuncName);
				EditorGUILayout.PropertyField(m_OnHoverSound);
				if (m_OnHoverSound.boolValue) {
					++EditorGUI.indentLevel;
					EditorGUILayout.PropertyField(m_HoverSoundName);
					EditorGUILayout.PropertyField(m_HoverSoundVolume);
					--EditorGUI.indentLevel;
				}
				--EditorGUI.indentLevel;
			}
			serializedObject.ApplyModifiedProperties();
			if (!m_HoverState && hoverState) {
				((ButtonWdgt)target).AllocHoverFunc();
				// 외부에서 값을 수정하는 경우 업데이트를 해줘야 한다.
				serializedObject.Update();
				serializedObject.ApplyModifiedProperties();
			}
			m_HoverState = hoverState;

			EditorGUILayout.Space(12f);
			base.OnInspectorGUI();

			//m_Target.m_OnClickSound = EditorGUILayout.Toggle("Enable ClickSound", m_Target.m_OnClickSound);
			//m_Target.m_OnHoverSound = EditorGUILayout.Toggle("Enable HoverSound", m_Target.m_OnHoverSound);
		}
	}
}