

using System.Runtime.CompilerServices;

// 현재 어셈블리에서 에디터 어셈블리에 인터널 메소드를 보여줌
// -> 에디터 어셈블리에서 현재 어셈블리의 인터널 메소드 사용 가능하도록 변경
// assembly 키워드는 attribute를 어셈블리에 적용하도록 변경하는 역할을 한다.
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]