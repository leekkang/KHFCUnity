using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace KHFC {
	[CustomEditor(typeof(MonoBehaviour), true)]
	public class MonoBehaviourInspector : UnityEditor.Editor {
		[SerializeField] private List<MethodInfo> m_ListMethod;

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
				GUILayout.Space(10);
				if (GUILayout.Button(method.Name.SpacingUpperCase())) {
					method.Invoke(target, null);
				}
			}
		}

		private void CreateInspectorButton() {
			m_ListMethod = new List<MethodInfo>();

			Type type = target.GetType();
			MethodInfo[] arrMethod = type.GetMethods();

			int count = arrMethod.Length;
			for (int i = 0; i < count; i++) {
				MethodInfo method = arrMethod[i];
				object[] arrAttribute = method.GetCustomAttributes(typeof(InspectorButtonAttribute), false);

				int len = arrAttribute.Length;
				for (int j = 0; j < len; j++) {
					if (arrAttribute[j] is InspectorButtonAttribute) {
						//InspectorButtonAttribute att = (InspectorButtonAttribute)arrAttribute[j];
						//if (!string.IsNullOrEmpty(att.m_Msg))
						//	((InspectorButtonAttribute)arrAttribute[j]).m_Msg;
						m_ListMethod.Add(method);
					}
				}
			}
		}
	}

}
