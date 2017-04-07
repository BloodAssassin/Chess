using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameModeSelect : MonoBehaviour {

    public void OnePersonModeButton()
    {
        SceneManager.LoadScene("SingleGame");
    }

    public void TwoPersonModeButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void NetBattleModeButton()
    {
        SceneManager.LoadScene("NetGame");
    }

    public void TeachingModeButton()
    {
        //SceneManager.LoadScene("TeachGame");
    }

    public void ExitButton()
    {
        Application.Quit();  
    }
}
