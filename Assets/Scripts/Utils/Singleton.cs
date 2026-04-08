public class Singleton<Type> where Type : class, new()
{
    private static Type instance = null;
    public static Type Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Type();
            }
            return instance;
        }
    }
}
