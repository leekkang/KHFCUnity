
using System.Collections.Generic;
using System;
using System.Collections;

namespace KHFC {
	public class RefList<T> where T : class {
		class RefItem {
			public int refCount;
			public T item;
		}
		System.Collections.Generic.List<RefItem> m_List;

		/// <summary> 현재 참조 횟수를 리턴한다. </summary>
		public int Add(T item) {
			if (m_List == null) {
				m_List = new List<RefItem> {
					new RefItem { refCount = 1, item = item }
				};
				return 1;
			}

			for (int i = 0; i < m_List.Count; i++) {
				if (m_List[i].item == item)
					return ++m_List[i].refCount;
			}

			m_List.Add(new RefItem { refCount = 1, item = item });

			return 1;
		}

		/// <summary> 리스트에서 제거되면 true, 아니면 false </summary>
		public bool Remove(T item) {
			for (int i = 0; i < m_List.Count; i++) {
				if (m_List[i].item == item) {
					if (m_List[i].refCount <= 1) {
						m_List.RemoveAtBySwap(i);
						return true;
					} else {
						--m_List[i].refCount;
						return false;
					}
				}
			}
			return false;
		}

		/// <summary> 리스트에서 제거되면 true, 아니면 false </summary>
		public bool Remove(Predicate<T> match) {
			for (int i = 0; i < m_List.Count; i++) {
				if (match(m_List[i].item)) {
					if (m_List[i].refCount <= 1) {
						m_List.RemoveAtBySwap(i);
						return true;
					} else {
						--m_List[i].refCount;
						return false;
					}
				}
			}
			return false;
		}

		public T Find(Predicate<T> match) {
			for (int i = 0; i < m_List.Count; i++) {
				if (match(m_List[i].item)) {
					return m_List[i].item;
				}
			}
			return null;
		}
	}
}
