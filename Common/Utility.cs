using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization.Formatters.Binary;

namespace KHFC {
	public class Utility {
		public static Color ColorFromInt(int r, int g, int b) {
			return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
		}
		public static Color ColorFromInt(int r, int g, int b, int a) {
			return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
		}

		/// <summary> 소수점 4자리 까지 비교 </summary>
		public static bool FloatEqual(float a, float b, float epsilon = 1.0E-4f) {
			return System.Math.Abs(a - b) < epsilon;
		}

		/// <summary> Transform의 값으로 사용하기 위해 각도를 <paramref name="min"/> ~ <paramref name="min"/>+360도 범위 내 값으로 만든다 </summary>
		public static float ClampAngle(float angle, float min = 0) {
			if (angle < min) {
				while (angle < min)
					angle += 360f;
			} else if (angle > (min + 360f)) {
				while (angle > (min + 360f))
					angle -= 360f;
			}
			return angle;
		}

		/// <summary> y축 기준 <paramref name="vec"/> 와의 사이각을 반환한다. </summary>
		/// <remarks> 왼손좌표계 기준 반시계 방향이 + 이다 -> 왼쪽을 보고있으면 +값, 오른쪽을 보고있으면 -값을 반환한다. </remarks>
		public static float GetLookAngle(Vector2 vec) {
			return Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
		}

		/// <summary> 파일 혹은 폴더가 존재하는지 확인 </summary>
		public static bool CheckExistFileOrDir(string name) {
			return (Directory.Exists(name) || File.Exists(name));
		}

		/// <summary> 파일인지 확인 </summary>
		public static bool IsFile(string path) {
			// get the file attributes for file or directory
			FileAttributes attr = File.GetAttributes(path);

			//detect whether its a directory or file
			return (attr & FileAttributes.Directory) != FileAttributes.Directory;
		}

		/// <summary> 파일 혹은 폴더 복사 </summary>
		public static void CopyFileOrDirectory(string srcFullPath, string destPath) {
			if (Utility.IsFile(srcFullPath) && System.IO.File.Exists(srcFullPath)) {
				System.IO.File.Copy(srcFullPath, destPath, true);
			}
			else if (!Utility.IsFile(srcFullPath) && System.IO.Directory.Exists(srcFullPath)) {
				DirectoryCopy(srcFullPath, destPath, true);
			}
		}

		/// <summary> 폴더 복사 </summary>
		public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists) {
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			// If the destination directory doesn't exist, create it. 
			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location. 
			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		/// <summary> 파일 또는 폴더 복사 </summary>
		public static void DeleteFileOrDirectory(string path) {
			if (CheckExistFileOrDir(path) == false)
				return;

			if (Utility.IsFile(path) && System.IO.File.Exists(path)) {
				System.IO.File.Delete(path);
			}
			else if (!Utility.IsFile(path) && System.IO.Directory.Exists(path)) {
				System.IO.Directory.Delete(path, true);
			}
		}

		/// <summary> 폴더 생성 </summary>
		public static void CreateDir(string path, bool removeExist = false) {
			if (System.IO.Directory.Exists(path)) {
				if (removeExist) {
					System.IO.Directory.Delete(path, true);
					System.IO.Directory.CreateDirectory(path);
				}
			}
			else {
				System.IO.Directory.CreateDirectory(path);
			}
		}

		/// <summary> 실제 시간만큼 대기 </summary>
		public static IEnumerator WaitForRealSeconds(float time) {
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + time) {
				yield return null;
			}
		}

		/// <summary> 메모리 복사 </summary>
		public static T DeepClone<T>(T obj) {
			using (var ms = new MemoryStream()) {
				var formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				ms.Position = 0;

				return (T)formatter.Deserialize(ms);
			}
		}

		/// <summary> 문자열을 바이트로 복사 </summary>
		public static byte[] GetBytes(string str) {
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		/// <summary> 바이트를 문자열로 복사 </summary>
		public static string GetString(byte[] bytes) {
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		// for just ngui button, enable, disable
		private static Color FromHex(string color) {

			color = color.TrimStart('#');
			float red = (HexToInt(color[1]) + HexToInt(color[0]) * 16f) / 255f;
			float green = (HexToInt(color[3]) + HexToInt(color[2]) * 16f) / 255f;
			float blue = (HexToInt(color[5]) + HexToInt(color[4]) * 16f) / 255f;
			Color finalColor = new Color { r = red, g = green, b = blue, a = 1 };
			return finalColor;
		}

		/// <summary> 16진수를 int로 변환 </summary>
		private static int HexToInt(char hexValue) {
			return int.Parse(hexValue.ToString(), System.Globalization.NumberStyles.HexNumber);
		}
	}

	public static class StopWatchUtil {
#if UNITY_EDITOR
		static Dictionary<string, System.Diagnostics.Stopwatch> mDicWatch = new Dictionary<string, System.Diagnostics.Stopwatch>();
#endif
		static bool mUseLog = false;
		static System.Text.StringBuilder mLog;

		public static System.Text.StringBuilder GetLog() {
			mUseLog = false;
			return mLog;
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void InitLog() {
			mUseLog = true;
			mLog = new System.Text.StringBuilder();
			mLog.Remove(0, mLog.Length);
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void CheckTimeStart(string msg) {
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

#if UNITY_EDITOR
			if (!mDicWatch.ContainsKey(msg))
				mDicWatch.Add(msg, watch);

			watch.Reset();
			watch.Start();

			string resultMsg = string.Format("StopWatch : {0} START", msg);
			UnityEngine.Debug.Log(resultMsg);

			if (!mUseLog)
				return;

			if (mLog == null)
				mLog = new System.Text.StringBuilder();

			mLog.AppendLine(resultMsg);
#endif
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void CheckTimeEnd(string msg) {
#if UNITY_EDITOR
			if (!mDicWatch.ContainsKey(msg))
				return;

			System.Diagnostics.Stopwatch watch = mDicWatch[msg];

			watch.Stop();

			string resultMsg = string.Format("StopWatch : {0} End, Time : {1}", msg, watch.Elapsed.TotalSeconds);
			UnityEngine.Debug.Log(resultMsg);


			if (mUseLog)
				mLog.AppendLine(resultMsg);
#endif
		}
	}


	public static class Base64Util {
		public static string Encoding(string text, System.Text.Encoding encodeType = null) {
			if (encodeType == null)
				encodeType = System.Text.Encoding.UTF8;

			byte[] bytes = encodeType.GetBytes(text);
			return System.Convert.ToBase64String(bytes);
		}

		public static string Decoding(string text, System.Text.Encoding encodeType = null) {
			if (encodeType == null)
				encodeType = System.Text.Encoding.UTF8;

			byte[] bytes = System.Convert.FromBase64String(text);
			return encodeType.GetString(bytes);
		}
	}
}
