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

    /*********************协议说明***********************/
    //根据协议解码
    /*
     * 1）执红方还是黑方：这个信息由客户端发出，服务器接收
     *    [ 命令1：棋子颜色(1位) | 持子的颜色(1位) | ...]
     *    第一个字节固定是1；最后一个字节是1或者0，1表示走红棋，0表示走黑棋
     *    
     * 2）点击信息
     *    [ 命令2：点击信息(1位) | ip(对方的ip 25位) | 移动的棋子id（4位）| 移动到的行数row(4位) | 移动到的列数col(4位) | ...]
     *    点击的棋子id（有可能是-1）
     *    注意：两个客户端之间的棋盘是颠倒的，所以传给另一个客户端的row应该是9-row，col是8-col
     *    注意2：判断点击的棋子是否是自己的棋子，若不是则不能点中（s[id]._red==_beSide）
     *    
     * 3)悔棋
     *    [ 命令3：悔棋(1位) | ip(对方的ip 25位) | ...]
     *    
     * 4)初始化棋盘：这个信息由服务器发出，客户端接收
     *    [ 命令4：初始化棋盘(1位) | ip(自己的ip和对方的ip 50位) | 持子的颜色(1位) | ...] 
     *    
     * 5)对手已掉线：这个信息由服务器发出，客户端接收
     *    [ 命令5：对手掉线(1位) | ...] 
     *    
     * 6)心跳检测：这个信息由客户端发出，服务器接收
     *    [ 命令6：定时检测对手是否掉线(1位) | ip(对方的ip和自己的ip 50位) | ...] 
     *    
     */


    //对手的IP地址
    public static string _rivalIP = "";
    //对手的Email地址
    public static string _rivalEmail = "";

    public float lastTime;

    //StoneManager脚本
    public StoneManager SM;
    //BattleManager脚本
    public BattleManager BM;


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
        BM = (BattleManager)GameObject.Find("BattleManager").GetComponent("BattleManager");
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


    #region 按钮点击事件

    /// <summary>
    /// 悔棋，并向对手发送消息
    /// </summary>
    public override void Back()
    {
        if (base._beRedTurn != _beSide)
            return;
        BackOne();
        BackOne();

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
        BM._hintMessage.SetActive(false);
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


    #endregion


    #region 处理从服务端接收到的消息，包括点击信息、悔棋信息、初始化棋盘信息

    /// <summary>
    /// 处理从服务端接收到的消息
    /// </summary>
    public void ProcessingMessage()
    {
        if (NetworkManager.bufferList.Count != 0)
        {
            byte[] buffer = NetworkManager.bufferList[0];
            //获取口令
            int command = buffer[0];

            switch (command)
            {
                case 2:
                    ClickFromNetwork(buffer);
                    break;
                case 3:
                    BackFromNetwork();
                    break;
                case 4:
                    //InitFromNetwork(buffer);
                    break;
                case 5:
                    DisconnectFromNetwork();
                    break;
                default:
                    break;
            }
            NetworkManager.bufferList.RemoveAt(0);
        }
    }

    /// <summary>
    /// 接收对手下棋的消息
    /// </summary>
    /// <param name="buffer"></param>
    void ClickFromNetwork(byte[] buffer)
    {
        //协议：
        //[ 命令2：点击信息(1位) | ip(自己的ip 25位) | 移动的棋子id（4位）| 移动到的行数row(4位) | 移动到的列数col(4位) | ...]
        byte[] tmpId = new byte[4];
        byte[] tmpRow = new byte[4];
        byte[] tmpCol = new byte[4];
        Array.Copy(buffer, 26, tmpId, 0, 4);
        Array.Copy(buffer, 30, tmpRow, 0, 4);
        Array.Copy(buffer, 34, tmpCol, 0, 4);
        int id = System.BitConverter.ToInt32(tmpId, 0);
        int row = System.BitConverter.ToInt32(tmpRow, 0);
        int col = System.BitConverter.ToInt32(tmpCol, 0);

        base.Click(id, 9 - row, 8 - col);
    }

    /// <summary>
    /// 接收到对手的悔棋消息
    /// </summary>
    void BackFromNetwork()
    {
        BackOne();
        BackOne();
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
        BM._hintMessage.SetActive(false);
    }

    /// <summary>
    /// 接收到对手离线的消息
    /// </summary>
    void DisconnectFromNetwork()
    {
        BM._hintMessage.SetActive(true);
        BM._hintMessage.GetComponent<Text>().text = "您的对手已离线...";

        StartCoroutine(ToolManager.DelayToInvokeDo(() => { Restart(); }, 2f));
    }

    #endregion


    #region 发送消息给服务端，包括选择的棋子颜色、点击信息、悔棋信息、心跳检测

    /// <summary>
    /// 告诉服务器自己选的是什么颜色的棋子
    /// </summary>
    /// <param name="isRed"></param>
    void SendColorMessageToServer(bool isRed)
    {
        //[ 命令10(2位) | 账户邮箱(20位) | 棋子颜色(1位) | ...]
        List<byte> list = new List<byte>();
        list.Insert(0, 1);
        list.Insert(1, 0);

        byte[] sendEmail = Encoding.UTF8.GetBytes(NetworkManager._myEmail);
        list.AddRange(sendEmail);
        //sendEmail 不够20位
        if (sendEmail.Length < 20)
        {
            for (int i = 0; i < 20 - sendEmail.Length; i++)
            {
                list.Add(0);
            }
        }

        if (isRed)
        {
            list.Insert(22, 1);
        }
        else
        {
            list.Insert(22, 0);
        }

        //开始发送
        try
        {
            Common.connSocket.Send(list.ToArray());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            BM.ConnectProblem();
        }
    }

    /// <summary>
    /// 告诉服务器自己的点击信息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    void SendClickMessageToServer(int id, int row, int col)
    {
        //[ 命令2：点击信息(1位) | ip(对方的ip 25位) | 移动的棋子id（4位）| 移动到的行数row(4位) | 移动到的列数col(4位) | ...]

        //发送给对方
        byte[] sendIp = Encoding.UTF8.GetBytes(_rivalIP.Trim('\0'));
        byte[] sendId = System.BitConverter.GetBytes(id);
        byte[] sendRow = System.BitConverter.GetBytes(row);
        byte[] sendCol = System.BitConverter.GetBytes(col);

        List<byte> list = new List<byte>();
        list.Insert(0, 2);
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
            BM.ConnectProblem();
        }
    }

    /// <summary>
    /// 告诉服务器自己悔棋了
    /// </summary>
    void SendBackMessageToServer()
    {
        //[ 命令3：悔棋(1位) | ip(对方的ip 25位) | ...]
        byte[] sendIp = Encoding.UTF8.GetBytes(_rivalIP.Trim('\0'));

        List<byte> list = new List<byte>();
        list.Insert(0, 3);
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
            BM.ConnectProblem();
        }
    }

    /// <summary>
    /// 告诉服务器，需要获取对手是否在线的消息
    /// </summary>
    void SendCheckMessageToServer()
    {
        if (Common.connSocket != null && NetworkManager._myIP != "" && _rivalIP != "")
        {
            //[ 命令6：定时检测对手是否掉线(1位) | ip(对方的ip和自己的ip 50位) | ...] 
            string allIp = string.Format("{0},{1}", _rivalIP.Trim('\0'), NetworkManager._myIP.Trim('\0'));
            byte[] sendIp = Encoding.UTF8.GetBytes(allIp);
            List<byte> list = new List<byte>();
            list.Insert(0, 6);
            list.AddRange(sendIp);
            //sendIp 不够50位
            if (sendIp.Length < 50)
            {
                for (int i = 0; i < 50 - sendIp.Length; i++)
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
                BM.ConnectProblem();
                Debug.Log(ex.Message);
            }
        }
    }

    #endregion

}