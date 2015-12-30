using WSSharp;

namespace SampleCommonParts
{
	public interface IClient
	{
		[MethodContract]
		void DoClientStuff();

		[MethodContract]
		void DoOtherClientStuff();
	}
}
