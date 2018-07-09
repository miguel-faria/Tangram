using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Tangram.GameModes {
    class HelpMode : GameMode {

        private bool _rotation = false;
        private bool _asked_help = false;
        private bool _asking_help = false;
        private bool _just_started_turn = false;
        private bool _first_asking = false;
        private bool _flash_piece = false;
        private bool _piece_moving = false;
        private bool _chose_correct = false;
        private int _n_help_asked = 0;
        private int _delay_time;
        private string _current_player = "";
        private string _current_piece_name = "";
        private Text _current_player_display;
        private System.Random _rand = new System.Random();
        private DateTime _start_asking;
        private DateTime _flashing_start;
        private DateTime _last_switch;
        private DateTime _robot_play_time;
        private DateTime _player_turn_remind;
        private Vector3 _initial_position;
        private Vector3 _final_position;

        public HelpMode(bool rotation, GameObject pieces, GameObject solution) {
            _rotation = rotation;
            _current_player_display = GameObject.Find("CurrentPlayer").GetComponent<Text>();
        }

        public void player_turn() {
            _current_player_display.text = Util_Methods.capitalize_word(_current_player);
            if (GameManager.Use_Connection) { 
                if (_just_started_turn) {
                    _player_turn_remind = DateTime.Now;
                    _just_started_turn = false;
                } else if ((DateTime.Now - _player_turn_remind).TotalSeconds > Constants.REMIND_TIMEOUT) {
                    _player_turn_remind = DateTime.Now;
                    ((Networking.RosBridge.AskHelpROS)GameManager.Robot_Connection).child_turn("play", _current_player);
                    ((Networking.RosBridge.AskHelpROS)GameManager.Robot_Connection).publish(GameManager.Instance.get_pub_topic(), "std_msgs/String");
                }
            }
        }

        public void update_turn_info(string player, string piece, int turn_time, DateTime start) {
            _current_player = player;
            _current_piece_name = piece;
            _just_started_turn = true;
            _first_asking = false;
            _n_help_asked = 0;
            _delay_time = turn_time;
        }

        public void robot_turn() {

            _current_player_display.text = "Robot";
            if (_just_started_turn && !_asked_help) {

                float ask_help_seed = UnityEngine.Random.Range(Math.Abs(PuzzleManager.Instance.get_n_placed_pieces() - PuzzleManager.Instance.get_n_remaining_pieces()), PuzzleManager.Instance.get_n_pieces());
                bool ask_help = (_rand.NextDouble() > (1 - (ask_help_seed / (float) PuzzleManager.Instance.get_n_pieces())));

                if (ask_help || PuzzleManager.Instance.get_n_remaining_pieces() < 3) { 
                    _asking_help = true;
                    _asked_help = true;
                    _first_asking = true;
                } else {
                    _asking_help = false;
                    _robot_play_time = DateTime.Now.AddSeconds(_delay_time);
                }

                _just_started_turn = false;

            } else if(_just_started_turn) {
                _asking_help = false;
                _robot_play_time = DateTime.Now.AddSeconds(_delay_time);
                _just_started_turn = false;
            }

            if (_asking_help && (_n_help_asked < (Constants.MAX_N_ASKS + 1))) {

                if (_first_asking) { 
                    _start_asking = DateTime.Now;
                    _last_switch = DateTime.MinValue;
                    _flashing_start = _start_asking;
                    _first_asking = false;
                    _flash_piece = true;

                    if (GameManager.Use_Connection) {
                        ((Networking.RosBridge.AskHelpROS)GameManager.Robot_Connection).ask_kid_help("place", _n_help_asked, _current_piece_name);
                        if (((Networking.ROSConnection)GameManager.Robot_Connection).publish(GameManager.Instance.get_pub_topic(), "std_msgs/String"))
                            _n_help_asked = 1;
                    }
                }

                if ((DateTime.Now - _start_asking).TotalSeconds > (Constants.REMIND_TIMEOUT + _n_help_asked * Constants.REMIND_TIMEOUT)) {

                    // remember player to help
                    _flash_piece = true;
                    _flashing_start = DateTime.Now;
                    _last_switch = DateTime.MinValue;
                    Debug.Log("Where should the piece go: " + _current_piece_name);

                    if (_n_help_asked < Constants.MAX_N_ASKS) {
                        if (GameManager.Use_Connection) {
                            ((Networking.RosBridge.AskHelpROS)GameManager.Robot_Connection).ask_kid_help("place", _n_help_asked, _current_piece_name);
                            if (((Networking.ROSConnection)GameManager.Robot_Connection).publish(GameManager.Instance.get_pub_topic(), "std_msgs/String"))
                                _n_help_asked++;
                        }
                    } else
                        _n_help_asked++;
                        

                }

                if (_flash_piece)
                    flash_piece();
                
                if (!_piece_moving) { 

                    if (Input.GetMouseButtonDown(0)) {
                        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                        string hit_name;
                        
                        if (hit) {
                            hit_name = hit.transform.name;
                            if (hit_name.Contains("socket")) {
                                hit_name = (new Regex("_?socket_?")).Replace(hit_name, "");
                                _initial_position = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name].transform.position;
                                _final_position = PuzzleManager.Instance.get_solution_pieces()[hit_name].transform.position;
                                _piece_moving = true;
                                PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name].GetComponent<SpriteRenderer>().enabled = true;
                                PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name].SetActive(true);
                                _flash_piece = false;
                            }
                        }
                    }

                    if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) {
                        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                        string hit_name;

                        if (hit) {
                            hit_name = hit.transform.name;
                            if (hit_name.Contains("socket")) {
                                hit_name = (new Regex("_?socket_?")).Replace(hit_name, "");
                                _initial_position = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name].transform.position;
                                _final_position = PuzzleManager.Instance.get_solution_pieces()[hit_name].transform.position;
                                _piece_moving = true;
                                PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name].GetComponent<SpriteRenderer>().enabled = true;
                                PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name].SetActive(true);
                                _flash_piece = false;
                            }
                        }
                    }

                } else {

                    GameObject piece = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name];
                    GameObject piece_solution = PuzzleManager.Instance.get_solution_pieces()[_current_piece_name];

                    piece.GetComponent<MovePiece>().piece_position(_final_position);
                    piece.GetComponent<MovePiece>().set_position_change(true);
                    piece.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

                    if (Vector3.Distance(piece.gameObject.transform.position, _final_position) < 0.2f) {

                        if (_final_position != piece_solution.transform.position) { 
                            piece.gameObject.transform.position = _initial_position;
                            if (GameManager.Use_Connection) {
                                switch (GameManager.Instance.get_connection_mode()) {
                                    case "ros":
                                        ((Networking.RosBridge.AskHelpROS)GameManager.Robot_Connection).wrong_piece_placed("ask_help", "placed", _current_piece_name);
                                        ((Networking.ROSConnection)GameManager.Robot_Connection).publish(GameManager.Instance.get_pub_topic(), "std_msgs/String");
                                        break;
                                    case "thalamus":
                                        break;
                                    default:
                                        break;
                                }
                            }
                        } else {
                            piece.GetComponent<MovePiece>().piece_position(piece_solution.transform.position);
                            piece.GetComponent<MovePiece>().set_position_change(true);
                            _asking_help = false;
                            if (GameManager.Use_Connection) {
                                switch (GameManager.Instance.get_connection_mode()) {
                                    case "ros":
                                        ((Networking.RosBridge.AskHelpROS)GameManager.Robot_Connection).correct_piece_placed("ask_help", "placed", _current_piece_name);
                                        ((Networking.ROSConnection)GameManager.Robot_Connection).publish(GameManager.Instance.get_pub_topic(), "std_msgs/String");
                                        break;
                                    case "thalamus":
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        _piece_moving = false;
                    }
                }

            } else {

                if (_n_help_asked >= Constants.MAX_N_ASKS)
                    _robot_play_time = DateTime.Now.AddSeconds(-0.5);

                if (DateTime.Now > _robot_play_time)
                    move_piece();

            }
        }

        private void flash_piece() {

            GameObject current_piece = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name];
            DateTime curr_time = DateTime.Now;

            if ((curr_time - _flashing_start).TotalSeconds < Constants.BLINKING_TIMEOUT) { 
                
                current_piece.gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

                if ((curr_time - _last_switch).TotalSeconds > Constants.BLINK_TIME) {

                    current_piece.gameObject.SetActive(!current_piece.gameObject.activeSelf);
                    current_piece.GetComponent<SpriteRenderer>().enabled = !current_piece.GetComponent<SpriteRenderer>().enabled;
                    _last_switch = curr_time;
                }


            } else {
                current_piece.GetComponent<SpriteRenderer>().enabled = true;
                current_piece.gameObject.SetActive(true);
                _flash_piece = false;
            }

        }

        private void move_piece() {
            GameObject piece = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name];
            GameObject piece_solution = PuzzleManager.Instance.get_solution_pieces()[_current_piece_name];

            piece.GetComponent<MovePiece>().piece_position(piece_solution.transform.position);
            piece.GetComponent<MovePiece>().set_position_change(true);
            piece.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        private void try_piece_place(Vector3 final_pos) {
            GameObject piece = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name];
            GameObject piece_solution = PuzzleManager.Instance.get_solution_pieces()[_current_piece_name];

            piece.GetComponent<MovePiece>().piece_position(final_pos);
            piece.GetComponent<MovePiece>().set_position_change(true);
            piece.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            if(piece.gameObject.transform.position == final_pos) {

                if (final_pos != piece_solution.transform.position)
                    piece.gameObject.transform.position = _initial_position;
                else {
                    piece.GetComponent<MovePiece>().piece_position(piece_solution.transform.position);
                    piece.GetComponent<MovePiece>().set_position_change(true);
                }


            }
        }

    }
}
