using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils 
{
    public static List<T> GetListFromChild<T>(Transform parent)
    {
        List<T> res = new List<T>();
        for(int i = 0; i < parent.childCount; i++)
        {
            var component = parent.GetChild(i).GetComponent<T>();
            if (component != null) res.Add(component);
        }
        return res;
    }
    public static List<T> TakeAndRemoveRandom<T>(List<T> source,int n)
    {
        List<T> result = new List<T>();
        n = Mathf.Min(n, source.Count);
        for(int i = 0; i < n; i++)
        {
            int rand = Random.Range(0,source.Count);
            result.Add(source[rand]);
            source.RemoveAt(rand);
        }
        return result;
    }
}
