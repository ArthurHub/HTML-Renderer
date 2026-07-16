using System.Collections.Generic;

#if !NET40 && !SL50

namespace System.Threading.Tasks
{
    internal static class TaskEx
    {
        public static Task Run(Action action, CancellationToken cancel)
        {
            return Task.Run(action, cancel);
        }

        public static Task Delay(int millisecondsDelay, CancellationToken cancel)
        {
            return Task.Delay(millisecondsDelay, cancel);
        }

        public static Task WhenAll(IEnumerable<Task> tasks)
        {
            return Task.WhenAll(tasks);
        }
    }
}

#endif