using UnityEngine;
using DG.Tweening;

public static class ItemTweenHelper
{
    public static Tween MoveArc(Transform item, Vector3 targetWorldPos, float duration, float height)
    {
        Vector3 startPos = item.position;
        Vector3 midPos = (startPos + targetWorldPos) * 0.5f + Vector3.up * height;

        return item.DOPath(
            new Vector3[] { midPos, targetWorldPos },
            duration,
            PathType.CatmullRom
        ).SetEase(Ease.OutQuad);
    }

    public static void DrawZoneGizmos(BoxCollider col, Transform transform, Color color)
    {
        if (col == null) return;

        Color fill = color;
        fill.a = 0.3f;

        Gizmos.color = fill;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.center, col.size);
        Gizmos.color = color;
        Gizmos.DrawWireCube(col.center, col.size);
        Gizmos.matrix = oldMatrix;
    }
}
