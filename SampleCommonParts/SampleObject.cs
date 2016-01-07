using ProtoBuf;

namespace SampleCommonParts
{
	[ProtoContract]
	public class Person
	{
		[ProtoMember(1)]
		public string Name;
		[ProtoMember(2)]
		public int Age;
	}
}
