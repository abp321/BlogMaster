using System.Runtime.InteropServices;

namespace BlogMaster.Client.Native
{
    public unsafe static partial class QueueTimer
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressGCTransition]
        internal static partial bool CreateTimerQueueTimer(
        out nint phNewTimer,
        nint TimerQueue,
        delegate* unmanaged[Stdcall]<nint, byte, void> Callback,
        nint Parameter,
        uint DueTime,
        uint Period,
        uint Flags);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressGCTransition]
        internal static partial bool ChangeTimerQueueTimer(
        nint TimerQueue,
        nint Timer,
        uint DueTime,
        uint Period);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressGCTransition]
        internal static partial bool DeleteTimerQueueTimer(
            nint TimerQueue,
            nint Timer,
            nint CompletionEvent
        );
    }
}
