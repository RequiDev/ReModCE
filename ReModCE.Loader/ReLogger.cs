using System;
using MelonLoader;

namespace ReModCE.Loader
{
    /// <summary>
    /// Logger class that forwards all logging to MelonLoader through the ReModCE Loader so the colors are set up properly.
    /// </summary>
    public class ReLogger
    {
        private static MelonLogger.Instance _instance;

        public ReLogger(MelonLogger.Instance instance)
        {
            _instance = instance;
        }

        public static void Msg(string txt) => _instance.Msg(txt);
        public static void Msg(string txt, params object[] args) => _instance.Msg(txt, args);
        public static void Msg(object obj) => _instance.Msg(obj);
        public static void Msg(ConsoleColor txtcolor, string txt) => _instance.Msg(txtcolor, txt);
        public static void Msg(ConsoleColor txtcolor, string txt, params object[] args) => _instance.Msg(txtcolor, txt, args);
        public static void Msg(ConsoleColor txtcolor, object obj) => _instance.Msg(txtcolor, obj);

        public static void Warning(string txt) => _instance.Warning(txt);
        public static void Warning(string txt, params object[] args) => _instance.Warning(txt, args);
        public static void Warning(object obj) => _instance.Warning(obj);

        public static void Error(string txt) => _instance.Error(txt);
        public static void Error(string txt, params object[] args) => _instance.Error(txt, args);
        public static void Error(object obj) => _instance.Error(obj);
    }
}
