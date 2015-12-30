using System;
using SampleCommonParts;

namespace SampleServer
{
	class Server: IServer
	{
		public void DoServerStuff()
		{
			Console.WriteLine("CLIENT IS CALLING");
		}
	}
}
