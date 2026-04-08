using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WarningUI : MonoBehaviour
{
    public static WarningUI Instance { get; private set; }

    [Header("UI")]
    public Text warningText;
    public float displayDuration = 2f;
    public float fadeDuration = 0.5f;

    private Tween currentTween;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (warningText == null) return;

        if (currentTween != null)
            currentTween.Kill();

        warningText.text = message;
        warningText.gameObject.SetActive(true);

        Color color = warningText.color;
        color.a = 1f;
        warningText.color = color;

        currentTween = warningText.DOFade(0f, fadeDuration)
            .SetDelay(displayDuration)
            .OnComplete(() => warningText.gameObject.SetActive(false));
    }
}
