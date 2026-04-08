using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IdleHintUI : MonoBehaviour
{
    public static IdleHintUI Instance { get; private set; }

    [Header("References")]
    [Tooltip("조이스틱의 CanvasGroup (없으면 자동으로 추가됨)")]
    public CanvasGroup joystickCanvasGroup;
    [Tooltip("손가락 아이콘 Image")]
    public Image fingerIcon;

    [Header("Idle Settings")]
    [Tooltip("힌트가 표시되기까지의 대기 시간 (초)")]
    public float idleThreshold = 3f;

    [Header("Animation Settings")]
    public float fadeDuration = 0.3f;
    [Tooltip("손가락 드래그 반경")]
    public float dragRadius = 80f;
    [Tooltip("한 사이클 소요 시간 (초)")]
    public float cycleDuration = 1.5f;
    [Tooltip("사이클 사이 대기 시간 (초)")]
    public float cycleDelay = 0.3f;

    private Sequence hintSequence;
    private Vector2 fingerOrigin;
    private bool isShowing;
    private RectTransform fingerRect;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (fingerIcon != null)
        {
            fingerRect = fingerIcon.GetComponent<RectTransform>();
            fingerOrigin = fingerRect.anchoredPosition;
            fingerIcon.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        hintSequence?.Kill();
        if (Instance == this)
            Instance = null;
    }

    public void Show()
    {
        if (isShowing || fingerIcon == null) return;
        isShowing = true;

        // 조이스틱 페이드 아웃 (입력은 계속 받을 수 있도록 blocksRaycasts 유지)
        if (joystickCanvasGroup != null)
            joystickCanvasGroup.DOFade(0f, fadeDuration);

        // 손가락 아이콘 표시 (터치가 뒤의 조이스틱으로 통과하도록 raycastTarget 해제)
        fingerIcon.raycastTarget = false;
        fingerIcon.gameObject.SetActive(true);
        fingerRect.anchoredPosition = fingerOrigin;

        Color c = fingerIcon.color;
        c.a = 0f;
        fingerIcon.color = c;
        fingerIcon.DOFade(1f, fadeDuration);

        // 드래그 애니메이션 시작
        PlayDragAnimation();
    }

    public void Hide()
    {
        if (!isShowing) return;
        isShowing = false;

        hintSequence?.Kill();

        // 손가락 아이콘 페이드 아웃
        if (fingerIcon != null)
        {
            fingerIcon.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                fingerIcon.gameObject.SetActive(false);
                fingerRect.anchoredPosition = fingerOrigin;
            });
        }

        // 조이스틱 페이드 인
        if (joystickCanvasGroup != null)
            joystickCanvasGroup.DOFade(1f, fadeDuration);
    }

    private void PlayDragAnimation()
    {
        hintSequence?.Kill();

        hintSequence = DOTween.Sequence();

        // 시작 위치 (중앙)
        hintSequence.Append(
            fingerRect.DOAnchorPos(fingerOrigin, 0f)
        );

        // 위로 드래그
        hintSequence.Append(
            fingerRect.DOAnchorPos(fingerOrigin + Vector2.up * dragRadius, cycleDuration * 0.25f)
                .SetEase(Ease.OutSine)
        );

        // 중앙으로 복귀
        hintSequence.Append(
            fingerRect.DOAnchorPos(fingerOrigin, cycleDuration * 0.15f)
                .SetEase(Ease.InSine)
        );

        // 오른쪽으로 드래그
        hintSequence.Append(
            fingerRect.DOAnchorPos(fingerOrigin + Vector2.right * dragRadius, cycleDuration * 0.25f)
                .SetEase(Ease.OutSine)
        );

        // 중앙으로 복귀
        hintSequence.Append(
            fingerRect.DOAnchorPos(fingerOrigin, cycleDuration * 0.15f)
                .SetEase(Ease.InSine)
        );

        // 왼쪽 아래로 드래그
        hintSequence.Append(
            fingerRect.DOAnchorPos(fingerOrigin + new Vector2(-0.7f, -0.7f) * dragRadius, cycleDuration * 0.25f)
                .SetEase(Ease.OutSine)
        );

        // 중앙으로 복귀
        hintSequence.Append(
            fingerRect.DOAnchorPos(fingerOrigin, cycleDuration * 0.15f)
                .SetEase(Ease.InSine)
        );

        // 사이클 간 딜레이
        hintSequence.AppendInterval(cycleDelay);

        // 무한 반복
        hintSequence.SetLoops(-1, LoopType.Restart);
    }
}
