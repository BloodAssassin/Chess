using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.IO;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;

namespace ServerForm
{
    class AccountManager
    {
        

        /// <summary>
        /// 接收到来自客户端的关于账户系统的相关指令，并分类处理
        /// </summary>
        /// <param name="data"></param>
        public void ReceiveMessage(byte[] data,string ip)
        {
            //判断协议位
            int command = data[1];

            switch (command)
            {
                case 0:
                    //用户请求注册账户
                    RegisterAccount(data, ip);
                    break;
                case 1:
                    //用户请求通过账户登录
                    LoginAccount(data, ip);
                    break;
                case 2:
                    //给需要找回密码的邮箱发送验证码
                    SendIdentifyingCodeToAccount(data, ip);
                    break;
                case 3:
                    //验证验证码是否正确
                    ValidateIdentifyingCode(data, ip);
                    break;
                case 4:
                    //给邮箱重新设置密码
                    ResetPassword(data, ip);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 注册账户
        /// </summary>
        /// <param name="data"></param>
        private void RegisterAccount(byte[] data,string ip)
        {
            // [ 命令00(2位) | 账户邮箱(20位) | 账户昵称(30位) | 账户密码(20位) | ...]

            //从指令中获取需要注册的邮箱、昵称、密码等信息
            string email = Encoding.UTF8.GetString(data, 2, 20).Trim('\0');
            string name = Encoding.UTF8.GetString(data, 22, 30).Trim('\0');
            string password = Encoding.UTF8.GetString(data, 52, 20).Trim('\0');

            try
            {
                //打开并连接数据库
                SqlAccess sql = new SqlAccess();

                //从“AccountManager”表中选择“email=email”的所有数据
                DataSet ds = sql.SelectWhere("AccountManager", new string[] { "*" }, new string[] { "email" }, new string[] { "=" }, new string[] { email });

                //查找表中是否已经存在该邮箱，若已存在，则返回错误信息；若不存在，则向数据库中注册插入新的数据
                if (IsExit(ds))
                {
                    SendMessage(data, ip, false);
                    sql.Close();
                    return;
                }
                else
                {
                    //插入数据
                    sql.InsertInto("AccountManager", new string[] { "email", "name", "password" }, new string[] { email, name, password });
                    sql.InsertInto("BattleInformation", new string[] { "email", "allnumber", "score", "winnumber" }, new string[] { email, "0", "1200", "0" });
                    sql.Close();
                    SendMessage(data, ip, true);
                }          
            }
            catch (Exception ex)
            {
                SendMessage(data, ip, false);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 账户登录
        /// </summary>
        /// <param name="data"></param>
        private void LoginAccount(byte[] data, string ip)
        {
            //[ 命令01(2位) | 账户邮箱(20位) | 账户密码(20位) | ...]
            string email = Encoding.UTF8.GetString(data, 2, 20).Trim('\0');
            string password = Encoding.UTF8.GetString(data, 22, 20).Trim('\0');

            try
            {
                SqlAccess sql = new SqlAccess();

                //从“AccountManager”表中选择“email=email”的“password”的数据
                DataSet ds = sql.SelectWhere("AccountManager", new string[] { "password" }, new string[] { "email" }, new string[] { "=" }, new string[] { email });

                //查找表中是否已经存在该邮箱，若已存在，则验证密码是否正确；若不存在或者密码不正确，则返回错误信息
                if (IsExit(ds,password))
                {
                    SendMessage(data, ip, email);
                    sql.Close();
                }
                else
                {                   
                    SendMessage(data, ip, false);
                    sql.Close();
                }          
            }
            catch (Exception ex)
            {
                SendMessage(data, ip, false);
                MessageBox.Show(ex.Message);
            }
        }


        #region 找回密码

        /// <summary>
        /// 给需要找回密码的邮箱发送验证码
        /// </summary>
        /// <param name="data"></param>
        private void SendIdentifyingCodeToAccount(byte[] data, string ip)
        {
            //[ 命令02(2位) | 账户邮箱(20位) | ...]
            string email = Encoding.UTF8.GetString(data, 2, 20).Trim('\0');

            try
            {
                SqlAccess sql = new SqlAccess();

                //从“AccountManager”表中选择“email=email”的所有数据
                DataSet ds = sql.SelectWhere("AccountManager", new string[] { "*" }, new string[] { "email" }, new string[] { "=" }, new string[] { email });

                //查找表中是否已经存在该邮箱，若已存在，则发送验证邮件；若不存在，则返回错误信息
                if (IsExit(ds))
                {
                    //发送验证邮件
                    SendEmail(email);
                    SendMessage(data, ip, true);
                    sql.Close();
                    return;
                }
                else
                {
                    SendMessage(data, ip, false);
                    sql.Close();
                    return;
                }          
            }
            catch (Exception ex)
            {
                SendMessage(data, ip, false);
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// 验证验证码是否符合
        /// </summary>
        /// <param name="data"></param>
        private void ValidateIdentifyingCode(byte[] data, string ip)
        {
            //[ 命令03(2位) | 账户邮箱(20位) | 验证码(6位) | ...]
            string email = Encoding.UTF8.GetString(data, 2, 20).Trim('\0');
            string code = Encoding.UTF8.GetString(data, 22, 6);

            /*
             * 首先在验证码池中，寻找是否存在待验证的邮箱
             * 其次判断验证码是否过期
             * 最后判断验证码是否正确
             */
            foreach (string[] s in mainServer.identifyingCode)
            {
                if (s[0] == email)
                {
                    if (DateDiff(s[2], DateTime.Now.ToString()))
                    {
                        if (s[1] == code)
                        {
                            mainServer.identifyingCode.Remove(s);
                            SendMessage(data, ip, true);
                            return;
                        }
                    }
                    else
                    {
                        mainServer.identifyingCode.Remove(s);
                    }
                }
            }
            SendMessage(data, ip, false);
        }


        /// <summary>
        /// 重新设置密码
        /// </summary>
        /// <param name="data"></param>
        private void ResetPassword(byte[] data, string ip)
        {
            //[ 命令04(2位)  | 账户邮箱(20位) | 账户密码(20位) | ...]
            string email = Encoding.UTF8.GetString(data, 2, 20).Trim('\0');
            string password = Encoding.UTF8.GetString(data, 22, 20).Trim('\0');

            try
            {
                SqlAccess sql = new SqlAccess();

                //从“AccountManager”表中选择“email”= email 的那一行，将“password”的数据改为password
                sql.UpdateInto("AccountManager", new string[] { "password" }, new string[] { password }, "email", email);

                sql.Close();

                SendMessage(data, ip, true);
            }
            catch (Exception ex)
            {
                SendMessage(data, ip, false);
                MessageBox.Show(ex.Message);
            }
        }

        #endregion


        #region 功能类函数

        /// <summary>
        /// 向用户提供反馈
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        /// <param name="isSucceed"></param>
        private void SendMessage(byte[] data,string ip, bool isSucceed)
        {
            //编写反馈指令
            byte[] buffer = new byte[3];
            buffer[0] = data[0];
            buffer[1] = data[1];            

            if (isSucceed)
            {
                buffer[2] = 0;
            }
            else
            {
                buffer[2] = 1;
            }

            //发送指令
            if (Common.connSocket.ContainsKey(ip))
            {
                Common.connSocket[ip].Send(buffer);
            }
        }


        /// <summary>
        /// 当用户登录成功后向用户反馈消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        /// <param name="email"></param>
        /// <param name="isSucceed"></param>
        private void SendMessage(byte[] data, string ip, string email)
        {
            //编写反馈指令
            //[ 命令01(2位) | 是否成功(1位) | 账户邮箱(客户端的邮箱20位) | 账户昵称(30位) | 分数(4位) | 胜场(4位) | 总场次(4位) | ...]
            byte[] sendEmail = Encoding.UTF8.GetBytes(email);
            MatchManager MM = new MatchManager();
            byte[] sendName = Encoding.UTF8.GetBytes(MM.GetName(email));
            byte[] sendScore = System.BitConverter.GetBytes(Convert.ToInt32(MM.GetScore(email)));
            byte[] sendAllNumber = System.BitConverter.GetBytes(Convert.ToInt32(MM.GetAllNumber(email)));
            byte[] sendWinNumber = System.BitConverter.GetBytes(Convert.ToInt32(MM.GetWinNumber(email)));

            List<byte> list = new List<byte>();
            list.Insert(0, data[0]);
            list.Insert(1, data[1]);

            list.Insert(2, 0);

            list.AddRange(sendEmail);
            //sendEmail 不够20位
            if (sendEmail.Length < 20)
            {
                for (int i = 0; i < 20 - sendEmail.Length; i++)
                {
                    list.Add(0);
                }
            }

            list.AddRange(sendName);
            //sendName 不够30位
            if (sendName.Length < 30)
            {
                for (int i = 0; i < 30 - sendName.Length; i++)
                {
                    list.Add(0);
                }
            }

            list.AddRange(sendScore);
            //sendScore 不够4位
            if (sendScore.Length < 4)
            {
                for (int i = 0; i < 4 - sendScore.Length; i++)
                {
                    list.Add(0);
                }
            }

            list.AddRange(sendWinNumber);
            //sendWinNumber 不够4位
            if (sendWinNumber.Length < 4)
            {
                for (int i = 0; i < 4 - sendWinNumber.Length; i++)
                {
                    list.Add(0);
                }
            }

            list.AddRange(sendAllNumber);
            //sendAllNumber 不够4位
            if (sendAllNumber.Length < 4)
            {
                for (int i = 0; i < 4 - sendAllNumber.Length; i++)
                {
                    list.Add(0);
                }
            }

            //发送指令
            if (Common.connSocket.ContainsKey(ip))
            {
                Common.connSocket[ip].Send(list.ToArray());
            }
        }


        /// <summary>
        /// 发送验证码到指定邮箱
        /// </summary>
        /// <param name="email"></param>
        private void SendEmail(string email)
        {
            try
            {
                SmtpClient mailClient = new SmtpClient("smtp.qq.com");
                mailClient.EnableSsl = true;
                //Credentials登陆SMTP服务器的身份验证.
                mailClient.Credentials = new NetworkCredential("2432603461@qq.com", "frzmlzcumwnzeagj");

                MailMessage message = new MailMessage(new MailAddress("2432603461@qq.com"), new MailAddress(email));

                //生成验证码
                string code = CreateIdentifyingCode(6);

                //将验证信息保存
                string[] tmpList = new string[3] { email, code, DateTime.Now.ToString() };
                mainServer.identifyingCode.Add(tmpList);

                //邮件主题
                message.Subject = "账号密码找回";
                //邮件内容
                message.Body = "\n尊敬的用户，您好！\n我们收到了来自您的账号的安全请求。请使用下面的验证码验证您的账号归属。\n\n以下是您的验证码：\n" + code + "\n\n请注意：该验证码将在10分钟后过期，请尽快验证！\n享受您的游戏之旅！\n中国象棋";
                
                //发送邮件
                mailClient.Send(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }                       
        }


        /// <summary>
        /// 生成随机字母与数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        private string CreateIdentifyingCode(int Length)
        {
            char[] Pattern = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            string result = "";
            int n = Pattern.Length;
            System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < Length; i++)
            {
                int rnd = random.Next(0, n);
                result += Pattern[rnd];
            }
            return result;
        }


        /// <summary>
        /// 计算时间差，判断验证码是否过期
        /// </summary>
        /// <param name="DateTime1"></param>
        /// <param name="DateTime2"></param>
        /// <returns></returns>
        private bool DateDiff(string DateTime1, string DateTime2)
        {
            TimeSpan ts1 = new TimeSpan(Convert.ToDateTime(DateTime1).Ticks);
            TimeSpan ts2 = new TimeSpan(Convert.ToDateTime(DateTime2).Ticks);

            TimeSpan ts = ts1.Subtract(ts2).Duration();

            if (ts.Days.ToString() == "0" && ts.Hours.ToString() == "0" && ts.Minutes<=9)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 查找缓存表中是否存在数据
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private bool IsExit(DataSet ds)
        {
            //遍历这些数据，若存在则返回true，若不存在则返回false
            if (ds != null)
            {
                DataTable table = ds.Tables[0];

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// 查找缓存表中是否存在数据并验证密码信息
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private bool IsExit(DataSet ds,string password)
        {
            //遍历这些数据，若存在则返回true，若不存在则返回false
            if (ds != null)
            {
                DataTable table = ds.Tables[0];

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        if (row[column].ToString() == password)
                        {
                            return true;
                        }          
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
