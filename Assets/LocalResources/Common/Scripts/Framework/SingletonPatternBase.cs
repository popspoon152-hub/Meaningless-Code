using System;

public class SingletonPatternBase<T> where T : SingletonPatternBase<T>
{
    private static T _instance;

    public static T Ins
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)Activator.CreateInstance(typeof(T));
            }
            return _instance;
        }
    }

    protected SingletonPatternBase() { }
}
