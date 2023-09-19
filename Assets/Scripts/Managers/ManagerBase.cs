public class ManagerBase<T> where T : new()
{
    public static T CreateManager()
    {
        if (s_instance == null)
        {
            s_instance = new T();
        }

        return s_instance;
    }

    public virtual bool Initialize()
    {
        return true;
    }

    public virtual bool Dispose()
    {
        return true;
    }

    public static T Instance => s_instance;
    private static T s_instance;
}