using UnityEngine;

[CreateAssetMenu(fileName = "MiningLevel", menuName = "Mining/Level Data")]
public class MiningLevelData : ScriptableObject
{
    [Header("기본 정보")]
    public string levelName;
    public GameObject workerPrefab;

    [Header("채광 능력")]
    public float miningSpeed = 1f;
    public int miningDamage = 1;
    public int maxCarryCount = 5;
    public float miningRadius = 2f;
    public float autoMiningInterval = 0f;

    [Header("업그레이드")]
    public int upgradeCost;
}
