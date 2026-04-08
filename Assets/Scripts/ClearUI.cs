using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ClearUI : MonoBehaviour
{
    public static ClearUI Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public Text clearText;
    public Button clearButton;
    public float animDuration = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    void Start()
    {
        if (clearButton != null)
            clearButton.onClick.AddListener(OnClearButtonClicked);
    }

    public void Show(string message)
    {
        if (panel == null) return;

        if (clearText != null)
            clearText.text = message;

        panel.SetActive(true);
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, animDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() => Time.timeScale = 0f);
    }

    void OnClearButtonClicked()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
