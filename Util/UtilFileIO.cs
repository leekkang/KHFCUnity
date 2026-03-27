

namespace KHFC {
	internal partial class Util {
		/// <summary> 파일 혹은 폴더가 존재하는지 확인 </summary>
		public static bool CheckExistFileOrDir(string name) {
			return (System.IO.Directory.Exists(name) || System.IO.File.Exists(name));
		}

		/// <summary> 파일인지 확인 </summary>
		public static bool IsFile(string path) {
			// get the file attributes for file or directory
			System.IO.FileAttributes attr = System.IO.File.GetAttributes(path);

			//detect whether its a directory or file
			return (attr & System.IO.FileAttributes.Directory) != System.IO.FileAttributes.Directory;
		}

		/// <summary> 폴더 생성 </summary>
		public static void CreateDir(string path, bool delIfExists = false) {
			if (!System.IO.Directory.Exists(path)) {
				System.IO.Directory.CreateDirectory(path);
			} else if (delIfExists) {
				System.IO.Directory.Delete(path, true);
				System.IO.Directory.CreateDirectory(path);
			}
		}

		/// <summary> 파일 혹은 폴더 복사 </summary>
		public static void CopyFileOrDirectory(string srcFullPath, string destPath) {
			if (IsFile(srcFullPath) && System.IO.File.Exists(srcFullPath))
				System.IO.File.Copy(srcFullPath, destPath, true);
			else if (!IsFile(srcFullPath) && System.IO.Directory.Exists(srcFullPath))
				CopyDirectory(srcFullPath, destPath, true);
		}

		/// <summary> 파일 또는 폴더 삭제 </summary>
		public static void DeleteFileOrDirectory(string path) {
			if (CheckExistFileOrDir(path) == false)
				return;

			if (IsFile(path) && System.IO.File.Exists(path))
				System.IO.File.Delete(path);
			else if (!IsFile(path) && System.IO.Directory.Exists(path))
				System.IO.Directory.Delete(path, true);
		}

		/// <summary> 폴더 복사 </summary>
		public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs) {
			// Get the subdirectories for the specified directory
			System.IO.DirectoryInfo dir = new(sourceDirName);
			System.IO.DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists)
				throw new System.IO.DirectoryNotFoundException(
					$"Source directory does not exist or could not be found: {sourceDirName}");

			// If the destination directory doesn't exist, create it
			if (!System.IO.Directory.Exists(destDirName))
				System.IO.Directory.CreateDirectory(destDirName);

			// Get the files in the directory and copy them to the new location
			System.IO.FileInfo[] files = dir.GetFiles();
			foreach (System.IO.FileInfo file in files) {
				string temppath = System.IO.Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location
			if (copySubDirs) {
				foreach (System.IO.DirectoryInfo subdir in dirs) {
					string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
					CopyDirectory(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		/// <summary> 메모리 복사, 주의 : Serialized Field만 복사됨 </summary>
		public static T DeepClone<T>(T obj) {
			using System.IO.MemoryStream ms = new();
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new();
			formatter.Serialize(ms, obj);
			ms.Position = 0;

			return (T)formatter.Deserialize(ms);
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
	}
}