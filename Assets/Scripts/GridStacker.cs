using UnityEngine;
using System.Collections.Generic;

public class GridStacker : MonoBehaviour
{
    [Header("Stack Layout")]
    public int itemsPerRow = 3;
    public float rowSpacing = 0.5f;
    public float layerHeight = 0.3f;
    public int maxCount = 0;

    private List<GameObject> stackedItems = new List<GameObject>();

    public int Count => stackedItems.Count;
    public bool IsFull => maxCount > 0 && stackedItems.Count >= maxCount;

    public Vector3 GetNextPosition()
    {
        return GetStackPosition(stackedItems.Count);
    }

    public bool AddItem(GameObject item)
    {
        if (IsFull) return false;

        Vector3 localPos = GetStackPosition(stackedItems.Count);
        item.transform.SetParent(transform);
        item.transform.localPosition = localPos;
        stackedItems.Add(item);
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

    public Vector3 GetStackPosition(int index)
    {
        int layer = index / itemsPerRow;
        int posInLayer = index % itemsPerRow;

        float offsetX = (posInLayer - (itemsPerRow - 1) * 0.5f) * rowSpacing;
        float offsetY = layer * layerHeight;

        return new Vector3(offsetX, offsetY, 0f);
    }
}
