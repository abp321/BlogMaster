using BlogMaster.Client.Utility;
using System.Collections.Concurrent;

namespace BlogMaster.Client.Taskschedulers
{
    public partial class BackgroundTaskScheduler : TaskScheduler, IDisposable
    {
        private bool _isDisposed;
        private bool _disposing;
        private static readonly BackgroundTaskScheduler _instance = new();
        public static BackgroundTaskScheduler Instance => _instance;

        private readonly ConcurrentQueue<Task> _taskQueue = [];
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        public TaskFactory BackgroundTaskFactory { get; }

        public BackgroundTaskScheduler()
        {
            AsyncTimer.SetTimerQueueAction(Execute);
            BackgroundTaskFactory = CreateTaskFactory();
        }

        private TaskFactory CreateTaskFactory()
        {
            return new(
            CancellationToken,
            TaskCreationOptions.DenyChildAttach,
            TaskContinuationOptions.DenyChildAttach,
            this);
        }

        private void Execute()
        {
            try
            {
                while (_taskQueue.TryDequeue(out var task))
                {
                    TryExecuteTask(task);
                }
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
        }

        protected override void QueueTask(Task task)
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            _taskQueue.Enqueue(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return [.. _taskQueue];
        }

        public void Dispose()
        {
            if (Volatile.Read(ref _isDisposed) || Volatile.Read(ref _disposing)) return;

            Volatile.Write(ref _disposing, true);
            try
            {
                _cancellationTokenSource.Cancel();
                _taskQueue.Clear();
                _cancellationTokenSource.Dispose();
                AsyncTimer.Dispose();
            }
            finally
            {
                Volatile.Write(ref _isDisposed, true);
                GC.SuppressFinalize(this);
            }
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    }
}
