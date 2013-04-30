using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using NLog;

namespace Test
{
	public class TaskTest
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public delegate int AsyncMethodCaller(int count);

		// hold onto all the task handles, so we can dispose them when they are complete.
		private List<Task> tasks = new List<Task>();
		private string formatStr = "{0,2}  {1,10}  {2,10}  {3,10}  {4,10}";

		public void Run(bool createTasks = true, int taskCount = 25000, int iterations = 10)
		{
			PrintHeader(createTasks, taskCount, iterations);

			for(int i = 0; i < iterations; i++)
			{
				// record memory stats
				long memBefore = GC.GetTotalMemory(false);
				long workingSetBefore = Process.GetCurrentProcess().WorkingSet64;

				GC.Collect();
				GC.Collect();
				GC.Collect();

				long memAfter = GC.GetTotalMemory(false);
				long workingSetAfter = Process.GetCurrentProcess().WorkingSet64;

				// print out the mem stats for this iteration
				logger.Info(formatStr, i, memBefore, memAfter, workingSetBefore, workingSetAfter);

				for(int tc = 0; tc < taskCount; tc++)
				{
					// start a async call.
					AsyncMethodCaller caller = new AsyncMethodCaller(this.StartTask);
					IAsyncResult res = caller.BeginInvoke(tc, null, null);

					if (createTasks)
					{
						// now, create a task factory to finish some more work.
						var t = Task.Factory.FromAsync(res, EndTask);

						// store the task handle to clean it up later
						tasks.Add(t);
					}
				}

				DisposeOfTasks();
				Thread.Sleep(500);
			}
		}
		
		public int StartTask(int count)
		{
			// some random "work"
			Random r = new Random(count);
			int s = r.Next(50);
			Thread.Sleep(s);

//			if (count % 500 == 0)
//				logger.Info("started " + count);

			return count;
		}

		public void EndTask(IAsyncResult asyncResult)
		{
			// do something here.
		}

		void PrintHeader(bool createTasks, int taskCount, int iterations)
		{
			logger.Info("TaskTest({0}, {1}, {2})", createTasks, taskCount, iterations);
			logger.Info("MemoryUse");
			logger.Info(" MB-before = GC.GetTotalMemory() before any garbage collection");
			logger.Info(" MB-after = GC.GetTotalMemory() after forcing garbage collection");
			logger.Info(" WS-before = Process.WorkingSet64 before any garbage collection");
			logger.Info(" WS-after = Process.WorkingSet64 after forcing garbage collection");

			string tenDashes = "----------";
			logger.Info(formatStr, "#", "MB-before", "MB-after", "WS-before", "WS-after");
			logger.Info(formatStr, "--", tenDashes, tenDashes, tenDashes, tenDashes);

		}

		void DisposeOfTasks()
		{
			// loop through all the tasks and dispose any that are complete
			while (tasks.Count > 0)
			{
				Console.Write("** Attempting to cleanup the tasks.  There are {0} tasks left to clean.  ", tasks.Count);
				Console.CursorLeft = 0;
				
				// wait a bit between each "cleanup"
				Thread.Sleep(500);
				
				for (int i = 0; i < tasks.Count; i++)
				{
					Task t = tasks[i];
					
					// if the task is completed, removed it from the list, and Dispose it.
					if (t.IsCompleted)
					{
						// logger.Info("  Getting rid of task #{0}", i);
						tasks.RemoveAt(i);
						t.Dispose();
					}
				}
			}
			Console.Write (" ".PadRight(Console.WindowWidth - 1));
			Console.SetCursorPosition(0, Console.CursorTop);
			// logger.Info("Disposed of up all Task objects.\t\t\t");
		}
	}
}

