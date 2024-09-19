using UnityEngine;

namespace KHFC {
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
		private static T m_Instance;

		private static object _lockObj = new object();

		public static T inst {
			get {
				if (applicationIsQuitting) {
					Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
									"' already destroyed on application quit." +
									" Won't create again - returning null.");
					return null;
				}

				lock (_lockObj) {
					if (m_Instance == null) {
						m_Instance = (T)FindObjectOfType(typeof(T));

						if (m_Instance == null) {
							// Need to create a new GameObject to attach the singleton to
							GameObject obj = new GameObject();
							m_Instance = obj.AddComponent<T>();
							obj.name = typeof(T).ToString();

							// Make instance persistent
							DontDestroyOnLoad(obj);
						}

						if (FindObjectsOfType(typeof(T)).Length > 1) {
							Debug.LogError("[Singleton] Something went really wrong " +
											" - there should never be more than 1 Singleton!" +
											" Reopenning the scene might fix it.");
							return m_Instance;
						}
					}

					return m_Instance;
				}
			}
		}

		private static bool applicationIsQuitting = false;
		/// <summary>
		/// When Unity quits, it destroys objects in a random order.
		/// In principle, a Singleton is only destroyed when application quits.
		/// If any script calls Instance after it have been destroyed, 
		///   it will create a buggy ghost object that will stay on the Editor scene
		///   even after stopping playing the Application. Really bad!
		/// So, this was made to be sure we're not creating that buggy ghost object.
		/// </summary>
		public void OnDestroy() {
			applicationIsQuitting = true;
		}
	}
}
