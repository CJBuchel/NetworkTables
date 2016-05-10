﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net;
using System.Net.Sockets;
using NetworkTables.Streams;

namespace NetworkTables.TcpSockets
{
    internal class NtTcpClient : IDisposable
    {
        private readonly AddressFamily m_family;
        private Socket m_clientSocket;
        private NtNetworkStream m_dataStream;
        private bool m_cleanedUp = false;
        private bool m_active;

        public NtTcpClient() : this(AddressFamily.InterNetwork)
        {
            
        }

        public NtTcpClient(AddressFamily family)
        {
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Invalid TCP Family", nameof(family));
            }

            m_family = family;
            m_clientSocket = new Socket(m_family, SocketType.Stream, ProtocolType.Tcp);
        }

        internal NtTcpClient(Socket acceptedSocket)
        {
            m_clientSocket = acceptedSocket;
            m_active = true;
        }

        public bool Active
        {
            get { return m_active; }
            set { m_active = value; }
        }

        public IAsyncResult BeginConnect(IPAddress[] address, int port, AsyncCallback requestCallback, object state)
        {
            IAsyncResult result = m_clientSocket.BeginConnect(address, port, requestCallback, state);
            return result;
        }

        public void EndConnect(IAsyncResult result)
        {
            m_clientSocket.EndConnect(result);
            m_active = true;
        }

        public NtNetworkStream GetStream()
        {
            if (m_cleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (!m_clientSocket.Connected)
            {
                throw new InvalidOperationException("Not Connected");
            }
            if (m_dataStream == null)
            {
                m_dataStream = new NtNetworkStream(m_clientSocket, true);
            }

            return m_dataStream;;

        }

        public void Connect(IPAddress[] ipAddresses, int port)
        {
            m_clientSocket.Connect(ipAddresses, port);
            m_active = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_cleanedUp)
            {
                return;
            }

            if (disposing)
            {
                IDisposable dataStream = m_dataStream;
                if (dataStream != null)
                {
                    dataStream.Dispose();
                }
                else
                {
                    Socket chkClientSocket = m_clientSocket;
                    if (chkClientSocket != null)
                    {
                        try
                        {
                            chkClientSocket.Shutdown(SocketShutdown.Both);
                        }
                        finally
                        {
                            chkClientSocket.Dispose();
                            m_clientSocket = null;
                        }
                    }
                }

                GC.SuppressFinalize(this);
            }

            m_cleanedUp = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~NtTcpClient()
        {
            Dispose(false);
        }

        public bool Connected => m_clientSocket.Connected;

        public string PeerIP
        {
            get
            {
                IPEndPoint ipEp = m_clientSocket.RemoteEndPoint as IPEndPoint;
                return ipEp?.Address.ToString();
            }
        }

        public int PeerPort
        {
            get
            {
                IPEndPoint ipEp = m_clientSocket.RemoteEndPoint as IPEndPoint;
                return ipEp?.Port ?? 0;
            }
        }
        /*
        private bool m_active;
        

        private readonly AddressFamily m_family = AddressFamily.InterNetwork;

        public NtTcpClient(AddressFamily family)
        {
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Address Family MUST be InterNetwork or InterNetworkV6");
            }

            m_family = family;

            Initialize();
        }

        private void Initialize()
        {
            Client = new Socket(m_family, SocketType.Stream, ProtocolType.Tcp);
            m_active = false;
        }

        public Socket Client { get; private set; }

        public bool Connected => Client.Connected;

        public void Connect(IPAddress[] ipAddresses, int port)
        {
            Client.Connect(ipAddresses, port);
            m_active = true;
        }

        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            var result = Client.BeginConnect(addresses, port, requestCallback, state);
            return result;
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            Client.EndConnect(asyncResult);
            m_active = true;
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        private bool m_cleanedUp = false;

        protected virtual void Dispose(bool disposing)
        {
            if (m_cleanedUp)
            {
                return;
            }

            if (disposing)
            {
                Socket chkClientSocket = Client;
                if (chkClientSocket != null)
                {
                    chkClientSocket.Dispose();
                    Client = null;
                }

                GC.SuppressFinalize(this);
            }

            m_cleanedUp = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~NtTcpClient()
        {
            Dispose(false);
        }
        */
    }

}