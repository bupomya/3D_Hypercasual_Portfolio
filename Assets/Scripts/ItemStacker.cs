using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ItemStacker : MonoBehaviour
{
    [Header("Stacking Settings")]
    public GameObject itemPrefab;
    public float stackInterval = 0.3f;
    public int maxCount = 99;
    public Vector3 itemRotation = Vector3.zero;

    private List<GameObject> stackedItems = new List<GameObject>();

    public int Count => stackedItems.Count;
    public bool IsFull => stackedItems.Count >= maxCount;

    public bool AddItem()
    {
        if (IsFull || itemPrefab == null) return false;

        GameObject item = Instantiate(itemPrefab, transform);
        item.transform.localPosition = Vector3.up * (stackedItems.Count * stackInterval);
        item.transform.localRotation = Quaternion.Euler(itemRotation);

        stackedItems.Add(item);
        return true;
    }

    public bool AddItem(GameObject item)
    {
        if (IsFull || item == null) return false;

        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.up * (stackedItems.Count * stackInterval);
        item.transform.localRotation = Quaternion.Euler(itemRotation);

        stackedItems.Add(item);
        return true;
    }

    [Header("Remove Animation")]
    public float removeDuration = 0.3f;

    public bool RemoveItem()
    {
        if (stackedItems.Count == 0) return false;

        int lastIndex = stackedItems.Count - 1;
        GameObject item = stackedItems[lastIndex];
        stackedItems.RemoveAt(lastIndex);

        item.transform.DOScale(Vector3.zero, removeDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(item));

        return true;
    }

    public GameObject PopItem()
    {
        if (stackedItems.Count == 0) return null;

        int lastIndex = stackedItems.Count - 1;
        GameObject item = stackedItems[lastIndex];
        stackedItems.RemoveAt(lastIndex);
        item.transform.SetParent(null);
        return item;
    }

    public int RemoveAll()
    {
        int count = stackedItems.Count;

        for (int i = stackedItems.Count - 1; i >= 0; i--)
            Destroy(stackedItems[i]);

        stackedItems.Clear();
        return count;
    }
}
