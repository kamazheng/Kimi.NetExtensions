﻿namespace Kimi.NetExtensions.Extensions;

public static class IEnumerableExtensions
{
    static IEnumerableExtensions()
    {
        LicenceHelper.CheckLicense();
    }

    public static bool HasItem<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable?.Any() == true;
    }

    public static void AddIfNotExistsByJson<T>(this List<T> list, T item)
    {
        if (!list.Any(i => i.ToJson() == item.ToJson()))
        {
            list.Add(item);
        }
    }

    public static IEnumerable<T> AddIfNotExistsByJson<T>(this IEnumerable<T> source, T item)
    {
        var existingItems = new HashSet<string>(source.Select(i => i.ToJson()));
        var itemJson = item.ToJson();

        if (!existingItems.Contains(itemJson))
        {
            return source.Concat(new[] { item });
        }

        return source; // Item already exists, return the original collection
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

    public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>().ToList();
    }

    public static bool HasDuplicates<T>(this IEnumerable<T> list)
    {
        return list.GroupBy(x => x).Any(x => x.Skip(1).Any());
    }

    public static IEnumerable<T> FindUniqueItems<T>(IEnumerable<T> firstSequence, IEnumerable<T> secondSequence)
       where T : class
    {
        // 找出firstSequence中有而secondSequence中没有的项
        var uniqueToFirstSequence = firstSequence.Except(secondSequence).ToList();
        // 找出secondSequence中有而firstSequence中没有的项
        var uniqueToSecondSequence = secondSequence.Except(firstSequence).ToList();

        // 合并两个序列中的独特项
        return uniqueToFirstSequence.Union(uniqueToSecondSequence);
    }

    public static List<Dictionary<string, object?>> ToDictionaryList<T>(this IEnumerable<T> list)
    {
        return list.Select(a => a.GetType().GetProperties().ToDictionary(
            prop => prop.Name,
            prop => prop.GetValue(a, null)))
        .ToList();
    }

    public static void OverrideDictionary(this List<Dictionary<string, object?>> targetDicts, List<Dictionary<string, object?>> fromDicts,
        string identityKey,
        params string[] overrideKeys)
    {
        // Iterate over the target dictionary list
        foreach (var targetDict in targetDicts)
        {
            if (targetDict.TryGetValue(identityKey, out object? targetId))
            {
                // Find the matching dictionary in the source list based on the identity key
                var fromDict = fromDicts.FirstOrDefault(d => d.ContainsKey(identityKey)
                    && d[identityKey].ToString()?.Equals(targetId.ToString()) == true
                    );
                if (fromDict != null)
                {
                    // Override the specified keys
                    foreach (var key in overrideKeys)
                    {
                        if (fromDict.TryGetValue(key, out object? value))
                        {
                            targetDict[key] = value;
                        }
                    }
                }
            }
        }
    }
}