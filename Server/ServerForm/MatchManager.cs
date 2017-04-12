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
        public const int deno = 400;

        public const double novice = 1000.0;  //0-999 初学者
        public const double someExp = 1500.0;  //1000-1499 有经验者
        public const double skill = 2000.0;  //1500-1999 熟练者
        public const double expert = 2200.0;  //2000-2199 专家
        public const double master = 2400.0;  //2200-2399 大师 >2400 宗师



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
                case 1:
                    //胜利方发来游戏结果
                    CalcPoint(data);
                    break;
                case 2:
                    //有玩家检测到对手离线
                    CalcPoint2(data);
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
            string score = GetScore(email);

            if (score != "")
            {
                //尝试匹配对手
                MatchingRival(ip, email, score, data[22]);
            }                  
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
            if (sendEmail.Length < 20)
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


        /// <summary>
        /// 根据正常游戏结果进行积分计算
        /// </summary>
        /// <param name="data"></param>
        private void CalcPoint(byte[] data)
        {
            //[ 命令11(2位) | 胜利方的账户邮箱(20位) | 失败方的账户邮箱(20位) | ...]

            //获取邮箱
            string winEmail = Encoding.UTF8.GetString(data, 2, 20).Trim('\0');
            string loseEmail = Encoding.UTF8.GetString(data, 22, 20).Trim('\0');

            //获取当前积分
            int winScore = Convert.ToInt32(GetScore(winEmail));
            int loseScore = Convert.ToInt32(GetScore(loseEmail));

            //计算最终积分
            int winLastScore = calcResult(winScore, loseScore, true);
            int loseLastScore = calcResult(loseScore, winScore, false);

            //修改数据库
            ChangeScoreInDatabase(winEmail, winLastScore.ToString());
            ChangeScoreInDatabase(loseEmail, loseLastScore.ToString());
            ChangeGameNumberInDatabase(winEmail, true);
            ChangeGameNumberInDatabase(loseEmail, false);
        }

        /// <summary>
        /// 因有人强退而进行的积分计算
        /// </summary>
        /// <param name="data"></param>
        private void CalcPoint2(byte[] data)
        {
            //[ 命令12(2位) | 我方账户邮箱(20位) | 对手账户邮箱(20位) | ...]

            //获取邮箱
            string winEmail = Encoding.UTF8.GetString(data, 2, 20).Trim('\0');
            string loseEmail = Encoding.UTF8.GetString(data, 22, 20).Trim('\0');

            //获取当前积分
            int winScore = Convert.ToInt32(GetScore(winEmail));
            int loseScore = Convert.ToInt32(GetScore(loseEmail));

            //计算最终积分
            int winLastScore = calcUnusualResult(winScore, loseScore, true);
            int loseLastScore = calcUnusualResult(loseScore, winScore, false);

            //修改数据库
            ChangeScoreInDatabase(winEmail, winLastScore.ToString());
            ChangeScoreInDatabase(loseEmail, loseLastScore.ToString());
            ChangeGameNumberInDatabase(winEmail, true);
            ChangeGameNumberInDatabase(loseEmail, false);
        }


        #region 数据库相关操作

        /// <summary>
        /// 根据邮箱地址在数据库中查找对应的积分
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private string GetScore(string email)
        {
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
                            return row[column].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("根据邮箱查找分数函数报错：" + ex.Message);
            }

            return "";
        }

        /// <summary>
        /// 根据邮箱地址在数据库中查找对应的游戏总场次
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private string GetAllNumber(string email)
        {
            try
            {
                SqlAccess sql = new SqlAccess();

                //从“BattleInformation”表中选择“email=email”的“allnumber”的数据
                DataSet ds = sql.SelectWhere("BattleInformation", new string[] { "allnumber" }, new string[] { "email" }, new string[] { "=" }, new string[] { email });

                //遍历这些数据，获取用户的积分
                if (ds != null)
                {
                    DataTable table = ds.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            return row[column].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("根据邮箱查找总场次函数报错：" + ex.Message);
            }

            return "";
        }

        /// <summary>
        /// 根据邮箱地址在数据库中查找对应的游戏获胜场次
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private string GetWinNumber(string email)
        {
            try
            {
                SqlAccess sql = new SqlAccess();

                //从“BattleInformation”表中选择“email=email”的“winnumber”的数据
                DataSet ds = sql.SelectWhere("BattleInformation", new string[] { "winnumber" }, new string[] { "email" }, new string[] { "=" }, new string[] { email });

                //遍历这些数据，获取用户的积分
                if (ds != null)
                {
                    DataTable table = ds.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            return row[column].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("根据邮箱查找获胜场次函数报错：" + ex.Message);
            }

            return "";
        }


        /// <summary>
        /// 在数据库中修改积分
        /// </summary>
        /// <param name="email"></param>
        /// <param name="score"></param>
        private void ChangeScoreInDatabase(string email, string score)
        {          
            try
            {
                SqlAccess sql = new SqlAccess();

                //从“BattleInformation”表中选择“email”= email 的那一行，将“score”的数据改为score
                sql.UpdateInto("BattleInformation", new string[] { "score" }, new string[] { score }, "email", email);

                sql.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 在数据库中修改比赛场数
        /// </summary>
        /// <param name="email"></param>
        /// <param name="isWin"></param>
        private void ChangeGameNumberInDatabase(string email, bool isWin)
        {
            try
            {
                SqlAccess sql = new SqlAccess();

                string allnumber = (Convert.ToInt32(GetAllNumber(email)) + 1).ToString();

                //从“BattleInformation”表中选择“email”= email 的那一行，将“allnumber”的数据改为allnumber
                sql.UpdateInto("BattleInformation", new string[] { "allnumber" }, new string[] { allnumber }, "email", email);

                if (isWin)
                {
                    string winnumber = (Convert.ToInt32(GetWinNumber(email)) + 1).ToString();

                    //从“BattleInformation”表中选择“email”= email 的那一行，将“winnumber”的数据改为winnumber
                    sql.UpdateInto("BattleInformation", new string[] { "winnumber" }, new string[] { winnumber }, "email", email);
                }

                sql.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #endregion


        #region 利用ELO算法进行积分计算

        /// <summary>
        /// 自适应k值
        /// </summary>
        /// <param name="score">积分</param>
        /// <param name="isWin">是否获胜</param>
        /// <returns>k值结果</returns>
        private double adaptationK(double score, bool isWin)
        {
            return score < novice ? (32.0 + (isWin ? 32.0 : 0.0)) :
                score < someExp ? (32.0 + (isWin ? 16.0 : 0.0)) :
                score < skill ? 32.0 :
                score < expert ? 20.0 :
                score < master ? 15.0 : 10.0;
        }

        /// <summary>
        /// 计算a相对b的胜率
        /// </summary>
        /// <param name="a">a的积分</param>
        /// <param name="b">b的积分</param>
        /// <returns>胜率</returns>
        private double getWinRate(double a, double b)
        {
            return 1 / (1 + Math.Pow(10, (b - a) / deno));
        }

        /// <summary>
        /// 计算积分变化
        /// </summary>
        /// <param name="a">a的积分</param>
        /// <param name="b">b的积分</param>
        /// <param name="isWin">是否获胜</param>
        /// <returns>a的积分终值</returns>
        private double getScoreChg(double a, double b, bool isWin)
        {
            double w = isWin ? 1.0 : 0.0;
            double winRate = getWinRate(a, b);
            return (w - winRate) * adaptationK(a, isWin);
        }

        /// <summary>
        /// 计算积分结果
        /// </summary>
        /// <param name="a">a的积分</param>
        /// <param name="b">b的积分</param>
        /// <param name="isWin">是否获胜</param>
        /// <returns>a的积分终值</returns>
        private int calcResult(int a, int b, bool isWin)
        {
            double da = Convert.ToDouble(a);
            double db = Convert.ToDouble(b);

            double result = getScoreChg(da, db, isWin);

            return (a + Convert.ToInt32(Math.Round(result)));
        }

        /// <summary>
        /// 计算有人强退的积分结果
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="isWin"></param>
        /// <returns></returns>
        private int calcUnusualResult(int a, int b, bool isWin)
        {
            double da = Convert.ToDouble(a);
            double db = Convert.ToDouble(b);
            double result = 0;

            if (isWin)
            {
                result = getScoreChg(da, db, isWin) / 2;
            }
            else
            {
                result = getScoreChg(da, db, isWin) * 2;
            }

            return (a + Convert.ToInt32(Math.Round(result)));
        }

        #endregion
    }
}
