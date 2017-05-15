using System;
using System.Collections.Generic;
namespace AssemblyCSharp
{
	internal class ConcurrentQueue<T> 
	{

		private readonly object queueLock = new object();   
		private Queue<T> queue = new Queue<T>();

		public int Count 
		{
			get 
			{
				lock(queueLock)
				{
					return queue.Count;
				}
			}
		}

		public T Peek()
		{
			lock(queueLock)
			{
				return queue.Peek();
			}
		}

		public void Enqueue(T obj)
		{
			lock(queueLock) 
			{
				queue.Enqueue(obj);
			}
		}
		public T[] CopyTo()
		{
			lock(queueLock)
			{
				if(queue.Count == 0)
				{
					return new T[0];
				}

				T[] values = new T[queue.Count];
				queue.CopyTo(values, 0);	
				return values;
			}
		}
		public T Dequeue()
		{
			lock(queueLock)
			{
				return queue.Dequeue();
			}
		}
		public bool TryDequeue(out T state)
		{
			lock(queueLock)
			{
				if (queue.Count > 0) {
					state = queue.Dequeue ();
					return true;
				} else {
					state = default(T);
					return false;
				}

			}
		}

		public void Clear()
		{
			lock(queueLock)
			{
				queue.Clear();
			}
		}
	}
}

