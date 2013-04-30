using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using NLog;

namespace Test
{
	public class ByteTest
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public delegate int AsyncMethodCaller(int count);

		public void Run()
		{
			logger.Info(" How many iterations do you want? (numbers only...)");
			int cnt = int.Parse(Console.ReadLine());

			Run(cnt);
		}

		public void Run(int cnt)
		{
			for(int i = 0; i < cnt; i++)
			{
				// start a async call.
				AsyncMethodCaller caller = new AsyncMethodCaller(this.StartTask);
				caller.BeginInvoke(i, null, null);
			}
			logger.Info("Created {0} AsyncMethodCaller instances", cnt);

		}
		
		public int StartTask(int count)
		{
			byte[] b = new byte[1024];

			for(int i =0; i < 1024; i++)
			{
				b[i] = (byte) (i % 255);
			}

			return count;
		}
		
	}
}

