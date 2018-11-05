using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BankSwitcher
{
    class UsbipClient
    {
        public bool client(string address, int port, string command)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                socket.Connect(ipPoint);

                byte[] data = Encoding.UTF8.GetBytes(command);
                socket.Send(data);

                data = new byte[1024];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;

                do
                {
                    bytes = socket.Receive(data, data.Length, 0);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }

                while (socket.Available > 0);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                if (builder.ToString().Equals("success") || builder.ToString().Equals("rebooting"))
                {
                    return true;                    
                }
                else
                {
                    return false;
                }
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
