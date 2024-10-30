using BlogMaster.Client.Native;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlogMaster.Client.Utility
{
    public static unsafe partial class AsyncTimer
    {
        private const nint _timerQueueId = 1;
        private static readonly nint _timerQueueHandle;
        private static readonly delegate* unmanaged[Stdcall]<nint, byte, void> _timerQueueCallback = &TimerQueueTick;
        private const uint WT_EXECUTEINTIMERTHREAD = 0x00000020;

        private static Action? _timerQueueAction;
        private static readonly object _stateLock = new();
        private static int _isPaused;
        private static bool IsPaused
        {
            get => Interlocked.CompareExchange(ref _isPaused, 0, 0) == 1;
            set => Interlocked.Exchange(ref _isPaused, value ? 1 : 0);
        }

        static AsyncTimer()
        {
            if (!QueueTimer.CreateTimerQueueTimer(
                out _timerQueueHandle,
                nint.Zero,
                _timerQueueCallback,
                _timerQueueId,
                0u,
                1u,
                WT_EXECUTEINTIMERTHREAD))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static void SetTimerQueueAction(Action? newAction)
        {
            Interlocked.Exchange(ref _timerQueueAction, newAction);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        [SuppressGCTransition]
        private static void TimerQueueTick(nint lpParameter, byte TimerOrWaitFired)
        {
            _timerQueueAction?.Invoke();
        }

        public static void Pause()
        {
            if (IsPaused) return;

            lock (_stateLock)
            {
                try
                {
                    if (!QueueTimer.ChangeTimerQueueTimer(
                    nint.Zero,
                    _timerQueueHandle,
                    uint.MaxValue,
                    0))
                    {
                        throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                finally
                {
                    IsPaused = true;
                }
            }
        }

        public static void Resume()
        {
            if (!IsPaused) return;

            lock (_stateLock)
            {
                try
                {
                    if (!QueueTimer.ChangeTimerQueueTimer(
                        nint.Zero,
                        _timerQueueHandle,
                        0u,
                        1u))
                    {
                        throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                finally
                {
                    IsPaused = false;
                }
            }
        }

        public static void Dispose()
        {
            QueueTimer.DeleteTimerQueueTimer(nint.Zero, _timerQueueHandle, nint.Zero);
        }
    }
}
