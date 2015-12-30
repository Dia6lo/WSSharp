using System;
using System.Threading;
using SampleCommonParts;
using WSSharp;

namespace SampleClient
{
	internal static class Program
	{
		private static Client clientHandler;

		private static void Main(string[] args)
		{
			Thread.Sleep(500);
			clientHandler = new Client();
			var client = new WebSocketClient<IClient, IServer>("ws://127.0.0.1:8181", clientHandler);
			var task = client.Connect(OnConnection);
			var spinner = new ConsoleSpinner();
			Console.Write("Connecting...");
			while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
			{
				spinner.Turn();
			}
			Console.ReadKey();
		}

		private static void OnConnection(WebSocketConnection<IClient, IServer> webSocketConnection)
		{
			webSocketConnection.Outcoming.DoServerStuff();
		}
	}
}