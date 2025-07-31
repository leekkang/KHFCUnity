
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace KHFC.Editor {
	[CustomEditor(typeof(Graphic), true)]
	public class RaycastPaddingDebugger : UnityEditor.Editor {
		void OnSceneGUI() {
			Graphic graphic = (Graphic)target;

			if (!graphic.raycastTarget || !graphic.enabled || !graphic.gameObject.activeInHierarchy)
				return;

			RectTransform rt = graphic.rectTransform;
			Vector3[] corners = new Vector3[4];
			rt.GetWorldCorners(corners);

			Vector4 padding = graphic.raycastPadding;

			// World-space rect
			Rect baseRect = new Rect(corners[0], corners[2] - corners[0]);

			// Expand rect by padding (local space to world space conversion)
			Vector2 size = baseRect.size;
			Vector2 pivotOffset = rt.pivot;
			float scaleX = rt.lossyScale.x;
			float scaleY = rt.lossyScale.y;

			Rect paddedRect = new Rect(baseRect.x - padding.x * scaleX,
									   baseRect.y - padding.w * scaleY,
									   size.x + (padding.x + padding.z) * scaleX,
									   size.y + (padding.y + padding.w) * scaleY);

			// Draw original rect (green)
			//Handles.color = Color.green;
			//Handles.DrawSolidRectangleWithOutline(baseRect, new Color(0, 1, 0, 0.05f), Color.green);

			// Draw padded rect (red outline)
			Handles.color = Color.red;
			Handles.DrawSolidRectangleWithOutline(paddedRect, new Color(1, 0, 0, 0.05f), Color.red);
		}
	}
}