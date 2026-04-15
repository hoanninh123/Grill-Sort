using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Utils
{
    private static EventSystem _cachedEventSystem;
    private static PointerEventData _cachedPointerEventData;
    private static readonly List<RaycastResult> _cachedRaycastResults = new List<RaycastResult>();

    public static List<T> GetListInChild<T>(Transform parent)
    {
        List<T> result = new List<T>();

        for (int i = 0; i < parent.childCount; i++)
        {
            var component = parent.GetChild(i).GetComponent<T>();
            if (component != null)
                result.Add(component);
        }

        return result;
    }

    public static List<T> TakeAndRemoveRandom<T>(List<T> source, int n)
    {
        List<T> result = new List<T>(); // khoi tao list de tra ve
        n = Mathf.Min(n, source.Count); // check de ddam bao so luong lay ve khong vuot qua so luong list co san

        for (int i = 0; i < n; i++)
        {
            int ranIndex = Random.Range(0, source.Count);
            result.Add(source[ranIndex]);
            source.RemoveAt(ranIndex);
        }

        return result;
    }

    public static T GetRayCastUI<T>(Vector2 position) where T : MonoBehaviour
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return null;

        if (_cachedPointerEventData == null || _cachedEventSystem != eventSystem)
        {
            _cachedEventSystem = eventSystem;
            _cachedPointerEventData = new PointerEventData(eventSystem);
        }

        _cachedPointerEventData.position = position;
        _cachedRaycastResults.Clear();
        eventSystem.RaycastAll(_cachedPointerEventData, _cachedRaycastResults);

        for (int i = 0; i < _cachedRaycastResults.Count; i++)
        {
            T component = _cachedRaycastResults[i].gameObject.GetComponent<T>();
            if (component != null)
                return component;
        }

        return null;
    }
}
