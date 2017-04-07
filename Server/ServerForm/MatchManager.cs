using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerForm
{
    class MatchManager
    {

        /// <summary>
        /// 接收到来自客户端的关于匹配系统的相关指令，并分类处理
        /// </summary>
        /// <param name="data"></param>
        public void ReceiveMessage(byte[] data, string ip)
        {
            //判断协议位
            int command = data[1];

            switch (command)
            {
                case 0:
                    //玩家请求匹配对手
                    GetUserInformation(data, ip);
                    break;
                default:
                    break;
            }
        }


        #region 获取用户信息

        /// <summary>
        /// 将玩家数据放入匹配池
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        private void GetUserInformation(byte[] data, string ip)
        {
            //* [ 命令10(2位) | 账户邮箱(20位) | 棋子颜色(1位) | ...]

            //获取用户邮箱、积分等信息
            string email = Encoding.UTF8.GetString(data, 2, 20).Trim('\0');
            string score = "";

            try
            {
                SqlAccess sql = new SqlAccess();

                //从“BattleInformation”表中选择“email=email”的“score”的数据
                DataSet ds = sql.SelectWhere("BattleInformation", new string[] { "score" }, new string[] { "email" }, new string[] { "=" }, new string[] { email });

                //遍历这些数据，获取用户的积分
                if (ds != null)
                {
                    DataTable table = ds.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            score = row[column].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetUserInformation函数报错：" + ex.Message);
            }

            //尝试匹配对手
            MatchingRival(ip, email, score,data[22]);        
        }

        #endregion 


        #region 匹配对手

        /// <summary>
        /// 匹配对手
        /// </summary>
        /// <param name="ip">玩家的IP</param>
        /// <param name="email">玩家的邮箱</param>
        /// <param name="score">玩家的积分</param>
        /// /// <param name="color">玩家选择的棋子颜色</param>
        private void MatchingRival(string ip, string email, string score, byte color)
        {
            //color==1，代表玩家选择执红棋，则在黑棋的匹配池中寻找对象
            if (color == 1)
            {
                MatchingBlackPool(ip, email, score);
            }
            //color==0，代表玩家选择执黑棋，则在红棋的匹配池中寻找对象
            else if (color == 0)
            {
                MatchingRedPool(ip, email, score);
            }
            else
            {
                MessageBox.Show("MatchingRival报错：棋子的颜色获取有误");
            }          
        }

        /// <summary>
        /// 在执红色棋子的匹配池中寻找对手
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="email"></param>
        /// <param name="score"></param>
        private void MatchingRedPool(string ip, string email, string score)
        {
            foreach (string[] s in mainServer.redMatchPool)
            {
                if (Math.Abs(Convert.ToInt32(s[2]) - Convert.ToInt32(score)) < 30)
                {
                    //将对战信息发送给双方玩家
                    SendRivalMessageToOther(s[0],ip,s[1],email);
                    //将红色匹配池中将已经匹配到对手的信息移除
                    mainServer.redMatchPool.Remove(s);
                    return;
                }
            }

            //暂时没有找到合适的匹配对象，将信息放入匹配池
            string[] information = new string[] { ip, email, score };
            mainServer.blackMatchPool.Add(information);   
        }

        /// <summary>
        /// 在执黑色棋子的匹配池中寻找对手
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="email"></param>
        /// <param name="score"></param>
        private void MatchingBlackPool(string ip, string email, string score)
        {
            foreach (string[] s in mainServer.blackMatchPool)
            {
                if (Math.Abs(Convert.ToInt32(s[2]) - Convert.ToInt32(score)) < 30)
                {
                    //将对战信息发送给双方玩家
                    SendRivalMessageToOther(ip, s[0], email, s[1]);
                    //将黑色匹配池中将已经匹配到对手的信息移除
                    mainServer.blackMatchPool.Remove(s);
                    return;
                }
            }

            //暂时没有找到合适的匹配对象，将信息放入匹配池
            string[] information = new string[] { ip, email, score };
            mainServer.redMatchPool.Add(information);  
        }

        #endregion


        #region 将匹配完成的对手信息打包发送给玩家

        /// <summary>
        /// 将匹配到的对手的信息分别发送给两个客户端
        /// </summary>
        /// <param name="redIp">红色方的IP</param>
        /// <param name="blackIp">黑色方的IP</param>
        /// <param name="redEmail">红色方的Email</param>
        /// <param name="blackEmail">黑色方的Email</param>
        private void SendRivalMessageToOther(string redIp, string blackIp, string redEmail, string blackEmail)
        {
            if (Common.connSocket.ContainsKey(redIp))
            {             
                Common.connSocket[redIp].Send(CreatBytes(blackIp,blackEmail,true));
            }

            if (Common.connSocket.ContainsKey(blackIp))
            {
                Common.connSocket[blackIp].Send(CreatBytes(redIp, redEmail, false));
            }
        }

        /// <summary>
        /// 将匹配到的对手信息打包
        /// </summary>
        /// <param name="ip">对手的IP</param>
        /// <param name="email">对手的邮箱</param>
        /// <param name="isRed">自己的执棋颜色</param>
        private byte[] CreatBytes(string ip, string email, bool isRed)
        {
            //[ 命令10(2位) | 对方的ip (25位) | 对方的邮箱(20位) | 棋子颜色(1位) | ...]
            byte[] sendIp = Encoding.UTF8.GetBytes(ip);
            byte[] sendEmail = Encoding.UTF8.GetBytes(email);

            List<byte> list = new List<byte>();
            list.Insert(0, 1);
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

            list.AddRange(sendEmail);
            //sendEmail 不够20位
            if (sendIp.Length < 20)
            {
                for (int i = 0; i < 20 - sendEmail.Length; i++)
                {
                    list.Add(0);
                }
            }

            if (isRed)
            {
                list.Insert(47, 1);
            }
            else
            {
                list.Insert(47, 0);
            }

            return list.ToArray();
        }

        #endregion

    }
}
