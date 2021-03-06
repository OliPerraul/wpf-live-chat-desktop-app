﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IFT585_TP3.Common.UdpServer
{
    public class UDPSocket
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Server(string address, int port)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            Receive();
        }

        public void Client(string address, int port)
        {
            _socket.Connect(IPAddress.Parse(address), port);
            Receive();
        }

        public void Send(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                _socket.EndSend(ar);
            }, state);
        }

        private void Receive()
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);

                var endPoint = (IPEndPoint)epFrom;
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs()
                {
                    Address = endPoint.Address.ToString(),
                    Port = endPoint.Port,
                    Message = Encoding.ASCII.GetString(so.buffer, 0, bytes),
                    EndPoint = epFrom
                });
            }, state);
        }

        public void Respond(EndPoint endPoint, string text)
        {
            SocketAsyncEventArgs saeaSend = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = endPoint
            };

            var bytesToSend = Encoding.ASCII.GetBytes(text);
            saeaSend.SetBuffer(bytesToSend, 0, bytesToSend.Length);

            try
            {
                _socket.SendToAsync(saeaSend);
            }
            catch (SocketException)
            {
                // do nothing.
            }
        }
    }
}
