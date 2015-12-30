using WSSharp;

namespace SampleCommonParts
{
	public interface IServer
	{
		[MethodContract]
		void DoServerStuff();
	}
}
