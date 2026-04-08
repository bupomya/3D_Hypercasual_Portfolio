using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GuideArrowController : MonoBehaviour
{
    public enum GuidePhase
    {
        GoToMiningZone,
        GoToDropZone,
        GoToRedPickupZone,
        GoToExchangeZone,
        GoToMoneyPickup,
        Complete
    }

    [Header("UI")]
    public RectTransform arrowRect;
    public Vector2 screenOffset = new Vector2(0, 100f);
    public float rotationSpeed = 10f;
    public float bobDistance = 15f;
    public float bobDuration = 0.5f;

    [Header("플레이어")]
    public PlayerMiner player;

    [Header("가이드 대상")]
    public Transform miningZoneTarget;
    public Transform dropZoneTarget;
    public Transform redPickupZoneTarget;
    public Transform exchangeZoneTarget;
    public Transform moneyPickupTarget;

    [Header("확장 존")]
    public ArriveZone arriveZone;
    public GameObject expansionZoneObject;

    [Header("레벨업 존")]
    public GameObject levelUpZoneObject;

    [Header("카메라 연출")]
    public TopDownCamera topDownCamera;
    public float cameraPanDuration = 2f;

    [Header("조이스틱")]
    public CanvasGroup joystickCanvasGroup;

    private GuidePhase currentPhase = GuidePhase.GoToMiningZone;
    private Camera mainCamera;
    private bool hadMineralRed;
    private bool expansionActivated;
    private Tweener bobTween;
    private float bobOffset;

    void Start()
    {
        mainCamera = Camera.main;

        if (expansionZoneObject != null)
            expansionZoneObject.SetActive(false);

        if (levelUpZoneObject != null)
            levelUpZoneObject.SetActive(false);

        if (arriveZone != null)
            arriveZone.OnNPCRegistered += CheckExpansionZone;

        StartBobTween();
    }

    void OnDestroy()
    {
        if (arriveZone != null)
            arriveZone.OnNPCRegistered -= CheckExpansionZone;
    }

    void Update()
    {
        if (currentPhase == GuidePhase.Complete) return;

        CheckPhaseTransition();
        UpdateArrow();
    }

    void CheckPhaseTransition()
    {
        switch (currentPhase)
        {
            case GuidePhase.GoToMiningZone:
                if (player.mineralStacker != null &&
                    player.mineralStacker.Count >= player.mineralStacker.maxCount)
                    SetPhase(GuidePhase.GoToDropZone);
                break;

            case GuidePhase.GoToDropZone:
                if (player.mineralStacker != null &&
                    player.mineralStacker.Count == 0)
                    SetPhase(GuidePhase.GoToRedPickupZone);
                break;

            case GuidePhase.GoToRedPickupZone:
                if (player.mineralRedStacker != null &&
                    player.mineralRedStacker.Count > 0)
                    SetPhase(GuidePhase.GoToExchangeZone);
                break;

            case GuidePhase.GoToExchangeZone:
                if (player.mineralRedStacker.Count > 0)
                    hadMineralRed = true;

                if (hadMineralRed && player.mineralRedStacker.Count == 0)
                    SetPhase(GuidePhase.GoToMoneyPickup);
                break;

            case GuidePhase.GoToMoneyPickup:
                if (player.moneyStacker != null &&
                    player.moneyStacker.Count > 0)
                    CompleteGuide();
                break;
        }
    }

    Transform GetCurrentTarget()
    {
        switch (currentPhase)
        {
            case GuidePhase.GoToMiningZone: return miningZoneTarget;
            case GuidePhase.GoToDropZone: return dropZoneTarget;
            case GuidePhase.GoToRedPickupZone: return redPickupZoneTarget;
            case GuidePhase.GoToExchangeZone: return exchangeZoneTarget;
            case GuidePhase.GoToMoneyPickup: return moneyPickupTarget;
            default: return null;
        }
    }

    void UpdateArrow()
    {
        Transform target = GetCurrentTarget();
        if (target == null || mainCamera == null) return;

        Vector3 playerScreen = mainCamera.WorldToScreenPoint(player.transform.position);
        Vector3 targetScreen = mainCamera.WorldToScreenPoint(target.position);

        // 화살표 회전: 타겟 방향
        Vector2 dir = ((Vector2)targetScreen - (Vector2)playerScreen).normalized;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
        arrowRect.rotation = Quaternion.Lerp(arrowRect.rotation, targetRot, Time.deltaTime * rotationSpeed);

        // 화살표 위치: 플레이어 위 + 가리키는 방향으로 왔다갔다
        Vector2 bobDir = dir * bobOffset;
        arrowRect.position = playerScreen + (Vector3)screenOffset + (Vector3)bobDir;
    }

    void SetPhase(GuidePhase phase)
    {
        currentPhase = phase;
    }

    void CheckExpansionZone()
    {
        if (expansionActivated) return;
        if (arriveZone == null || expansionZoneObject == null) return;

        if (arriveZone.CurrentCount >= arriveZone.maxCapacity)
        {
            expansionActivated = true;
            StartCoroutine(ShowExpansionZoneRoutine());
        }
    }

    void StartBobTween()
    {
        bobTween?.Kill();
        bobOffset = 0f;
        bobTween = DOTween.To(() => bobOffset, x => bobOffset = x, bobDistance, bobDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void CompleteGuide()
    {
        currentPhase = GuidePhase.Complete;
        bobTween?.Kill();
        arrowRect.gameObject.SetActive(false);

        if (levelUpZoneObject != null)
        {
            StartCoroutine(ShowLevelUpZoneRoutine());
        }
    }

    IEnumerator ShowLevelUpZoneRoutine()
    {
        if (topDownCamera == null) yield break;

        HideJoystick();

        Transform originalTarget = topDownCamera.target;
        topDownCamera.target = levelUpZoneObject.transform;

        yield return new WaitForSeconds(cameraPanDuration);

        Vector3 originalScale = levelUpZoneObject.transform.localScale;
        levelUpZoneObject.transform.localScale = Vector3.zero;
        levelUpZoneObject.SetActive(true);
        levelUpZoneObject.transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(cameraPanDuration);

        topDownCamera.target = originalTarget;
        ShowJoystick();
    }

    IEnumerator ShowExpansionZoneRoutine()
    {
        if (topDownCamera == null || expansionZoneObject == null) yield break;

        HideJoystick();

        Transform originalTarget = topDownCamera.target;
        topDownCamera.target = expansionZoneObject.transform;

        yield return new WaitForSeconds(cameraPanDuration);

        Vector3 originalScale = expansionZoneObject.transform.localScale;
        expansionZoneObject.transform.localScale = Vector3.zero;
        expansionZoneObject.SetActive(true);
        expansionZoneObject.transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(cameraPanDuration);

        topDownCamera.target = originalTarget;
        ShowJoystick();
    }

    void HideJoystick()
    {
        if (joystickCanvasGroup == null) return;
        joystickCanvasGroup.DOFade(0f, 0.3f);
        joystickCanvasGroup.blocksRaycasts = false;
    }

    void ShowJoystick()
    {
        if (joystickCanvasGroup == null) return;
        joystickCanvasGroup.DOFade(1f, 0.3f);
        joystickCanvasGroup.blocksRaycasts = true;
    }

    IEnumerator CameraPanRoutine(Transform target)
    {
        if (topDownCamera == null) yield break;

        Transform originalTarget = topDownCamera.target;
        topDownCamera.target = target;

        yield return new WaitForSeconds(cameraPanDuration);

        topDownCamera.target = originalTarget;
    }
}
