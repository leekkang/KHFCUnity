
using System;

namespace KHFC {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class InspectorButtonAttribute : Attribute {
		public readonly string m_Msg;

		public InspectorButtonAttribute(string msg = "") {
			m_Msg = msg;
		}
	}
}
