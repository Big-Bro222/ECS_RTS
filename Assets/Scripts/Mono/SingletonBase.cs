using UnityEngine;

/// <summary>
/// The base for all the singleton, using singleton for a large project is in general not practical method, should replace with DI framework at later step
/// </summary>
/// <typeparam name="T"></typeparam>
/// TODO: Get rid of singletons
public abstract class SingletonBase<T> : MonoBehaviour
    where T : class
{
    /// <summary>
    /// SingletoneBase instance back field
    /// </summary>
    private static T instance = null;
    /// <summary>
    /// SingletoneBase instance
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (instance == null)
                    Debug.LogError("SingletoneBase<T>: Could not found GameObject of type " + typeof(T).Name);
            }
            return instance;
        }
    }
}