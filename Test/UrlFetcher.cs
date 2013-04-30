using System;
using System.Net;

namespace Test
{
	public class UrlFetcher
	{
		int count = 1;
		string url = "http://www.yahoo.com";
		
		public UrlFetcher (int count, string url)
		{
			this.count = count;
			this.url = url;
		}
		
		public void Get()
		{
			try
			{
				using(var client = new WebClient())
				{
					int i = 0;
					
					Console.WriteLine ();
					
					while (i++ < count)
					{
						client.DownloadString(url);
						Console.Write('.');
					}
					Console.WriteLine ();
				}
			}
			catch(Exception e) {
				Console.WriteLine ("OOPS!!!  {0}", e.ToString ());
			}
		}

	}
}

