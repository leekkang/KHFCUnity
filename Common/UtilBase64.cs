
namespace KHFC {
	internal partial class Util {
		public static string Encoding(string text, System.Text.Encoding encodeType = null) {
			encodeType ??= System.Text.Encoding.UTF8;
			byte[] bytes = encodeType.GetBytes(text);
			return System.Convert.ToBase64String(bytes);
		}

		public static string Decoding(string text, System.Text.Encoding encodeType = null) {
			encodeType ??= System.Text.Encoding.UTF8;
			byte[] bytes = System.Convert.FromBase64String(text);
			return encodeType.GetString(bytes);
		}
	}
}