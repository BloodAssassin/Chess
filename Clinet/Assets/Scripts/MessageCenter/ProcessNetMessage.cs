﻿using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;
using System;

public class ProcessNetMessage : MonoBehaviour
{

    //关于账户功能的消息处理中心
    private GameObject AccountManager;
    //关于联网对战过程中对游戏流程的消息处理中心
    private GameObject MatchManager;
    //关于联网对战过程中对棋子操作的消息处理中心
    private GameObject NetGame;


    void Update()
    {
        ProcessingMessage();
    }


    /// <summary>
    /// 处理消息
    /// </summary>
    void ProcessingMessage()
    {
        if (NetworkManager.bufferList.Count != 0)
        {
            byte[] buffer = NetworkManager.bufferList[0];
            //获取口令
            int command = buffer[0];
            Debug.Log("处理的指令为：" + command);
            switch (command)
            {
                case 0:
                    //处理账户功能相关的消息
                    AccountManager = GameObject.Find("AccountManager");
                    AccountManager.GetComponent<AccountManager>().ProcessingMessage(buffer);
                    break;
                case 1:
                    //处理联网对战中游戏流程的相关消息
                    MatchManager = GameObject.Find("MatchManager");
                    MatchManager.GetComponent<MatchManager>().ProcessingMessage(buffer);
                    break;
                case 2:
                    //处理联网对战中棋子操作的相关消息
                    NetGame = GameObject.Find("ChessBoard");
                    NetGame.GetComponent<NetGame>().ProcessingMessage(buffer);
                    break;
                case 9:
                    //当游戏一开始连上网的时候，获取本地联网的IP地址
                    SetMyIP(buffer);
                    break;
                default:
                    break;
            }

            NetworkManager.bufferList.RemoveAt(0);
        }
    }


    /// <summary>
    /// 获取本地IP地址
    /// </summary>
    /// <param name="buffer"></param>
    private void SetMyIP(byte[] buffer)
    {
        string myIP = Encoding.UTF8.GetString(buffer, 1, 25);
       
        NetworkManager._myIP = myIP.Length > 0 ? myIP.Trim('\0') : "";

        Debug.Log("本地的IP地址为：" + NetworkManager._myIP);
    }
}
