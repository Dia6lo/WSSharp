using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WSSharp
{
	public class SocketWrapper : ISocket
	{
		private readonly Socket socket;
		private Stream stream;
		private readonly TaskFactory taskFactory;
		private readonly CancellationTokenSource tokenSource;

		public SocketWrapper(Socket socket)
		{
			tokenSource = new CancellationTokenSource();
			taskFactory = new TaskFactory(tokenSource.Token);
			this.socket = socket;
			if (this.socket.Connected)
				stream = new NetworkStream(this.socket);
		}

		private Stream Stream => stream = stream ?? new NetworkStream(socket);

		public void Listen(int backlog)
		{
			socket.Listen(backlog);
		}

		public async Task Connect(EndPoint endPoint, Action onComplete, Action<Exception> onError)
		{
			try {
				var args = new SocketAsyncEventArgs { RemoteEndPoint = endPoint };
				args.Completed += (sender, eventArgs) => onComplete();
				await taskFactory.StartNew(() => socket.ConnectAsync(args));
			}
			catch (Exception e) {
				onError(e);
			}
		}

		public void Bind(EndPoint endPoint)
		{
			socket.Bind(endPoint);
		}

		public bool Connected => socket.Connected;

		public async Task Receive(byte[] buffer, Action<int> onComplete, Action<Exception> onError, int offset)
		{
			try {
				var count = await Stream.ReadAsync(buffer, 0, buffer.Length, tokenSource.Token);
				onComplete(count);
			}
			catch (Exception e) {
				onError(e);
			}
		}

		public async Task Accept(Action<ISocket> onComplete, Action<Exception> onError)
		{
			try {
				var args = new SocketAsyncEventArgs();
				args.Completed += (sender, eventArgs) => 
					onComplete(new SocketWrapper(eventArgs.AcceptSocket));
				await taskFactory.StartNew(() => { socket.AcceptAsync(args); });
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
			socket?.Close();
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