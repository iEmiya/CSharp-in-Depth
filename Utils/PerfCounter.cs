using System;
using System.Runtime.InteropServices;

namespace CSharp_in_Depth.Utils
{
    public class PerfCounter
    {
        private Int64 _start;

        public void Start()
        {
            _start = 0;
            QueryPerformanceCounter(ref _start);
        }

        public float Finish()
        {
            Int64 finish = 0;
            QueryPerformanceCounter(ref finish);

            Int64 freq = 0;
            QueryPerformanceFrequency(ref freq);
            return (((float)(finish - _start) / (float)freq));
        }

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(ref Int64 performanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(ref Int64 frequency);
    }
}