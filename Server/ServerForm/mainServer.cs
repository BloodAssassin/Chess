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

                    switch (command)
                    {
                        case 0:
                            //服务器接收到来自客户端的关于账户系统方面的请求
                            AccountManager AM = new AccountManager();
                            AM.ReceiveMessage(data, ip);
                            break;
                        case 1:
                            //服务器保存发送方的IP、邮箱和其棋子的颜色，并寻求配对
                            MatchManager MM = new MatchManager();
                            MM.ReceiveMessage(data, ip);
                            break;
                        case 2:
                            //接收发送方发来的移动棋子的消息，并转发给接收方
                            BattleManager BM = new BattleManager();
                            BM.ReceiveMessage(data, ip);
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
