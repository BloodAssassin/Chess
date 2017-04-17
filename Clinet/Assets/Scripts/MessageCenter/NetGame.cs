using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading;

public class NetGame : GameManager
{
    //对手的IP地址
    public static string _rivalIP = "";
    //对手的Email地址
    public static string _rivalEmail = "";

    public float lastTime;

    public GameObject RetractButtonPanel;
    public GameObject ExitButtonPanel;
    public GameObject RestartButtonPanel;

    public GameObject RetractMessagePanel;
    public GameObject AgreeRetractMessage;
    public GameObject DisgreeRetractMessage;
    public GameObject WaitingMessage;


    //StoneManager脚本
    public StoneManager SM;
    //MatchManager脚本
    public MatchManager MM;


    void Start()
    {
        //每隔5S，向检测对手是否在线
        InvokeRepeating("SendCheckMessageToServer", 0.0f, 5);
    }

    void Awake()
    {
        //加载资源
        normalChess = Resources.LoadAll("_newNormalChess");
        seletcedChess = Resources.LoadAll("_newSelectChess");

        SM = (StoneManager)GameObject.Find("ChessBoard").GetComponent("StoneManager");
        MM = (MatchManager)GameObject.Find("MatchManager").GetComponent("MatchManager");
    }

    void Update()
    {
        base.MainProcess();
    }


    /// <summary>
    /// 移动棋子，并向对手发送消息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public override void Click(int id, int row, int col)
    {
        if (base._beRedTurn != base._beSide)
            return;

        base.Click(id, row, col);

        //将点击信息发送给服务器
        SendClickMessageToServer(id, row, col);
    }

    /// <summary>
    /// 判断游戏结果，并将游戏结果发送给服务器
    /// </summary>
    public override void JudgeVictory()
    {      
        //如果是执红棋
        if (_beSide)
        {
            //黑色的将死了，则获胜；红色的将死了，则失败
            if (StoneManager.s[20]._dead == true)
            {
                //获胜
                SendResultToServer();
            }
        }
        //如果是执黑棋，则与红棋相反
        else
        {
            if (StoneManager.s[4]._dead == true)
            {
                //获胜
                SendResultToServer();
            }
        }

        base.JudgeVictory();
    }


    #region 按钮点击事件

    /// <summary>
    /// 悔棋，并向对手发送消息
    /// </summary>
    public override void Back()
    {
        if (base._beRedTurn != _beSide)
            return;

        RetractMessagePanel.SetActive(true);
        WaitingMessage.SetActive(true);

        //将悔棋消息发送给服务器
        SendBackMessageToServer();
    }

    /// <summary>
    /// 重新开始
    /// </summary>
    public override void Restart()
    {
        //告诉服务器自己选择重开，服务器告诉对手玩家已放弃比赛
        Common.connSocket.Close();
        _rivalIP = "";
        MM._hintMessage.SetActive(false);
        SceneManager.LoadScene("NetGame");
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public override void ReturnToMenu()
    {
        //告诉服务器自己选择重开，服务器告诉对手玩家已放弃比赛
        Common.connSocket.Close();
        _rivalIP = "";
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// 将对方是否悔棋的panel隐藏
    /// </summary>
    public void SetRetractMessagePanelDisable()
    {
        RetractMessagePanel.SetActive(false);
        AgreeRetractMessage.SetActive(false);
        DisgreeRetractMessage.SetActive(false);
        WaitingMessage.SetActive(false);
    }

    /// <summary>
    /// 同意悔棋
    /// </summary>
    public void AgreeRetract()
    {
        SendAgreeBackMessageToServer(true);
        RetractButtonPanel.SetActive(false);

        BackOne();
        BackOne();
    }

    /// <summary>
    /// 不同意悔棋
    /// </summary>
    public void DisagreeRetract()
    {
        SendAgreeBackMessageToServer(false);
        RetractButtonPanel.SetActive(false);
    }

    /// <summary>
    /// 显示确认退出的界面
    /// </summary>
    public void EnableExitPanel()
    {
        ExitButtonPanel.SetActive(true);
    }

    /// <summary>
    /// 显示确认重开的界面
    /// </summary>
    public void EnableRestartPanel()
    {
        RestartButtonPanel.SetActive(true);
    }

    /// <summary>
    /// 隐藏确认和重开的界面
    /// </summary>
    public void DisableRestartAndExitPanel()
    {
        ExitButtonPanel.SetActive(false);
        RestartButtonPanel.SetActive(false);
    }

    #endregion


    #region 处理从服务端接收到的消息，包括点击信息、悔棋信息、初始化棋盘信息

    /// <summary>
    /// 处理从服务端接收到的消息
    /// </summary>
    public void ProcessingMessage(byte[] data)
    {
        //获取口令
        int command = data[1];

        switch (command)
        {
            case 0:
                ClickFromNetwork(data);
                break;
            case 1:
                BackFromNetwork(data);
                break;
            case 2:
                AgreeBackMessage(data);
                break;
            case 3:
                DisconnectFromNetwork();
                break;
            default:
                break;
        }          
    }

    /// <summary>
    /// 接收对手下棋的消息
    /// </summary>
    /// <param name="buffer"></param>
    private void ClickFromNetwork(byte[] buffer)
    {
        //[ 命令20 (2位) | ip(自己的ip 25位) | 移动的棋子id（4位）| 移动到的行数row(4位) | 移动到的列数col(4位) | ...]
        byte[] tmpId = new byte[4];
        byte[] tmpRow = new byte[4];
        byte[] tmpCol = new byte[4];

        Array.Copy(buffer, 27, tmpId, 0, 4);
        Array.Copy(buffer, 31, tmpRow, 0, 4);
        Array.Copy(buffer, 35, tmpCol, 0, 4);

        int id = System.BitConverter.ToInt32(tmpId, 0);
        int row = System.BitConverter.ToInt32(tmpRow, 0);
        int col = System.BitConverter.ToInt32(tmpCol, 0);

        base.Click(id, 9 - row, 8 - col);
    }

    /// <summary>
    /// 接收到对手的悔棋消息
    /// </summary>
    private void BackFromNetwork(byte[] data)
    {
        //弹出弹窗，让玩家确认是否同意对方悔棋
        RetractButtonPanel.SetActive(true);
        Debug.Log("对方申请悔棋");
    }

    /// <summary>
    /// 接收到对方是否同意悔棋的消息
    /// </summary>
    /// <param name="data"></param>
    private void AgreeBackMessage(byte[] data)
    {
        //[ 命令22 (2位) | 是否同意悔棋(1位) | ...] (同意为1，不同意为0)
        if (data[2] == 1)
        {
            WaitingMessage.SetActive(false);
            AgreeRetractMessage.SetActive(true);
           
            BackOne();
            BackOne();
        }
        else if (data[2] == 0)
        {
            //提醒玩家对方不同意悔棋的请求
            WaitingMessage.SetActive(false);
            DisgreeRetractMessage.SetActive(true);
            Debug.Log("对方不同意悔棋");
        }
    }

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    /// <param name="buffer"></param>
    public void InitFromNetwork(bool isRed)
    {      
        //判断自己是红棋还是黑棋
        bool beRedSide = isRed;
        SM.StoneInit(beRedSide);
        _beSide = beRedSide;
        MM._hintMessage.SetActive(false);
    }

    /// <summary>
    /// 接收到对手离线的消息
    /// </summary>
    private void DisconnectFromNetwork()
    {
        MM._hintMessage.SetActive(true);
        MM._hintMessage.GetComponent<Text>().text = "您的对手已离线...";
        SendRivalExitToServer();

        StartCoroutine(ToolManager.DelayToInvokeDo(() => { Restart(); }, 2f));
    }

    #endregion


    #region 发送消息给服务端，包括点击信息、悔棋信息、心跳检测、游戏结果

    /// <summary>
    /// 告诉服务器自己的点击信息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    private void SendClickMessageToServer(int id, int row, int col)
    {
        //[ 命令20 (2位) | ip(对方的ip 25位) | 移动的棋子id（4位）| 移动到的行数row(4位) | 移动到的列数col(4位) | ...]

        //发送给对方
        byte[] sendIp = Encoding.UTF8.GetBytes(_rivalIP.Trim('\0'));
        byte[] sendId = System.BitConverter.GetBytes(id);
        byte[] sendRow = System.BitConverter.GetBytes(row);
        byte[] sendCol = System.BitConverter.GetBytes(col);

        List<byte> list = new List<byte>();

        list.Insert(0, 2);
        list.Insert(1, 0);

        list.AddRange(sendIp);
        //sendIp 不够25位
        if (sendIp.Length < 25)
        {
            for (int i = 0; i < 25 - sendIp.Length; i++)
            {
                list.Add(0);
            }
        }

        list.AddRange(sendId);
        list.AddRange(sendRow);
        list.AddRange(sendCol);

        //开始发送
        try
        {
            Common.connSocket.Send(list.ToArray());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MM.ConnectProblem();
        }
    }

    /// <summary>
    /// 告诉服务器自己申请悔棋
    /// </summary>
    private void SendBackMessageToServer()
    {
        //[ 命令21 (2位) | ip(对方的ip 25位) | ...]
        byte[] sendIp = Encoding.UTF8.GetBytes(_rivalIP.Trim('\0'));

        List<byte> list = new List<byte>();

        list.Insert(0, 2);
        list.Insert(1, 1);

        list.AddRange(sendIp);
        //sendIp 不够25位
        if (sendIp.Length < 25)
        {
            for (int i = 0; i < 25 - sendIp.Length; i++)
            {
                list.Add(0);
            }
        }

        //开始发送
        try
        {
            Common.connSocket.Send(list.ToArray());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MM.ConnectProblem();
        }
    }

    /// <summary>
    /// 告诉服务器是否同意悔棋
    /// </summary>
    /// <param name="isAgree"></param>
    private void SendAgreeBackMessageToServer(bool isAgree)
    {
        //[ 命令22 (2位) | ip(对方的ip 25位) | 是否同意悔棋(1位) | ...](同意为1，不同意为0)
        byte[] sendIp = Encoding.UTF8.GetBytes(_rivalIP.Trim('\0'));

        List<byte> list = new List<byte>();

        list.Insert(0, 2);
        list.Insert(1, 2);

        list.AddRange(sendIp);
        //sendIp 不够25位
        if (sendIp.Length < 25)
        {
            for (int i = 0; i < 25 - sendIp.Length; i++)
            {
                list.Add(0);
            }
        }

        if (isAgree)
        {
            list.Insert(27, 1);
        }
        else
        {
            list.Insert(27, 0);
        }

        //开始发送
        try
        {
            Common.connSocket.Send(list.ToArray());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MM.ConnectProblem();
        }
    }  

    /// <summary>
    /// 告诉服务器，需要获取对手是否在线的消息
    /// </summary>
    private void SendCheckMessageToServer()
    {
        //[ 命令23 (2位) | ip(对方的ip 25位) | ...] 
        if (Common.connSocket != null && NetworkManager._myIP != "" && _rivalIP != "")
        {
            byte[] sendIp = Encoding.UTF8.GetBytes(_rivalIP.Trim('\0'));

            List<byte> list = new List<byte>();

            list.Insert(0, 2);
            list.Insert(1, 3);

            list.AddRange(sendIp);
            //sendIp 不够25位
            if (sendIp.Length < 25)
            {
                for (int i = 0; i < 25 - sendIp.Length; i++)
                {
                    list.Add(0);
                }
            }

            //开始发送
            try
            {
                Common.connSocket.Send(list.ToArray());
            }
            catch (Exception ex)
            {
                MM.ConnectProblem();
                Debug.Log(ex.Message);
            }
        }
    }

    /// <summary>
    /// 将游戏的结果发送给服务器
    /// </summary>
    private void SendResultToServer()
    {
        //[ 命令11(2位) | 胜利方的账户邮箱(20位) | 失败方的账户邮箱(20位) | ...]
        byte[] sendMyEmail = Encoding.UTF8.GetBytes(NetworkManager._myEmail);
        byte[] sendRivalEmail = Encoding.UTF8.GetBytes(_rivalEmail);

        List<byte> list = new List<byte>();

        list.Insert(0, 1);
        list.Insert(1, 1);

        list.AddRange(sendMyEmail);
        //sendMyEmail 不够20位
        if (sendMyEmail.Length < 20)
        {
            for (int i = 0; i < 20 - sendMyEmail.Length; i++)
            {
                list.Add(0);
            }
        }

        list.AddRange(sendRivalEmail);
        //sendRivalEmail 不够20位
        if (sendRivalEmail.Length < 20)
        {
            for (int i = 0; i < 20 - sendRivalEmail.Length; i++)
            {
                list.Add(0);
            }
        }

        //开始发送
        try
        {
            Common.connSocket.Send(list.ToArray());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MM.ConnectProblem();
        }
    }

    /// <summary>
    /// 将对手离线的消息告诉服务器
    /// </summary>
    private void SendRivalExitToServer()
    {
        //[ 命令12(2位) | 我方账户邮箱(20位) | 对手账户邮箱(20位) | ...]
        byte[] sendMyEmail = Encoding.UTF8.GetBytes(NetworkManager._myEmail);
        byte[] sendRivalEmail = Encoding.UTF8.GetBytes(_rivalEmail);

        List<byte> list = new List<byte>();

        list.Insert(0, 1);
        list.Insert(1, 2);

        list.AddRange(sendMyEmail);
        //sendMyEmail 不够20位
        if (sendMyEmail.Length < 20)
        {
            for (int i = 0; i < 20 - sendMyEmail.Length; i++)
            {
                list.Add(0);
            }
        }

        list.AddRange(sendRivalEmail);
        //sendRivalEmail 不够20位
        if (sendRivalEmail.Length < 20)
        {
            for (int i = 0; i < 20 - sendRivalEmail.Length; i++)
            {
                list.Add(0);
            }
        }

        //开始发送
        try
        {
            Common.connSocket.Send(list.ToArray());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MM.ConnectProblem();
        }
    }

    #endregion

}