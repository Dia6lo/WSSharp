using System;
using SampleCommonParts;

namespace SampleClient
{
	class Client: IClient
	{
		public void DoClientStuff()
		{
			Console.WriteLine("SERVER IS CALLING");
		}

		public void DoOtherClientStuff()
		{
			Console.WriteLine("SERVER IS CALLING AGAIN");
		}
	}
}
