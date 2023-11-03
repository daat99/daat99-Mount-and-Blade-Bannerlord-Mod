using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Xml
{
    public static class XmlExtensions
    {
        public static T ChangeType<T>(this object obj) => (T)Convert.ChangeType(obj, typeof(T));

        public static T ReadChildTextAs<T>(this XmlNode xmlNode, string name, T defaultValue = default(T))
        {
            T result = defaultValue;
            try
            {
                string innerText = xmlNode?.SelectSingleNode(name)?.InnerText ?? "";
                result = innerText.ChangeType<T>();
            }
            catch (Exception) { }
            return result;
        }

        public static XmlElement GetFirstElementByName(this XmlNode xmlNode, string name) => xmlNode?.SelectNodes(name)?.OfType<XmlElement>()?.FirstOrDefault();
    }

}
