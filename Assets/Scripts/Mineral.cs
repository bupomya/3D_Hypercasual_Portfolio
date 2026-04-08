using UnityEngine;
using System.Collections;

public class Mineral : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 3;
    public float respawnTime = 5f;

    private int currentHealth;
    private Coroutine respawnCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public bool Mine(int damage = 1)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
            respawnCoroutine = MiningManager.Instance.StartCoroutine(Respawn());
            return true;
        }

        return false;
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        respawnCoroutine = null;
    }
}
