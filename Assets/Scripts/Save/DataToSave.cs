using UnityEngine;

[System.Serializable]

public class DataToSave
{
    public string action = "save_data";
    public int idPlayer = 0;
    public int totalPoints = 0;
    public int currentPoints = 0;
    public string currentLevel = "";
    public float[] position = new float[3];
    public int timeGame = 0;


    public DataToSave(PlayerStats playerStats, float[] position, int timeGame)
    {
        idPlayer = PlayerPrefs.GetInt("playerId");
        totalPoints = playerStats.GetTotalPoints();
        currentPoints = playerStats.GetTotalPointsActuality();
        currentLevel = playerStats.GetCurrentLevel();
        this.position = position;
        this.timeGame = timeGame;
    }
}
