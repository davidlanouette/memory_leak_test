using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Net;
using System.Web;
using System.Reflection;
using NLog;

namespace Test
{
	class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		
		const string URL0 = "http://localhost:5000/";
		const string URL1 = "http://localhost:5001/";
		const string URL2 = "http://localhost:5002/";
		const string URL3 = "http://localhost:5003/";
		const string URL4 = "http://localhost:5004/";
		const string URL5 = "http://localhost:5005/";
		
		public static void Main(string[] args)
		{
			string answer = string.Empty;
			while (answer != "q")
			{
				Console.WriteLine("Which test do you want?");
				Console.WriteLine("1. Syncrnous HTTP server test?");
				Console.WriteLine("2. Asyncrnous HTTP server test 1 with Threads?");
				Console.WriteLine("3. Asyncrnous HTTP server test 2 with Tasks?");
				Console.WriteLine("4. Asyncrnous HTTP server test 3 with Tasks - second method?");
				Console.WriteLine("5. Asyncrnous HTTP server test 4 with Tasks - second method with sleep()?");
				Console.WriteLine("6. Asyncrnous HTTP server test 5 with Tasks - ???");
				Console.WriteLine("13. Byte allocation test?");
				Console.WriteLine("14. Task test?");
				Console.WriteLine("15. Task test (without creating task objects)?");
                Console.WriteLine("99. Get runtime version information.");
				Console.WriteLine("A. Auto-test.  Rus all the tests and output to a log file");
				Console.WriteLine("0: GC?");
				Console.WriteLine("q. Quit");
				answer = Console.ReadLine();


				PrintMemoryUsage ("before");				

				if (answer == "1")
				{
					SyncTest s = StartSyncTestServer(URL0);
					WaitToEnd();
					s.Stop();					
				}
				
				if (answer == "2")
				{
					AsyncTest1 a = StartAsyncTest1Server(URL1);
					WaitToEnd();
					a.Stop();
				}

				if (answer == "3")
				{
					AsyncTest2 a = StartAsyncTest2Server(URL2);
					WaitToEnd();
					a.Stop();
				}
				
				if (answer == "4")
				{
					AsyncTest3 a = StartAsyncTest3Server(URL3);
					WaitToEnd();
					a.Stop ();
				}
				
				if (answer == "5")
				{
					AsyncTest4 a = StartAsyncTest4Server(URL4);
					WaitToEnd();
					a.Stop();
				}

				if (answer == "6")
				{
					AsyncTest5 a = StartAsyncTest5Server(URL5);
					WaitToEnd();
					a.Stop();
				}

				if (answer == "13")
				{
					ByteTest b = new ByteTest();
					b.Run ();
				}

				if (answer == "14")
				{
					TaskTest t = new TaskTest();
					t.Run(true, 50000, 50);
				}

				if (answer == "15")
				{
					TaskTest t = new TaskTest();
					t.Run(false, 50000, 50);
				}

                if (answer == "99")
                {
                    PrintRuntimeInfo();
                }

				if (answer == "A" || answer == "a")
				{
					autoTest();
				}

				if (answer == "0")
				{
					GC.Collect();
				}

				PrintMemoryUsage ("after");
			}
			
			
		}

		static void PrintMemoryUsage (string when)
		{
			StringBuilder b = new StringBuilder();

			long mem = GC.GetTotalMemory (false);
			b.AppendFormat("MemoryUse ({0}) = {1}\n", when, mem.ToString ("N"));
			long workingSet = Process.GetCurrentProcess ().WorkingSet64;
			b.AppendFormat("WorkingSet ({0}) = {1}", when, workingSet.ToString ("N"));

			logger.Info(b.ToString());
		}

        private static void PrintRuntimeInfo()
        {
            var version = System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion();
			var monoVersion = GetMonoVersion ();

            logger.Info(".NET runtime version = {0}", version);
			logger.Info ("Mono version = {0}", monoVersion);
        }

		private static void autoTest()
		{
			Thread thread;
			string url;
			int testCount = 50000;
			
			PrintRuntimeInfo();
			PrintMemoryUsage("initial");
			
			// run the ByteTest
			ByteTest b = new ByteTest();
			b.Run (testCount*10);

			GC.Collect();
			PrintMemoryUsage("post ByteTest");

			//start the Sync test server
			url = "http://localhost:5000/";			
			StartSyncTestServer(url);
			
			UrlFetcher u = new UrlFetcher(testCount, url);			
			thread = new Thread(u.Get);
			thread.Start();
					
			thread.Join();

			GC.Collect();
			PrintMemoryUsage("post SyncTest");
			
			
			// Test the first Async server
			url = "http://localhost:5001/";
			AsyncTest1 a1 = StartAsyncTest1Server(url);
			
			u = new UrlFetcher(testCount, url);	
			thread = new Thread(u.Get);
			thread.Start();
			thread.Join();
			
			a1.Stop();
			
			GC.Collect();
			PrintMemoryUsage("post AsyncTest1");


			// Test the second Async server
			url = "http://localhost:5002/";
			AsyncTest2 a2 = StartAsyncTest2Server(url);
			
			u = new UrlFetcher(testCount, url);	
			thread = new Thread(u.Get);
			thread.Start();
			thread.Join();		
			
			a2.Stop();
			
			GC.Collect();
			PrintMemoryUsage("post AsyncTest2");


			// Test the third Async server
			url = "http://localhost:5003/";
			AsyncTest3 a3 = StartAsyncTest3Server(url);
			
			u = new UrlFetcher(testCount, url);	
			thread = new Thread(u.Get);
			thread.Start();
			thread.Join();
			
			a3.Stop();
			
			GC.Collect();
			PrintMemoryUsage("post AsyncTest3");

			
			// Test the forth Async server
			url = "http://localhost:5004";
			AsyncTest4 a4 = StartAsyncTest4Server(url);

			u = new UrlFetcher(testCount, url);	
			thread = new Thread(u.Get);
			thread.Start();
			thread.Join();
			
			a4.Stop();
			
			GC.Collect();
			PrintMemoryUsage("post AsyncTest4");

			
			// Test the fifth Async server
			url = "http://localhost:5005/";
			AsyncTest5 a5 = StartAsyncTest5Server(url);
			
			u = new UrlFetcher(testCount, url);	
			thread = new Thread(u.Get);
			thread.Start();
			thread.Join();
			
			a5.Stop ();
			
			GC.Collect();
			PrintMemoryUsage("post AsyncTest5");
			
			
			// run the task test
			TaskTest t1 = new TaskTest();
			t1.Run(true, testCount, 50);

			GC.Collect();
			PrintMemoryUsage("post TaskTest (true, 500000, 50)");

			
			/* This is known to leak a LOT
			TaskTest t2 = new TaskTest();
			t2.Run(false, testCount, 50);

			GC.Collect();
			PrintMemoryUsage("post TaskTest (false, 50000, 50)");
			*/

			PrintMemoryUsage("finished");
		}



		private static SyncTest StartSyncTestServer(string url)
		{
			SyncTest s = new SyncTest();
			s.Run(url);
			return s;
		}

		static AsyncTest1 StartAsyncTest1Server (string url)
		{
			AsyncTest1 a = new AsyncTest1();
			a.Run (url);
			return a;
		}

		static AsyncTest2 StartAsyncTest2Server (string url)
		{
			AsyncTest2 a = new AsyncTest2 ();
			a.Run (url);
			return a;
		}

		static AsyncTest3 StartAsyncTest3Server (string url)
		{
			AsyncTest3 a = new AsyncTest3 ();
			a.Run (url);
			return a;
		}

		static AsyncTest4 StartAsyncTest4Server (string url)
		{
			AsyncTest4 a = new AsyncTest4 ();
			a.Run (url);
			return a;
		}

		static AsyncTest5 StartAsyncTest5Server (string url)
		{
			AsyncTest5 a = new AsyncTest5 ();
			a.Run (url);
			return a;
		}
		static void WaitToEnd()
		{
			Console.Write("Press Enter to stop");
			Console.ReadLine();
		}

		private static string GetMonoVersion()
		{
			Type type = Type.GetType("Mono.Runtime");
			if (type != null)
			{                                          
				// This works for Mono 3.0.5 and above
				MethodInfo displayName = type.GetMethod("GetDisplayName"); 

				// This works for Mono 3.0.4 and below
				if (displayName == null)
					displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static); 

				if (displayName != null)
					return (string)displayName.Invoke(null, null);
			}
			return "Unknown";
		}
	}
}
