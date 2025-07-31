
using UnityEngine;
using UnityEngine.UI;

namespace KHFC {
	[RequireComponent(typeof(CanvasRenderer))]
	[DisallowMultipleComponent]
	public class InvisibleRaycastGraphic : Graphic {
		protected override void OnPopulateMesh(VertexHelper vh) {
			vh.Clear();		// 아무것도 렌더링하지 않음
		}

		/// <summary> 무조건 클릭 허용 </summary>
		public override bool Raycast(Vector2 sp, Camera eventCamera) {
			return true;
		}
	}
}