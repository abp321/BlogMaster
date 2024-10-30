using BlogMaster.Client.Taskschedulers;

namespace BlogMaster.Client.Utility
{
    public static class BackgroundTask
    {
        public static TaskFactory Factory => BackgroundTaskScheduler.Instance.BackgroundTaskFactory;
        public static void Schedule(Action action) => Factory.StartNew(action);
        public static Task Run(Action action) => Factory.StartNew(action);
        public static Task<T> Run<T>(Func<T> function) => Factory.StartNew(function);
    }
}
