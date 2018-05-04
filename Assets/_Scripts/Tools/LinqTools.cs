using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hedge
{
    namespace Tools
    {
        static public class LinqTools
        {

            public static bool NotNullOrEmpty<T>(this IEnumerable<T> items)
            {
                return items != null && items.Any();
            }
        }
    }
}
