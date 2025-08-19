
using UnityEngine;
using UnityEditor;

namespace KHFC.Editor {
	[CustomEditor(typeof(UnityEngine.UI.Image), true)]
	[CanEditMultipleObjects]
	public class ImageExInspector : UnityEditor.UI.ImageEditor {
		static readonly Vector2[] presetPositions = new Vector2[] {
			Vector2.zero, new Vector2(0.5f, 0), new Vector2(1, 0),
			new Vector2(0, 1), new Vector2(0.5f, 1), Vector2.one
		};

		bool flipX;
		bool flipY;

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			UnityEngine.UI.Image image = (UnityEngine.UI.Image)target;
			RectTransform rect = image.rectTransform;

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Flip Controls", EditorStyles.boldLabel);

			// 현재 상태 기준으로 토글 초기화
			flipX = rect.localScale.x < 0;
			flipY = rect.localScale.y < 0;

			++EditorGUI.indentLevel;
			EditorGUILayout.BeginHorizontal();
			bool newFlipX = EditorGUILayout.ToggleLeft("Flip X", flipX, GUILayout.Width(100));
			bool newFlipY = EditorGUILayout.ToggleLeft("Flip Y", flipY, GUILayout.Width(100));
			EditorGUILayout.EndHorizontal();
			--EditorGUI.indentLevel;

			// 상태 변화 감지 후 스케일 반전
			if (newFlipX != flipX) {
				Undo.RecordObject(rect, "Flip X");
				Vector3 scale = rect.localScale;
				scale.x *= -1;
				rect.localScale = scale;
				EditorUtility.SetDirty(rect);
			}

			if (newFlipY != flipY) {
				Undo.RecordObject(rect, "Flip Y");
				Vector3 scale = rect.localScale;
				scale.y *= -1;
				rect.localScale = scale;
				EditorUtility.SetDirty(rect);
			}

			EditorGUILayout.LabelField("Preset RectTransform Pivot And Anchor", EditorStyles.boldLabel);

			++EditorGUI.indentLevel;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Pivot X ", EditorStyles.boldLabel, GUILayout.Width(80));
			if (GUILayout.Button("Left", GUILayout.Width(150))) SetRectTransformPivot(rect, 0f, true);
			if (GUILayout.Button("Center", GUILayout.Width(150))) SetRectTransformPivot(rect, 0.5f, true);
			if (GUILayout.Button("Right", GUILayout.Width(150))) SetRectTransformPivot(rect, 1f, true);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Pivot Y ", EditorStyles.boldLabel, GUILayout.Width(80));
			if (GUILayout.Button("Top", GUILayout.Width(150))) SetRectTransformPivot(rect, 1f, false);
			if (GUILayout.Button("Center", GUILayout.Width(150))) SetRectTransformPivot(rect, 0.5f, false);
			if (GUILayout.Button("Bottom", GUILayout.Width(150))) SetRectTransformPivot(rect, 0f, false);
			EditorGUILayout.EndHorizontal();
			--EditorGUI.indentLevel;

			EditorGUILayout.Space(8f);

			++EditorGUI.indentLevel;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Anchor X ", EditorStyles.boldLabel, GUILayout.Width(80));
			if (GUILayout.Button("Left", GUILayout.Width(150))) SetRectTransformAnchor(rect, 0f, true);
			if (GUILayout.Button("Center", GUILayout.Width(150))) SetRectTransformAnchor(rect, 0.5f, true);
			if (GUILayout.Button("Right", GUILayout.Width(150))) SetRectTransformAnchor(rect, 1f, true);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Anchor Y ", EditorStyles.boldLabel, GUILayout.Width(80));
			if (GUILayout.Button("Top", GUILayout.Width(150))) SetRectTransformAnchor(rect, 1f, false);
			if (GUILayout.Button("Center", GUILayout.Width(150))) SetRectTransformAnchor(rect, 0.5f, false);
			if (GUILayout.Button("Bottom", GUILayout.Width(150))) SetRectTransformAnchor(rect, 0f, false);
			EditorGUILayout.EndHorizontal();
			--EditorGUI.indentLevel;

			EditorGUILayout.Space(12f);
		}

		void SetRectTransformPivot(RectTransform rect, float pivotVal, bool isXLine) {
			Undo.RecordObject(rect, "Set RectTransform Pivot");

			if (isXLine) {
				Vector2 tmp = rect.pivot;
				tmp.x = pivotVal;
				rect.pivot = tmp;

				tmp = rect.anchoredPosition;
				tmp.x += (pivotVal - rect.pivot.x) * rect.sizeDelta.x;
				rect.anchoredPosition = tmp;
			} else {
				Vector2 tmp = rect.pivot;
				tmp.y = pivotVal;
				rect.pivot = tmp;

				tmp = rect.anchoredPosition;
				tmp.y += (pivotVal - rect.pivot.y) * rect.sizeDelta.y;
				rect.anchoredPosition = tmp;
			}

			EditorUtility.SetDirty(rect);
		}

		void SetRectTransformAnchor(RectTransform rect, float anchorVal, bool isXLine) {
			Undo.RecordObject(rect, "Set RectTransform Anchors");

			if (isXLine) {
				Vector2 tmp = rect.anchorMin;
				tmp.x = anchorVal;
				rect.anchorMin = tmp;
				tmp = rect.anchorMax;
				tmp.x = anchorVal;
				rect.anchorMax = tmp;
			} else {
				Vector2 tmp = rect.anchorMin;
				tmp.y = anchorVal;
				rect.anchorMin = tmp;
				tmp = rect.anchorMax;
				tmp.y = anchorVal;
				rect.anchorMax = tmp;
			}

			// 위치 0으로 초기화
			Vector2 pos = rect.anchoredPosition;
			if (isXLine)	pos.x = 0f;
			else			pos.y = 0f;
			rect.anchoredPosition = pos;

			EditorUtility.SetDirty(rect);
		}
	}
}