
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KHFC {
//	public static class ItemEditorUtil {
//		const string ITEM_MATERIAL_PATH = "Assets/Media/Material/SpriteMaterial.mat";

//		/// <summary>
//		/// 자동으로 대상 오브젝트에 사용하는 스프라이트들을 링크하고 <see cref="SpriteRenderer"/> 오브젝트들을 생성해주는 함수
//		/// </summary>
//		public static void AutoSetItemSprite<T>(T item, string path, string prefix, Transform parentTr) where T : Item {
//#if UNITY_EDITOR
//			UnityEditor.Undo.SetCurrentGroupName($"Auto Set Item Sprite: {item.name}");
//			int undoGroup = UnityEditor.Undo.GetCurrentGroup();

//			Transform parent = parentTr != null ? parentTr : item.transform;
//			//Transform parent = parentTr ?? item.transform;

//			UnityEditor.Undo.RecordObject(item, "Update Item Data");

//			// 아이템에 사용하는 머테리얼을 가져온다.
//			Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(ITEM_MATERIAL_PATH);

//			// 스프라이트 렌더러를 임시저장할 리스트
//			List<SpriteRenderer> listRenderer = new List<SpriteRenderer>();

//			// 스프라이트 렌더러 링크를 리스트에 채운다. 없으면 해당 이름으로 만들어서 채운다.
//			Sprite[] arrSprite = KHFC.AssetUtility.LoadAllSprites(path);
//			for (int i = 0; i < arrSprite.Length; i++) {
//				string name = arrSprite[i].name;
//				name = name.Replace(prefix, "spr_");

//				SpriteRenderer renderer = GetRendererObject(name, parent);
//				UnityEditor.Undo.RecordObject(renderer, "Update Renderer");
//				renderer.material = mat;
//				renderer.sprite = arrSprite[i];
//				UnityEditor.EditorUtility.SetDirty(renderer);

//				listRenderer.Add(renderer);
//			}

//			// 스프라이트 렌더러 리스트를 아이템에 저장
//			item.m_ArrRenderer = listRenderer.ToArray();

//			Debug.Log($"==== {typeof(T)} Setting is Finished ====");

//			UnityEditor.EditorUtility.SetDirty(item);
//			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(item.gameObject);

//			UnityEditor.Undo.CollapseUndoOperations(undoGroup);
//#endif
//		}

//		/// <summary>
//		/// 자동으로 대상 오브젝트에 사용하는 스프라이트들을 링크하고 <see cref="SpriteRenderer"/> 오브젝트들을 생성해주는 함수
//		/// </summary>
//		/// <remarks> 패널은 m_ListSpriteRendererer를 1개만 등록한다. 스프라이트 자체를 교체하는 경우가 많기 때문 </remarks>
//		public static void AutoSetPanelSprite<T>(T panel, string path, string prefix, Transform parentTr) where T : Panel {
//#if UNITY_EDITOR
//			UnityEditor.Undo.SetCurrentGroupName($"Auto Set Panel Sprite: {panel.name}");
//			int undoGroup = UnityEditor.Undo.GetCurrentGroup();

//			Transform parent = parentTr != null ? parentTr : panel.transform;
//			//Transform parent = parentTr ?? item.transform;

//			UnityEditor.Undo.RecordObject(panel, "Update Panel Data");

//			// 아이템에 사용하는 머테리얼을 가져온다.
//			Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(ITEM_MATERIAL_PATH);

//			// 스프라이트 렌더러 링크를 리스트에 채운다. 없으면 해당 이름으로 만들어서 채운다.
//			Sprite[] arrSprite = KHFC.AssetUtility.LoadAllSprites(path);
//			for (int i = 0; i < arrSprite.Length; i++) {
//				string name = arrSprite[i].name;
//				name = name.Replace(prefix, "spr_");

//				SpriteRenderer renderer = GetRendererObject(name, parent);
//				UnityEditor.Undo.RecordObject(renderer, "Update Renderer");
//				renderer.material = mat;
//				renderer.sprite = arrSprite[i];
//				UnityEditor.EditorUtility.SetDirty(renderer);
//			}

//			Debug.Log($"==== {typeof(T)} Setting is Finished ====");

//			UnityEditor.EditorUtility.SetDirty(panel);
//			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(panel.gameObject);

//			UnityEditor.Undo.CollapseUndoOperations(undoGroup);
//#endif
//		}

//		/// <summary>
//		/// 색이 존재하는 스프라이트들을 리스트에 링크하고 <see cref="SpriteRenderer"/> 오브젝트들을 생성해주는 함수
//		/// </summary>
//		/// <remarks>
//		/// <para> 스프라이트 애니메이션을 생성하는 이미지들의 경우 <paramref name="isSpriteAnimation"/> 파라미터를 true로 해야한다 </para>
//		/// <para> 오브젝트에 들어가는 이미지는 <paramref name="prefix"/>에 해당하는 이미지 중 마지막 이미지가 들어간다 </para>
//		/// </remarks>
//		/// <param name="path"> 이미지가 존재하는 경로 </param>
//		/// <param name="prefix"> 이미지가 공통으로 사용하는 접두사. 해당 이미지만 로드 </param>
//		/// <param name="parentTr"> 렌더러 오브젝트를 붙일 부모 오브젝트 </param>
//		/// <param name="isSpriteAnimation"> 스프라이트 애니메이션을 사용하는 경우 true </param>
//		public static void AutoSetSpriteWithColor<T>(T item, string path, string prefix, Transform parentTr, bool isSpriteAnimation) where T : Item {
//#if UNITY_EDITOR
//			UnityEditor.Undo.SetCurrentGroupName($"Auto Set Sprite With Color: {item.name}");
//			int undoGroup = UnityEditor.Undo.GetCurrentGroup();

//			Transform parent = parentTr != null ? parentTr : item.transform;
//			//Transform parent = parentTr ?? item.transform;

//			UnityEditor.Undo.RecordObject(item, "Update Item Data");

//			// 아이템에 사용하는 머테리얼을 가져온다.
//			Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(ITEM_MATERIAL_PATH);

//			// 컬러 스프라이트 링크 리스트를 초기화한다.
//			item.m_ListKindSprite = new List<Item.ItemSprite>();
//			item.m_ListKindSprite.Resize(Config.MAX_JEWEL_COLOR);
//			for (int i = 0; i < Config.MAX_JEWEL_COLOR; ++i) {
//				item.m_ListKindSprite[i] = new() { m_ListSprite = new List<Sprite>() };
//			}

//			List<SpriteRenderer> listRenderer = new List<SpriteRenderer>();
//			// 컬러 구분 없이 적용하는 스프라이트가 있는 경우 마지막에 리스트에 넣기 위해 따로 임시 저장
//			List<SpriteRenderer> listNonColorSprite = new List<SpriteRenderer>();

//			// postfix의 형태가 "0{0}_{1}"인 경우 컬러 구분을 해야하는 스프라이트로 규정한다.
//			Func<string, int> GetColorNumber = (string postfix) => {
//				// TODO : 컬러 넘버 뒤에 언더바가 안오는 단일 이미지들이 있다. 일단 체크하지 말자
//				if (postfix.Length >= 2 && postfix[0] == '0'/* && postfix[2] == '_'*/)
//					return (int)(postfix[1] - '0');
//				else
//					return 0;
//			};

//			// 스프라이트 이미지와 렌더러 링크를 리스트에 채운다. 렌더러 오브젝트가 없으면 해당 이름으로 만들어서 채운다.
//			Sprite[] arrSprite = KHFC.AssetUtility.LoadAllSprites(path);
//			for (int i = 0; i < arrSprite.Length; i++) {
//				string name = arrSprite[i].name;
//				if (name.Length < prefix.Length)
//					continue;
//				int colorNum = GetColorNumber(name[prefix.Length..]);

//				// 스프라이트 애니메이션은 렌더러가 1개만 있으면 된다.
//				// 논컬러 스프라이트가 존재할 수 있기 때문에 컬러넘버 체크를 같이 한다.
//				if (isSpriteAnimation && colorNum != 0)
//					name = "spr_body";
//				else
//					name = "spr_" + name[(prefix.Length + (colorNum == 0 ? 0 : 3))..];

//				SpriteRenderer renderer = GetRendererObject(name, parent);
//				UnityEditor.Undo.RecordObject(renderer, "Update Renderer");
//				renderer.material = mat;
//				renderer.sprite = arrSprite[i];
//				UnityEditor.EditorUtility.SetDirty(renderer);

//				if (colorNum == 0)
//					listNonColorSprite.Add(renderer);
//				else {
//					if (listRenderer.IndexOf(renderer) == -1)
//						listRenderer.Add(renderer);

//					// 어차피 이름이 같기 때문에 listRenderer 순서대로 들어간다고 가정한다.
//					// 오류가 나면 이름이 틀린 것
//					item.m_ListKindSprite[colorNum - 1].m_ListSprite.Add(arrSprite[i]);
//				}
//			}

//			// 논컬러 스프라이트를 렌더러 배열에 추가한다
//			listRenderer.AddRange(listNonColorSprite);
//			// 스프라이트 렌더러 리스트를 아이템에 저장
//			item.m_ArrRenderer = listRenderer.ToArray();

//			Debug.Log($"==== {typeof(T)} Setting is Finished ====");

//			UnityEditor.EditorUtility.SetDirty(item);
//			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(item.gameObject);

//			UnityEditor.Undo.CollapseUndoOperations(undoGroup);
//#endif
//		}

//		// 에디터의 계층구조 뷰의 "Create Empty" 커맨드가 수행하는 함수와 비슷한 코드를 사용한 함수

//		/// <summary> 명시적인 Undo 트래킹을 지원하는 오브젝트 생성 함수 </summary>
//		static GameObject CreateGameObject(string name, Transform parent) {
//#if UNITY_EDITOR
//			//GameObject go = UnityEditor.ObjectFactory.CreateGameObject(name);
//			GameObject go = new GameObject(name);
//			UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Obj: " + name);

//			UnityEditor.Undo.SetTransformParent(go.transform, parent, "Reparenting");

//			go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//			go.transform.localScale = Vector3.one;
//			go.layer = parent.gameObject.layer;

//			if (parent.GetComponent<RectTransform>())
//				UnityEditor.ObjectFactory.AddComponent<RectTransform>(go);

//			return go;
//#else
//			return null;
//#endif
//		}


//		/// <summary> <see cref="SpriteRenderer"/> 오브젝트를 찾아서 반환한다. 없으면 생성한다. </summary>
//		/// <param name="name"></param>
//		/// <param name="parent"></param>
//		/// <returns></returns>
//		static SpriteRenderer GetRendererObject(string name, Transform parent) {
//			SpriteRenderer renderer;
//			Transform target = parent.FindRecursively(name);
//			if (target != null) {
//				// 같은 이름을 가진 오브젝트가 존재할 경우 컴포넌트를 붙인다.
//				if (!target.TryGetComponent(out renderer))
//					renderer = UnityEditor.Undo.AddComponent<SpriteRenderer>(target.gameObject);
//				return renderer;
//			}

//			GameObject newObj = CreateGameObject(name, parent);
//			renderer = UnityEditor.Undo.AddComponent<SpriteRenderer>(newObj);

//			return renderer;
//		}
//	}
}