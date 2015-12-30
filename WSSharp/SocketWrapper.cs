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

		public Task Connect(EndPoint endPoint, Action callback, Action<Exception> error)
		{
			try {
				Func<AsyncCallback, object, IAsyncResult> begin =
					(cb, s) => socket.BeginConnect(endPoint, cb, s);

				var task = Task.Factory.FromAsync(begin, socket.EndConnect, null);
				task.ContinueWith(t => callback(), TaskContinuationOptions.NotOnFaulted)
					.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
				task.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
				return task;
			}
			catch (Exception e) {
				error(e);
				return null;
			}
		}

		public void Bind(EndPoint endPoint)
		{
			socket.Bind(endPoint);
		}

		public bool Connected => socket.Connected;

		public Task<int> Receive(byte[] buffer, Action<int> callback, Action<Exception> error, int offset)
		{
			try {
				Func<AsyncCallback, object, IAsyncResult> begin =
					(cb, s) => Stream.BeginRead(buffer, offset, buffer.Length, cb, s);

				var task = Task.Factory.FromAsync(begin, Stream.EndRead, null);
				task.ContinueWith(t => callback(t.Result), TaskContinuationOptions.NotOnFaulted)
					.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
				task.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
				return task;
			}
			catch (Exception e) {
				error(e);
				return null;
			}
		}

		public Task<ISocket> Accept(Action<ISocket> callback, Action<Exception> error)
		{
			Func<IAsyncResult, ISocket> end =
				r => tokenSource.Token.IsCancellationRequested ? null : new SocketWrapper(socket.EndAccept(r));
			var task = taskFactory.FromAsync(socket.BeginAccept, end, null);
			task.ContinueWith(t => callback(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion)
				.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
			task.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
			return task;
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

		public Task Send(byte[] buffer, Action callback, Action<Exception> error)
		{
			if (tokenSource.IsCancellationRequested)
				return null;

			try {
				Func<AsyncCallback, object, IAsyncResult> begin =
					(cb, s) => Stream.BeginWrite(buffer, 0, buffer.Length, cb, s);

				var task = Task.Factory.FromAsync(begin, Stream.EndWrite, null);
				task.ContinueWith(t => callback(), TaskContinuationOptions.NotOnFaulted)
					.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
				task.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);

				return task;
			}
			catch (Exception e) {
				error(e);
				return null;
			}
		}
	}
}