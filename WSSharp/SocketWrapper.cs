using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WSSharp
{
	public class SocketWrapper
	{
		private Stream stream;
		private readonly TaskFactory taskFactory;
		private readonly CancellationTokenSource tokenSource;

		public SocketWrapper(Socket socket)
		{
			tokenSource = new CancellationTokenSource();
			taskFactory = new TaskFactory(tokenSource.Token);
			Socket = socket;
			if (Socket.Connected)
				stream = new NetworkStream(Socket);
		}

		public Socket Socket { get; }

		private Stream Stream => stream = stream ?? new NetworkStream(Socket);

		public void Listen(int backlog)
		{
			Socket.Listen(backlog);
		}

		public async Task Connect(EndPoint endPoint, Action onComplete, Action<Exception> onError)
		{
			try {
				var args = new SocketAsyncEventArgs { RemoteEndPoint = endPoint };
				args.Completed += (sender, eventArgs) => onComplete();
				await taskFactory.StartNew(() => Socket.ConnectAsync(args));
			}
			catch (Exception e) {
				onError(e);
			}
		}

		public void Bind(EndPoint endPoint)
		{
			Socket.Bind(endPoint);
		}

		public bool Connected => Socket.Connected;


		public async Task Receive(byte[] buffer, Action<int> onComplete, Action<Exception> onError, int offset = 0)
		{
			try {
				var count = await Stream.ReadAsync(buffer, 0, buffer.Length, tokenSource.Token);
				onComplete(count);
			}
			catch (Exception e) {
				onError(e);
			}
		}

		public async Task Accept(Action<SocketWrapper> onComplete, Action<Exception> onError)
		{
			try {
				var args = new SocketAsyncEventArgs();
				args.Completed += (sender, eventArgs) => 
					onComplete(new SocketWrapper(eventArgs.AcceptSocket));
				await taskFactory.StartNew(() => { Socket.AcceptAsync(args); });
			}
			catch (Exception e) {
				onError(e);
			}
		}
		
		public void Dispose()
		{
			Close();
		}

		public void Close()
		{
			tokenSource.Cancel();
			stream?.Close();
			Socket?.Close();
		}

		public async Task Send(byte[] buffer, Action onComplete, Action<Exception> onError)
		{
			try {
				await Stream.WriteAsync(buffer, 0, buffer.Length, tokenSource.Token);
				onComplete();
			}
			catch (Exception e) {
				onError(e);
			}
		}
	}
}