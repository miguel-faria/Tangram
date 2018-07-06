using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using LitJson;


namespace Tangram.Networking {
    public class RobotConnection {

        protected string _connection_ip = "";
        protected int _connection_port;
        protected Socket _bridge_connection;
        protected IPEndPoint _connection_end_point;
        protected bool _connection_established = false;
        protected JsonData _message_data = new JsonData();

        public RobotConnection(string ip = "localhost", int port = 9098) {

            _connection_ip = ip;
            _connection_port = port;
            
        }
    }
}
