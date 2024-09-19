using UnityEngine;

/// <summary>
/// Display a field as read-only in the inspector.
/// CustomPropertyDrawers will not work when this attribute is used.
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { }

/// <summary>
/// 인스펙터에 필드를 텍스트로 표시해 읽기 전용으로 만들어주는 어트리뷰트
/// </summary>
/// <remarks> 단순한 타입만 지원한다. 클래스는 게임오브젝트만 지원한다. </remarks>
public class ShowOnlyAttribute : PropertyAttribute {
	public string name {  get; private set; }

	public ShowOnlyAttribute() {
		name = "";
	}
	public ShowOnlyAttribute(string displayName) {
		name = displayName;
	}
}