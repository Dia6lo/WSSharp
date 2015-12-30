using System;
using System.Linq;
using SampleCommonParts;
using WSSharp;

namespace SampleServer
{
	internal static class Program
	{
		private static Server serverHandler;

		private static void Main(string[] args)
		{
			serverHandler = new Server();
			var server = new WebSocketServer<IServer, IClient>("ws://127.0.0.1:8181", serverHandler);
			server.Start(OnConnection);
			Console.ReadKey();
		}

		private static void OnConnection(WebSocketConnection<IServer, IClient> webSocketConnection)
		{
			webSocketConnection.Outcoming.DoClientStuff();
			webSocketConnection.Outcoming.DoOtherClientStuff();
		}
	}
}