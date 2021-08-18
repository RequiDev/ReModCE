using System;
using MelonLoader;

namespace ReModCE.Loader
{
    /// <summary>
    /// Logger class that forwards all logging to MelonLoader through the ReModCE Loader so the colors are set up properly.
    /// </summary>
    public static class ReLogger
    {
        public static void Msg(string txt) => MelonLogger.Msg(txt);
        public static void Msg(string txt, params object[] args) => MelonLogger.Msg(txt, args);
        public static void Msg(object obj) => MelonLogger.Msg(obj);
        public static void Msg(ConsoleColor txtcolor, string txt) => MelonLogger.Msg(txtcolor, txt);
        public static void Msg(ConsoleColor txtcolor, string txt, params object[] args) => MelonLogger.Msg(txtcolor, txt, args);
        public static void Msg(ConsoleColor txtcolor, object obj) => MelonLogger.Msg(txtcolor, obj);

        public static void Warning(string txt) => MelonLogger.Warning(txt);
        public static void Warning(string txt, params object[] args) => MelonLogger.Warning(txt, args);
        public static void Warning(object obj) => MelonLogger.Warning(obj);

        public static void Error(string txt) => MelonLogger.Error(txt);
        public static void Error(string txt, params object[] args) => MelonLogger.Error(txt, args);
        public static void Error(object obj) => MelonLogger.Error(obj);
    }
}
