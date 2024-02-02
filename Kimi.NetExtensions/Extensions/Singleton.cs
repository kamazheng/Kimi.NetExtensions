/// <summary>
/// 这个C#类是一个单例模式的实现，通过使用Lazy<T>类和new关键字来保证只创建一个实例。
/// 通过获取Instance属性可以获取该类的单例实例。由于使用了sealed修饰符，该类无法被继承。
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Singleton<T> where T : class, new()
{
    private static readonly Lazy<T> lazy = new Lazy<T>(() => new T());
    public static T Instance { get { return lazy.Value; } }

    private Singleton()
    {
    }
}
