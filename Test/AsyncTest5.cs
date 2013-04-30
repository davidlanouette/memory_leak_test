using System;
using System.Net;
using System.IO;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Test
{
	public class AsyncTest5 : TestServer
	{
		private ListenerService5 listener;
			
		public void Run(string url = "http://localhost:5005")
		{
			listener = new ListenerService5(new Uri(url));

			listener.StartListening(processRequest);
		}
		
		public void Stop()
		{
			listener.StopListening();
			Thread.Sleep(250);
			listener.Dispose();
		}
		
		public void processRequest(HttpRequestEventArgs eventArgs)
		{
			eventArgs.Response.StatusCode = (int)HttpStatusCode.OK;
			
			string msg = "<html><body>Here it is!</body></html>";
			// logger.Info(msg);
			
			eventArgs.Response.ContentLength64 = msg.Length;
			using (Stream s = eventArgs.ResponseStream)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(msg);
				s.Write(bytes, 0, bytes.Length);
			}
		}
	}
	
	
	class ListenerService5 : IDisposable
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The I/O operation has been aborted because of either a thread exit or an application request.
		/// </summary>
		private const int RequestThreadAbortedExceptionCode = 995;
		
		private volatile HttpListener _listener;
		private volatile bool _listening;
		private CancellationTokenSource _cancelToken;
		private Action<HttpRequestEventArgs> _requestHandler;
		
		public ListenerService5(Uri listenerPrefix)
		{
			Uri = listenerPrefix;
			AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		}
		
		public Uri Uri { get; private set; }
		
		public AuthenticationSchemes AuthenticationSchemes { get; set; }
		
		public AuthenticationSchemeSelector AuthenticationSchemeSelector { get; set; }
		
		public bool Listening
		{
			get
			{
				return _listening;
			}
			
			private set
			{
				_listening = value;
			}
		}
		
		public string BasicPassword { get; set; }
		
		public void Dispose()
		{
			StopListening();
		}
		
		public void StartListening(Action<HttpRequestEventArgs> requestHandler)
		{
			StopListening();
			
			_cancelToken = new CancellationTokenSource();
			_requestHandler = requestHandler;
			
			var prefix = GetListenerPrefix();
			
			_listener = new HttpListener();
			_listener.Prefixes.Add(prefix);
			_listener.AuthenticationSchemes = AuthenticationSchemes;
			_listener.IgnoreWriteExceptions = true;
			_listener.UnsafeConnectionNtlmAuthentication = true;
			
			_listener.Start();
			Listening = true;
			
			Thread t = new Thread(StartWaitingForConnections);
			t.Start();
		}
		
		public void StopListening()
		{
			Listening = false;
			
			if (_cancelToken != null && !_cancelToken.IsCancellationRequested)
			{
				_cancelToken.Cancel();
			}
			
			if (_listener != null)
			{
				try
				{
					_listener.Stop();
				}
				catch (Exception)
				{
					// the listener crashing on stop is irrelevant since we are destroying it anyway 
				}
				
				_listener.Close();
				_listener = null;
			}
		}
		
		private string GetListenerPrefix()
		{
			var prefix = new StringBuilder();
			
			prefix.Append(Uri.Scheme + "://");
			prefix.Append("+:" + Uri.Port);
			prefix.Append(Uri.PathAndQuery);
			
			return prefix.ToString();
		}
		
		private void StartWaitingForConnections()
		{
			var getContextTask = Task.Factory.FromAsync<HttpListenerContext>(_listener.BeginGetContext,
			                                                                 EndGetListenerContext,
			                                                                 null);
			
			var handleRequestTask = getContextTask.ContinueWith(antecedent =>
			                                                    {
				if (_listening)
				{
					if (antecedent.Result != null)
					{
						try
						{
							HandleIncomingRequest(antecedent.Result);
							Thread.Sleep(500);
						}
						catch (HttpListenerException exception)
						{
							logger.Info("OOPS, and exception occured: {0}", exception.Message);
						}
						
						// now, start up a new task for the next connection
						StartWaitingForConnections();
					}
					else
					{
						logger.Info("Antecedent was null");
					}
				}
			},
			TaskContinuationOptions.OnlyOnRanToCompletion);
			
			getContextTask.ContinueWith(antecedent => HandleGetContextException(antecedent.Exception),
			                            TaskContinuationOptions.OnlyOnFaulted);
			
			handleRequestTask.ContinueWith(antecedent => HandleRequestHandlerTaskException(antecedent.Exception),
			                               TaskContinuationOptions.OnlyOnFaulted);
			
			
		}
		
		private HttpListenerContext EndGetListenerContext(IAsyncResult asyncResult)
		{
			try
			{
				if (_listener != null)
				{
					return _listener.EndGetContext(asyncResult);
				}
				
				return null;
			}
			catch (HttpListenerException exception)
			{
				// Check for I/O operation has been aborted because of either a thread exit or an application request.
				if (exception.ErrorCode == RequestThreadAbortedExceptionCode)
				{
					return null;
				}
				
				throw;
			}
		}		
		
		private void HandleIncomingRequest(object data)
		{
			HttpListenerContext context = data as HttpListenerContext;
			
			if (context != null)
				_requestHandler(new HttpRequestEventArgs(context));
			
		}
		
		private void HandleGetContextException(AggregateException exception)
		{
			logger.Info("OOPS! " + exception.Message);
		}
		
		private static void HandleRequestHandlerTaskException(AggregateException exception)
		{
			logger.Info("OOPS! " + exception.Message);
		}
	}
}

