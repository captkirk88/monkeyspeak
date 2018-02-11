using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Tests
{
    public class GC
    {
        public static bool IsServerGC { get => System.Runtime.GCSettings.IsServerGC; }

        /// <summary>
        /// Sets the low latency mode on the Garbage Collector.
        /// <para>
        /// See
        /// https://blogs.msdn.microsoft.com/dotnet/2012/07/20/the-net-framework-4-5-includes-new-garbage-collector-enhancements-for-client-and-server-apps/
        /// for more information.
        /// </para>
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetSustainedLowLatencyMode(bool value)
        {
            System.Runtime.GCSettings.LatencyMode = value ? System.Runtime.GCLatencyMode.SustainedLowLatency : System.Runtime.GCLatencyMode.Interactive;
        }

        /// <summary>
        /// Enables compaction on the GC heap to reduce memory footprint.
        /// <para>
        /// See
        /// https://blogs.msdn.microsoft.com/dotnet/2012/07/20/the-net-framework-4-5-includes-new-garbage-collector-enhancements-for-client-and-server-apps/
        /// for more information.
        /// </para>
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void CompactTheLargeObjectHeap(bool value)
        {
            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = value ?
                System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce : System.Runtime.GCLargeObjectHeapCompactionMode.Default;
        }

        public static void CollectNonBlocking(int generation = -1)
        {
            if (generation > System.GC.MaxGeneration) generation = System.GC.MaxGeneration;
            if (generation == -1)
            {
                System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced, false);
            }
            else System.GC.Collect(generation, GCCollectionMode.Forced, false);
        }

        public static string Statistics
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i <= System.GC.MaxGeneration - 1; i++)
                {
                    sb.AppendLine($"Gen {i} collection count = {System.GC.CollectionCount(i)}");
                }
                sb.Append($"Memory = {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
                return sb.ToString();
            }
        }
    }
}