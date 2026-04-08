using UnityEngine;
using System.Collections.Generic;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("풀링")]
    public int initialPoolSize = 5;

    private AudioSource audioSource;
    private readonly Dictionary<GameObject, Queue<GameObject>> effectPools = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Play(FeedbackData data, Vector3 position)
    {
        if (data == null) return;

        PlaySound(data);
        PlayEffect(data, position);
    }

    void PlaySound(FeedbackData data)
    {
        if (data.clip == null) return;

        float pitch = Random.Range(data.pitchMin, data.pitchMax);
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(data.clip, data.volume);
    }

    void PlayEffect(FeedbackData data, Vector3 position)
    {
        if (data.effectPrefab == null) return;

        GameObject effect = GetFromPool(data.effectPrefab);
        effect.transform.position = position;
        effect.SetActive(true);

        StartCoroutine(ReturnToPoolAfter(data.effectPrefab, effect, data.effectDuration));
    }

    GameObject GetFromPool(GameObject prefab)
    {
        if (!effectPools.ContainsKey(prefab))
            effectPools[prefab] = new Queue<GameObject>();

        Queue<GameObject> pool = effectPools[prefab];

        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            if (obj != null) return obj;
        }

        GameObject newObj = Instantiate(prefab, transform);
        newObj.SetActive(false);
        return newObj;
    }

    System.Collections.IEnumerator ReturnToPoolAfter(GameObject prefab, GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (instance == null) yield break;

        instance.SetActive(false);

        if (!effectPools.ContainsKey(prefab))
            effectPools[prefab] = new Queue<GameObject>();

        effectPools[prefab].Enqueue(instance);
    }
}
