using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	/// <summary>
	/// 게임 상태들을 조절, 변경할 수 있는 클래스
	/// </summary>
	/// <remarks> 호스트 프로젝트의 매니저 클래스가 관리해야 한다. </remarks>
	public class GameStateCtrl : MonoBehaviour {
		public int m_CurState = -1;
		public int m_PrevState = -1;
		
		readonly List<AbstractGameState> m_ListState = new();

		/// <summary> 게임의 상태를 등록, 리스트에 저장한다. </summary>
		/// <remarks> 주의 : <paramref name="stageName"/>은 반드시 클래스명과 동일해야 한다. </remarks>
		/// <param name="stageName"> 스테이지 이름 </param>
		public void RegistGameState(string stageName) {
			GameObject state = new() {
				name = stageName
			};
			AbstractGameState stateBase = (AbstractGameState)state.AddComponent(System.Type.GetType(stageName));
			state.transform.parent = transform;

			m_ListState.Add(stateBase);
			stateBase.m_StateIndex = m_ListState.Count - 1;
		}

		public AbstractGameState GetCurState() {
			return GetState(m_CurState);
		}

		public void ChangeState(int nextStageIndex) {
			int curStateIndex = m_CurState;

			if (curStateIndex > -1) {
				AbstractGameState curState = GetState(curStateIndex);
				curState.OnLeave(nextStageIndex);

				// 이전 스테이트의 인덱스를 기억
				m_PrevState = curStateIndex;
			}

			m_CurState = nextStageIndex;

			AbstractGameState nextState = GetState(nextStageIndex);
			nextState.m_StateIndex = nextStageIndex;

			nextState.OnEnter(curStateIndex);
		}

		public AbstractGameState GetState(int index) {
			return (-1 < index && index < m_ListState.Count) ? m_ListState[index] : null;
		}

		public T GetState<T>() where T : AbstractGameState {
			return (T)m_ListState.Find(state => state is T);
		}
	}
}
