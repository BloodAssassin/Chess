﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameModeSelect : MonoBehaviour {

    public GameObject ErrorPanel;
    public GameObject StatePanel;

    public GameObject NetworkUnavailable;
    public GameObject PassbyMode;

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
        if (NetworkManager.isPassby)
        {
            ErrorPanel.SetActive(true);
            PassbyMode.SetActive(true);
        }
        else if(!NetworkManager.isConnected)
        {
            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ConnectServer();
            if (NetworkManager.isConnected)
            {
                SceneManager.LoadScene("NetGame");
            }
        }
        else
        {
            SceneManager.LoadScene("NetGame");
        }
    }

    public void TeachingModeButton()
    {
        SceneManager.LoadScene("PhoneTeach");
    }

    public void StateButton()
    {
        StatePanel.SetActive(true);
    }

    public void LogoutButton()
    {
        SceneManager.LoadScene("Login");
    }

    


    public void ExitButton()
    {
        Application.Quit();  
    }

    /// <summary>
    /// 将提示错误的panel设置为不可见
    /// </summary>
    public void btn_SetErrorPanelDisable()
    {
        ErrorPanel.SetActive(false);
        NetworkUnavailable.SetActive(false);
        PassbyMode.SetActive(false);
    }

    /// <summary>
    /// 将游戏说明的panel设置为不可见
    /// </summary>
    public void btn_SetStatePanelDisable()
    {
        StatePanel.SetActive(false);
    }
}
