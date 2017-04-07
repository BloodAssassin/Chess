using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class AccountManager : MonoBehaviour
{
    //两个panel（登陆和注册）
    public GameObject LoginPanel;
    public GameObject RegisterPanel;

    //LoginPanel上的各种控件
    public GameObject IF_Account;
    public GameObject IF_Password;

    //RegisterPanel上的各种控件
    public GameObject IF_Emaill;
    public GameObject IF_Name;
    public GameObject IF_RegisterPassword;


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
            //弹出消息窗，告知用户注册成功
            //在弹出窗中点击确认键，回到登录界面
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
        //[ 命令01(2位) | 是否成功(1位) | 账户邮箱(客户端的邮箱20位) | ...]（若失败则没有客户端的邮箱）
        if (data[2] == 0)
        {
            Debug.Log("登陆成功");

            //获取正在使用的用户账号
            string myEmail = Encoding.UTF8.GetString(data, 3, 20);
            NetworkManager._myEmail = myEmail.Length > 0 ? myEmail.Trim('\0') : "";
            Debug.Log("本地的Email为：" + NetworkManager._myEmail);

            //跳转到游戏主界面
            SceneManager.LoadScene("Menu");
        }
        else if (data[2] == 1)
        {
            Debug.Log("登陆失败");
            //弹出消息窗，告知用户登录失败
            //在弹出窗中点击确认键，回到登录界面
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

        }
        else if (data[2] == 1)
        {

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

        }
        else if (data[2] == 1)
        {

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

        }
        else if (data[2] == 1)
        {

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

        if (!EmailIsRight(_email))
        {
            //告知用户email格式错误
            Debug.Log("email格式错误");
            return;
        }
        else if (!NameIsRight(_name))
        {
            //告知用户name格式错误
            Debug.Log("name格式错误");
            return;
        }
        else if (!PasswordIsRight(_password))
        {
            //告知用户password格式错误
            Debug.Log("password格式错误");
            return;
        }
        else
        {
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
    }

    /// <summary>
    /// 向服务器申请登陆账号
    /// </summary>
    private void SendLoginMessageToServer(string email, string password)
    {
        byte[] _email = Encoding.UTF8.GetBytes(email);
        byte[] _password = Encoding.UTF8.GetBytes(password);

        if (!EmailIsRight(_email))
        {
            //告知用户email格式错误
            Debug.Log("email格式错误");
            return;
        }
        else if (!PasswordIsRight(_password))
        {
            //告知用户password格式错误
            Debug.Log("password格式错误");
            return;
        }
        else
        {
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
    }

    #region 找回密码

    /// <summary>
    /// 向服务器申请找回密码
    /// </summary>
    public void SendFindPasswordMessageToServer()
    {

    }

    /// <summary>
    /// 向服务器申请验证邮箱
    /// </summary>
    public void SendIdentifyingCodeMessageToServer()
    {

    }

    /// <summary>
    /// 向服务器申请重新设置密码
    /// </summary>
    public void SendResetPasswordMessageToServer()
    {

    }

    #endregion

    #endregion


    #region 按钮点击事件

    /// <summary>
    /// 跳转到注册界面
    /// </summary>
    public void btn_SwitchToRegister()
    {
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(true);
    }

    /// <summary>
    /// 跳转到登陆界面
    /// </summary>
    public void btn_SwitchToLogin()
    {
        LoginPanel.SetActive(true);
        RegisterPanel.SetActive(false);
    }

    /// <summary>
    /// 跳转到找回密码页面
    /// </summary>
    public void btn_SwitchToFindPassword()
    {
        //跳转到找回密码页面
    }

    /// <summary>
    /// 注册账号
    /// </summary>
    public void btn_RegisterAccount()
    {
        string email = IF_Emaill.GetComponent<InputField>().text;
        string name = IF_Name.GetComponent<InputField>().text;
        string password = IF_RegisterPassword.GetComponent<InputField>().text;

        SendRegisterMessageToServer(email, name, password);
    }

    /// <summary>
    /// 登陆账号
    /// </summary>
    public void btn_LoginAccount()
    {
        string email = IF_Account.GetComponent<InputField>().text;
        string password = IF_Password.GetComponent<InputField>().text;

        SendLoginMessageToServer(email, password);
    }

    /// <summary>
    /// 向服务器申请获取验证码
    /// </summary>
    public void btn_FindPassword()
    {
        //验证邮箱格式
        //发送邮箱给服务器
    }

    /// <summary>
    /// 向服务器申请验证邮箱
    /// </summary>
    public void btn_SendIdentifyingCode()
    {
        //发送验证码给服务器
    }

    /// <summary>
    /// 向服务器申请重新设置密码
    /// </summary>
    public void ResetPassword()
    {
        //验证密码格式
        //发送新密码给服务器
    }


    #endregion


    #region 判断输入的信息是否合法

    /// <summary>
    /// 判断输入的email是否合法
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    private bool EmailIsRight(byte[] email)
    {
        if (email.Length <= 20 && email.Length > 0)
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
    private bool NameIsRight(byte[] name)
    {
        if (name.Length <= 30 && name.Length > 0)
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
    private bool PasswordIsRight(byte[] password)
    {
        if (password.Length <= 20 && password.Length > 0)
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

    #endregion
}
