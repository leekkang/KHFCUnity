
using System;
using System.Collections.Generic;

public static class ListExt {
	/// <summary> Array.Copy를 호출하지 않는 제거함수 </summary>
	/// <remarks> Time Complexity : O(1) </remarks>
	public static bool RemoveAtBySwap<T>(this List<T> list, int index) {
		if (index < 0 || index >= list.Count)
			return false;
		list[index] = list[^1];		// c# 8.0 : Indices and har operator
		//list[index] = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return true;
	}

	/// <summary> Array.Copy를 호출하지 않는 제거함수 </summary>
	/// <remarks> Time Complexity : O(n) </remarks>
	public static bool RemoveBySwap<T>(this List<T> list, T item) {
		int index = list.IndexOf(item);
		return RemoveAtBySwap(list, index);
	}

	/// <summary> Array.Copy를 호출하지 않는 제거함수 </summary>
	/// <remarks> Time Complexity : O(n) </remarks>
	public static bool RemoveBySwap<T>(this List<T> list, Predicate<T> predicate) {
		int index = list.FindIndex(predicate);
		return RemoveAtBySwap(list, index);
	}

	/// <summary> 리스트를 <paramref name="size"/> 크기로 변경하고 값을 넣어준다 </summary>
	public static void Resize<T>(this List<T> list, int size) {
		list.Clear();
		list.Capacity = size;
		for (int i = 0; i < size; ++i) {
			list.Add(default);
		}
	}

	/// <summary> 리스트 내 값이 있으면 true, 없으면 false를 리턴 </summary>
	public static bool TryGetValue<T>(this List<T> list, out T value, Predicate<T> predicate) {
		int index = list.FindIndex(predicate);
		value = index > -1 ? list[index] : default;
		return index != -1;
	}

	/// <summary> 리스트를 섞는다. Fisher-Yates shuffle 방식 사용 </summary>
	public static void Shuffle<T>(this List<T> list, Random rnd = null) {
		rnd ??= new();
		for (int i = list.Count - 1; i > 1; --i) {
			int k = rnd.Next(i + 1);
			(list[k], list[i]) = (list[i], list[k]);
		}
	}

	/// <summary> 리스트를 <paramref name="size"/> 크기로 변경하고 값을 넣어준다 </summary>
	//public static void Resize<T>(this List<T> list, int size) where T : class {
	//	list.Clear();
	//	list.Capacity = size;
	//	for (int i = 0; i < size; ++i) {
	//		list.Add(null);
	//	}
	//}
}