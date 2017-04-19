using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class AccountManager : MonoBehaviour
{

    public GameObject _networkManager;

    //各个panel界面
    public GameObject LoginPanel;
    public GameObject RegisterPanel1;
    public GameObject RegisterPanel2;
    public GameObject RegisterPanel3;
    public GameObject FindPasswordPanel1;
    public GameObject FindPasswordPanel2;
    public GameObject FindPasswordPanel3;
    public GameObject ErrorPanel;

    //LoginPanel上的各种控件
    public GameObject IF_LoginEmail;
    public GameObject IF_LoginPassword;

    //RegisterPanel1上的各种控件   
    public GameObject IF_RegisterName;
    public GameObject IF_RegisterPassword;

    //RegisterPanel2上的各种控件   
    public GameObject IF_RegisterEmaill;

    //FindPasswordPanel1上的各种控件   
    public GameObject IF_FindPasswordEmaill;
    public GameObject IF_FindPasswordCode;

    //FindPasswordPanel2上的各种控件   
    public GameObject IF_FindPasswordPassword1;
    public GameObject IF_FindPasswordPassword2;

    //ErrorPanel
    //登陆时账号或密码错误
    public GameObject LoginWrongful;

    //注册时输入的昵称或密码不合法
    public GameObject NameOrPasswordWrongful;
    //注册时输入的邮箱不合法
    public GameObject EmailWrongful;

    //找回密码时邮箱不存在
    public GameObject EmailNonexistent;
    //找回密码时验证码错误
    public GameObject CodeWrongful;
    //找回密码时新设置的密码不合法
    public GameObject NewPasswordWrongful;
    //找回密码时两次输入的密码不一致
    public GameObject PasswordsNotSame;

    //打开游戏时提示网络不可用
    public GameObject NetworkUnavailable;

    public string registerName = "";
    public string registerPassword = "";
    public string registerEmail = "";

    public string findPasswordEmail = "";



    #region 接收来自服务器的消息，处理消息

    /// <summary>
    /// 消息处理中心
    /// </summary>
    /// <param name="data"></param>
    public void ProcessingMessage(byte[] data)
    {
        int command = data[1];
        switch (command)
        {
            case 0:
                //注册账号（告知用户注册是否成功）
                RegisterAccount(data);
                break;
            case 1:
                //登录账号（告知用户用户名或密码是否正确）
                LoginAccount(data);
                break;
            case 2:
                //找回密码——需要找回密码的邮箱（告知用户需要找回的邮箱是否存在）
                EmailIsExist(data);
                break;
            case 3:
                //找回密码——邮箱验证码（告知用户验证码是否正确或过期）
                IdentifyingCodeIsRight(data);
                break;
            case 4:
                //找回密码——新的密码（告知用户新的密码是否设置成功）
                ResetPasswordSucceed(data);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 注册账号（告知用户注册是否成功）
    /// </summary>
    /// <param name="data"></param>
    private void RegisterAccount(byte[] data)
    {
        if (data[2] == 0)
        {
            Debug.Log("注册成功");
            IF_RegisterEmaill.GetComponent<InputField>().text = "";
            RegisterPanel3.SetActive(true);
        }
        else if (data[2] == 1)
        {
            Debug.Log("注册失败");
            //弹出消息窗，告知用户注册失败
            //在弹出窗中点击确认键，回到注册界面
        }
    }

    /// <summary>
    /// 登录账号（告知用户用户名或密码是否正确）
    /// </summary>
    /// <param name="data"></param>
    private void LoginAccount(byte[] data)
    {
        //[ 命令01(2位) | 是否成功(1位) | 账户邮箱(客户端的邮箱20位) | 账户昵称(30位) | 分数(4位) | 胜场(4位) | 总场次(4位) | ...]（若失败则没有客户端的邮箱及后面的信息）
        if (data[2] == 0)
        {
            Debug.Log("登陆成功"+data.Length);

            //获取正在使用的用户账号
            string myEmail = Encoding.UTF8.GetString(data, 3, 20);
            NetworkManager._myEmail = myEmail.Length > 0 ? myEmail.Trim('\0') : "";
            Debug.Log("本地的Email为：" + NetworkManager._myEmail);

            //获取正在使用的用户昵称
            string myName = Encoding.UTF8.GetString(data, 23, 30);
            NetworkManager._myName = myName.Length > 0 ? myName.Trim('\0') : "";
            Debug.Log("本地的Name为：" + NetworkManager._myName);

            //获取正在使用的用户的积分
            byte[] tmpScore = new byte[4];
            Array.Copy(data, 53, tmpScore, 0, 4);
            int score = System.BitConverter.ToInt32(tmpScore, 0);
            NetworkManager._myScore = score;
            Debug.Log("本地的Score为：" + NetworkManager._myScore);

            //获取正在使用的用户的胜场
            byte[] tmpWinNumber = new byte[4];
            Array.Copy(data, 57, tmpWinNumber, 0, 4);
            int winNumber = System.BitConverter.ToInt32(tmpWinNumber, 0);
            NetworkManager._myWinNumber = winNumber;
            Debug.Log("本地的WinNumber为：" + NetworkManager._myWinNumber);

            //获取正在使用的用户的总场次
            byte[] tmpAllNumber = new byte[4];           
            Array.Copy(data, 61, tmpAllNumber, 0, 4);         
            int allNumber = System.BitConverter.ToInt32(tmpAllNumber, 0);
            NetworkManager._myAllNumber = allNumber;
            Debug.Log("本地的AllNumber为：" + NetworkManager._myAllNumber);
                           

            IF_LoginEmail.GetComponent<InputField>().text = "";
            IF_LoginPassword.GetComponent<InputField>().text = "";

            //跳转到游戏主界面
            SceneManager.LoadScene("Menu");
        }
        else if (data[2] == 1)
        {
            Debug.Log("登陆失败");
            //弹出消息窗，告知用户登录失败
            ErrorPanel.SetActive(true);
            LoginWrongful.SetActive(true);

            IF_LoginPassword.GetComponent<InputField>().text = "";
        }
    }

    #region 找回密码

    /// <summary>
    /// 找回密码——需要找回密码的邮箱（告知用户需要找回的邮箱是否存在）
    /// </summary>
    /// <param name="data"></param>
    private void EmailIsExist(byte[] data)
    {
        if (data[2] == 0)
        {
            Debug.Log("要找回密码的邮箱存在");
        }
        else if (data[2] == 1)
        {
            Debug.Log("要找回密码的邮箱不存在");
            ErrorPanel.SetActive(true);
            EmailNonexistent.SetActive(true);
        }
    }

    /// <summary>
    /// 找回密码——邮箱验证码（告知用户验证码是否正确或过期）
    /// </summary>
    /// <param name="data"></param>
    private void IdentifyingCodeIsRight(byte[] data)
    {
        if (data[2] == 0)
        {
            Debug.Log("验证码正确");
            IF_FindPasswordCode.GetComponent<InputField>().text = "";
            FindPasswordPanel2.SetActive(true);
        }
        else if (data[2] == 1)
        {
            Debug.Log("验证码错误");
            IF_FindPasswordCode.GetComponent<InputField>().text = "";
            ErrorPanel.SetActive(true);
            CodeWrongful.SetActive(true);
        }
    }

    /// <summary>
    /// 找回密码——新的密码（告知用户新的密码是否设置成功）
    /// </summary>
    /// <param name="data"></param>
    private void ResetPasswordSucceed(byte[] data)
    {
        if (data[2] == 0)
        {
            FindPasswordPanel3.SetActive(true);
            IF_FindPasswordPassword1.GetComponent<InputField>().text = "";
            IF_FindPasswordPassword2.GetComponent<InputField>().text = "";
        }
        else if (data[2] == 1)
        {
            Debug.Log("重置密码失败");
        }
    }

    #endregion

    #endregion


    #region 向服务器发送请求

    /// <summary>
    /// 向服务器申请注册账号
    /// </summary>
    private void SendRegisterMessageToServer(string email, string name, string password)
    {
        byte[] _email = Encoding.UTF8.GetBytes(email);
        byte[] _name = Encoding.UTF8.GetBytes(name);
        byte[] _password = Encoding.UTF8.GetBytes(password);

        byte[] data = CreatRegisterBytes(_email, _name, _password);
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

    /// <summary>
    /// 向服务器申请登陆账号
    /// </summary>
    private void SendLoginMessageToServer(string email, string password)
    {
        byte[] _email = Encoding.UTF8.GetBytes(email);
        byte[] _password = Encoding.UTF8.GetBytes(password);

        byte[] data = CreatLoginBytes(_email, _password);
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

    #region 找回密码

    /// <summary>
    /// 向服务器申请找回密码
    /// </summary>
    private void SendFindPasswordMessageToServer(string email)
    {
         byte[] _email = Encoding.UTF8.GetBytes(email);

         byte[] data = CreatFindPasswordBytes1(_email);

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

    /// <summary>
    /// 向服务器申请验证邮箱
    /// </summary>
    private void SendIdentifyingCodeMessageToServer(string email, string code)
    {
        byte[] _email = Encoding.UTF8.GetBytes(email);

        byte[] _code = Encoding.UTF8.GetBytes(code);

        byte[] data = CreatFindPasswordBytes2(_email, _code);

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

    /// <summary>
    /// 向服务器申请重新设置密码
    /// </summary>
    public void SendResetPasswordMessageToServer(string email, string password)
    {
        //[ 命令04(2位) | 账户邮箱(20位) | 账户密码(20位) | ...]
        byte[] _email = Encoding.UTF8.GetBytes(email);

        byte[] _password = Encoding.UTF8.GetBytes(password);

        byte[] data = CreatFindPasswordBytes3(_email, _password);

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

    #endregion


    #region 按钮点击事件

    /// <summary>
    /// 跳转到注册界面
    /// </summary>
    public void btn_SwitchToRegister()
    {
        if (NetworkManager.isConnected)
        {
            RegisterPanel1.SetActive(true);
            Debug.Log(NetworkManager.isConnected);
        }
        else
        {
            _networkManager.GetComponent<NetworkManager>().ConnectServer();
        }
    }

    /// <summary>
    /// 跳转到登陆界面
    /// </summary>
    public void btn_SwitchToLogin()
    {
        LoginPanel.SetActive(true);
        RegisterPanel1.SetActive(false);
        RegisterPanel2.SetActive(false);
        RegisterPanel3.SetActive(false);
        FindPasswordPanel1.SetActive(false);
        FindPasswordPanel2.SetActive(false);
        FindPasswordPanel3.SetActive(false);
    }

    /// <summary>
    /// 跳转到找回密码页面
    /// </summary>
    public void btn_SwitchToFindPassword()
    {
        if (NetworkManager.isConnected)
        {
            FindPasswordPanel1.SetActive(true);
        }
        else
        {
            _networkManager.GetComponent<NetworkManager>().ConnectServer();
        }
    }

    /// <summary>
    /// 注册账号(注册界面1)
    /// </summary>
    public void btn_RegisterAccount1()
    {
        if (NetworkManager.isConnected)
        {
            registerName = IF_RegisterName.GetComponent<InputField>().text;
            registerPassword = IF_RegisterPassword.GetComponent<InputField>().text;

            //检测输入的昵称和密码是否合法
            if (NameIsRight(registerName) && PasswordIsRight(registerPassword))
            {
                IF_RegisterName.GetComponent<InputField>().text = "";
                IF_RegisterPassword.GetComponent<InputField>().text = "";
                //跳转到下一个界面
                RegisterPanel2.SetActive(true);
            }
            else if (!NameIsRight(registerName) || !PasswordIsRight(registerPassword))
            {
                //弹出窗口提示错误
                ErrorPanel.SetActive(true);
                NameOrPasswordWrongful.SetActive(true);
                Debug.Log("昵称过长或密码过长");
            }
        }
        else
        {
            _networkManager.GetComponent<NetworkManager>().ConnectServer();
        }     
    }

    /// <summary>
    /// 注册账号(注册界面2)
    /// </summary>
    public void btn_RegisterAccount2()
    {
        if (NetworkManager.isConnected)
        {
            registerEmail = IF_RegisterEmaill.GetComponent<InputField>().text;

            //检测输入的邮箱是否有问题
            if (EmailIsRight(registerEmail))
            {
                SendRegisterMessageToServer(registerEmail, registerName, registerPassword);
            }
            else
            {
                ErrorPanel.SetActive(true);
                EmailWrongful.SetActive(true);
                Debug.Log("输入的邮箱不合法");
            }
        }
        else
        {
            _networkManager.GetComponent<NetworkManager>().ConnectServer();
        }       
    }


    /// <summary>
    /// 登陆账号
    /// </summary>
    public void btn_LoginAccount()
    {
        if (NetworkManager.isConnected)
        {
            string email = IF_LoginEmail.GetComponent<InputField>().text;
            string password = IF_LoginPassword.GetComponent<InputField>().text;

            SendLoginMessageToServer(email, password);
        }
        else
        {
            _networkManager.GetComponent<NetworkManager>().ConnectServer();
        }
    }


    /// <summary>
    /// 向服务器申请获取验证码
    /// </summary>
    public void btn_FindPassword()
    {
        if (NetworkManager.isConnected)
        {
            //获取邮箱地址
            findPasswordEmail = IF_FindPasswordEmaill.GetComponent<InputField>().text;

            //发送获取验证码的请求给服务器
            SendFindPasswordMessageToServer(findPasswordEmail); ;
        }
        else
        {
            _networkManager.GetComponent<NetworkManager>().ConnectServer();
        }
    }

    /// <summary>
    /// 向服务器申请验证邮箱
    /// </summary>
    public void btn_SendIdentifyingCode()
    {
        if (NetworkManager.isConnected)
        {
            //获取验证码
            string code = IF_FindPasswordCode.GetComponent<InputField>().text;

            Debug.Log("发送的验证码为：" + code);

            //发送验证码给服务器
            SendIdentifyingCodeMessageToServer(findPasswordEmail, code);
        }
        else
        {
            _networkManager.GetComponent<NetworkManager>().ConnectServer();
        }
    }

    /// <summary>
    /// 向服务器申请重新设置密码
    /// </summary>
    public void btn_ResetPassword()
    {
        if (NetworkManager.isConnected)
        {
            //获取两次输入的密码
            string password1 = IF_FindPasswordPassword1.GetComponent<InputField>().text;
            string password2 = IF_FindPasswordPassword2.GetComponent<InputField>().text;

            //验证密码格式
            if (!PasswordIsRight(password1) || !PasswordIsRight(password2))
            {
                ErrorPanel.SetActive(true);
                NewPasswordWrongful.SetActive(true);
                Debug.Log("输入的密码不合法");
            }
            else if (password1 != password2)
            {
                ErrorPanel.SetActive(true);
                PasswordsNotSame.SetActive(true);
                Debug.Log("两次输入的密码不一致");
            }
            else
            {
                //发送新密码给服务器
                SendResetPasswordMessageToServer(findPasswordEmail, password1);
            }       
        }
        else
        {
            _networkManager.GetComponent<NetworkManager>().ConnectServer();
        }      
    }

    /// <summary>
    /// 将提示错误的panel设置为不可见
    /// </summary>
    public void btn_SetErrorPanelDisable()
    {
        ErrorPanel.SetActive(false);
        LoginWrongful.SetActive(false);

        NameOrPasswordWrongful.SetActive(false);
        EmailWrongful.SetActive(false);

        EmailNonexistent.SetActive(false);
        CodeWrongful.SetActive(false);
        NewPasswordWrongful.SetActive(false);

        PasswordsNotSame.SetActive(false);

        NetworkUnavailable.SetActive(false);
    }

    /// <summary>
    /// 游客登录
    /// </summary>
    public void btn_LoginByPassby()
    {
        NetworkManager.isPassby = true;
        SceneManager.LoadScene("Menu");
    }

    #endregion


    #region 判断输入的信息是否合法

    /// <summary>
    /// 判断输入的email是否合法
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    private bool EmailIsRight(string email)
    {
        //正则表达式字符串
        string emailStr = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        //邮箱正则表达式对象
        Regex emailReg = new Regex(emailStr);

        byte[] _email = Encoding.UTF8.GetBytes(email);

        if (_email.Length <= 20 && _email.Length > 0 && emailReg.IsMatch(email))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 判断输入的name是否合法
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private bool NameIsRight(string name)
    {
        byte[] _name = Encoding.UTF8.GetBytes(name);

        if (_name.Length <= 30 && _name.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 判断输入的password是否合法
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private bool PasswordIsRight(string password)
    {
        byte[] _password = Encoding.UTF8.GetBytes(password);

        if (_password.Length <= 20 && _password.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    #endregion


    #region 将要发送的所有内容打包起来

    /// <summary>
    /// 打包要发送的注册账号的消息
    /// </summary>
    /// <param name="_email">邮箱</param>
    /// <param name="_name">昵称</param>
    /// <param name="_password">密码</param>
    /// <returns></returns>
    private byte[] CreatRegisterBytes(byte[] _email, byte[] _name, byte[] _password)
    {
        //[ 命令00(2位) | 账户邮箱(20位) | 账户昵称(30位) | 账户密码(20位) | ...]
        List<byte> list = new List<byte>();

        list.Insert(0, 0);
        list.Insert(1, 0);

        list.AddRange(_email);
        //_email 不够20位
        for (int i = 0; i < 20 - _email.Length; i++)
        {
            list.Add(0);
        }

        list.AddRange(_name);
        //_name 不够30位
        for (int i = 0; i < 30 - _name.Length; i++)
        {
            list.Add(0);
        }

        list.AddRange(_password);
        //_password 不够20位
        for (int i = 0; i < 20 - _password.Length; i++)
        {
            list.Add(0);
        }

        return list.ToArray();
    }

    /// <summary>
    /// 打包要发送的登陆账号的消息
    /// </summary>
    /// <param name="_email"></param>
    /// <param name="_password"></param>
    /// <returns></returns>
    private byte[] CreatLoginBytes(byte[] _email, byte[] _password)
    {
        //[ 命令01(2位) | 账户邮箱(20位) | 账户密码(20位) | ...]
        List<byte> list = new List<byte>();

        list.Insert(0, 0);
        list.Insert(1, 1);

        list.AddRange(_email);
        //_email 不够20位
        for (int i = 0; i < 20 - _email.Length; i++)
        {
            list.Add(0);
        }

        list.AddRange(_password);
        //_password 不够20位
        for (int i = 0; i < 20 - _password.Length; i++)
        {
            list.Add(0);
        }

        return list.ToArray();
    }

    /// <summary>
    /// 打包要发送的获取验证码的消息
    /// </summary>
    /// <param name="_email"></param>
    /// <returns></returns>
    private byte[] CreatFindPasswordBytes1(byte[] _email)
    {
        //[ 命令02(2位) | 账户邮箱(20位) | ...]
        List<byte> list = new List<byte>();

        list.Insert(0, 0);
        list.Insert(1, 2);

        list.AddRange(_email);
        //_email 不够20位
        for (int i = 0; i < 20 - _email.Length; i++)
        {
            list.Add(0);
        }
      
        return list.ToArray();
    }

    /// <summary>
    /// 打包要发送的验证验证码的消息
    /// </summary>
    /// <param name="_email"></param>
    /// <param name="_code"></param>
    /// <returns></returns>
    private byte[] CreatFindPasswordBytes2(byte[] _email, byte[] _code)
    {
        //[ 命令03(2位) | 账户邮箱(20位) | 验证码(6位) | ...]
        List<byte> list = new List<byte>();

        list.Insert(0, 0);
        list.Insert(1, 3);

        list.AddRange(_email);
        //_email 不够20位
        for (int i = 0; i < 20 - _email.Length; i++)
        {
            list.Add(0);
        }

        list.AddRange(_code);

        return list.ToArray();
    }

    /// <summary>
    /// 打包要发送的重置密码的消息
    /// </summary>
    /// <param name="_email"></param>
    /// <param name="_code"></param>
    /// <returns></returns>
    private byte[] CreatFindPasswordBytes3(byte[] _email, byte[] _password)
    {
        //[ 命令04(2位) | 账户邮箱(20位) | 账户密码(20位) | ...]
        List<byte> list = new List<byte>();

        list.Insert(0, 0);
        list.Insert(1, 4);

        list.AddRange(_email);
        //_email 不够20位
        for (int i = 0; i < 20 - _email.Length; i++)
        {
            list.Add(0);
        }

        list.AddRange(_password);
        //_password 不够20位
        for (int i = 0; i < 20 - _password.Length; i++)
        {
            list.Add(0);
        }

        return list.ToArray();
    }

    #endregion
}
