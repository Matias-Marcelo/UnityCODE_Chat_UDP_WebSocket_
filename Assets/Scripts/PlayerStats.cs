using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    private int totalItemCollect = 0;
    private int totalItemCollectActuality = 0;
    private string currentLevel = "";

    public void Awake()
    {
        if (PlayerStats.Instance == null)
        {
            PlayerStats.Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int AddTotalsPoints(int points)
    {
        totalItemCollect += points;
        return totalItemCollect;
    }

    public int GetTotalPoints()
    {
        return totalItemCollect;
    }

    public void SetTotalPoints(int points)
    {
        totalItemCollect = points;
    }


    public int AddPointsActuality(int points)
    {
        totalItemCollectActuality += points;
        return totalItemCollectActuality;
    }

    public int GetTotalPointsActuality()
    {
        return totalItemCollectActuality;
    }

    public void SetTotalPointsActuality(int points)
    {
        totalItemCollectActuality = points;
    }

    public string GetCurrentLevel()
    {
        return this.currentLevel;
    }

    public void SetCurrentLevel(string level)
    {
        this.currentLevel = level;
    }
}
