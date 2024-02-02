
public static class IEnumerableExtension
{
    static IEnumerableExtension() { LicenceHelper.CheckLicense(); }
    public static bool HasItem<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable?.Any() == true;
    }
    /// <summary>
    /// 这个C#函数使用多任务处理技术来遍历一个列表中的每个元素，并对每个元素执行一个给定的操作。
    /// 它将列表分成多个子列表，并为每个子列表创建一个任务。任务在后台线程中运行，然后等待所有任务完成。最后，它释放任务的资源并清空任务列表。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="totalItems"></param>
    /// <param name="taskcount"></param>
    /// <param name="taskToRun"></param>
    public static void ForEachByMultiTasks<T>(this IEnumerable<T> totalItems, int taskcount, Action<T> taskToRun)
    {
        int workerThreads;
        int portThreads;
        ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);

        if (taskcount > workerThreads / 2)
        {
            taskcount = workerThreads / 2;
        }

        List<Task> tasks = new List<Task>();
        var eachTaskItems = (int)Math.Ceiling((decimal)totalItems.Count() / taskcount);
        for (int i = 0; i < taskcount; i++)
        {
            var taskItems = totalItems.Skip(i * eachTaskItems).Take(eachTaskItems).ToList();
            var t = Task.Run(() =>
                {
                    foreach (var item in taskItems)
                    {
                        taskToRun(item);
                    }
                }
            );
            tasks.Add(t);
        }
        Task.WaitAll(tasks.ToArray());
        tasks.ForEach((e) => e.Dispose());
    }

}
