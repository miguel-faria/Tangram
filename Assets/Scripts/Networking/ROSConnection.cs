using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using LitJson;

namespace Tangram.Networking {
    class ROSConnection : RobotConnection {

        public ROSConnection(string ip = "localhost", int port = 9098) : base(ip, port) {

            try {

                byte[] hand_shake = Encoding.UTF8.GetBytes("raw\r\n\r\n");
                IPAddress ip_addr;
                try {
                    ip_addr = IPAddress.Parse(ip);
                } catch (FormatException e) {
                    IPHostEntry host = Dns.GetHostEntry(ip);
                    ip_addr = host.AddressList[0];
                }
                _connection_end_point = new IPEndPoint(ip_addr, port);
                _bridge_connection = new Socket(ip_addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _bridge_connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _bridge_connection.Connect(_connection_end_point);
                _bridge_connection.Send(hand_shake);
                _connection_established = true;
                Debug.Log("Connection established to RosBridge at " + ip + ":" + port.ToString());

            } catch (Exception e) {
                _connection_established = false;
                Debug.LogError("Failed to establish connection to RosBridge at " + ip + ":" + port.ToString() + " with exception: " + e.Message);
            }

        }

    }
}
