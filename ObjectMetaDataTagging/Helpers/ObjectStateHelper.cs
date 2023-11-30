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
            //CheckConcurrencyState(obj);
            //CheckErrorState(obj);
            //CheckVisibilityState(obj);
        }

        // get fine-grained information about its internal state.


        private static bool CheckExistenceState(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            // this may not work as i intend it to
            var weakRef = new WeakReference(obj);
            GC.Collect(); 
            return weakRef.IsAlive;
        }
    }
}
