#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;


public class RectTransformDrawer {
	public enum DrawType { All, SelectedWithChild, Selected, Disable }

	private static DrawType m_Type = DrawType.SelectedWithChild;
	private static Color32 m_Color = new Color32(204, 108, 231, 255);

	private const string DrawTypePrefsKey = "RectTransformDrawer_Type";

	[InitializeOnLoadMethod]
	private static void UpdateOnLoadMethod() {
		if (EditorPrefs.HasKey(DrawTypePrefsKey)) {
			int index = EditorPrefs.GetInt(DrawTypePrefsKey);
			m_Type = (DrawType)index;
		}
	}


	[DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
	static void DrawGizmosAll(RectTransform rect, GizmoType gizmoType) {
		if (m_Type == DrawType.All)
			DrawRect(rect);
	}
	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active)]
	static void DrawGizmosSelectedWithChild(RectTransform rect, GizmoType gizmoType) {
		if (m_Type == DrawType.SelectedWithChild)
			DrawRect(rect);
	}
	[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
	static void DrawGizmosSelected(RectTransform rect, GizmoType gizmoType) {
		if (m_Type == DrawType.Selected)
			DrawRect(rect);
	}

	private static void DrawRect(RectTransform rect) {
		Color origin = Gizmos.color;
		Gizmos.color = m_Color;

		Vector3[] corners = new Vector3[4];
		rect.GetWorldCorners(corners);
		//rect.GetLocalCorners(corners);
		Rect globalRect = new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);

		Gizmos.DrawWireCube(globalRect.center, globalRect.size);
		Gizmos.color = origin;
	}



	[MenuItem("Tools/RectTransformDrawer/Draw All", priority = 1)]
	private static void SetModeToAll() {
		m_Type = DrawType.All;
		EditorPrefs.SetInt(DrawTypePrefsKey, (int)m_Type);
		SceneView.RepaintAll();
	}

	[MenuItem("Tools/RectTransformDrawer/Draw Selected with Child", priority = 2)]
	private static void SetModeToSelectedWithChild() {
		m_Type = DrawType.SelectedWithChild;
		EditorPrefs.SetInt(DrawTypePrefsKey, (int)m_Type);
		SceneView.RepaintAll();
	}

	[MenuItem("Tools/RectTransformDrawer/Draw Selected", priority = 3)]
	private static void SetModeToSelected() {
		m_Type = DrawType.Selected;
		EditorPrefs.SetInt(DrawTypePrefsKey, (int)m_Type);
		SceneView.RepaintAll();
	}

	[MenuItem("Tools/RectTransformDrawer/Disable", priority = 4)]
	private static void SetModeToDisable() {
		m_Type = DrawType.Disable;
		EditorPrefs.SetInt(DrawTypePrefsKey, (int)m_Type);
		SceneView.RepaintAll();
	}
}

#endif
