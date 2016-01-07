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

		public void DisplayStuff(Person stuff)
		{
			Console.WriteLine("CLIENT IS ASKING TO DISPLAY STUFF: " + stuff.Name + " " + stuff.Age);
		}
	}
}
