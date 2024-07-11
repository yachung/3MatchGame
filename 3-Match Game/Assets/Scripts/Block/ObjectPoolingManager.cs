using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingManager : MonoSingleton<ObjectPoolingManager>
{
    private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    public void CreatePool(string key, GameObject prefab, int initialSize)
    {
        GameObject poolObject = new GameObject(key + " Pool");
        poolObject.transform.SetParent(this.transform);

        ObjectPool pool = poolObject.AddComponent<ObjectPool>();
        pool.Initialize(prefab, initialSize, poolObject.transform);

        pools[key] = pool;
    }

    public GameObject GetObject(string key)
    {
        if (pools.ContainsKey(key))
        {
            return pools[key].GetObject();
        }

        return null;
    }

    public void DelayReturnObject(string key, GameObject obj, float delay)
    {
        StartCoroutine(CoReturnObject(key, obj, delay));
    }

    private IEnumerator CoReturnObject(string key, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        ReturnObject(key, obj);
    }

    public void ReturnObject(string key, GameObject obj)
    {
        if (pools.ContainsKey(key))
        {
            pools[key].ReturnObject(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
