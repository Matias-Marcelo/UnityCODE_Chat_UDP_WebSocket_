using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private TMP_Text diamondCollect;
    [SerializeField] private TMP_Text username;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuSave;

    public void Start()
    {
        Time.timeScale = 1f;
        pauseButton.SetActive(true);
        menuPause.SetActive(false);
        menuSave.SetActive(false);
        username.text = PlayerPrefs.GetString("username");
    }
    public void UpdateDiamondCollect(int points)
    {
        diamondCollect.text = points.ToString();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseButton.SetActive(false);
        menuPause.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseButton.SetActive(true);
        menuPause.SetActive(false);
    }

    public void FinishGame()
    {
        PlayerStats.Instance.AddTotalsPoints(PlayerStats.Instance.GetTotalPointsActuality()); // Guardar puntos antes de salir
        PlayerStats.Instance.SetTotalPointsActuality(0); // Reiniciar los puntos actuales
        SceneManager.LoadScene(1);
    }


    public void SelectSaveOption()
    {
        //GameManager.Instance.SavePlayer();
        menuSave.SetActive(true);
        pauseButton.SetActive(false);
        menuPause.SetActive(false);
    }

    public void ReturnToPause()
    {
        menuPause.SetActive(true);
        pauseButton.SetActive(false);
        menuSave.SetActive(false) ;
    }

    public void SaveGame(int slotNumber)
    {
        string slotName = "SaveSlot" + slotNumber;
        GameManager.Instance.SavePlayer(slotName);
    }

}
