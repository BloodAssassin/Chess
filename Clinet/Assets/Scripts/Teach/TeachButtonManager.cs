using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TeachButtonManager : MonoBehaviour {

    public GameObject Menu;
    public GameObject Panel_Jiang;
    public GameObject Panel_Shi;
    public GameObject Panel_Xiang;
    public GameObject Panel_Che;
    public GameObject Panel_Ma;
    public GameObject Panel_Pao;
    public GameObject Panel_Bing;

    public void btn_MenuBack()
    {
        SceneManager.LoadScene("Menu");
    }

    public void btn_BackToMenu()
    {
        Panel_Jiang.SetActive(false);
        Panel_Shi.SetActive(false);
        Panel_Xiang.SetActive(false);
        Panel_Che.SetActive(false);
        Panel_Ma.SetActive(false);
        Panel_Pao.SetActive(false);
        Panel_Bing.SetActive(false);

        Menu.SetActive(true);
    }


    public void btn_ToJiang()
    {
        Panel_Jiang.SetActive(true);
        Menu.SetActive(false);
    }

    public void btn_ToShi()
    {
        Panel_Shi.SetActive(true);
        Menu.SetActive(false);
    }

    public void btn_ToXiang()
    {
        Panel_Xiang.SetActive(true);
        Menu.SetActive(false);
    }

    public void btn_ToChe()
    {
        Panel_Che.SetActive(true);
        Menu.SetActive(false);
    }

    public void btn_ToMa()
    {
        Panel_Ma.SetActive(true);
        Menu.SetActive(false);
    }

    public void btn_ToPao()
    {
        Panel_Pao.SetActive(true);
        Menu.SetActive(false);
    }

    public void btn_ToBing()
    {
        Panel_Bing.SetActive(true);
        Menu.SetActive(false);
    }
}
