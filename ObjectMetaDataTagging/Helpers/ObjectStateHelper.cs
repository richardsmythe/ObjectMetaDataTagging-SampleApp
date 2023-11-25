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

        private static void CheckVisibilityState(object obj)
        {
            throw new NotImplementedException();
        }

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

        private static void CheckExistenceState(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
