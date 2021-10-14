using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace ReModCE.Core
{
    public static class XrefUtils
    {

        /// <summary>
        /// Returns if a string is contained within the given method's body.
        /// </summary>
        /// <param name="method">The method to check</param>
        /// <param name="match">The string to check</param>
        public static bool CheckMethod(MethodInfo method, string match)
        {
            try
            {
                return XrefScanner.XrefScan(method)
                    .Where(instance => instance.Type == XrefType.Global && instance.ReadAsObject().ToString().Contains(match)).Any();
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Returns if the given method is called by the other given method.
        /// </summary>
        /// <param name="method">The method to check</param>
        /// <param name="methodName">The name of the method that uses the given method</param>
        /// <param name="type">The type of the method that uses the given method</param>
        public static bool CheckUsedBy(MethodInfo method, string methodName, Type type = null)
        {
            foreach (XrefInstance instance in XrefScanner.UsedBy(method))
            {
                if (instance.Type == XrefType.Method)
                {
                    try
                    {
                        if ((type == null || instance.TryResolve().DeclaringType == type) && instance.TryResolve().Name.Contains(methodName))
                            return true;
                    }
                    catch
                    {

                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns whether the given method is using another the other given method.
        /// </summary>
        /// <param name="method">The method to check</param>
        /// <param name="methodName">The name of the method that is used by the given method</param>
        /// <param name="type">The type of the method that is used by the given method</param>
        public static bool CheckUsing(MethodInfo method, string methodName, Type type = null)
        {
            foreach (XrefInstance instance in XrefScanner.XrefScan(method))
            {
                if (instance.Type == XrefType.Method)
                {
                    try
                    {
                        if ((type == null || instance.TryResolve().DeclaringType == type) && instance.TryResolve().Name.Contains(methodName))
                            return true;
                    }
                    catch
                    {

                    }
                }
            }
            return false;
        }
    }
}
