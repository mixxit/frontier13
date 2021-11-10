using System;

namespace projectrarahat.src
{
    internal class LogUtils<T>
    {
        internal static void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[Server:{typeof(T)}] " + message);
        }
    }
}