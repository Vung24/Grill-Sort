using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class Pool
{
    [Header("POOL_INIT")]
    [SerializeField] private GameObject poolObject;
    [SerializeField] private int count;
    
    
    //Pool Storage
    List<GameObject> actives = new ();
    private Queue<GameObject> deactives = new ();

    public void InitPool(Transform container, Dictionary<int, int> dicClones)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnClone(container, dicClones);
        }
    }

    void SpawnClone(Transform container, Dictionary<int, int> dicClones)
    {
        var clone = Object.Instantiate(poolObject, container);
        clone.SetActive(false);
        clone.transform.localScale = Vector3.one;
        deactives.Enqueue(clone);
        dicClones.Add(clone.GetHashCode(), GetHashCode());
    }

    public GameObject Get(Transform container, Dictionary<int, int> dicClones)
    {
        if (deactives.Count == 0)
            SpawnClone(container, dicClones);
        var clone = deactives.Dequeue();
        actives.Add(clone);
        return clone;
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        actives.Remove(go);
        deactives.Enqueue(go);
    }

    public void ReturnAll()
    {
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            Return(actives[i]);
        }
    }
}

public interface IMonoBehaviourPool
{
    void InitPool(Transform parent, Dictionary<int, int> clones);
}


[System.Serializable]
public class MonoBehaviourPool<T> : IMonoBehaviourPool where T : MonoBehaviour
{
    [Header("POOL_INIT")]
    [SerializeField] private T prefab;
    [SerializeField] int count;

    private List<T> actives = new();
    private Queue<T> deactives = new();
    
    public void InitPool(Transform container, Dictionary<int, int> dicClones)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnClone(container, dicClones);
        }
    }

    private void SpawnClone(Transform container, Dictionary<int, int> dicClones)
    {
        var clone = Object.Instantiate(prefab, container);
        clone.gameObject.SetActive(false);
        clone.transform.localScale = Vector3.one;
        deactives.Enqueue(clone);
        dicClones.Add(clone.GetHashCode(), GetHashCode());
    }

    public T Get(Transform container, Dictionary<int, int> dicClones)
    {
        if (deactives.Count == 0)
            SpawnClone(container, dicClones);
        var clone = deactives.Dequeue();
        actives.Add(clone);
        return clone;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        actives.Remove(obj);
        deactives.Enqueue(obj);
    }
    
    public void ReturnAll()
    {
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            Return(actives[i]);
        }
    }
}

public class ObjectPool : MonoBehaviour 
{
    protected Dictionary<int, int> clones = new ();

    protected Dictionary<int, Pool> pools = new ();
    protected Dictionary<int, IMonoBehaviourPool> behaviourPools = new ();

#if UNITY_EDITOR
    public Pool examplePool;
    
    private void ExampleInitPool()
    {
        examplePool.InitPool(transform, clones);
    }
#endif
    
    protected virtual void Start()
    {
        #if UNITY_EDITOR
            pools.Add(examplePool.GetHashCode(), examplePool);
        #endif
        
        foreach (var monoPools in behaviourPools.Values)
        {
            monoPools.InitPool(transform,clones);
        }
        foreach (var pool in pools.Values)
        {
            pool.InitPool(transform, clones);
        }
    }

    public GameObject Get(Pool p)
    {
        return p.Get(transform, clones);
    }

    public T Get<T>(MonoBehaviourPool<T> p) where T: MonoBehaviour
    {
        return p.Get(transform, clones);
    }

    public void Return(GameObject clone)
    {
        var hash = clone.GetHashCode();
        if (clones.ContainsKey(hash))
        {
            var p = GetPool(clones[hash]);
            if (p != null)
            {
                p.Return(clone);
            }
        }
    }

    public void Return<T>(T clone) where T: MonoBehaviour
    {
        var hash = clone.GetHashCode();
        if (clones.ContainsKey(hash))
        {
            var p = GetMonoPool<T>(clones[hash]);
            if(p != null) p.Return(clone);
        }
    }
    
    public virtual void ReturnAll()
    {
        
    }
    
    protected virtual Pool GetPool(int hash)
    {
        if (pools.TryGetValue(hash, out var pool))
        {
            return pool;
        }
        
        #if UNITY_EDITTOY
        if (examplePool.GetHashCode() == hash)
            return examplePool;
        #endif
        
        return null;
    }

    protected virtual MonoBehaviourPool<T> GetMonoPool<T>(int hash) where T : MonoBehaviour
    {
        if (behaviourPools.TryGetValue(hash, out var pool))
        {
            return pool as MonoBehaviourPool<T>;
        }
        
        return null;
    }
}