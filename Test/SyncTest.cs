using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using NLog;

namespace Test
{
	public class SyncTest
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		
		bool _listening = true;
		HttpListener listener;
		
		public void Run(string url = "http://localhost:5000/")
		{
			listener = new HttpListener();

			listener.Start();
			listener.Prefixes.Add(url);

			Thread t = new Thread(StartWaitingForConnections);
			t.Start();
		}
		
		public void Stop()
		{
			_listening = false;
			Thread.Sleep(250);
			
			listener.Close();
		}
		
		void StartWaitingForConnections()
		{
			// just handle each request as it comes in.
			while (_listening)
			{
				try
				{
					HttpListenerContext request = listener.GetContext();
					processRequest(request);
				}
				catch (Exception e) { 
					logger.Info(e.Message); Console.Write(">"); 
				}
			}
		}

		public void processRequest(HttpListenerContext eventArgs)
		{
			eventArgs.Response.StatusCode = (int)HttpStatusCode.OK;
			
			string msg = "<html><body>Here it is!</body></html>";
			// logger.Info(msg);
			
			eventArgs.Response.ContentLength64 = msg.Length;
			using (Stream s = eventArgs.Response.OutputStream)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(msg);
				s.Write(bytes, 0, bytes.Length);
			}
		}
	}
}

