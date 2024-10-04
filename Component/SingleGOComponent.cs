using UnityEngine;

/// <summary> 씬 내에 존재하는 오브젝트를 대상으로 하는 간단한 싱글톤 컴포넌트 </summary>
/// <remarks> 인스턴스는 <see cref="Awake"/> 함수에서 초기화한다. </remarks>
public abstract class SingleGOComponent<T> : MonoBehaviour where T : MonoBehaviour {
	public static T inst;

	public virtual void Awake() {
		inst = this as T;
	}
}
