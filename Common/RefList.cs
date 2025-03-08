

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
				m_List = new System.Collections.Generic.List<RefItem> {
					new() { refCount = 1, item = item }
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
				if (m_List[i].item != item)
					continue;

				bool remove = m_List[i].refCount <= 1;
				if (remove)
					m_List.RemoveAtBySwap(i);
				else
					--m_List[i].refCount;
				return remove;
			}
			return false;
		}

		/// <summary> 리스트에서 제거되면 true, 아니면 false </summary>
		public bool Remove(System.Predicate<T> match) {
			for (int i = 0; i < m_List.Count; i++) {
				if (!match(m_List[i].item))
					continue;
				
				bool remove = m_List[i].refCount <= 1;
				if (remove)
					m_List.RemoveAtBySwap(i);
				else
					--m_List[i].refCount;
				return remove;
			}
			return false;
		}

		public T Find(System.Predicate<T> match) {
			for (int i = 0; i < m_List.Count; i++) {
				if (match(m_List[i].item))
					return m_List[i].item;
			}
			return null;
		}
	}
}