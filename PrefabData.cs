using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
	public class PrefabData : MonoBehaviour {
		[System.Serializable]
		public class AtlasInfo {
			public string m_atlas_name;
			public List<UISprite> m_l_sprite;
			public bool m_b_is_set = false;
			/// <summary>
			/// 공용으로 사용되는 아틀라스 인가? ( 지금은 atl_ui로 시작하는 것들이 대상 )
			/// </summary>
			public bool m_b_use_common = false;
		}

		[System.Serializable]
		public class FontInfo {
			public string m_font_name;
			public List<UILabel> m_l_label;
		}

		[System.Serializable]
		public class CachedInfo {
			public string m_gameobject_fullname;
			public GameObject m_cached_go;

			public CachedInfo(string fullName, GameObject go_target) {
				m_gameobject_fullname = fullName;
				m_cached_go = go_target;
			}
		}
		/// <summary>@로케일 처리를 해야 하는 라벨</summary>
		[System.Serializable]
		public class NestedLabel {
			public UILabel m_label;
			public int m_n_locale_id;

			public NestedLabel(UILabel label, int n_locale_id) {
				m_label = label;
				m_n_locale_id = n_locale_id;
			}
		}
		public List<AtlasInfo> m_l_atlas_info = new List<AtlasInfo>();
		public List<FontInfo> m_l_sub_font_info = new List<FontInfo>();
		public List<CachedInfo> m_l_cache = new List<CachedInfo>();
		//동적으로 런타임에서 아틀라스 연결을 하는 UISprite들을 저장해 놓는다.
		//UnloadAssets할때 UiSprite의 atlas를 null로 채워주기 위함
		public List<UISprite> m_l_dynamic_sprites = new List<UISprite>();
		public List<UITweener> m_l_tweener = new List<UITweener>();
		public List<NestedLabel> m_l_nested_label = new List<NestedLabel>();

		//public List<EffPrfDataOn2D> m_l_effect = new List<EffPrfDataOn2D>();
		//public UIFont m_ref_font;
		//public List<NURelSortOrder> m_l_rel_sortorder = new List<NURelSortOrder>();
		//public List<NUEffect> m_l_nueffect = new List<NUEffect>();

#if UNITY_EDITOR
		public void Init() {
			m_l_atlas_info.Clear();
			m_l_sub_font_info.Clear();
			m_l_cache.Clear();
			m_l_dynamic_sprites.Clear();
			m_l_tweener.Clear();
			//m_l_effect.Clear();
			//m_l_rel_sortorder.Clear();
			//m_l_nueffect.Clear();
			m_l_nested_label.Clear();
		}
#endif

		/// <summary>
		/// 전체 UITweener 중에서 Duration과 Delay의 총합이 가장 큰 값을 리턴 한다.
		/// </summary>
		public float GetMaxTweenDuration() {
			float res_duration = 0;

			for (int idx = 0; idx < m_l_tweener.Count; idx++) {
				UITweener tweener = m_l_tweener[idx];

				if (res_duration < tweener.duration + tweener.delay)
					res_duration = tweener.duration + tweener.delay;
			}

			return res_duration;
		}
	}
}
