using System;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace WindBot
{
    public static class Logger
    {
        private static readonly object _syncRoot = new object();

        // Each server-mode bot is driven by its own worker thread, so its log context must not be shared.
        [ThreadStatic]
        private static Func<string> _contextProvider;

        public static void SetContext(Func<string> contextProvider)
        {
            _contextProvider = contextProvider;
        }

        public static void ClearContext()
        {
            _contextProvider = null;
        }

        public static void WriteLine(string message)
        {
#if UNITY_5_3_OR_NEWER
            Debug.Log("[" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "] " + message);
#else
            lock (_syncRoot)
                Console.WriteLine("[" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "] " + message);
#endif
        }

        public static void DebugWriteLine(string message, bool printStackTrace = false)
        {
#if UNITY_5_3_OR_NEWER
            Debug.Log("[" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "] " + message);
#elif DEBUG
            lock (_syncRoot)
            {
                Console.WriteLine("[" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "] " + message);
                if (printStackTrace)
                    Console.WriteLine(new System.Diagnostics.StackTrace(1, true).ToString());
            }
#endif
        }

        public static void WriteErrorLine(string message)
        {
            WriteErrorLine(message, null, new System.Diagnostics.StackTrace(1, true).ToString());
        }

        public static void WriteErrorLine(string message, Exception exception)
        {
            WriteErrorLine(message, exception, null);
        }

        private static void WriteErrorLine(string message, Exception exception, string callStack)
        {
#if UNITY_5_3_OR_NEWER
            string text = "[" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "] " + message;
            string context = _contextProvider?.Invoke();
            if (!string.IsNullOrEmpty(context))
                text += "\nContext: " + context;
            if (exception != null)
                text += "\n" + exception;
            else if (!string.IsNullOrEmpty(callStack))
                text += "\nCall stack:\n" + callStack;
            Debug.LogError(text);
#else
            lock (_syncRoot)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Error.WriteLine("[" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "] " + message);
                Console.ResetColor();

                string context = _contextProvider?.Invoke();
                if (!string.IsNullOrEmpty(context))
                    Console.Error.WriteLine("Context: " + context);
                if (exception != null)
                    Console.Error.WriteLine(exception);
                else if (!string.IsNullOrEmpty(callStack))
                {
                    Console.Error.WriteLine("Call stack:");
                    Console.Error.WriteLine(callStack);
                }
            }
#endif
        }
    }
}
