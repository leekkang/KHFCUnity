using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public static class TransformEx {
	/// <summary> 자식들 중 <paramref name="name"/> 이름을 가진 게임 오브젝트를 반환한다. (DFS) </summary>
	public static Transform FindRecursively(this Transform target, string name) {
		if (target.name == name)
			return target;

		foreach (Transform child in target) {
			Transform tr = child.FindRecursively(name);
			if (tr != null)
				return tr;
		}
		return null;
	}
	/// <summary> 자식들 중 <paramref name="name"/> 이름을 가진 게임 오브젝트를 반환한다. (BFS) </summary>
	public static Transform FindRecursivelyB(this Transform target, string name) {
		if (target.name == name)
			return target;

		List<Transform> listTmp = new() { target }; // c# 9.0: Target-typed new expressions
		while (listTmp.Count > 0) {
			int count = listTmp.Count;
			Transform[] arrParent = new Transform[count];
			listTmp.CopyTo(arrParent, 0);
			listTmp.Clear();

			// 현재 depth의 모든 transform을 돌면서 이름 비교
			for (int i = 0; i < count; ++i) {
				foreach (Transform child in arrParent[i]) {
					if (child.name == name)
						return child;
					listTmp.Add(child);
				}
			}
		}
		return null;
	}

	/// <summary> 자식들 중 해당 컴포넌트를 가진 게임 오브젝트의 컴포넌트를 반환한다. (DFS) </summary>
	public static T FindRecursively<T>(this Transform target) where T : Component {
		if (target.TryGetComponent<T>(out T comp))
			return comp;

		foreach (Transform child in target) {
			comp = child.FindRecursively<T>();
			if (comp != null)
				return comp;
		}
		return null;
	}
	/// <summary> 자식들 중 해당 컴포넌트를 가진 게임 오브젝트의 컴포넌트를 반환한다. (BFS) </summary>
	public static T FindRecursivelyB<T>(this Transform parent) where T : Component {
		if (parent.TryGetComponent(out T comp))
			return comp;

		List<Transform> listTmp = new() { parent }; // c# 9.0: Target-typed new expressions
		while (listTmp.Count > 0) {
			int count = listTmp.Count;
			Transform[] arrParent = new Transform[count];
			listTmp.CopyTo(arrParent, 0);
			listTmp.Clear();

			// 현재 depth의 모든 transform을 돌면서 이름 비교
			for (int i = 0; i < count; ++i) {
				foreach (Transform child in arrParent[i]) {
					if (child.TryGetComponent(out comp))
						return comp;
					listTmp.Add(child);
				}
			}
		}
		return null;
	}

	/// <summary> 자식들 중 <paramref name="name"/> 으로 시작하는 게임 오브젝트 어레이를 반환한다. </summary>
	public static Transform[] FindChildren(this Transform parent, string name) {
		List<Transform> listChild = new();

		foreach (Transform child in parent) {
			if (child.gameObject.name.StartsWith(name))
				listChild.Add(child);
		}

		return listChild.ToArray();
	}

	/// <summary> 자식들 중 <paramref name="name"/> 이름인 게임 오브젝트를 반환한다. </summary>
	public static Transform FindChild(this Transform parent, string name) {
		foreach (Transform child in parent) {
			if (null == child)
				continue;

			if (child.gameObject.name == name)
				return child;
		}

		return null;
	}

	/// <summary> Depth-First Search 순서로 <paramref name="action"/>을 실행한다. </summary>
	public static void DoRecursively(this Transform root, System.Action<Transform> action) {
		action(root);
		foreach (Transform child in root)
			child.DoRecursively(action);
	}

	/// <summary> Breadth-First Search 순서로 <paramref name="action"/>을 실행한다. </summary>
	public static void DoRecursivelyB(this Transform root, System.Action<Transform> action) {
		action(root);

		List<Transform> listTmp = new() { root }; // c# 9.0: Target-typed new expressions
		while (listTmp.Count > 0) {
			int count = listTmp.Count;
			Transform[] arrParent = new Transform[count];
			listTmp.CopyTo(arrParent, 0);
			listTmp.Clear();

			// 현재 depth의 모든 transform을 돌면서 이름 비교
			for (int i = 0; i < count; ++i) {
				foreach (Transform child in arrParent[i]) {
					action(child);
					listTmp.Add(child);
				}
			}
		}
	}

	public static string GetFullName(this Transform target) {
		var list = new Stack<string>();

		while (target != null) {
			list.Push(target.name);
			target = target.parent;
		}

		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		while (list.Count > 0)
			sb.AppendFormat("/{0}", list.Pop());

		return sb.ToString();
	}

	/// <summary>
	/// Obsolete : transform.Find 사용
	/// </summary>
	public static Transform GetTransformByFullName(this Transform target, string fullName) {
		int current = fullName.IndexOf(@"/");

		if (current == -1) {
			Transform tr = target.Find(fullName);

			if (null == tr) {
				Debug.LogError("Can't Find Transfrom by FullName : " + fullName);
				return null;
			}
			return tr;
		}

		string gameObjectName = fullName[..current];	// c# 8.0 : Range operator
		//string gameObjectName = fullName.Substring(0, current);

		fullName = fullName.Substring(current + 1, fullName.Length - current - 1);

		if (target.name == gameObjectName)
			return target.GetTransformByFullName(fullName);

		Transform child = target.Find(gameObjectName);

		if (child == null) {
			Debug.LogError("Can't Find Transfrom by FullName : " + fullName);
			return null;
		}

		return child.GetTransformByFullName(fullName);
	}

	public static void SetPosX(this Transform tr, float x) {
		Vector3 pos = tr.position;
		pos.x = x;
		tr.position = pos;
	}
	public static void SetPosY(this Transform tr, float y) {
		Vector3 pos = tr.position;
		pos.y = y;
		tr.position = pos;
	}
	public static void SetPosZ(this Transform tr, float z) {
		Vector3 pos = tr.position;
		pos.z = z;
		tr.position = pos;
	}
	public static void SetLocalPosX(this Transform tr, float x) {
		Vector3 pos = tr.localPosition;
		pos.x = x;
		tr.localPosition = pos;
	}
	public static void SetLocalPosY(this Transform tr, float y) {
		Vector3 pos = tr.localPosition;
		pos.y = y;
		tr.localPosition = pos;
	}
	public static void SetLocalPosZ(this Transform tr, float z) {
		Vector3 pos = tr.localPosition;
		pos.z = z;
		tr.localPosition = pos;
	}
	public static void AddPosX(this Transform tr, float x) {
		Vector3 pos = tr.position;
		pos.x += x;
		tr.position = pos;
	}

	public static void AddPosY(this Transform tr, float y) {
		Vector3 pos = tr.position;
		pos.y += y;
		tr.position = pos;
	}
	public static void AddPosZ(this Transform tr, float z) {
		Vector3 pos = tr.position;
		pos.z += z;
		tr.position = pos;
	}
	public static void AddLocalPosX(this Transform tr, float x) {
		Vector3 pos = tr.localPosition;
		pos.x += x;
		tr.localPosition = pos;
	}
	public static void AddLocalPosY(this Transform tr, float y) {
		Vector3 pos = tr.localPosition;
		pos.y += y;
		tr.localPosition = pos;
	}
	public static void AddLocalPosZ(this Transform tr, float z) {
		Vector3 pos = tr.localPosition;
		pos.z += z;
		tr.localPosition = pos;
	}
	public static void SetScaleX(this Transform tr, float x) {
		Vector3 scale = tr.localScale;
		scale.x = x;
		tr.localScale = scale;
	}
	public static void SetScaleY(this Transform tr, float y) {
		Vector3 scale = tr.localScale;
		scale.y = y;
		tr.localScale = scale;
	}
	public static void SetScaleZ(this Transform tr, float z) {
		Vector3 scale = tr.localScale;
		scale.z = z;
		tr.localScale = scale;
	}

	public static void SetPosX(this RectTransform tr, float x) {
		Vector2 pos = tr.anchoredPosition;
		pos.x = x;
		tr.anchoredPosition = pos;
	}
	public static void SetPosY(this RectTransform tr, float y) {
		Vector2 pos = tr.anchoredPosition;
		pos.y = y;
		tr.anchoredPosition = pos;
	}
	public static void SetPosZ(this RectTransform tr, float z) {
		Vector3 pos = tr.anchoredPosition3D;
		pos.z = z;
		tr.anchoredPosition3D = pos;
	}

	public static void AddPosX(this RectTransform tr, float x) {
		Vector2 pos = tr.anchoredPosition;
		pos.x += x;
		tr.anchoredPosition = pos;
	}
	public static void AddPosY(this RectTransform tr, float y) {
		Vector2 pos = tr.anchoredPosition;
		pos.y += y;
		tr.anchoredPosition = pos;
	}
	public static void AddPosZ(this RectTransform tr, float z) {
		Vector3 pos = tr.anchoredPosition3D;
		pos.z += z;
		tr.anchoredPosition3D = pos;
	}

	public static void SetWidth(this RectTransform tr, float width) {
		Vector2 size = tr.sizeDelta;
		size.x = width;
		tr.sizeDelta = size;
	}
	public static void SetHeight(this RectTransform tr, float height) {
		Vector2 size = tr.sizeDelta;
		size.y = height;
		tr.sizeDelta = size;
	}

	public static void AddWidth(this RectTransform tr, float width) {
		Vector2 size = tr.sizeDelta;
		size.x += width;
		tr.sizeDelta = size;
	}
	public static void AddHeight(this RectTransform tr, float height) {
		Vector2 size = tr.sizeDelta;
		size.y += height;
		tr.sizeDelta = size;
	}
}
