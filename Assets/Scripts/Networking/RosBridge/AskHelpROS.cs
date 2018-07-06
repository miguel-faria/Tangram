using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using LitJson;

namespace Tangram.Networking.RosBridge{

	public class AskHelpROS : ROSConnection {
        
		public AskHelpROS(string address = "localhost", int port = 9098) : base(address, port) {
			
		}

		public bool topic_advertized(string topic_name){
			return _advertizing_topics [topic_name];
		}


        /*protected override void event_over(){

			if (_last_event != null && _last_event != string.Empty)
			{

				/*if (_last_event.Contains("correct_piece_placed") || 
				    ((Peer.Instance.currentState != Peer.Instance.RobotTurnState && ( _last_event.Contains("child_turn") || _last_event.Contains("explain_rotate")  
																					|| (_last_event.Contains("start") && EventManager.Instance.get_without_help()))))) {
					GameState.Instance.haveToEnableAllPieces = true;

				} else if (_last_event.Contains("greeting") || _last_event.Contains("new_game")) {
					GameState.Instance.haveToEnablePlayButton = true;

				} else if (_last_event.Contains("give_clue")) {
					Peer.Instance.nFailedTries = 0;
					Peer.Instance.nWrongAngleTries = 0;
					GameState.Instance.showHardClue = true;

				} else if (_last_event.Contains("robot_turn")) {
					GameState.Instance.robotCanPlay = true;

				} else if (_last_event.Contains("ask_help") & _last_event.Contains("rotate")) {
					GameState.Instance.enableOnePiece = true;

				}
			}
			Debug.Log ("Finished Event: " + _last_event);

		}*/

        public void ask_kid_help(string mode, int number_asking, string piece_type){

			try{
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();

				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("ask_help");

				data.WritePropertyName("help_type");
				data.Write(mode);

				data.WritePropertyName("ask_times");
				data.Write(number_asking);

				data.WritePropertyName("piece");
				data.Write(piece_type);
				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
				_last_event = "ask_help_" + mode;
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for the ask kid for help event. Exception: " + e.Message);
			}
		}

		/*public void game_goodbye(){

			try{
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();


				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("");


				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for the goodbye stage. Exception: " + e.Message);
			}
		}*/

		/*public void motivate_child(){

			try{
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();


				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("");


				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for a motivate child event. Exception: " + e.Message);
			}
		}*/

		public void new_game(string child_name, int n_games){

			try{
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();


				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("new_game");

				data.WritePropertyName("child_name");
				data.Write(child_name);

				data.WritePropertyName("n_games");
				data.Write(n_games);
				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
				_last_event = "new_game";
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for a new game event. Exception: " + e.Message);
			}
		}

        public void correct_piece_placed(string game_trigger, string aux_info, string piece) {

            try {
                StringBuilder sb = new StringBuilder();
                JsonWriter data = new JsonWriter(sb);

                _message_data = new JsonData();


                data.WriteArrayStart();
                data.WriteObjectStart();
                data.WritePropertyName("event");
                data.Write("correct_piece_placed");

                data.WritePropertyName("state");
                data.Write(game_trigger);

                data.WritePropertyName("trigger");
                data.Write(aux_info);

                data.WritePropertyName("piece");
                data.Write(piece);
                data.WriteObjectEnd();
                data.WriteArrayEnd();

                _message_data = JsonMapper.ToObject(sb.ToString());
                _last_event = "correct_piece_placed";
            } catch (Exception e) {
                Debug.LogError("Failed create JSON message for a correct piece placed event. Exception: " + e.Message);
            }
        }

        public void wrong_piece_placed(string game_trigger, string aux_info, string piece) {

            try {
                StringBuilder sb = new StringBuilder();
                JsonWriter data = new JsonWriter(sb);

                _message_data = new JsonData();


                data.WriteArrayStart();
                data.WriteObjectStart();
                data.WritePropertyName("event");
                data.Write("wrong_piece_placed");

                data.WritePropertyName("state");
                data.Write(game_trigger);

                data.WritePropertyName("trigger");
                data.Write(aux_info);

                data.WritePropertyName("piece");
                data.Write(piece);
                data.WriteObjectEnd();
                data.WriteArrayEnd();

                _message_data = JsonMapper.ToObject(sb.ToString());
                _last_event = "wrong_piece_placed";
            } catch (Exception e) {
                Debug.LogError("Failed create JSON message for a correct piece placed event. Exception: " + e.Message);
            }
        }

        public void explain(string motive, string extra_info, string piece){

			try{
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();


				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("explain");

				data.WritePropertyName("motive");
				data.Write(motive);

				data.WritePropertyName("extra_info");
				data.Write(extra_info);

				data.WritePropertyName("piece");
				data.Write(piece);
				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
				_last_event = "explain";
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for an explain event. Exception: " + e.Message);
			}

		}

		public void explain_rotate(string mode){

			try{
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();


				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("explain_rotate");

				data.WritePropertyName("rotation_mode");
				data.Write(mode);
				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
				_last_event = "explain_rotate";
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for an explain rotation event. Exception: " + e.Message);
			}
		}

		public void suggestion(string cause, string extra_info, string piece){

			try{
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();


				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("suggestion");

				data.WritePropertyName("cause");
				data.Write(cause);

				data.WritePropertyName("extra_info");
				data.Write(extra_info);

				data.WritePropertyName("piece");
				data.Write(piece);
				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
				_last_event = "suggestion";
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for suggestion event. Exception: " + e.Message);
			}

		}

		public void give_clue(float clue_seconds){

			try{
				//EventManager.Instance.set_clue_seconds(clue_seconds);
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();

				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("give_clue");

				data.WritePropertyName("clue_seconds");
				data.Write(clue_seconds);

				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
				_last_event = "give_clue";
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for a give clue event. Exception: " + e.Message);
			}
		}

		public void give_help(string mode, string piece, string child_name){

			try{
				StringBuilder sb = new StringBuilder ();
				JsonWriter data = new JsonWriter(sb);

				_message_data = new JsonData();

				data.WriteArrayStart();
				data.WriteObjectStart ();
				data.WritePropertyName ("event");
				data.Write ("give_help");

				data.WritePropertyName("mode");
				data.Write(mode);

				data.WritePropertyName("piece");
				data.Write(piece);

				data.WritePropertyName("child_name");
				data.Write(child_name);
				data.WriteObjectEnd ();
				data.WriteArrayEnd();

				_message_data = JsonMapper.ToObject(sb.ToString());
				_last_event = "give_help_" + mode;
			} catch(Exception e){
				Debug.LogError ("Failed create JSON message for a give help event. Exception: " + e.Message);
			}
		}

	}
}
