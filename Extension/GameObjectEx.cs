
using UnityEngine;

public static class GameObjectEx {
	public static void ToggleEnable(this Behaviour behaviour) {
		if (behaviour != null)
			behaviour.enabled = !behaviour.enabled;
	}

	public static void ToggleActive(this GameObject obj) {
		if (obj != null)
			obj.SetActive(!obj.activeSelf);
	}

	static void ChangeLayerRecursivleyInternal(this GameObject obj, int layer) {
		foreach (Transform child in obj.transform) {
			child.gameObject.layer = layer;
			child.gameObject.ChangeLayerRecursivleyInternal(layer);
		}
	}
	public static void ChangeLayerRecursivley(this GameObject obj, int layer) {
		obj.layer = layer;
		obj.ChangeLayerRecursivleyInternal(layer);
	}


	static void ChangeLayerRecursivleyInternal(this GameObject obj, string layerName) {
		foreach (Transform child in obj.transform) {
			child.gameObject.layer = LayerMask.NameToLayer(layerName);
			child.gameObject.ChangeLayerRecursivleyInternal(layerName);
		}
	}
	public static void ChangeLayerRecursivley(this GameObject obj, string layerName) {
		obj.layer = LayerMask.NameToLayer(layerName);
		obj.ChangeLayerRecursivleyInternal(layerName);
	}

	public static void RemoveComponent<T>(this GameObject obj) where T : Component {
		if (obj.TryGetComponent<T>(out var component)) {
#if UNITY_EDITOR
			GameObject.DestroyImmediate(component, true);
#else
			GameObject.Destroy(component);
#endif
		}
	}

	public static GameObject FindRecursively(this GameObject parent, string name, bool depthFirst = true) {
		Transform item = depthFirst ? parent.transform.FindRecursively(name) :
									  parent.transform.FindRecursivelyB(name);
		return item ? item.gameObject : null;
	}

	public static bool TryFindComponent<T>(this GameObject parent, out T comp, bool depthFirst = true) where T : Component {
		comp = depthFirst ? parent.transform.FindRecursively<T>() :
							parent.transform.FindRecursivelyB<T>();
		return comp != null;
	}

	public static T SafeAddComponent<T>(this GameObject obj) where T : Component {
		if (!obj.TryGetComponent(out T comp))
			return obj.AddComponent<T>();

		return comp;
	}

	public static Component SafeAddComponent(this GameObject obj, string name) {
		//Component comp = obj.GetComponent(name) ?? obj.AddComponent(System.Type.GetType(name));
		Component comp = obj.GetComponent(name);
		if (comp == null) {
			//comp = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(obj, "Assets/script/Util/Extension/GameObjectEx.cs (74,8)", name);
			comp = obj.AddComponent(System.Type.GetType(name));
		}

		return comp;
	}
}