using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PoolConfig", menuName = "PoolConfig")]
public class PoolConfig : ScriptableObject
{
    [SerializeField] private List<MonoBehaviour> _prefabs = new List<MonoBehaviour>();

    public T Get<T>() where T : MonoBehaviour
    {
        return _prefabs.FirstOrDefault(p => p.GetType() == typeof(T)) as T;
    }
}
