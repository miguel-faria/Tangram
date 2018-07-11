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
    public class ROSConnection : RobotConnection {

        protected Dictionary<string, bool> _advertizing_topics = new Dictionary<string, bool>();
        protected Dictionary<string, bool> _subscribing_topics = new Dictionary<string, bool>();
        protected Dictionary<string, string> _pub_topics_type = new Dictionary<string, string>();
        protected Dictionary<string, string> _sub_topics_type = new Dictionary<string, string>();
        private string _triggering_event = "";

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

        public bool advertize(string pub_topic, string type) {

            try {
                Debug.Log("Alerting ROS about starting advertizing in topic " + pub_topic);
                //Begin construction of the JSON to send over to ROS
                StringBuilder sb = new StringBuilder();
                JsonWriter j_writer = new JsonWriter(sb);
                j_writer.WriteArrayStart();

                // "op" field
                j_writer.WriteObjectStart();
                j_writer.WritePropertyName("op");
                j_writer.Write("advertise");
                // "topic" field
                j_writer.WritePropertyName("topic");
                j_writer.Write(pub_topic);
                // "type" field
                j_writer.WritePropertyName("type");
                j_writer.Write(type);
                j_writer.WriteObjectEnd();

                j_writer.WriteArrayEnd();

                //send message to ROS
                Debug.Log(sb.ToString());
                _bridge_connection.Send(new byte[] { 0 });
                _bridge_connection.Send(Encoding.UTF8.GetBytes(sb.ToString()));
                _bridge_connection.Send(new byte[] { 255 });

                if (!_advertizing_topics.ContainsKey(pub_topic))
                    _advertizing_topics.Add(pub_topic, true);
                else
                    _advertizing_topics[pub_topic] = true;

                if (!_pub_topics_type.ContainsKey(pub_topic))
                    _pub_topics_type.Add(pub_topic, type);

                //System.Threading.Thread.Sleep(1500);
                Debug.Log("Message alerting starting advertizing in topic " + pub_topic + " sent to ROS");
                return true;

            } catch (Exception e) {
                Debug.LogError("Caught an exception while trying to advertize publishing in topic " + pub_topic + " through ROS Bridge: " + e.Message);
                if (!_advertizing_topics.ContainsKey(pub_topic))
                    _advertizing_topics.Add(pub_topic, false);
                return false;
            }
        }

        public bool publish(string pub_topic, string pub_type) {
            try {

                GameManager.Instance.set_can_play(false);
                //If there's no message data or no connection publish fails
                if (_message_data == null)
                    throw new ArgumentNullException();

                if (!_connection_established)
                    throw new NoConnectionEstablished();

                while (!_advertizing_topics.ContainsKey(pub_topic) ||
                   (_advertizing_topics.ContainsKey(pub_topic) && !_advertizing_topics[pub_topic]))
                    advertize(pub_topic, pub_type);

                //Begin construction of the JSON to send over to ROS
                StringBuilder sb = new StringBuilder();
                JsonWriter j_writer = new JsonWriter(sb);
                j_writer.WriteArrayStart();

                // "op" field
                j_writer.WriteObjectStart();
                j_writer.WritePropertyName("op");
                j_writer.Write("publish");
                // "topic" field
                j_writer.WritePropertyName("topic");
                j_writer.Write(pub_topic);
                // "type" field
                j_writer.WritePropertyName("type");
                j_writer.Write(pub_type);
                // "msg" field
                j_writer.WritePropertyName("msg");
                j_writer.WriteObjectStart();
                j_writer.WritePropertyName("data");
                j_writer.Write(_message_data.ToJson());
                j_writer.WriteObjectEnd();
                j_writer.WriteObjectEnd();

                j_writer.WriteArrayEnd();

                //send message to ROS
                Debug.Log(sb.ToString());
                _bridge_connection.Send(new byte[] { 0 });
                _bridge_connection.Send(Encoding.UTF8.GetBytes(sb.ToString()));
                _bridge_connection.Send(new byte[] { 255 });

                Debug.Log("Sent Message to ROS topic " + pub_topic);

                /*if (_last_event == "game_ready")
                    event_over();*/

                return true;

            } catch (Exception e) {
                Debug.LogError("Caught an exception while trying to publish a message through ROS Bridge to topic " + pub_topic + ": " + e.Message);
                GameManager.Instance.set_can_play(true);
                return false;
            }

        }

        public void unadvertize(string pub_topic, string pub_type) {

            if (!_advertizing_topics.ContainsKey(pub_topic) ||
                (_advertizing_topics.ContainsKey(pub_topic) && !_advertizing_topics[pub_topic])) {
                Debug.Log("Topic " + pub_topic + " not being advertized");
                return;
            }

            try {

                Debug.Log("Started unadvertizing process for topic " + pub_topic);

                StringBuilder sb = new StringBuilder();
                JsonWriter j_writer = new JsonWriter(sb);
                j_writer.WriteArrayStart();

                // "op" field
                j_writer.WriteObjectStart();
                j_writer.WritePropertyName("op");
                j_writer.Write("unadvertise");
                // "topic" field
                j_writer.WritePropertyName("topic");
                j_writer.Write(pub_topic);
                j_writer.WritePropertyName("type");
                j_writer.Write(pub_type);
                j_writer.WriteObjectEnd();

                j_writer.WriteArrayEnd();

                //send message to ROS
                Debug.Log(sb.ToString());
                _bridge_connection.Send(new byte[] { 0 });
                _bridge_connection.Send(Encoding.UTF8.GetBytes(sb.ToString()));
                _bridge_connection.Send(new byte[] { 255 });

                _advertizing_topics[pub_topic] = false;

            } catch (Exception e) {
                Debug.LogError("Caught an exeception while unadvertizing publishing in topic " + pub_topic + ": " + e.Message);
                _advertizing_topics[pub_topic] = true;
            }

        }

        public bool subscribe(string sub_topic, string sub_type) {
            try {
                Debug.Log("Alerting ROS about starting subscribing to topic " + sub_topic);
                StringBuilder sb = new StringBuilder();
                JsonWriter j_writer = new JsonWriter(sb);
                j_writer.WriteArrayStart();

                //Begin construction of the JSON to send over to ROS
                j_writer.WriteObjectStart();

                // "op" field
                j_writer.WritePropertyName("op");
                j_writer.Write("subscribe");
                // "topic" field
                j_writer.WritePropertyName("topic");
                j_writer.Write(sub_topic);
                // "type" field
                j_writer.WritePropertyName("type");
                j_writer.Write(sub_type);
                j_writer.WriteObjectEnd();

                //send message to ROS
                Debug.Log(sb.ToString());
                _bridge_connection.Send(new byte[] { 0 });
                _bridge_connection.Send(Encoding.UTF8.GetBytes(sb.ToString()));
                _bridge_connection.Send(new byte[] { 255 });

                if (!_subscribing_topics.ContainsKey(sub_topic))
                    _subscribing_topics.Add(sub_topic, true);
                else
                    _subscribing_topics[sub_topic] = true;

                if (!_sub_topics_type.ContainsKey(sub_topic))
                    _sub_topics_type.Add(sub_topic, sub_type);

                //System.Threading.Thread.Sleep(1500);
                Debug.Log("Message alerting starting subscribing to topic " + sub_topic + " sent to ROS");
                Receive(_bridge_connection);
                return true;

            } catch (Exception e) {
                Debug.LogError("Caught an exception while trying to subscribing in topic " + sub_topic + " through ROS Bridge: " + e.Message);
                if (!_advertizing_topics.ContainsKey(sub_topic))
                    _advertizing_topics.Add(sub_topic, false);
                return false;
            }
        }

        public void unsubscribe(string sub_topic) {

            if (!_subscribing_topics.ContainsKey(sub_topic) ||
                (_subscribing_topics.ContainsKey(sub_topic) && !_subscribing_topics[sub_topic])) {
                Debug.Log("Topic " + sub_topic + " not being advertized");
                return;
            }

            try {

                Debug.Log("Started unadvertizing process for topic " + sub_topic);

                StringBuilder sb = new StringBuilder();
                JsonWriter j_writer = new JsonWriter(sb);
                j_writer.WriteArrayStart();

                // "op" field
                j_writer.WriteObjectStart();
                j_writer.WritePropertyName("op");
                j_writer.Write("unsubscribe");
                // "topic" field
                j_writer.WritePropertyName("topic");
                j_writer.Write(sub_topic);
                j_writer.WriteObjectEnd();

                j_writer.WriteArrayEnd();

                //send message to ROS
                Debug.Log(sb.ToString());
                _bridge_connection.Send(new byte[] { 0 });
                _bridge_connection.Send(Encoding.UTF8.GetBytes(sb.ToString()));
                _bridge_connection.Send(new byte[] { 255 });

                _subscribing_topics[sub_topic] = false;

            } catch (Exception e) {
                Debug.LogError("Caught an exeception while unadvertizing publishing in topic " + sub_topic + ": " + e.Message);
                _subscribing_topics[sub_topic] = true;
            }

        }

        void Receive(Socket listener) {
            byte[] buffer = new byte[1024];
            object[] obj = new object[2];
            obj[0] = buffer;
            obj[1] = listener;
            EndPoint senderRemote = (EndPoint)_connection_end_point;
            _bridge_connection.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref senderRemote, new AsyncCallback(Process_Receive), obj);
        }

        private void Process_Receive(IAsyncResult ar) {
            Debug.Log("Received Message from ROS Bridge!");
            string data_received = string.Empty;
            JsonData content = new JsonData();
            JsonData event_data = new JsonData();
            JsonReader data_reader;
            string trig_event = string.Empty;

            try {

                object[] obj = new object[2];
                obj = (object[])ar.AsyncState;

                byte[] buffer = (byte[])obj[0];
                Socket handler = (Socket)obj[1];
                int bytes_received = handler.EndReceive(ar);

                //Process only if received more than 0 bytes
                if (bytes_received > 0) {
                    data_received += Encoding.UTF8.GetString(buffer, 0, bytes_received);
                    Debug.Log("Received " + bytes_received + " bytes: " + data_received);

                    //If bytes are not an empty message
                    if (data_received.Length > 1) {
                        content = JsonMapper.ToObject<JsonData>(data_received);
                        if (content.Keys.Contains("msg") && content["msg"].Keys.Contains("data")) {
                            data_reader = new JsonReader(content["msg"]["data"].ToString());
                            event_data = JsonMapper.ToObject<JsonData>(data_reader)[0];

                            trig_event = event_data["event"].ToString();
                            _triggering_event = event_data["trigger_event"].ToString();

                            Debug.Log(trig_event + "\t" + _triggering_event);

                            if (trig_event.Contains("animation") && trig_event.Contains("over")) {
                                event_over();
                            } else {
                                Debug.Log("Event not recognized: " + trig_event);
                            }

                        } else {
                            Debug.Log("Received messsage without data");
                        }

                    } else {
                        Debug.Log("Received empty message.");
                    }
                }

            } catch (Exception e) {
                Debug.LogError("Error Processing Received Message: " + e.ToString());
            }

            Receive(_bridge_connection);
        }

        public void close_connection() {
            try {
                Debug.Log("Started shutting down of ROS Bridge connection");
                Debug.Log("Unadvertizing topics");
                List<string> keys = new List<string>(_advertizing_topics.Keys);
                foreach (string key in keys) {
                    while (_advertizing_topics[key]) {
                        Debug.Log("Unadvertizing topic " + key);
                        unadvertize(key, _pub_topics_type[key]);
                    }
                }
                _advertizing_topics.Clear();
                _pub_topics_type.Clear();
                Debug.Log("All topics unadvertized");
                Debug.Log("Unsubscribing topics");
                keys = new List<string>(_subscribing_topics.Keys);
                foreach (string key in keys) {
                    while (_subscribing_topics[key]) {
                        Debug.Log("Unadvertizing topic " + key);
                        unsubscribe(key);
                    }
                }
                _subscribing_topics.Clear();
                _sub_topics_type.Clear();
                Debug.Log("Disconnecting from ROS Bridge");
                _bridge_connection.Shutdown(SocketShutdown.Both);
                //while (_bridge_connection.Connected)
                _bridge_connection.Disconnect(false);
                //_bridge_connection.Close();
                Debug.Log("Disconnected from ROS");
            } catch (Exception e) {
                Debug.LogError("Caught an exception while closing ROS Bridge connection: " + e.Message);
            }
        }

        protected virtual void event_over() {
            GameManager.Instance.set_can_play(true);
            if (_triggering_event.Contains("game") && _triggering_event.Contains("begun"))
                GameManager.Instance.set_game_started(true);
            else if (_triggering_event.Contains("game") && _triggering_event.Contains("ready"))
                GameManager.Instance.set_response_game_started(true);
        }
        
        public void game_ready(string child_name, int game_number) {
            try {
                StringBuilder sb = new StringBuilder();
                JsonWriter data = new JsonWriter(sb);

                _message_data = new JsonData();

                data.WriteArrayStart();
                data.WriteObjectStart();
                data.WritePropertyName("event");
                data.Write("game_ready");

                data.WritePropertyName("child_name");
                data.Write(child_name);

                data.WritePropertyName("game_number");
                data.Write(game_number);
                data.WriteObjectEnd();
                data.WriteArrayEnd();

                _message_data = JsonMapper.ToObject(sb.ToString());
                _last_event = "game_ready";
            } catch (Exception e) {
                Debug.LogError("Failed create JSON message for the game started stage. Exception: " + e.Message);
            }
        }

        public void game_started(string puzzle, bool no_help) {

            try {
                StringBuilder sb = new StringBuilder();
                JsonWriter data = new JsonWriter(sb);

                _message_data = new JsonData();

                data.WriteArrayStart();
                data.WriteObjectStart();
                data.WritePropertyName("event");
                data.Write("game_begun");

                data.WritePropertyName("puzzle");
                data.Write(puzzle);
                data.WriteObjectEnd();
                data.WriteArrayEnd();

                _message_data = JsonMapper.ToObject(sb.ToString());
                //EventManager.Instance.set_without_help(no_help);
                _last_event = "game_started";
            } catch (Exception e) {
                Debug.LogError("Failed create JSON message for the game started stage. Exception: " + e.Message);
            }
        }

        public void game_finished(string puzzle) {

            try {
                StringBuilder sb = new StringBuilder();
                JsonWriter data = new JsonWriter(sb);

                _message_data = new JsonData();

                data.WriteArrayStart();
                data.WriteObjectStart();
                data.WritePropertyName("event");
                data.Write("game_finished");

                data.WritePropertyName("puzzle");
                data.Write(puzzle);
                data.WriteObjectEnd();
                data.WriteArrayEnd();

                _message_data = JsonMapper.ToObject(sb.ToString());
                _last_event = "game_finished";
            } catch (Exception e) {
                Debug.LogError("Failed create JSON message for the game finished stage. Exception: " + e.Message);
            }
        }

        public void robot_turn(string turn_event, string child_name, int n_warnings) {

            try {
                StringBuilder sb = new StringBuilder();
                JsonWriter data = new JsonWriter(sb);

                _message_data = new JsonData();


                data.WriteArrayStart();
                data.WriteObjectStart();
                data.WritePropertyName("event");
                data.Write("robot_turn");

                data.WritePropertyName("turn_event");
                data.Write(turn_event);

                data.WritePropertyName("child_name");
                data.Write(child_name);

                data.WritePropertyName("warnings");
                data.Write(n_warnings);
                data.WriteObjectEnd();
                data.WriteArrayEnd();

                _message_data = JsonMapper.ToObject(sb.ToString());
                _last_event = "robot_turn";
            } catch (Exception e) {
                Debug.LogError("Failed create JSON message for the robot turn event. Exception: " + e.Message);
            }
        }

        public void child_turn(string game_event, string child_name) {

            try {
                StringBuilder sb = new StringBuilder();
                JsonWriter data = new JsonWriter(sb);

                _message_data = new JsonData();


                data.WriteArrayStart();
                data.WriteObjectStart();
                data.WritePropertyName("event");
                data.Write("child_turn");

                data.WritePropertyName("game_event");
                data.Write(game_event);

                data.WritePropertyName("child_name");
                data.Write(child_name);
                data.WriteObjectEnd();
                data.WriteArrayEnd();

                _message_data = JsonMapper.ToObject(sb.ToString());
                _last_event = "child_turn";
            } catch (Exception e) {
                Debug.LogError("Failed create JSON message for the child turn event. Exception: " + e.Message);
            }
        }

        public void game_feedback(string type, string child_name, string game_info) {

            try {
                StringBuilder sb = new StringBuilder();
                JsonWriter data = new JsonWriter(sb);

                _message_data = new JsonData();


                data.WriteArrayStart();
                data.WriteObjectStart();
                data.WritePropertyName("event");
                data.Write("game_feedback");

                data.WritePropertyName("feedback_type");
                data.Write(type);

                data.WritePropertyName("child_name");
                data.Write(child_name);

                data.WritePropertyName("game_info");
                data.Write(game_info);
                data.WriteObjectEnd();
                data.WriteArrayEnd();

                _message_data = JsonMapper.ToObject(sb.ToString());
                _last_event = "game_feedback";
            } catch (Exception e) {
                Debug.LogError("Failed create JSON message for the game feedback event. Exception: " + e.Message);
            }
        }
        
    }

}
