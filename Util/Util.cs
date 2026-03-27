

namespace KHFC {
	internal partial class Util {
		/// <summary> <paramref name="cam"/> 카메라에 비치는 2d좌표를 월드좌표로 변경 </summary>
		public static UnityEngine.Vector3 UIToRealPos(UnityEngine.Vector2 uiPos, UnityEngine.Camera cam = null) {
			cam = cam != null ? cam : UnityEngine.Camera.main;
			return cam.ScreenToWorldPoint(new UnityEngine.Vector3(uiPos.x, uiPos.y, (cam.nearClipPlane + cam.farClipPlane) * 0.5f));
		}

		/// <summary> 월드좌표를 <paramref name="cam"/> 카메라에 비치는 2d좌표로 변경 </summary>
		public static UnityEngine.Vector2 RealPosToUI(UnityEngine.Vector3 realPos, UnityEngine.Camera cam = null) {
			cam = cam != null ? cam : UnityEngine.Camera.main;
			return cam.WorldToScreenPoint(realPos);
		}

		public static UnityEngine.Color ColorFromInt(int r, int g, int b) {
			return new UnityEngine.Color(r / 255.0f, g / 255.0f, b / 255.0f);
		}
		public static UnityEngine.Color ColorFromInt(int r, int g, int b, int a) {
			return new UnityEngine.Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
		}

		// for just ngui button, enable, disable
		public static UnityEngine.Color FromHex(string color) {

			color = color.TrimStart('#');
			float red = (HexToInt(color[1]) + HexToInt(color[0]) * 16f) / 255f;
			float green = (HexToInt(color[3]) + HexToInt(color[2]) * 16f) / 255f;
			float blue = (HexToInt(color[5]) + HexToInt(color[4]) * 16f) / 255f;
			UnityEngine.Color finalColor = new() { r = red, g = green, b = blue, a = 1 };
			return finalColor;
		}

		/// <summary> 16진수를 int로 변환 </summary>
		public static int HexToInt(char hexValue) {
			return int.Parse(hexValue.ToString(), System.Globalization.NumberStyles.HexNumber);
		}
	}
}