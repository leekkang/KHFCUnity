
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if KHFC_UNITASK
using AsyncReturn = Cysharp.Threading.Tasks.UniTask<KHFC.AbstractPanel>;
#else
using AsyncReturn = System.Threading.Tasks.Task<KHFC.AbstractPanel>;
#endif

namespace KHFC {
	/// <summary>
	/// UGUI 패널 오브젝트들을 관리, 변경할 수 있는 클래스
	/// </summary>
	/// <remarks> 호스트 프로젝트의 매니저 클래스가 관리해야 한다. </remarks>
	public class GUICtrl : MonoBehaviour {
		public class PanelStackData {
			public int m_Type;
			/// <summary> 현재 패널 캔버스의 소트 오더, 높으면 나중에 그림 </summary>
			public int m_SortOrder;
			/// <summary> 다음 패널에 의해 가려진 패널인가? - 다시 열 때 활성화 해야하는가? </summary>
			public bool m_NeedActivate;
			/// <summary> 패널 뒤에 모달 창을 띄울것인가 </summary>
			public bool m_ShowModal;

			public void Recycle() {
				m_Type = 1;
				m_SortOrder = 0;
				m_NeedActivate = false;
				m_ShowModal = false;
			}
			
		}

		/// <summary> UI들이 생성될 부모 오브젝트의 트랜스폼 </summary>
		public Transform m_GUIParent;

		/// <summary> 패널이 생성된 이후 호출할 함수. 없어도 됨 </summary>
		public System.Action<AbstractPanel> m_OnAfterCreatePanel = null;
		/// <summary> 패널이 로드된 이후 호출할 함수. 무조건 등록해야 함 </summary>
		public System.Action<PanelStackData, AbstractPanel> m_OnAfterShowPanel;

		public delegate string AddressCallback(int type);
		/// <summary> 생성해야 하는 패널의 어드레스를 가져오는 함수 </summary>
		/// <remarks> 호스트 프로젝트의 매니저 클래스에서 무조건 등록해야 한다. </remarks>
		public AddressCallback m_OnGetPanelAddress;

		/// <summary> 패널이 로드 된 순서를 기억하는 리스트, 스택처럼 활용한다 </summary>
		readonly List<PanelStackData> m_History = new();
		/// <summary> 현재 로드한 패널들 </summary>
		readonly Dictionary<int, AbstractPanel> m_DicPanel = new();

		/// <summary> 스택 데이터 오브젝트 풀 </summary>
		readonly List<PanelStackData> m_ListDataPool = new();

		public AbstractPanel curPanel => m_History.Count > 0 ? GetPanel(m_History[^1].m_Type) : null;
		public int curPanelType => m_History.Count > 0 ? m_History[^1].m_Type : -1;
		public int prevPanelType => m_History.Count > 1 ? m_History[^2].m_Type : -1;

		void Awake() {
			m_GUIParent = transform.Find("PanelStack");
			if (m_GUIParent == null) {
				Transform tr = new GameObject("PanelStack").transform;
				tr.parent = transform;
				tr.position = Vector3.zero;
				m_GUIParent = tr;
			}
		}

		public AbstractPanel GetPanel(int panelType)
			=> m_DicPanel.TryGetValue(panelType, out AbstractPanel panel) ? panel : null;
		public T GetPanel<T>(int panelType) where T : AbstractPanel
			=> m_DicPanel.TryGetValue(panelType, out AbstractPanel panel) ? (T)panel : null;


		/// <summary> 패널을 생성, 딕셔너리에 저장한다 </summary>
		/// <remarks> 패널의 경우 풀에 넣지 않고 여기서 관리하기 때문에 Unregist에서 Despawn 대신 Release를 해야한다. </remarks>
		public AbstractPanel RegistPanel(int panelType) {
			string address = m_OnGetPanelAddress(panelType);
			if (TryCheckRegist(panelType, ref address, out AbstractPanel panel))
				return panel;

			GameObject go = PoolMgr.inst.CreateGameObject(address, activeEnable:false);
			_RegistProcess(go, panelType);
			m_OnAfterCreatePanel?.Invoke(panel);

			return panel;
		}

		/// <summary> 이미 생성된 패널을 딕셔너리에 저장한다. 이미 존재하면 리턴값에 넣어줌 </summary>
		public AbstractPanel RegistPanel(int panelType, AbstractPanel panel) {
			if (m_DicPanel.ContainsKey(panelType)) {
				if (m_DicPanel[panelType] != null)
					return m_DicPanel[panelType];
				m_DicPanel.Remove(panelType);
			}

			_RegistProcess(panel.gameObject, panelType);
			return panel;
		}

		/// <summary> 패널을 생성, 딕셔너리에 저장한다 </summary>
		/// <remarks> 패널의 경우 풀에 넣지 않고 여기서 관리하기 때문에 Unregist에서 Despawn 대신 Release를 해야한다. </remarks>
		public async AsyncReturn RegistPanelAsync(int panelType) {
			string address = m_OnGetPanelAddress(panelType);
			if (TryCheckRegist(panelType, ref address, out AbstractPanel panel))
				return panel;

			GameObject go = await PoolMgr.inst.CreateGameObjectAsync(address, activeEnable: false);
			_RegistProcess(go, panelType);
			m_OnAfterCreatePanel?.Invoke(panel);

			return panel;
		}

		/// <summary> 패널을 생성, 딕셔너리에 저장한다 </summary>
		/// <remarks> 패널의 경우 풀에 넣지 않고 여기서 관리하기 때문에 Unregist에서 Despawn 대신 Release를 해야한다. </remarks>
		public void RegistPanelAsync(int panelType, System.Action<AbstractPanel> onAfter) {
			string address = m_OnGetPanelAddress(panelType);
			if (TryCheckRegist(panelType, ref address, out AbstractPanel panel)) {
				onAfter?.Invoke(panel);
				return;
			}

			PoolMgr.inst.CreateGameObjectAsync(address, (go) => {
				_RegistProcess(go, panelType);
				m_OnAfterCreatePanel?.Invoke(panel);
			}, activeEnable: false);
		}

		void _RegistProcess(GameObject go, int panelType) {
			go.transform.SetParent(m_GUIParent, false); // 로컬 좌표를 유지하겠다
			AbstractPanel panel = go.GetComponent<AbstractPanel>();
			if (!panel.initialized)
				panel.Init();

			m_DicPanel.Add(panelType, panel);
		}

		/// <summary> 코드중복 제거용 체크 함수. 패널이 없으면 false를 리턴한다. </summary>
		/// <remarks> 오류가 있어 진행이 안되어도 true를 리턴. 이건 코드 문제가 아니기 때문이다 </remarks>
		bool TryCheckRegist(int panelType, ref string address, out AbstractPanel panel) {
			if (address == null) {
				Debug.Log($"PanelType Address is not registered : {panelType}");
				panel = null;
				return true;
			}
			if (m_DicPanel.TryGetValue(panelType, out panel)) {
				if (panel != null)
					return true;

				m_DicPanel.Remove(panelType);
			}
			return false;
		}

		/// <summary> 딕셔너리에 있는 패널을 제거한다 </summary>
		/// <remarks> Addressable을 사용하는 경우 여기서 release를 해준다. </remarks>
		public void UnregistPanel(int panelType) {
			if (m_DicPanel.TryGetValue(panelType, out AbstractPanel panel)) {
				panel.OnDestroyProcess();
				PoolMgr.inst.ReleaseAsset(m_DicPanel[panelType].gameObject);
				m_DicPanel.Remove(panelType);
			}
		}

		/// <summary> 딕셔너리에 있는 모든 패널을 제거한다 </summary>
		/// <param name="exclude"> 제거하지 않을 패널 종류 </param>
		public void UnregistAllPanel(params int[] exclude) {
			List<int> list = new();
			foreach (KeyValuePair<int, AbstractPanel> kvp in m_DicPanel) {
				if (exclude != null && exclude.Contains(kvp.Key))
					continue;
				list.Add(kvp.Key);
			}

			for (int i = 0; i < list.Count; i++) {
				UnregistPanel(list[i]);
			}
		}


		/// <summary> 패널을 히스토리에 추가하고 최상단에 활성화한다. </summary>
		/// <param name="hidePrevPanel"> 이전 패널을 비활성화 할 것인가? </param>
		/// <param name="showModal"> 현재 패널이 모달 패널인가? </param>
		public AbstractPanel PushPanel(int panelType, bool showModal = true, bool hidePrevPanel = false) {
			return ShowPanel(_PushPanelProcess(panelType, showModal, hidePrevPanel));
		}

		/// <summary> 패널을 히스토리에 추가하고 최상단에 활성화한다. </summary>
		/// <param name="hidePrevPanel"> 이전 패널을 비활성화 할 것인가? </param>
		/// <param name="showModal"> 현재 패널이 모달 패널인가? </param>
		public async AsyncReturn PushPanelAsync(int panelType, bool showModal = true, bool hidePrevPanel = false) {
			return await ShowPanelAsync(_PushPanelProcess(panelType, showModal, hidePrevPanel));
		}

		/// <summary> 현재 패널을 제거하고 이전 패널을 활성화한다 </summary>
		/// <param name="removeOnly"> 팝업 연출 없이 제거하고 싶을 때 사용 </param>
		public AbstractPanel PopPanel(bool removeOnly = false) {
			if (m_History.Count <= 1) {
				Debug.Log("Stack History has only single Panel");
				return null;
			}

			PanelStackData curData = m_History[^1];
			m_History.RemoveAt(m_History.Count - 1);
			AbstractPanel cur = GetPanel(curData.m_Type);
			if (cur != null) {
				cur.Close(removeOnly);
				AbstractPanel next = GetPanel(m_History[^1].m_Type);
				if (cur != next)
					cur.UnloadAsset(next);
			}

			RecycleStackData(curData);
			return ShowPanel(m_History[^1]);
		}
		
		/// <summary> 현재 히스토리를 전부 제거하고 패널을 활성화한다. </summary>
		public AbstractPanel GotoPanel(int panelType) {
			return ShowPanel(ClearHistoryAndReady(panelType));
		}

		/// <summary> 현재 히스토리를 전부 제거하고 패널을 활성화한다. </summary>
		public async AsyncReturn GotoPanelAsync(int panelType) {
			return await ShowPanelAsync(ClearHistoryAndReady(panelType));
		}

		/// <summary> 현재 히스토리를 전부 제거하고 <paramref name="panelType"/>의 데이터를 넣는다 </summary>
		/// <param name="panelType"> 처음 띄울 패널의 타입 </param>
		public PanelStackData ClearHistoryAndReady(int panelType) {
			HideAllPanel(panelType);

			PanelStackData data;
			AbstractPanel target = GetPanel(panelType);
			for (int i = m_History.Count - 1; i >= 0; --i) {
				data = m_History[i];
				AbstractPanel stack = GetPanel(data.m_Type);

				if (stack != null && stack != target)
					stack.UnloadAsset(target);

				RecycleStackData(data);
			}
			m_History.Clear();

			data = GetPooledStackData();
			data.m_Type = panelType;
			data.m_NeedActivate = true;
			data.m_ShowModal = false;
			data.m_SortOrder = 0;
			m_History.Add(data);

			return data;
		}

		/// <summary> 현재 띄워진 패널을 제외하고 히스토리를 전부 제거한다 GotoPanel(curPanelType) 과 동일함 </summary>
		/// <param name="panelType"> 처음 띄울 패널의 타입 </param>
		public PanelStackData ClearHistoryExceptCurPanel() {
			PanelStackData curData = m_History[^1];
			if (m_History.Count < 2)
				return curData;

			AbstractPanel cur = GetPanel(curData.m_Type);
			HideAllPanel(curData.m_Type);

			for (int i = m_History.Count - 2; i >= 0; --i) {
				PanelStackData data = m_History[i];
				AbstractPanel stack = GetPanel(data.m_Type);

				if (stack != null && stack != cur)
					stack.UnloadAsset(cur);

				RecycleStackData(data);
			}
			m_History.Clear();

			curData.m_NeedActivate = true;
			curData.m_ShowModal = false;
			curData.m_SortOrder = 0;
			m_History.Add(curData);

			return curData;
		}

		/// <summary> 모든 패널을 비활성화한다 </summary>
		/// <param name="exclude"> 예외 패널의 종류 </param>
		public void HideAllPanel(params int[] exclude) {
			foreach (KeyValuePair<int, AbstractPanel> kvp in m_DicPanel) {
				if (exclude != null && exclude.Contains(kvp.Key))
					continue;
				kvp.Value.gameObject.SetActive(false);
			}
		}

		/// <summary> 현재 히스토리의 모든 패널의 애셋을 언로드한다 </summary>
		/// <remarks> Addressable을 사용하면 굳이 사용하지 않아도 됨 </remarks>
		public void UnloadAssetAllPanelsInHistory() {
			for (int i = m_History.Count - 1; i >= 0; --i) {
				PanelStackData data = m_History[i];

				// 스택이 많이 쌓여서 히스토리에만 존재하는 패널이 있을 수 있다.
				if (m_DicPanel.ContainsKey(data.m_Type)) {
					AbstractPanel panel = GetPanel(data.m_Type);
					panel.UnloadAsset();
				}

				RecycleStackData(data);
			}
			m_History.Clear();

			Resources.UnloadUnusedAssets();
		}



		PanelStackData _PushPanelProcess(int panelType, bool showModal, bool hidePrevPanel) {
			PanelStackData prevData = null;
			if (m_History.Count > 0) {
				prevData = m_History[^1];
				prevData.m_NeedActivate = hidePrevPanel;
			}

			PanelStackData data = GetPooledStackData();
			data.m_Type = panelType;
			data.m_NeedActivate = true;
			data.m_ShowModal = showModal;
			data.m_SortOrder = prevData == null ? 0 : prevData.m_SortOrder + 5; // 여유롭게
			m_History.Add(data);

			AbstractPanel prev = prevData == null ? null : GetPanel(prevData.m_Type);  // 현재 최상단 패널
			if (hidePrevPanel && prev != null) {
				prev.gameObject.SetActive(false);

				AbstractPanel cur = GetPanel(panelType);
				if (prev != cur)
					prev.UnloadAsset(cur);
			}

			return data;
		}

		/// <summary> 패널을 푸시하거나 팝 한 이후 최상단 패널을 세팅한다 </summary>
		AbstractPanel ShowPanel(PanelStackData data) {
			if (!m_DicPanel.TryGetValue(data.m_Type, out AbstractPanel panel))
				panel = RegistPanel(data.m_Type);

			_ShowPanelProcess(panel, data);
			return panel;
		}
		/// <summary> 패널을 푸시하거나 팝 한 이후 최상단 패널을 세팅한다 </summary>
		async AsyncReturn ShowPanelAsync(PanelStackData data) {
			if (!m_DicPanel.TryGetValue(data.m_Type, out AbstractPanel panel))
				panel = await RegistPanelAsync(data.m_Type);

			_ShowPanelProcess(panel, data);
			return panel;
		}
		void _ShowPanelProcess(AbstractPanel panel, PanelStackData data) {
			panel.SetSortOrder(data.m_SortOrder);
			panel.Open(data.m_NeedActivate);

			m_OnAfterShowPanel(data, panel);
		}

		/// <summary> 풀링되어 있는 PanelStackData를 리턴, 없으면 생성 </summary>
		PanelStackData GetPooledStackData() {
			PanelStackData data;
			if (m_ListDataPool.Count == 0) {
				data = new PanelStackData();
			} else {
				int lastIndex = m_ListDataPool.Count - 1;
				data = m_ListDataPool[lastIndex];
				m_ListDataPool.RemoveAt(lastIndex);
			}
			return data;
		}

		/// <summary> 사용한 PanelStackData를 m_ListDataPool에 추가 한다 </summary>
		void RecycleStackData(PanelStackData data) {
			data.Recycle();
			m_ListDataPool.Add(data);
		}



		/// <summary>
		/// prefab의 이름을 통해서 UiBase 스크립트명을 구한다.
		/// Panel 여부에 따라서 스크립트의 기본 네이밍이 달라진다
		/// </summary>
		public static string GetScriptName(string prfName) {
			//ex) panelPrfName : "pnl_main_lobby"
			//         scriptName : "MainLobbyPanel"
			bool b_is_panel = prfName.StartsWith("pnl_");

			string prefix = b_is_panel ? "pnl_" : "wdgt_";
			string postfix = b_is_panel ? "Panel" : "Wdgt";

			prfName = prfName.Replace(prefix, "");
			string[] splittedAssetName = prfName.Split('_');
			string scriptName = "";            // 최종 스크립트 이름
			string script_last_name = "";    // 스크립트 이름중 마지막 이름을 저장

			for (int i = 0; i < splittedAssetName.Length; i++) {
				string partOfAssetName = splittedAssetName[i];
				script_last_name = char.ToUpper(partOfAssetName[0]) + partOfAssetName.Substring(1);
				scriptName += script_last_name;
			}

			scriptName += postfix;

			Type type = Type.GetType(scriptName + ",Assembly-CSharp");

			if (type == null) {
				if (!b_is_panel) {    // Wdgt일때만 검증 처리
					scriptName = scriptName.Replace(script_last_name, "");
					type = Type.GetType(scriptName + ",Assembly-CSharp");

					if (type == null) {    // 마지막 이름을 제거한 것도 스크립트가 없다면 빈문자 리턴
						scriptName = "";
					}
				} else {
					Debug.LogError("해당 Panel에 맞는 스크립트가 없습니다 Name : " + scriptName);
				}
			}

			return scriptName;
		}
	}
}