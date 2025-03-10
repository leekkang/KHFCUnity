
using System.Text;

public static class StringEx {
	/// <summary> 대문자를 기준으로 공백을 넣은 문자열로 변경해주는 함수 </summary>
	public static string SpacingUpperCase(this string value) {
		if (string.IsNullOrEmpty(value))
			return "";

		StringBuilder sb = new();   // c# 9.0: Target-typed new expressions
		sb.Append(value[0]);

		int length = value.Length;
		for (int i = 1; i < length; i++) {
			if (char.IsUpper(value[i]))
				sb.Append(" ");

			sb.Append(value[i]);
		}

		return sb.ToString();
	}

	/// <summary> 처음 나오는 <paramref name="pattern"/>과 일치하는 문자열을 제거하는 함수 </summary>
	public static string RemoveFirst(this string origin, string pattern) {
		int startIndex = origin.IndexOf(pattern);
		return origin[..startIndex] + origin[(startIndex + pattern.Length)..];	// c# 8.0 : Range operator
	}


	/// <summary> 문자열을 Enum으로 변경해주는 함수 </summary>
	public static bool TryConvertToEnum<T>(this string str, out T value) where T : System.Enum {
		if (!System.Enum.IsDefined(typeof(T), str)) {
			value = default;
			return false;
		}
		value = (T)System.Enum.Parse(typeof(T), str);
		return true;
	}
}
