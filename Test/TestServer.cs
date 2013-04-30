using System;
using System.Net;

namespace Test
{
	public interface TestServer
	{
		void Run(string url);
		void Stop();
		void processRequest(HttpRequestEventArgs request);
	}
}

