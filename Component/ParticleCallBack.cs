
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	public class ParticleOverCallBack : MonoBehaviour {
		System.Action m_OnComplete;

		void Start() {
			var main = GetComponent<ParticleSystem>().main;
			main.stopAction = ParticleSystemStopAction.Callback;
		}

		private void OnParticleSystemStopped() {
			m_OnComplete?.Invoke();
		}
	}
}