using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerForm
{
    public partial class mainServer : Form
    {
        //用于记录当前正在匹配的玩家数据(IP,Email,score)
        public static List<string[]> redMatchPool = new List<string[]>();
        public static List<string[]> blackMatchPool = new List<string[]>();

        //用于记录当前正在进行邮箱验证的邮箱及其验证码（第一位邮箱，第二位验证码，第三位时间）
        public static List<string[]> identifyingCode = new List<string[]>();

        public mainServer()
        {
            InitializeComponent();
            Init();
        }

        /// <summary>
        /// 初始化datagridview属性
        /// </summary>
        public void Init()
        {
            #region datagridview一些属性设置
            dgvList.AllowUserToAddRows = false;
            dgvList.AllowUserToDeleteRows = false;
            dgvList.AllowUserToResizeColumns = false;
            dgvList.AllowUserToResizeRows = false;
            dgvList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvList.MultiSelect = false;
            dgvList.ReadOnly = true;
            dgvList.RowHeadersVisible = false;
            dgvList.BackgroundColor = Color.White;
            dgvList.ScrollBars = ScrollBars.Vertical;
            dgvList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            #endregion
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            StartServer();     
        }


        #region 服务端开启服务、监听消息、接收消息

        /// <summary>
        /// 打开服务器
        /// </summary>
        void StartServer()
        {
            try
            {
                string _ip = tbIp.Text;
                int _point = int.Parse(tbPoint.Text);

                //创建监听客户端请求的socket
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //监听用的ip和端口
                IPAddress address = IPAddress.Parse(_ip);
                IPEndPoint point = new IPEndPoint(address, _point);

                //绑定
                socket.Bind(point);
                socket.Listen(10);

                //异步 开始监听
                socket.BeginAccept(new AsyncCallback(Listen), socket);

                //禁用当前按钮
                btnStart.Enabled = false;

                //启动时间
                startTime.Text = DateTime.Now.ToString();

                //底部提示消息
                tssMsg.Text = "服务器已经启动";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="result"></param>
        void Listen(IAsyncResult result)
        {
            try
            {
                //获取监听的socket
                Socket clientSocket = result.AsyncState as Socket;
                //与服务器通信的socket
                Socket connSocket = clientSocket.EndAccept(result);

                string ip = connSocket.RemoteEndPoint.ToString();

                //连接成功。保存信息,并告知客户端
                if (!Common.connSocket.ContainsKey(ip))
                {
                    Common.connSocket.Add(ip, connSocket);

                    tellClientConnectMessage(ip);
                }                    

                //连接成功，更新服务器信息
                changeList(connSocket);

                //等待新的客户端连接 ，相当于循环调用
                clientSocket.BeginAccept(new AsyncCallback(Listen), clientSocket);

                //接收来自客户端信息 ，相当于循环调用
                connSocket.BeginReceive(Common.ReceiveBuffer, 0, Common.ReceiveBuffer.Length, 0, new AsyncCallback(Receive), connSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        /// <summary>
        /// 接收来自客户端信息
        /// </summary>
        /// <param name="result"></param>
        void Receive(IAsyncResult result)
        {
            //与客户端通信的socket
            Socket clientSocket = result.AsyncState as Socket;
            string ip = clientSocket.RemoteEndPoint.ToString();

            try
            {
                //获取实际的长度值
                int num = clientSocket.EndReceive(result);
                if (num > 0)
                {
                    byte[] data = new byte[num];
                    //复制实际的长度到data字节数组中
                    Array.Copy(Common.ReceiveBuffer, 0, data, 0, num);
                    
                    //判断协议位
                    int command = data[0];
                    
                    #region 没什么用的
                    //if (command == 2)
                    //{
                    //    //[ 命令2：点击信息(1位) | ip(自己的ip 25位) | 移动的棋子id（4位）| 移动到的行数row(4位) | 移动到的列数col(4位) | ...]
                    //    string xieyiwei = command.ToString();
                        
                    //    byte[] tmpId = new byte[4];
                    //    Array.Copy(data, 26, tmpId, 0, 4);
                    //    string id = System.BitConverter.ToInt32(tmpId, 0).ToString();
                    //    text("发送者IP：" + ip + "   命令：" + xieyiwei + "   移动的棋子ID：" + id);
                    //}


                    //if (command == 9)
                    //{
                    //    try
                    //    {
                    //        //发送指令
                    //        if (Common.connSocket.ContainsKey(ip))
                    //        {
                    //            byte[] tellIP = Encoding.UTF8.GetBytes(ip);
                    //            //[ 命令9(1位) | IP(自己的IP 25位) | ...]
                    //            List<byte> list = new List<byte>();

                    //            list.Insert(0, 9);

                    //            byte[] _myIP = Encoding.UTF8.GetBytes(ip);

                    //            list.AddRange(_myIP);
                    //            //_myIP 不够25位
                    //            for (int i = 0; i < 25 - _myIP.Length; i++)
                    //            {
                    //                list.Add(0);
                    //            }

                    //            Common.connSocket[ip].Send(list.ToArray());
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        MessageBox.Show("发送指令9失败" + ex.Message);
                    //    }
                    //}
                    #endregion 

                    switch (command)
                    {
                        case 0:
                            //服务器接收到来自客户端的关于账户系统方面的请求
                            AccountManager AM = new AccountManager();
                            AM.ReceiveMessage(data,ip);
                            break;
                        case 1:
                            //服务器保存发送方的IP、邮箱和其棋子的颜色，并寻求配对
                            MatchManager MM = new MatchManager();
                            MM.ReceiveMessage(data,ip);
                            break;
                        case 2:
                            //接收发送方发来的移动棋子的消息，并转发给接收方
                            SendMoveMessageToClient(data);
                            break;
                        case 3:
                            //接收发送方的悔棋消息，并转发给接收方
                            SendRetractMessageToClient(data);
                            break;
                        case 6:
                            //接收到发送方想要确定接收方是否在线的意愿，并做检测
                            SendDisconnectMessageToClient(data);
                            break;
                        default:
                            break;
                    }

                    //接收其他信息
                    clientSocket.BeginReceive(Common.ReceiveBuffer, 0, Common.ReceiveBuffer.Length, 0, new AsyncCallback(Receive), clientSocket);

                }
                else //客户端断开
                {
                    clientOff(clientSocket);
                }
            }
            catch (Exception ex)
            {
                clientOff(clientSocket);
                MessageBox.Show("尝试接收消息失败"+ex.Message);
            }

        }

        #endregion


        #region 更新服务端的在线列表和人数，以及通知客户端连接服务器成功

        /// <summary>
        /// 更新列表
        /// </summary>
        /// <param name="socket"></param>
        void changeList(Socket socket)
        {
            //获取客户端信息 ip和端口号
            string ip = socket.RemoteEndPoint.ToString();
            //客户端登陆时间
            string time = DateTime.Now.ToString();

            //跨线程操作ui
            this.Invoke(new Action(() =>
            {
                //新增一行
                dgvList.Rows.Add();

                //获取当前dgvList的行
                int rows = dgvList.Rows.Count;

                //赋值
                dgvList.Rows[rows - 1].Cells[0].Value = ip;
                dgvList.Rows[rows - 1].Cells[1].Value = time;

                //把ip当作当前行的tag标记一下，为了删除行的时候可以找到该行
                dgvList.Rows[rows - 1].Tag = ip;

                //更新在线人数
                changOnlineCount(true);
            }));

        }

        /// <summary>
        /// 更新在线人数
        /// </summary>
        /// <param name="tag">true=>+ false=>-</param>
        void changOnlineCount(bool tag)
        {
            int num = 0;
            if (tag) num = int.Parse(lbCount.Text) + 1;
            else num = int.Parse(lbCount.Text) - 1;

            this.Invoke(new Action(() =>
            {
                //更新在线人数
                lbCount.Text = num.ToString();
                if (num == 0) Common.connSocket.Clear();

            }));
        }

        /// <summary>
        /// 告知客户端已经成功连接服务器，并告知其ip和email
        /// </summary>
        /// <param name="ip"></param>
        void tellClientConnectMessage(string ip)
        {
            //[ 命令9(1位) | IP(客户端的IP 25位) | ...]
            byte[] sendIp = Encoding.UTF8.GetBytes(ip);

            List<byte> list = new List<byte>();
            list.Insert(0, 9);

            list.AddRange(sendIp);
            //sendIp 不够25位
            if (sendIp.Length < 25)
            {
                for (int i = 0; i < 25 - sendIp.Length; i++)
                {
                    list.Add(0);
                }
            }

            Common.connSocket[ip].Send(list.ToArray());
        }

        #endregion


        #region 服务端对从客户端接收到的消息，通过协议进行分类处理


        #region 一个简单的匹配对手的方法

        ///// <summary>
        ///// 匹配对手
        ///// </summary>
        ///// <param name="data"></param>
        ///// <param name="ip"></param>
        //void MatchingRival(byte[] data, string ip)
        //{
        //    string color = data[1].ToString();
        //    string[] playerMessage = new string[2];
        //    playerMessage[0] = ip;
        //    playerMessage[1] = color;
        //    player.Add(playerMessage);
        //    foreach (string[] s in player)
        //    {
        //        if (s[1] != color)
        //        {
        //            string rivalIp = s[0];
        //            player.Remove(s);
        //            player.RemoveAt(player.Count - 1);
        //            int tmp = data[1];
        //            if (tmp == 1)
        //            {
        //                SendRivalMessageToOther(ip, rivalIp);
        //            }
        //            else
        //            {
        //                SendRivalMessageToOther(rivalIp, ip);
        //            }

        //            return;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 将匹配到的对手的信息分别发送给两个客户端
        ///// </summary>
        ///// <param name="redIp"></param>
        ///// <param name="blackIp"></param>
        //void SendRivalMessageToOther(string redIp, string blackIp)
        //{
        //    if (Common.connSocket.ContainsKey(redIp))
        //    {
        //        string allIp = string.Format("{0},{1}", redIp.Trim('\0'), blackIp.Trim('\0'));
        //        byte[] sendIp = Encoding.UTF8.GetBytes(allIp);
        //        List<byte> list = new List<byte>();
        //        list.Insert(0, 4);
        //        list.AddRange(sendIp);
        //        //sendIp 不够50位
        //        if (sendIp.Length < 50)
        //        {
        //            for (int i = 0; i < 50 - sendIp.Length; i++)
        //            {
        //                list.Add(0);
        //            }
        //        }
        //        list.Insert(51, 1);
        //        Common.connSocket[redIp].Send(list.ToArray());
        //    }

        //    if (Common.connSocket.ContainsKey(blackIp))
        //    {
        //        string allIp = string.Format("{0},{1}", blackIp.Trim('\0'), redIp.Trim('\0'));
        //        byte[] sendIp = Encoding.UTF8.GetBytes(allIp);
        //        List<byte> list = new List<byte>();
        //        list.Insert(0, 4);
        //        list.AddRange(sendIp);
        //        //sendIp 不够50位
        //        if (sendIp.Length < 50)
        //        {
        //            for (int i = 0; i < 50 - sendIp.Length; i++)
        //            {
        //                list.Add(0);
        //            }
        //        }
        //        list.Insert(51, 0);
        //        Common.connSocket[blackIp].Send(list.ToArray());
        //    }
        //}

        #endregion


        /// <summary>
        /// 将一方悔棋的信息发送给另一方
        /// </summary>
        /// <param name="data"></param>
        void SendRetractMessageToClient(byte[] data)
        {
            int num = data.Length;
            //获取接收者的ip
            string receiveIp = Encoding.UTF8.GetString(data, 1, 25).Trim('\0');

            //发送给接收者悔棋的消息
            if (Common.connSocket.ContainsKey(receiveIp))
            {
                Common.connSocket[receiveIp].Send(data);
            }
        }

        /// <summary>
        /// 将一方移动棋子的信息发送给另一方
        /// </summary>
        /// <param name="data"></param>
        void SendMoveMessageToClient(byte[] data)
        {
            int num = data.Length;
            //获取接收者的ip
            string receiveIp = Encoding.UTF8.GetString(data, 1, 25).Trim('\0');

            //发送给接受者移动棋子的消息
            if (Common.connSocket.ContainsKey(receiveIp))
            {
                Common.connSocket[receiveIp].Send(data);
            }
        }

        /// <summary>
        /// 将对手的掉线信息发送给另一方
        /// </summary>
        /// <param name="receiveIp"></param>
        void SendDisconnectMessageToClient(byte[] data)
        {
            //获取发送方对手的IP地址和发送方的IP地址
            string allIP = Encoding.UTF8.GetString(data, 1, 50);
            string[] temp = allIP.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            string receiveIp = temp.Length > 0 ? temp[0].Trim('\0') : "";
            string senderIp = temp.Length > 0 ? temp[1].Trim('\0') : "";

            //如果发送方的对手已经离线，则向发送方返回命令5
            if (!Common.connSocket.ContainsKey(receiveIp))
            {
                List<byte> list = new List<byte>();
                list.Insert(0, 5);
                Common.connSocket[senderIp].Send(list.ToArray());
            }
        }

        /// <summary>
        /// 客户端关闭
        /// </summary>
        void clientOff(Socket clientSocket)
        {
            //从集合删除下线的ip
            string outIp = clientSocket.RemoteEndPoint.ToString();
            if (Common.connSocket.ContainsKey(outIp))
                Common.connSocket.Remove(outIp);

            //更新服务器在线人数
            changOnlineCount(false);

            this.Invoke(new Action(() =>
            {
                //更新列表
                //删除退出的ip
                for (int i = 0; i < dgvList.Rows.Count; i++)
                {
                    if (dgvList.Rows[i].Tag.ToString() == outIp)
                    {
                        dgvList.Rows.RemoveAt(i);
                        break;
                    }
                }

                //删除其在匹配队列中保存的数据（若存在）
                foreach (string[] s in redMatchPool)
                {
                    if (s[0] == outIp)
                    {
                        redMatchPool.Remove(s);
                        break;
                    }
                }
                foreach (string[] s in blackMatchPool)
                {
                    if (s[0] == outIp)
                    {
                        blackMatchPool.Remove(s);
                        break;
                    }
                }

            }));

            clientSocket.Shutdown(SocketShutdown.Receive);
            clientSocket.Close();
        }

        #endregion


        /// <summary>
        /// 写入文本文件
        /// </summary>
        /// <param name="value"></param>
        public void text(string value)
        {
            string path = "C:/Users/Administrator/Desktop/1.txt";
            FileStream f = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(f);
            sw.WriteLine(value);
            sw.Flush();
            sw.Close();
            f.Close();
        }
    }

    /// <summary>
    /// 公共类
    /// </summary>
    public class Common
    {
        /// <summary>
        /// 保存服务器来的消息
        /// </summary>
        public static byte[] ReceiveBuffer = new byte[1024 * 1024];

        /// <summary>
        /// 监听用的socket
        /// </summary>
        public static Socket ListenSocket;

        /// <summary>
        /// 保存所有负责通信用是socket
        /// </summary>
        public static Dictionary<string, Socket> connSocket = new Dictionary<string, Socket>();
    }
}
