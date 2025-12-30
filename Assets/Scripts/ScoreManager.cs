using System.Collections.Generic;
using R3;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public ReactiveProperty<int> Score { get; } = new(0);

    private readonly Dictionary<int, int> levelStartScore = new();
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void AddScore(int amount)
    {
        Score.Value = Mathf.Max(0, Score.Value + amount);
    }

    public void ResetScore()
    {
        Score.Value = 0;
    }

    public void MarkScoreAtLevelStart(int levelId)
    {
        levelStartScore[levelId] = Score.Value;
    }

    public void RestoreScoreAtLevelStart(int levelId)
    {
        if (levelStartScore.TryGetValue(levelId, out int value))
        {
            Score.Value = value;
        }
        else
        {
            Debug.LogWarning($"No saved score for level {levelId}");
        }
    }

}
