
using UnityEngine;

namespace KHFC {
	public abstract class AbstractGameState : MonoBehaviour {
		public int m_StateIndex;

		public virtual void OnEnter(int prevStateIndex) { }
		public virtual void OnLeave(int nextStateIndex) { }
	}
}