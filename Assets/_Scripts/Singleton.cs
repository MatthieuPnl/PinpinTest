using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    protected static T instance = null;

    public static T GetInstance()
    {
        return instance;
    }

    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this as T;
    }
}