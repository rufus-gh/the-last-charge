using UnityEngine;



public class TokenBank : MonoBehaviour
{
    public const int PerKill = 1;
    const int FirstLevelReward = 20;
    const float RewardGrowth = 1.2f;

    
    
    public static int LevelReward(int level) =>
        Mathf.CeilToInt(FirstLevelReward * Mathf.Pow(RewardGrowth, Mathf.Max(0, level - 1)) - 0.001f);

    void OnEnable() => EnemyHealth.AnyDied += OnEnemyDied;

    void OnDisable() => EnemyHealth.AnyDied -= OnEnemyDied;

    static void OnEnemyDied(EnemyHealth enemy)
    {
        RunState.Kills++;
        RunState.AddTokens(enemy.TryGetComponent(out TokenReward reward) ? reward.Tokens : PerKill);
    }
}
