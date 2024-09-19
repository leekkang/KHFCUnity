using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	/// <summary>
	/// 게임 상태들을 조절, 변경할 수 있는 클래스
	/// </summary>
	/// <remarks> 호스트 프로젝트의 매니저 클래스가 관리해야 한다. </remarks>
	public class GameStateCtrl : MonoBehaviour {
		public string m_CurState = "";
		public string m_PrevState = "";
		
		List<AbstractGameState> m_ListState = new List<AbstractGameState>();

		/// <summary> 게임의 상태를 등록, 리스트에 저장한다. </summary>
		/// <remarks> 주의 : <paramref name="stageName"/>은 반드시 클래스명과 동일해야 한다. </remarks>
		/// <param name="stageName"> 스테이지 이름 </param>
		public void RegistGameState(string stageName) {
			GameObject state = new() {
				name = stageName
			};
			AbstractGameState stateBase = (AbstractGameState)state.AddComponent(System.Type.GetType(stageName));
			state.transform.parent = transform;

			//AbstractGameState stateBase = state.GetComponent<AbstractGameState>();
			stateBase.m_StateName = stageName;
			m_ListState.Add(stateBase);
		}

		public AbstractGameState GetCurState() {
			return GetState(m_CurState);
		}

		public void ChangeState(string nextStageName) {
			//NUCamera.ignoreAllEvents = true;

			string curStateName = m_CurState;

			if (!string.IsNullOrEmpty(curStateName)) {
				AbstractGameState curState = GetState(curStateName);

				curState.OnLeave(nextStageName);

				// 이전 스테이트의 이름을 기억
				m_PrevState = curStateName;
			}

			m_CurState = nextStageName;

			AbstractGameState nextState = GetState(nextStageName);
			nextState.m_StateName = nextStageName;

			nextState.OnEnter(curStateName);

			//NUCamera.ignoreAllEvents = false;
		}

		public AbstractGameState GetState(string name) {
			return m_ListState.Find(state => state.m_StateName == name);
		}
	}
}