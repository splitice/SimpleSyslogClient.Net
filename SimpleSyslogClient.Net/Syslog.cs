using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleSyslogClient.Net
{
    /// <summary>
    /// Syslog Client
    /// </summary>
    public class Syslog
    {
        #region Facilities enum

        public enum Facilities
        {
            Kernel = 0,
            User = 1,
            Mail = 2,
            Daemon = 3,
            Auth = 4,
            Syslog = 5,
            Lpr = 6,
            News = 7,
            UUCP = 8,
            Cron = 15,
            Local0 = 16,
            Local1 = 17,
            Local2 = 18,
            Local3 = 19,
            Local4 = 20,
            Local5 = 21,
            Local6 = 22,
            Local7 = 23,
        }

        #endregion

        #region Levels enum

        public enum Levels
        {
            Emergency = 0,
            Alert = 1,
            Critical = 2,
            Error = 3,
            Warning = 4,
            Notice = 5,
            Information = 6,
            Debug = 7,
        }

        #endregion

        private readonly IPAddress _ipLocal;
        private readonly Socket _udpClient;
        private IPEndPoint _syslogEndPoint;

        public Syslog(IPAddress serverIp, int port)
        {
            _ipLocal = Network.GetPrimaryAddress();
            _syslogEndPoint = new IPEndPoint(serverIp, port);
            //Console.WriteLine(String.Format("Syslog initialized to send to {0}", _syslogEndPoint));
            _udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public IPEndPoint Server
        {
            get { return _syslogEndPoint; }
            set { _syslogEndPoint = value; }
        }

        public void Close()
        {
            _udpClient.Close();
        }

        public void Send(Message message)
        {
            Send(message, DateTime.Now);
        }

        public void Send(Message message, DateTime time)
        {
            int priority = message.Facility*8 + message.Level;
            string msg = String.Format("<{0}>1 {1} {2} - - - - {3}",
                                       priority,
                                       time.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK",
                                                     DateTimeFormatInfo.InvariantInfo),
                                       _ipLocal,
                                       message.Text);
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            _udpClient.SendTo(bytes, bytes.Length, SocketFlags.None, _syslogEndPoint);
        }

        #region Nested type: Message

        public class Message
        {
            public Message()
            {
            }

            public Message(int facility, int level, string text)
            {
                Facility = facility;
                Level = level;
                Text = text;
            }

            public int Facility { get; set; }
            public int Level { get; set; }
            public string Text { get; set; }
        }

        #endregion

        #region Nested type: UdpClientEx

        /// need this helper class to expose the Active propery of UdpClient
        /// (why is it protected, anyway?) 
        private class UdpClientEx : UdpClient
        {
            public UdpClientEx()
            {
            }

            public UdpClientEx(IPEndPoint ipe)
                : base(ipe)
            {
            }

            public bool IsActive
            {
                get { return Active; }
            }

            ~UdpClientEx()
            {
                if (Active) Close();
            }
        }

        #endregion
    }
}