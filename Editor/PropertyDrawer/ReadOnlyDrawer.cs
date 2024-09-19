using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		using (var scope = new EditorGUI.DisabledGroupScope(true)) {
			EditorGUI.PropertyField(position, property, label, true);
		}
	}
}

[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
public class ShowOnlyDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label) {
		string name = (attribute as ShowOnlyAttribute).name;
		if (string.IsNullOrEmpty(name))
			name = label.text;

		string value;
		switch (prop.propertyType) {
			case SerializedPropertyType.Integer:
				value = prop.intValue.ToString();
				break;
			case SerializedPropertyType.Boolean:
				value = prop.boolValue.ToString();
				break;
			case SerializedPropertyType.Float:
				value = prop.floatValue.ToString("0.00000");
				break;
			case SerializedPropertyType.String:
				value = prop.stringValue;
				break;
			case SerializedPropertyType.Vector2:
				value = prop.vector2Value.ToString();
				break;
			case SerializedPropertyType.Vector3:
				value = prop.vector3Value.ToString();
				break;
			case SerializedPropertyType.ObjectReference:
				try {
					value = prop.objectReferenceValue.ToString();
				} catch (NullReferenceException) {
					value = "None (Game Object)";
				}
				break;
			default:
				value = "(not supported)";
				break;
		}

		EditorGUI.LabelField(position, name, value);
	}
}