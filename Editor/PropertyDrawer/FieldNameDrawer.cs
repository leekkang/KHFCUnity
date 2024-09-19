using System.Collections;
using UnityEditor;
using UnityEngine;

namespace KHFC {
	[CustomPropertyDrawer(typeof(FieldNameAttribute))]
	public class FieldNameDrawer : PropertyDrawer {
		// 자식을 가지고 있는 필드의 경우 제대로 표시해주기 위해 재정의가 필요하다.
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			//if (property.CountInProperty() == 0)
			label.text = (attribute as FieldNameAttribute).name;
			EditorGUI.PropertyField(position, property, label, true);
		}
	}
}