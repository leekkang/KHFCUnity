
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	public interface ICustomUpdate {
		void OnUpdate(float dt);
	}

	public class GameLoop : Singleton<GameLoop> {
		public static GameLoop inst;

		readonly List<ICustomUpdate> listActive = new(1024);
		readonly List<ICustomUpdate> listAdd = new(128);
		readonly List<ICustomUpdate> listRemove = new(128);

		public void Register(ICustomUpdate obj) => listAdd.Add(obj);
		public void Unregister(ICustomUpdate obj) => listRemove.Add(obj);

		public static void Init() {
			if (inst != null)
				return;
			inst = instance;
			DontDestroyOnLoad(inst.gameObject);
		}

		void Update() {
			int removeCount = listRemove.Count;
			if (removeCount > 0) {
				for (int i = 0; i < removeCount; ++i) {
					ICustomUpdate target = listRemove[i];
					int index = listActive.IndexOf(target);
					if (index > -1) {
						int lastIndex = listActive.Count - 1;
						listActive[index] = listActive[lastIndex];
						listActive.RemoveAt(lastIndex);
					}
				}
				listRemove.Clear();
			}

			int addCount = listAdd.Count;
			if (addCount > 0) {
				for (int i = 0; i < addCount; ++i)
					listActive.Add(listAdd[i]);
				listAdd.Clear();
			}

			float dt = Time.deltaTime;
			for (int i = 0, count = listActive.Count; i < count; ++i)
				listActive[i].OnUpdate(dt);
		}
	}
}