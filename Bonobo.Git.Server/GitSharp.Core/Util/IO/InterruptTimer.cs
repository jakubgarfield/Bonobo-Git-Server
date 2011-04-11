/*
 * Copyright (C) 2009, Google Inc.
 * Copyright (C) 2010, Henon <meinrad.recheis@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Eclipse Foundation, Inc. nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Tamir.SharpSsh.java.lang;
using Thread = Tamir.SharpSsh.java.lang.Thread;

namespace GitSharp.Core.Util
{

	// TODO: [henon] this approach does not work in .net. Either the calling thread must be aborted (which is problematic) or the IO stream closed. 
	// See how TimeoutStream uses a timer to abort IO.

	/// <summary>
	///  Triggers an interrupt on the calling thread if it doesn't complete a block.
	///  <para/>
	///  Classes can use this to trip an alarm interrupting the calling thread if it
	///  doesn't complete a block within the specified timeout. Typical calling
	///  pattern is:
	/// 
	/// <code>
	///  private InterruptTimer myTimer = ...;
	///  void foo() {
	///    try {
	///      myTimer.begin(timeout);
	///      // work
	///    } finally {
	///      myTimer.end();
	///    }
	///  }
	/// </code>
	/// <para/>
	///  An InterruptTimer is not recursive. To implement recursive timers,
	///  independent InterruptTimer instances are required. A single InterruptTimer
	///  may be shared between objects which won't recursively call each other.
	///  <para/>
	///  Each InterruptTimer spawns one background thread to sleep the specified time
	///  and interrupt the thread which called {@link #begin(int)}. It is up to the
	///  caller to ensure that the operations within the work block between the
	///  matched begin and end calls tests the interrupt flag (most IO operations do).
	///  <para/>
	///  To terminate the background thread, use {@link #terminate()}. If the
	///  application fails to terminate the thread, it will (eventually) terminate
	///  itself when the InterruptTimer instance is garbage collected.
	/// 
	/// </summary>
	public class InterruptTimer
	{
		private readonly AlarmState state;

		private readonly AlarmThread thread;

		private readonly AutoKiller autoKiller;

		/// <summary> Create a new timer with a default thread name./// </summary>
		public InterruptTimer()
			: this("JGit-InterruptTimer")
		{
		}

		/// <summary>
		///  Create a new timer to signal on interrupt on the caller.
		///  <para/>
		///  The timer thread is created in the calling thread's ThreadGroup.
		/// 
		///  <param name="threadName"> name of the timer thread.</param>
		/// </summary>
		public InterruptTimer(String threadName)
		{
			state = new AlarmState();
			autoKiller = new AutoKiller(state);
			thread = new AlarmThread(threadName, state);
			thread.start();
		}

		/// <summary>
		///  Arm the interrupt timer before entering a blocking operation.
		/// 
		///  <param name="timeout">
		///             number of milliseconds before the interrupt should trigger.
		///             Must be > 0.</param>
		/// </summary>
		public void begin(int timeout)
		{
			if (timeout <= 0)
				throw new ArgumentException("Invalid timeout: " + timeout);
			//Thread.interrupted();
			state.begin(timeout);
		}

		/// <summary> Disable the interrupt timer, as the operation is complete./// </summary>
		public void end()
		{
			state.end();
		}

		/// <summary> Shutdown the timer thread, and wait for it to terminate./// </summary>
		public void terminate()
		{
			state.terminate();
			//try {
			thread.InnerThread.Join();
			//} catch (InterruptedException e) {
			//   //
			//}
		}
	}

	public class AlarmThread : Thread
	{
		public AlarmThread(String name, AlarmState q)
			: base(q)
		{
			setName(name);
			InnerThread.IsBackground = true;
		}

		// Todo: [henon] this can easily break so we should better adapt our own Java-Thread implementation based on TamirSsh's Thread and expose the inner thread
		public System.Threading.Thread InnerThread
		{
			get
			{
				return typeof(Thread).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this) as System.Threading.Thread;
			}
		}
	}

	// The trick here is, the AlarmThread does not have a reference to the
	// AutoKiller instance, only the InterruptTimer itself does. Thus when
	// the InterruptTimer is GC'd, the AutoKiller is also unreachable and
	// can be GC'd. When it gets finalized, it tells the AlarmThread to
	// terminate, triggering the thread to exit gracefully.
	internal class AutoKiller
	{
		private AlarmState state;

		public AutoKiller(AlarmState s)
		{
			state = s;
		}

		~AutoKiller()
		{
			state.terminate();
		}
	}

	public class AlarmState : Runnable
	{


		private Thread callingThread;

		private long deadline;

		private bool terminated;

		public AlarmState()
		{
			callingThread = Thread.currentThread();
		}

		public void run()
		{
			lock (this)
			{
				while (!terminated && callingThread.isAlive())
				{
					//try
					//{
					if (0 < deadline)
					{
						long delay = deadline - now();
						if (delay <= 0)
						{
							deadline = 0;
							callingThread.interrupt();
						}
						else
						{
							Thread.sleep((int)delay);
						}
					}
					else
					{
						wait(1000);
					}
					//}
					//catch (InterruptedException e) // Note: [henon] Thread does not throw an equivalent exception in C# ??
					//{
					//   // Treat an interrupt as notice to examine state.
					//}
				}
			}
		}

		public void begin(int timeout)
		{
			lock (this)
			{
				if (terminated)
					throw new InvalidOperationException("Timer already terminated");
				callingThread = Thread.currentThread();
				deadline = now() + timeout;
				notifyAll();
			}
		}

		public void end()
		{
			lock (this)
			{
				//if (0 == deadline)
				//   Thread.interrupted(); // <-- Note: [henon] this code does nothing but reset an irrelevant java thread internal flag AFAIK (which is not supported by our thread implementation)
				//else
				deadline = 0;
				notifyAll();
			}
		}

		public void terminate()
		{
			lock (this)
			{
				if (!terminated)
				{
					deadline = 0;
					terminated = true;
					notifyAll();
				}
			}
		}

		private static long now()
		{
			return DateTime.Now.ToMillisecondsSinceEpoch();
		}

		#region --> Java helpers

		// Note: [henon] to simulate java's builtin wait and notifyAll we use a waithandle
		private AutoResetEvent wait_handle = new AutoResetEvent(false);

		private void wait(int milliseconds)
		{
			wait_handle.WaitOne(milliseconds);
		}

		private void notifyAll()
		{
			wait_handle.Set();
		}

		#endregion
	}

}
