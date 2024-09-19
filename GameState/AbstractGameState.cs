using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	public abstract class AbstractGameState : MonoBehaviour {
		public string m_StateName;

		public virtual void OnEnter(string prevState) { }
		public virtual void OnLeave(string nextState) { }
	}
}