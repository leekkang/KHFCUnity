using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if KHFC_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

class ProfileVariables {
	public int count;
	public List<string> names;
	public List<string> values;
	public List<string> editorValues;

	public ProfileVariables(List<string> names) {
		this.count = names.Count;
		this.names = names;
		values = new List<string>(count);
		editorValues = new List<string>(count);
	}

	public override string ToString() {
		string message = string.Empty;
		for (int i = 0; i < count; i++) {
			message += $"{names[i]} = '{values[i]}' -> '{editorValues[i]}'\n";
		}

		return message;
	}
}

public class ProfileVariableTester {
#if KHFC_ADDRESSABLES
	[MenuItem("KHFC/Addressable/Test Profile Variable")]
	private static void TestProfileVariable() {
		AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
		AddressableAssetProfileSettings profileSettings = addressableAssetSettings.profileSettings;
		string activeProfileID = addressableAssetSettings.activeProfileId;

		var variables = new ProfileVariables(profileSettings.GetVariableNames());

		for (int i = 0; i < variables.count; i++) {
			string variableName = variables.names[i];
			var value = profileSettings.GetValueByName(activeProfileID, variableName);
			variables.values.Add(value);

			var editorValue = profileSettings.EvaluateString(activeProfileID, value);
			variables.editorValues.Add(editorValue);
		}
		Debug.Log(variables);
	}
#endif
}