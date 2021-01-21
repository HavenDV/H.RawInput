using System;
using System.Runtime.InteropServices;

namespace H.Hooks.Core.Interop
{
    internal static class Kernel32
    {
        /// <summary>
        /// Retrieves the thread identifier of the calling thread.
        /// </summary>
        /// <returns>The return value is the thread identifier of the calling thread.</returns>
        /// <remarks>
        /// Until the thread terminates, the thread identifier uniquely identifies
        /// the thread throughout the system.
        /// </remarks>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetCurrentThreadId();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nStdHandle"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);
    }
}
