using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectEx {
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
		T component = obj.GetComponent<T> ();
		if (component != null) {
#if UNITY_EDITOR
			GameObject.DestroyImmediate(component, true);
#else
            GameObject.Destroy(component);
#endif
		}
	}

	public static GameObject FindRecursively(this GameObject parent, string name) {
		Transform item = parent.transform.FindRecursively(name);
		if (item) {
			return item.gameObject;
		}

		return null;
	}

	public static T SafeAddComponent<T>(this GameObject obj) where T : Component {
		T component = obj.GetComponent<T>();
		if (component == null)
			return obj.AddComponent<T>();

		return component;
	}

	public static Component SafeAddComponent(this GameObject obj, string name) {
		Component component = obj.GetComponent(name);

		if (component == null) {
			//component = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(obj, "Assets/script/Util/Extension/GameObjectEx.cs (74,8)", name);
			component = obj.AddComponent(System.Type.GetType(name));
		}

		return component;
	}

}
