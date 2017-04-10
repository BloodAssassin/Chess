using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{

    //选择颜色的两个Button所在的Plane
    public GameObject _selectColorPlane;

    //UI物体
    public GameObject _hintMessage;
    public GameObject _canvas;
    public GameObject _returnToMenuButton;
    public GameObject _selectColorPanel;

    //NetGame脚本
    public NetGame NG;

    void Awake()
    {
        _canvas = GameObject.Find("Canvas");
        _returnToMenuButton = _canvas.transform.Find("ReturnToMenuButton").gameObject;
        _hintMessage = _canvas.transform.Find("HintMessage").gameObject;
        _selectColorPanel = _canvas.transform.Find("SelectColorPanel").gameObject;

        NG = (NetGame)GameObject.Find("ChessBoard").GetComponent("NetGame");
    }



    public void ProcessingMessage(byte[] data)
    {
         int command = data[1];
         switch (command)
         {
             case 0:
                 //处理对战对手的信息
                 MatchRivalMessage(data);
                 break;
             case 1:

                 break;
             case 2:

                 break;
             case 3:

                 break;
             case 4:

                 break;
             default:
                 break;
         }
    }


    #region 接受消息事件

    private void MatchRivalMessage(byte[] data)
    {
        //[ 命令10(2位) | 对方的ip (25位) | 对方的邮箱(20位) | 棋子颜色(1位) | ...]
        NetGame._rivalIP = Encoding.UTF8.GetString(data, 2, 25).Trim('\0');
        NetGame._rivalEmail = Encoding.UTF8.GetString(data, 27, 20).Trim('\0');

        Debug.Log("对方的IP为：" + NetGame._rivalIP);
        Debug.Log("对方的Email为：" + NetGame._rivalEmail);
        Debug.Log("我方的Email为：" + NetworkManager._myEmail);

        //初始化棋盘（交给NetGame处理）
        if (data[47] == 1)
        {
            NG.InitFromNetwork(true);
        }
        else
        {
            NG.InitFromNetwork(false);
        }
    }

    #endregion


    #region 按钮点击事件

    /// <summary>
    /// 选择红棋
    /// </summary>
    public void RedButton()
    {
        if (NetworkManager.isConnected)
        {
            //将选择红棋的消息发送给服务器
            SendColorMessageToServer(true);

            _selectColorPlane.SetActive(false);
            _hintMessage.SetActive(true);
            _hintMessage.GetComponent<Text>().text = "正在匹配对手，请耐心等待...";
        }
        else
        {
            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ConnectServer();
        }
    }

    /// <summary>
    /// 选择黑棋
    /// </summary>
    public void BlackButton()
    {
        if (NetworkManager.isConnected)
        {
            //将选择黑棋的消息发送给服务器
            SendColorMessageToServer(false);

            _selectColorPlane.SetActive(false);
            _hintMessage.SetActive(true);
            _hintMessage.GetComponent<Text>().text = "正在匹配对手，请耐心等待...";
        }
        else
        {
            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ConnectServer();
        }
    }

    #endregion


    #region 发送消息事件

    private void SendColorMessageToServer(bool isRed)
    {
        //[ 命令10(2位) | 账户邮箱(20位) | 棋子颜色(1位) | ...]       
        byte[] data = CreatBytes(NetworkManager._myEmail, isRed);

        //开始发送
        try
        {
            Common.connSocket.Send(data);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }   
    }

    #endregion


    /// <summary>
    /// 将选择的颜色以及自身的Email地址打包
    /// </summary>
    /// <param name="email"></param>
    /// <param name="isRed"></param>
    /// <returns></returns>
    private byte[] CreatBytes(string email, bool isRed)
    {
        //[ 命令10(2位) | 账户邮箱(20位) | 棋子颜色(1位) | ...] 
        byte[] _email = Encoding.UTF8.GetBytes(email);

        List<byte> list = new List<byte>();

        list.Insert(0, 1);
        list.Insert(1, 0);

        list.AddRange(_email);
        //_email 不够20位
        for (int i = 0; i < 20 - _email.Length; i++)
        {
            list.Add(0);
        }

        if (isRed)
        {
            list.Insert(22, 1);
        }
        else
        {
            list.Insert(22, 0);
        }       

        return list.ToArray();
    }


    /// <summary>
    /// 连接出现问题时，UI相应
    /// </summary>
    public void ConnectProblem()
    {
        _hintMessage.SetActive(true);
        _selectColorPanel.SetActive(false);
        _returnToMenuButton.SetActive(true);
        _hintMessage.GetComponent<Text>().text = "联网暂时不可用\n点击屏幕任意位置退回菜单";
    }
}
