using UnityEngine;
using UnityEditor;

namespace KHFC {
	/// <summary> Assets 상위폴더에 [오늘날짜 + 시간 .png]로 생성 </summary>
	public class ScreenCapture {
		public static void TakeScreenCapture() {
			//string filename = System.DateTime.Now + ".png";
			//filename = filename.Replace('/', '_');
			//filename = filename.Replace(' ', '_');
			//filename = filename.Replace(':', '_');
			string filename = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			string path = $"{System.IO.Path.GetDirectoryName(Application.dataPath)}/{filename}.png";

			UnityEngine.ScreenCapture.CaptureScreenshot(path);

			Debug.Log("ScreenCaptured : " + path);
		}
	}

	public class RenderTextureCapture {
		public static void RenderTextureToPng(RenderTexture tex, string path) {
			
		}

		public static void CameraCapture(Camera cam) {

		}
	}
}