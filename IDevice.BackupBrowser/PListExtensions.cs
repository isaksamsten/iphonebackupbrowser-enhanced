using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PList;
using System.Collections.Specialized;

namespace IDevice
{
    public static class StringCollectionExtension
    {
        public static string[] ToArray(this StringCollection collection)
        {
            List<string> paths = new List<string>();
            if (collection == null)
                return paths.ToArray();

            foreach (string s in collection)
                paths.Add(s);

            return paths.ToArray();
        }

    }
    public static class PListExtensions
    {
        public static T ToType<T>(this PList.IPListElement element) where T : PList.IPListElement
        {
            return (T)element;
        }

        public static string TypeName(this IPListElement element)
        {
            return element.GetType().Name;
        }

        public static object Value(this PList.IPListElement element)
        {
            object value = null;
            switch (element.Tag)
            {
                case "string": value = element.ToType<PListString>().Value; break;
                case "date": value = element.ToType<PListDate>().Value; break;
                case "boolean": value = element.ToType<PListBool>().Value; break;
            }

            return value;
        }

        public static IPListElement Find(this PListDict dict, Func<IPListElement, bool> where)
        {
            foreach (var p in dict)
            {
                bool value = where.Invoke(p.Value);
                if (value)
                {
                    return p.Value;
                }
            }

            return null;
        }

        public static object Value(this KeyValuePair<string, IPListElement> p)
        {
            return p.Value.Value();
        }
    }
}
