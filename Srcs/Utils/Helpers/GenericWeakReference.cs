using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.Helpers
{
	public sealed class GenericWeakReference<T> where T : class
	{
		private WeakReference _weak = null;

		public GenericWeakReference(T target)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			_weak = new WeakReference(target, false);
		}

		public bool IsAlive
		{
			get
			{
				return (_weak.IsAlive && _weak.Target != null);
			}
		}

		public T Target
		{
			get
			{
				return (T)(_weak.Target);
			}
		}

		public T Get<T>()
		{
			return (T)_weak.Target;
		}
	}
}
