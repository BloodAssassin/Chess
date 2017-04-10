using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { set; get; }

    private GameObject AccountManager;

    /// <summary>
    /// 连接服务器的ip
    /// </summary>
    public static string _ip = "115.159.207.34"; //连接云服务器
    //public static string _ip = "127.0.0.1"; //连接本地

    /// <summary>
    /// 连接服务器的端口
    /// </summary>
    public static int _point = 6213;

    /// <summary>
    ///  自己的IP地址
    /// </summary>
    public static string _myIP = "";

    /// <summary>
    /// 玩家的账户邮箱
    /// </summary>
    public static string _myEmail = "";

    /// <summary>
    /// 保存服务器来的byte
    /// </summary>
    public static List<byte[]> bufferList = new List<byte[]>();

    /// <summary>
    /// 判断客户端是否连接着服务器
    /// </summary>
    public static bool isConnected = false;

    /// <summary>
    /// 判断客户端是否尝试重新连接服务器
    /// </summary>
    public static bool isPassby = false;

    private void Awake()
    {
        AccountManager = GameObject.Find("AccountManager");       
    }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            ConnectServer();
        }
        else if (Instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        ////检测是否连接着服务器，如没有连接服务器则显示连接错误，并且自动重连
        //if (!isConnected)
        //{
        //    ConnectProblem();

        //    //检测是否开始尝试重新连接
        //    if(!isReconnected)
        //    {
        //        //提示当前网络不可用
        //        AccountManager.GetComponent<AccountManager>().ErrorPanel.SetActive(true);
        //        AccountManager.GetComponent<AccountManager>().NetworkUnavailable.SetActive(true);
            
        //        isReconnected = true;
        //        //每隔2S，尝试重新连接
        //        InvokeRepeating("ConnectServer", 0.0f, 3);
        //    }
        //}
    }

    void OnApplicationQuit()
    {
        //当程序退出前，关闭socket连接
        Common.connSocket.Close();
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void ConnectServer()
    {
        try
        {
            Common.connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address = IPAddress.Parse(_ip);
            IPEndPoint point = new IPEndPoint(address, _point);
            Common.connSocket.Connect(point);
            isConnected = true;
            //isReconnected = false;

            //连接成功，获取服务器发来的消息
            Common.connSocket.BeginReceive(Common.buffer, 0, Common.buffer.Length, 0, new AsyncCallback(Receive), Common.connSocket);
        }
        catch (Exception ex)
        {
            isConnected = false;
            ConnectProblem();
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// 接收来自服务器的消息，并将消息保存起来供消息处理中心处理
    /// </summary>
    /// <param name="result"></param>
    void Receive(IAsyncResult result)
    {
        Socket clientSocket = result.AsyncState as Socket;
        try
        {
            //获取实际的长度
            int num = clientSocket.EndReceive(result);
            if (num > 0)
            {
                byte[] buffer = new byte[num];
                Array.Copy(Common.buffer, 0, buffer, 0, num); //复制数据到data
                bufferList.Add(buffer);

                //连接成功，再一次获取服务器发来的消息
                clientSocket.BeginReceive(Common.buffer, 0, Common.buffer.Length, 0, new AsyncCallback(Receive), clientSocket);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            isConnected = false;
            clientSocket.Shutdown(SocketShutdown.Receive);
            clientSocket.Close();
        }
    }

    /// <summary>
    /// 当联网出现问题的时候
    /// </summary>
    public void ConnectProblem()
    {
        //Application.loadedLevelName == "NetGame"
        //if (SceneManager.GetActiveScene().name == "NetGame")
        //{
        //    GameObject Canvas = GameObject.Find("Canvas");
        //    GameObject ReturnToMenuButton = Canvas.transform.Find("ReturnToMenuButton").gameObject;
        //    GameObject HintMessage = Canvas.transform.Find("HintMessage").gameObject;
        //    GameObject SelectColorPanel = Canvas.transform.Find("SelectColorPanel").gameObject;

        //    HintMessage.SetActive(true);
        //    SelectColorPanel.SetActive(false);
        //    ReturnToMenuButton.SetActive(true);
        //    HintMessage.GetComponent<Text>().text = "联网暂时不可用\n点击屏幕任意位置退回菜单";
        //}
        if(SceneManager.GetActiveScene().name == "Login")
        {         
            AccountManager.GetComponent<AccountManager>().ErrorPanel.SetActive(true);
            AccountManager.GetComponent<AccountManager>().NetworkUnavailable.SetActive(true);
        }
        else if(SceneManager.GetActiveScene().name == "Menu")
        {
            GameObject.Find("Main Camera").GetComponent<GameModeSelect>().ErrorPanel.SetActive(true);
            GameObject.Find("Main Camera").GetComponent<GameModeSelect>().NetworkUnavailable.SetActive(true);
        }
    }

}

/// <summary>
/// 与服务器通信的类
/// </summary>
public class Common
{
    /// <summary>
    /// 与服务器通信的socket
    /// </summary>
    public static Socket connSocket;

    /// <summary>
    /// 保存服务器来的byte
    /// </summary>
    public static byte[] buffer = new byte[1024 * 1024];

    /// <summary>
    /// 保存当登陆成功后。从服务器获取的所有用户ip
    /// </summary>
    public static string ip;

    /// <summary>
    /// time计时器
    /// </summary>
    //public static System.Windows.Forms.Timer time;

    /// <summary>
    /// 当前是否连接到服务器
    /// </summary>
    public static bool isConnect = false;
}
