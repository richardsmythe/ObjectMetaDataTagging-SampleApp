using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Helpers
{
    public static class ObjectStateHelper
    {
        public static void CheckObjectState(object obj)
        {
            CheckExistenceState(obj);
            CheckConcurrencyState(obj);
            CheckErrorState(obj);
            CheckVisibilityState(obj);
        }

        // get fine-grained information about its internal state.

        private static void CheckErrorState(object obj)
        {
            throw new NotImplementedException();
        }

        private static void CheckConcurrencyState(object obj)
        {
            // logic to check if the object is designed to be accessed in a thread-safe manner
            if (obj != null)
            {
                var isThreadSafe = obj.GetType().GetCustomAttributes(typeof(SemaphoreSlim), true).Length > 0;
                if (isThreadSafe) Console.WriteLine(obj.GetType().Name + "is threadsafe.");
            }
        }

        public static bool CheckVisibilityState(object obj) => obj.GetType().IsVisible;

        private static bool CheckExistenceState(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            // this may no work as i intend it to
            var weakRef = new WeakReference(obj);
            GC.Collect(); 
            return weakRef.IsAlive;
        }
    }
}
