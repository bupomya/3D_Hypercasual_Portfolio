using UnityEngine;
using DG.Tweening;

public class ArriveZoneExpansionZone : PurchaseZone
{
    [Header("대상")]
    public ArriveZone arriveZone;

    [Header("업그레이드")]
    public int expandAmount = 1;

    [Header("벽 연출")]
    public GameObject[] destroyWalls;
    public GameObject[] expansionWalls;
    public float wallAnimDuration = 0.4f;

    [Header("클리어")]
    public string clearMessage = "구역 확장 완료!";

    protected override bool CanPurchase()
    {
        return arriveZone != null;
    }

    protected override void OnPurchaseComplete()
    {
        arriveZone.maxCapacity += expandAmount;
        PlayExpansion();
    }

    void PlayExpansion()
    {
        // DestroyWall 축소 후 파괴
        for (int i = 0; i < destroyWalls.Length; i++)
        {
            if (destroyWalls[i] == null) continue;
            GameObject wall = destroyWalls[i];
            wall.transform.DOScale(Vector3.zero, wallAnimDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(wall));
        }

        // ExpansionWall 활성화 + 팝업 애니메이션
        for (int i = 0; i < expansionWalls.Length; i++)
        {
            if (expansionWalls[i] == null) continue;
            GameObject wall = expansionWalls[i];
            Vector3 originalScale = wall.transform.localScale;
            wall.transform.localScale = Vector3.zero;
            wall.SetActive(true);
            wall.transform.DOScale(originalScale, wallAnimDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(wallAnimDuration);
        }

        // 벽 애니메이션 완료 후 클리어 UI 표시
        float totalDelay = wallAnimDuration * 2f;
        DOVirtual.DelayedCall(totalDelay, () =>
        {
            if (ClearUI.Instance != null)
                ClearUI.Instance.Show(clearMessage);
        });
    }
}
