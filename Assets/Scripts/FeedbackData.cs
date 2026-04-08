using UnityEngine;

[CreateAssetMenu(fileName = "NewFeedback", menuName = "Game/Feedback Data")]
public class FeedbackData : ScriptableObject
{
    [Header("사운드")]
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.8f, 1.2f)]
    public float pitchMin = 1f;
    [Range(0.8f, 1.2f)]
    public float pitchMax = 1f;

    [Header("이펙트")]
    public GameObject effectPrefab;
    public float effectDuration = 1f;
}
