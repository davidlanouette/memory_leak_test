using System;
using System.IO;
using System.Net;

namespace Test
{
	public class HttpRequestEventArgs : EventArgs
	{
		public HttpRequestEventArgs(HttpListenerContext requestContext)
		{
			RequestContext = requestContext;
			RequestStream = requestContext.Request.InputStream;
			ResponseStream = requestContext.Response.OutputStream;
		}
		
		public HttpListenerContext RequestContext { get; private set; }
		
		public HttpListenerRequest Request { get { return RequestContext.Request; } }
		
		public HttpListenerResponse Response { get { return RequestContext.Response; } }
		
		public Stream RequestStream { get; private set; }
		
		public Stream ResponseStream { get; private set; }
	}
}

