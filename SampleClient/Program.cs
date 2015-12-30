using System;
using System.Linq;
using WSSharp;

namespace SampleClient
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			var client = new WebSocketClient("ws://127.0.0.1:8181");
			client.Connect(OnConnection);
			Console.ReadKey();
		}

		private static void OnConnection(IWebSocketConnection connection)
		{
			connection.OnMessage = b => { PrintArray(b, b.Length); };
			var data = Enumerable.Range(7, 10).Select(i => (byte) i).ToArray();
			connection.Send(data);
		}

		private static void PrintArray(byte[] array, int count)
		{
			Console.Write("Array: ");
			for (var i = 0; i < count; i++) {
				Console.Write(array[i]);
			}
			Console.WriteLine();
		}
	}
}