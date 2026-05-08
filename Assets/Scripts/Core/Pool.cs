using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pool : MonoBehaviour
{
    protected static Pool _instance;
    private static PoolConfig _config;

    protected static List<MonoBehaviour> PoolObjectsGeneral = new List<MonoBehaviour>();

    protected static PoolConfig Config
    {
        get
        {
            if (_config == null)
                _config = Resources.Load<PoolConfig>("PoolConfig");

            return _config;
        }
    }

    void Awake()
    {
        if (!_instance)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public static T Get<T>(Transform parent = null) where T : MonoBehaviour
    {
        var obj = default(T);
        if (PoolObjectsGeneral.OfType<T>().Count() == 0)
        {
            var prefab = Config.Get<T>();
            if (prefab == null)
            {
                Debug.LogError($"[Pool] Prefab of type {typeof(T).Name} is NOT registered in PoolConfig!");
                return null;
            }
            if (prefab)
                obj = (T)Instantiate(prefab, null);
        }
        else
        {
            obj = (T)PoolObjectsGeneral.FirstOrDefault(p => p.GetType() == typeof(T));
            PoolObjectsGeneral.Remove(obj);
        }

        obj.gameObject.SetActive(true);
        obj.transform.SetParent(parent);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        return (T)obj;
    }

    public static void Release(MonoBehaviour obj)
    {
        if (obj is IDisposable disposable)
            disposable.Dispose();

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_instance.transform);
        PoolObjectsGeneral.Add(obj);
    }
}
