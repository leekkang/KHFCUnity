using System;
using UnityEngine;
using UnityEditor;

namespace KHFC.Editor {
	[CustomEditor(typeof(AssetLinkData), true)]
	public class AssetLinkDataInspector : UnityEditor.Editor {

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if (Application.isBatchMode)
				return;

			GUILayout.Space(10);
			if (GUILayout.Button("Update All Link List")) {
				KHFCEditorMenu.FillAssetLinkData();
			}
		}
	}
}