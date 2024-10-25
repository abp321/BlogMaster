namespace BlogMaster.Utility
{
    public class SimpleTaskSceduler(TaskScheduler? scheduler = null)
    {
        private readonly TaskFactory _taskFactory = new(
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskContinuationOptions.DenyChildAttach,
            scheduler ?? TaskScheduler.Default);

        public void Schedule(Action action)
        {
            _taskFactory.StartNew(action);
        }
    }
}
