using UnityEngine;

namespace KHFC {
	/// <summary>
	/// 인스펙터의 필드 이름을 변경해주는 어트리뷰트
	/// </summary>
	/// <remarks>
	/// <para> 주의: 리스트, 배열의 경우 엘레멘트들의 이름이 변한다! </para>
	/// <para> <see cref="UnityEditor.EditorGUI.PropertyField"/> 를 사용하지 않는 방법을 찾아봐야 할듯? </para>
	/// </remarks>
	public class FieldNameAttribute : PropertyAttribute {
		public string name { get; private set; }

		public FieldNameAttribute(string fieldName) {
			name = fieldName;
		}
	}
}