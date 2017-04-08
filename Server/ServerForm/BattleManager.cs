using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerForm
{
    class BattleManager
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
                    //将玩家A移动棋子的消息转发给玩家B
                    SendMoveMessageToClient(data);
                    break;
                case 1:
                    //将玩家A申请悔棋的操作转发给玩家B
                    SendRetractMessageToClient(data);
                    break;
                case 2:
                    //将玩家B是否同意悔棋申请的操作转发给玩家A
                    SendAgreeRetractMessageToClinet(data);
                    break;
                case 3:
                    //检测玩家A的对手玩家B是否已经离线
                    SendDisconnectMessageToClient(data, ip);
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 将一方移动棋子的信息发送给另一方
        /// </summary>
        /// <param name="data"></param>
        private void SendMoveMessageToClient(byte[] data)
        {
            //[ 命令20 (2位) | ip(对方的ip 25位) | 移动的棋子id（4位）| 移动到的行数row(4位) | 移动到的列数col(4位) | ...]
            //获取接收者的ip
            string receiveIp = Encoding.UTF8.GetString(data, 2, 25).Trim('\0');

            //发送给接受者移动棋子的消息
            if (Common.connSocket.ContainsKey(receiveIp))
            {
                Common.connSocket[receiveIp].Send(data);
            }
        }

        /// <summary>
        /// 将一方悔棋的信息发送给另一方
        /// </summary>
        /// <param name="data"></param>
        private void SendRetractMessageToClient(byte[] data)
        {
            //接收的消息[ 命令21 (2位) | ip(对方的ip 25位) | ...]

            //获取接收者的ip
            string receiveIp = Encoding.UTF8.GetString(data, 2, 25).Trim('\0');

            //发送的消息[ 命令21 (2位) | ...]
            byte[] newData = new byte[2];
            newData[0] = data[0];
            newData[1] = data[1];

            //发送给接收者悔棋的消息
            if (Common.connSocket.ContainsKey(receiveIp))
            {

                Common.connSocket[receiveIp].Send(newData);
            }
        }

        /// <summary>
        /// 将一方是否同意悔棋的消息发送给另一方
        /// </summary>
        /// <param name="data"></param>
        private void SendAgreeRetractMessageToClinet(byte[] data)
        {
            //接收的消息[ 命令22 (2位) | ip(对方的ip 25位) | 是否同意悔棋(1位) | ...]

            //获取接收者的ip
            string receiveIp = Encoding.UTF8.GetString(data, 2, 25).Trim('\0');

            //发送的消息[ 命令22 (2位) | 是否同意悔棋(1位) | ...]
            byte[] newData = new byte[3];
            newData[0] = data[0];
            newData[1] = data[1];
            newData[2] = data[27];

            //发送给接收者悔棋的消息
            if (Common.connSocket.ContainsKey(receiveIp))
            {
                Common.connSocket[receiveIp].Send(newData);
            }
        }

        /// <summary>
        /// 将对手的掉线信息发送给另一方
        /// </summary>
        /// <param name="receiveIp"></param>
        void SendDisconnectMessageToClient(byte[] data, string ip)
        {
            //接收的消息[ 命令23 (2位) | ip(对方的ip 25位) | ...] 

            //获取接收者的ip
            string receiveIp = Encoding.UTF8.GetString(data, 2, 25).Trim('\0');

            //发送的消息[ 命令23 (2位) | ...] 
            byte[] newData = new byte[2];
            newData[0] = data[0];
            newData[1] = data[1];

            //如果发送方的对手已经离线，则向发送方返回消息
            if (!Common.connSocket.ContainsKey(receiveIp))
            {
                Common.connSocket[ip].Send(newData);
            }
        }

    }
}
