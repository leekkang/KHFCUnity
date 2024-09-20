using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace KHFC {
	[CustomEditor(typeof(MonoBehaviour), true)]
	public class MonoBehaviourInspector : UnityEditor.Editor {
		[SerializeField] private List<MethodInfo> m_ListMethod;
		[SerializeField] private List<string> m_ListInfo;

		private void OnEnable() {
			CreateInspectorButton();
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if (Application.isBatchMode)
				return;

			int methodCount = m_ListMethod.Count;
			for (int i = 0; i < methodCount; i++) {
				var method = m_ListMethod[i];
				GUILayout.Space(12);
				if (!string.IsNullOrEmpty(m_ListInfo[i]))
					//EditorGUILayout.LabelField(m_ListInfo[i], GUILayout.MinHeight(12));
					EditorGUILayout.HelpBox(m_ListInfo[i], MessageType.Info);
				if (GUILayout.Button(method.Name.SpacingUpperCase(), GUILayout.MinHeight(24))) {
					method.Invoke(target, null);
				}
			}
		}

		private void CreateInspectorButton() {
			m_ListMethod = new List<MethodInfo>();
			m_ListInfo = new List<string>();

			Type type = target.GetType();
			MethodInfo[] arrMethod = type.GetMethods();

			int count = arrMethod.Length;
			for (int i = 0; i < count; i++) {
				MethodInfo method = arrMethod[i];
				object[] arrAttribute = method.GetCustomAttributes(typeof(InspectorButtonAttribute), false);

				int len = arrAttribute.Length;
				for (int j = 0; j < len; j++) {
					if (arrAttribute[j] is InspectorButtonAttribute) {
						m_ListMethod.Add(method);
						m_ListInfo.Add(((InspectorButtonAttribute)arrAttribute[j]).m_Msg);
					}
				}
			}
		}
	}

}
