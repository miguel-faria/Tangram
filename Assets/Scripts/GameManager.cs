using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tangram{

	public class GameManager : MonoBehaviour {

		private string _parameters_text_file = "params_file.txt";
        private string _current_piece_name = "";
        private string _current_player = "";
        private int _n_turns = 0;
        private bool _change_turn = false;
        private bool _first_turn = false;
        private bool _puzzle_finished = false;
		private bool _unlock_puzzle_pieces = false;
		private bool _game_ready = false;
        private bool _started_logging = false;
        private bool _touched_piece_solution = false;
        private bool _can_play = true;
        private bool _game_started = false;
        private bool _response_game_started = false;
        private DateTime _puzzle_over;
        private DateTime _ask_start;
        private DateTime _game_begin;
        private DateTime _last_sent_begun = DateTime.MinValue;
        private List<string> _playing_order = new List<string>();
        private GameObject _touched_place;

        private static System.Random _rand_generator;
        //private static PuzzleLogger _logger = null;
        private static bool _send_message = false;
        private static bool _use_connection = false;
        private static GameManager _instance = null;
        private static GameModes.GameMode _game_mode = null;
        private static Networking.RobotConnection _robot_connection = null;
        public static GameManager Instance { get { return _instance; } }
        public static bool Use_Connection { get { return _use_connection; } }
        public static Networking.RobotConnection Robot_Connection { get { return _robot_connection; } }
        //public static GameModes.GameMode PlayMode { get { return _} }

        //Basic Game Settings
        private string _connection_mode = "ros";
        private string _ros_pub_topic = "/inside_tangram/tangram_connection_sub";
        private string _ros_sub_topic = "/inside_tangram/tangram_connection_pub";

		private static int _difficulty_level = (int)Difficulty_Levels.EASY;
        private static string _puzzle = "square";
        private static string _play_mode = "help";
        private static string _connection_ip = "localhost";
        private static bool _rotation = false;
        private static bool _robot = true;
        private static int _n_players = 1;
        private static int _connection_port = 9090;
        private static int _game_nr = 1;
        private static List<string> _player_names = new List<string>(new string[] { "Miguel" });

		// Use this for initialization
		void Start () {

			if (_instance == null){
				DontDestroyOnLoad (gameObject);
				_instance = this;

				// Initialize IPs and ports (Android vs Linux/Windows/Mac vs Editor)
				if(!Application.isEditor){
					string os = SystemInfo.operatingSystem;
					if(os.ToLower().Contains("android")){
						initialize_android ();
					} else if(os.ToLower().Contains("windows") || os.ToLower().Contains("mac") || os.ToLower().Contains("linux")){
						initialize_standalone ();
					}
				}

                _rand_generator = new System.Random();
                
                if (_use_connection && (_robot_connection == null)) {
                    _ask_start = DateTime.MinValue;
                    switch (_play_mode) {
                        case "regular":
                            if (_connection_mode.Contains("ros")) {
                                //TODO
                            } else if (_connection_mode.Contains("thalamus")) {
                                //TODO
                            }
                            break;
                        case "help":
                            if (_connection_mode.Contains("ros")) {
                                _robot_connection = new Networking.RosBridge.AskHelpROS(_connection_ip, _connection_port);
                                //((Networking.RosBridge.AskHelpROS)_robot_connection).advertize(_ros_pub_topic, "std_msgs/String");
                                ((Networking.RosBridge.AskHelpROS)_robot_connection).subscribe(_ros_sub_topic, "std_msgs/String");
                                _game_begin = DateTime.Now;
                            } else if (_connection_mode.Contains("thalamus")) {
                                //TODO
                            }
                            break;
                        default:
                            break;
                    }
                }

            } else if (_instance != this){
                gameObject.SetActive (false);
				return;
			}

		}

        void OnEnable() {

            SceneManager.sceneLoaded += OnSceneLoaded;

        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if(_game_mode == null) { 
                if(scene.name.Contains("level")) {
                    switch (_play_mode) {
                        case "regular":
                            _game_mode = new GameModes.RegularMode(_rotation, GameObject.Find("Pieces"), GameObject.Find("Solution"));
                            _game_ready = true;
                            _playing_order = new List<string>();
                            if(_n_players == 0 && _robot) {
                                _playing_order.Add("robot");
                            } else  if (_n_players > 1) {
                                List<string> names = new List<string>(_player_names);
                                int idx;
                                if (_robot)
                                    names.Add("robot");

                                while (names.Count > 0) {
                                    idx = _rand_generator.Next(0, names.Count - 1);
                                    _playing_order.Add(names[idx]);
                                    names.RemoveAt(idx);
                                }
                            } else {
                                if (_robot) {
                                    if (_rand_generator.Next(0, 1) > 0) {
                                        _playing_order.Add("robot");
                                        _playing_order.Add(_player_names[0]);
                                    }
                                    else {
                                        _playing_order.Add(_player_names[0]);
                                        _playing_order.Add("robot");
                                    }
                                }
                                else
                                    _playing_order.Add(_player_names[0]);
                            }

                            _first_turn = true;
                            break;
                        case "help":
                            _game_mode = new GameModes.HelpMode(_rotation, GameObject.Find("Pieces"), GameObject.Find("Solution"));
                            _game_ready = true;
                            _playing_order = new List<string>();
                            if (_n_players == 0 && _robot) {
                                _playing_order.Add("robot");
                            } else if (_n_players > 1) {
                                List<string> names = new List<string>(_player_names);
                                int idx;
                                if (_robot)
                                    names.Add("robot");

                                while (names.Count > 0) {
                                    idx = _rand_generator.Next(0, names.Count - 1);
                                    _playing_order.Add(names[idx]);
                                    names.RemoveAt(idx);
                                }
                            } else {
                                if (_robot) {
                                    if (_rand_generator.Next(0, 1) > 0) {
                                        _playing_order.Add("robot");
                                        _playing_order.Add(_player_names[0]);
                                    } else {
                                        _playing_order.Add(_player_names[0]);
                                        _playing_order.Add("robot");
                                    }
                                } else
                                    _playing_order.Add(_player_names[0]);
                            }

                            _first_turn = true;
                            break;

                        default:
                            Debug.Log("Invalid Game Type");
                            break;
                    }

                    if (_use_connection) {
                        switch (_connection_mode) {
                                case "ros":
                                    if (!(_robot_connection.get_last_event().Contains("game") && _robot_connection.get_last_event().Contains("begun"))) {
                                        _last_sent_begun = DateTime.Now;
                                        ((Networking.ROSConnection)_robot_connection).game_started(_puzzle, false);
                                        _send_message = true;
                                    }
                                    break;
                                case "thalamus":
                                    //TODO
                                    break;
                                default:
                                    break;
                        }
                    }
                }
            }
        }


        /*void Awake(){
            if (SceneManager.GetActiveScene().name.Contains("level")) {
                switch (_play_mode) {
                    case "regular":
                        _game_mode = new GameModes.RegularMode(_rotation, GameObject.Find("Pieces"), GameObject.Find("Solution"));
                        _game_started = true;
                        _playing_order = new List<string>();
                        if (_n_players > 1) {
                            List<string> names = new List<string>(_player_names);
                            int idx;
                            if (_robot)
                                names.Add("robot");

                            while (names.Count > 0) {
                                idx = rand_generator.Next(0, names.Count - 1);
                                _playing_order.Add(names[idx]);
                                names.RemoveAt(idx);
                            }
                        } else {
                            if (_robot) {
                                if (rand_generator.Next(0, 1) > 0) {
                                    _playing_order.Add("robot");
                                    _playing_order.Add(_player_names[0]);
                                } else {
                                    _playing_order.Add(_player_names[0]);
                                    _playing_order.Add("robot");
                                }
                            } else
                                _playing_order.Add(_player_names[0]);
                        }

                        _game_started = true;
                        break;
                }
            }
		}*/

		// Update is called once per frame
		void Update () {
            
            if (_send_message) {
                switch (_connection_mode) {
                    case "ros":
                        ((Networking.ROSConnection)_robot_connection).publish(_ros_pub_topic, "std_msgs/String");
                        break;
                    case "thalamus":
                        break;
                    default:
                        break;
                }
                _send_message = false;

            } else { 
                if (_game_ready && _game_started && !_puzzle_finished && (PuzzleManager.Instance.get_remaining_pieces().Count > 0)) {

                    if (can_play()) {

                        if (_first_turn || _change_turn) {
                            if (_change_turn) {
                                _n_turns++;
                                _change_turn = false;
                                _current_player = _playing_order[_n_turns % _playing_order.Count];
                            } else {
                                _current_player = _playing_order[0];
                                _first_turn = false;
                            }

                            if (_current_player.Contains("robot")) {
                                lock_pieces();
                                _current_piece_name = new List<string>(PuzzleManager.Instance.get_remaining_pieces().Keys)[_rand_generator.Next(0, PuzzleManager.Instance.get_remaining_pieces().Count)];
                                if (_difficulty_level == (int)Difficulty_Levels.HARD)
                                    _game_mode.update_turn_info(_current_player, _current_piece_name, UnityEngine.Random.Range(15, 20), DateTime.Now);
                                else
                                    _game_mode.update_turn_info(_current_player, _current_piece_name, UnityEngine.Random.Range(8, 10), DateTime.Now);
                                if (_use_connection)
                                    robot_turn_warning();
                            } else {
                                _current_piece_name = "";
                                _game_mode.update_turn_info(_current_player, _current_piece_name, 0, DateTime.Now);
                                if (_use_connection) {
                                    player_turn_warning();
                                    unlock_pieces();
                                } else {
                                    unlock_pieces();
                                }
                            }

                        }

                        if (_current_player.Contains("robot"))
                            _game_mode.robot_turn();
                        else
                            _game_mode.player_turn();
                    }
                } else if (_puzzle_finished && ((DateTime.Now - _puzzle_over).TotalSeconds > 1.0f)) {
                    quit_game();
                } else if(_game_ready && !_response_game_started && !_game_started && (DateTime.Now - _last_sent_begun).TotalSeconds > 10.0f) {
                    switch (_connection_mode) {
                        case "ros":
                            ((Networking.ROSConnection)_robot_connection).game_started(_puzzle, false);
                            ((Networking.ROSConnection)_robot_connection).publish(_ros_pub_topic, "std_msgs/String");
                            _last_sent_begun = DateTime.Now;
                            break;
                        case "thalamus":
                            //TODO
                            break;
                        default:
                            break;
                    }
                } else if (!_game_ready && !_game_started && _use_connection && (DateTime.Now - _ask_start).TotalSeconds > 20.0f) {
                    ask_start_game();
                    _ask_start = DateTime.Now;
                }
            }

        }

        private bool initialize_android(){

			try{
				string line;
				string[] words;

				StreamReader file_reader = new StreamReader(_parameters_text_file, Encoding.Default);

				using(file_reader){

					do{
						line = file_reader.ReadLine();
						if(line != null){
							words = line.Split(new[] {':', ',', '-', '=', ' '}, StringSplitOptions.RemoveEmptyEntries);
                            string arg = words[0];
                            string arg_value = words[1];
                            bool correct_parse = false;

                            switch (arg){

                                case "difficulty":
                                    if (arg_value.GetType().Equals(typeof(string))) {
                                        _difficulty_level = Util_Methods.difficulty_name_to_val(arg_value);
                                    } else if (arg_value.GetType().Equals(typeof(int))) {
                                        correct_parse = Int32.TryParse(arg_value, out _difficulty_level);
                                        if (!correct_parse) {
                                            Debug.LogError("Could not parse value for game level, defaulting to easy level.");
                                            _difficulty_level = (int)Difficulty_Levels.EASY;
                                        }
                                    }
                                    break;

                                case "puzzle":
                                    if (arg_value.GetType().Equals(typeof(string))) {
                                        if (arg_value.ToLower() == "tangram") {
                                            _puzzle = "square";
                                        } else if (arg_value == "") {
                                            System.Random r = new System.Random();
                                            Array vals = Enum.GetValues(typeof(Level_Names));
                                            _puzzle = Util_Methods.level_val_to_name(r.Next((int)vals.GetValue(0), (int)vals.GetValue(vals.Length - 1)));
                                        } else
                                            _puzzle = arg_value.ToLower();
                                    } else if (arg_value.GetType().Equals(typeof(int))) {
                                        int level;
                                        correct_parse = Int32.TryParse(arg_value, out level);
                                        if (!correct_parse) {
                                            System.Random r = new System.Random();
                                            Array vals = Enum.GetValues(typeof(Level_Names));
                                            _puzzle = Util_Methods.level_val_to_name(r.Next((int)vals.GetValue(0), (int)vals.GetValue(vals.Length - 1)));
                                            Debug.LogError("Could not parse value for puzzle, randomizing used puzzle. Puzzle set to: " + _puzzle);
                                        } else
                                            _puzzle = Util_Methods.level_val_to_name(level);
                                    }
                                    break;

                                case "robot":
                                    if (arg_value.ToLower().Contains("true"))
                                        _robot = true;
                                    else
                                        _robot = false;
                                    break;

                                case "game_mode":
                                    _play_mode = arg_value.ToLower();
                                    break;

                                case "rotation":
                                    if (arg_value.ToLower().Contains("true"))
                                        _rotation = true;
                                    else
                                        _rotation = false;
                                    break;

                                case "player_number":
                                    correct_parse = Int32.TryParse(arg_value, out _n_players);
                                    if (!correct_parse) {
                                        _n_players = 1;
                                        _player_names.Add(words[2].ToLower());
                                        Debug.LogError("Invalid number of players, considering only 1 player.");
                                    } else {
                                        for (int j = 0; j < _n_players; j++) {
                                            _player_names.Add(words[2 + j]);
                                        }
                                    }
                                    break;

                                case "game_nr":
                                    correct_parse = Int32.TryParse(arg_value, out _game_nr);
                                    if (!correct_parse) {
                                        Debug.LogError("Invalid parsing of game number, defaulting to game 1.");
                                        _game_nr = 1;
                                    }
                                    break;

                                case "use_connection":
                                    if (arg_value.ToLower().Contains("true"))
                                        _use_connection = true;
                                    else
                                        _use_connection = false;
                                    break;

                                case "connection_mode":
                                    _connection_mode = arg_value;
                                    break;

                                case "connection_ip":
                                    _connection_ip = arg_value;
                                    break;

                                case "connection_port":
                                    correct_parse = Int32.TryParse(arg_value, out _connection_port);
                                    if (!correct_parse) {
                                        _connection_port = 9090;
                                        Debug.Log("Invalid or impossible to parse port value, setting to default value.");
                                    }
                                    break;

                                case "ros_topic_pub":
                                    _ros_pub_topic = arg_value;
                                    break;

                                case "ros_topic_sub":
                                    _ros_sub_topic = arg_value;
                                    break;

                                default:
                                    Debug.Log("Unrecognized input argument: " + arg);
                                    break;

                            }

						}

					} while(line != null);

				}

				file_reader.Close();
				return true;

			} catch (Exception e){
				Debug.LogError ("Error reading setup parameters from file " + _parameters_text_file + ": " + e.ToString ());
				return false;
			}

		}

		private void initialize_standalone(){

			try{
				var command_args = System.Environment.GetCommandLineArgs ();
				string arg, arg_value;

				for(int i = 1; i < command_args.Length; i += 2){
					arg = command_args [i];
					arg_value = command_args [i + 1];
                    bool correct_parse = false;
					switch(arg){

						case "-difficulty":
                            if (arg_value.GetType().Equals(typeof(string))) {
                                _difficulty_level = Util_Methods.difficulty_name_to_val(arg_value);
                            } else if(arg_value.GetType().Equals(typeof(int))) {
                                correct_parse = Int32.TryParse(arg_value, out _difficulty_level);
                                if (!correct_parse) {
                                    Debug.LogError("Could not parse value for game level, defaulting to easy level.");
                                    _difficulty_level = (int)Difficulty_Levels.EASY;
                                }
                            }
						    break;

						case "-puzzle":
                            if (arg_value.GetType().Equals(typeof(string))) {
                                if (arg_value.ToLower() == "tangram") { 
                                    _puzzle = "square";
                                } else if (arg_value == "") {
                                    System.Random r = new System.Random();
                                    Array vals = Enum.GetValues(typeof(Level_Names));
                                    _puzzle = Util_Methods.level_val_to_name(r.Next((int)vals.GetValue(0), (int)vals.GetValue(vals.Length - 1)));
                                } else
                                    _puzzle = arg_value.ToLower();
                            }
                            else if (arg_value.GetType().Equals(typeof(int))) {
                                int level;
                                correct_parse = Int32.TryParse(arg_value, out level);
                                if (!correct_parse) {
                                    System.Random r = new System.Random();
                                    Array vals = Enum.GetValues(typeof(Level_Names));
                                    _puzzle = Util_Methods.level_val_to_name(r.Next((int)vals.GetValue(0), (int)vals.GetValue(vals.Length - 1)));
                                    Debug.LogError("Could not parse value for puzzle, randomizing used puzzle. Puzzle set to: " + _puzzle);
                                }
                                else
                                    _puzzle = Util_Methods.level_val_to_name(level);
                            }
                            break;

                        case "-robot":
                            if (arg_value.ToLower().Contains("true"))
                                _robot = true;
                            else
                                _robot = false;
                            break;
                        
                        case "-game_mode":
                            _play_mode = arg_value.ToLower();
                            break;

                        case "-rotation":
                            if (arg_value.ToLower().Contains("true"))
                                _rotation = true;
                            else
                                _rotation = false;
                            break;

						case "-player_number":
                            correct_parse = Int32.TryParse(arg_value, out _n_players);
                            if (!correct_parse) {
                                _n_players = 1;
                                _player_names.Add(command_args[i + 2].ToLower());
                                Debug.LogError("Invalid number of players, considering only 1 player.");
                            }else {
                                for(int j = 0; j < _n_players; j++) {
                                    _player_names.Add(command_args[i + 2 + j]);
                                }
                            }
                            i += _n_players;
                            break;

                        case "-game_nr":
                            correct_parse = Int32.TryParse(arg_value, out _game_nr);
                            if (!correct_parse) {
                                Debug.LogError("Invalid parsing of game number, defaulting to game 1.");
                                _game_nr = 1;
                            }
                            break;

                        case "-use_connection":
                            if (arg_value.ToLower().Contains("true"))
                                _use_connection = true;
                            else
                                _use_connection = false;
                            break;

                        case "-connection_mode":
                            _connection_mode = arg_value;
                            break;

                        case "-connection_ip":
                            _connection_ip = arg_value;
                            break;

                        case "-connection_port":
                            correct_parse = Int32.TryParse(arg_value, out _connection_port);
                            if (!correct_parse) {
                                _connection_port = 9090;
                                Debug.Log("Invalid or impossible to parse port value, setting to default value.");
                            }
                            break;

                        case "-ros_topic_pub":
                            _ros_pub_topic = arg_value;
                            break;

                        case "-ros_topic_sub":
                            _ros_sub_topic = arg_value;
                            break;

                        default:
                            Debug.Log ("Unrecognized input argument: " + arg);
                            break;
					}
				}
			} catch (Exception e){
				Debug.LogError ("Error reading setup parameters from command line: " + e.ToString ());
			}

		}

        private void ask_start_game() {
            switch (_connection_mode) {
                case "ros":
                    Debug.Log("Ask Start Game");
                    if(_n_players > 0) { 
                        if (_n_players == 1) { 
                            ((Networking.ROSConnection)_robot_connection).game_ready(_player_names[0], _game_nr);
                        } else if (_n_players > 1) {
                            ((Networking.ROSConnection)_robot_connection).game_ready("multi_player", _game_nr);
                        }
                        ((Networking.ROSConnection)_robot_connection).publish(_ros_pub_topic, "std_msgs/String");
                    }
                    break;
                case "thalamus":
                    break;
                default:
                    break;
            }
        }

        private void player_turn_warning() {
            switch (_connection_mode) {
                case "ros":
                    ((Networking.ROSConnection)_robot_connection).child_turn("play", _current_player);
                    ((Networking.ROSConnection)_robot_connection).publish(_ros_pub_topic, "std_msgs/String");
                    break;
                case "thalamus":
                    break;
                default:
                    break;
            }
        }

        private void robot_turn_warning() {
            switch (_connection_mode) {
                case "ros":
                    ((Networking.ROSConnection)_robot_connection).robot_turn("regular", _player_names[_rand_generator.Next(0, _player_names.Count)], 0);
                    ((Networking.ROSConnection)_robot_connection).publish(_ros_pub_topic, "std_msgs/String");
                    break;
                case "thalamus":
                    break;
                default:
                    break;
            }
        }

        void quit_game(){
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
            OnQuit ( );
            #else
            Application.Quit();
            #endif
        }

        void OnApplicationQuit() {
            OnQuit();
        }
        
        void OnQuit(){
            if (_use_connection) { 
                switch (_connection_mode) {
                    case "ros":
                        Debug.Log("Closing Connection with ROS");
                        if(_robot_connection != null) { 
                            ((Networking.ROSConnection)_robot_connection).close_connection();
                            _robot_connection = null;
                        }
                        break;
                    case "thalamus":
                        break;
                    default:
                        break;
                }
            }

            if (_instance != null)
                _instance = null;
        }

		public void change_scene(){
            try { 

                Debug.Log ("Change Scene");
			    string current_scene = SceneManager.GetActiveScene().name;
                string next_scene = "level_square";
                //bool clean_up = false;
                //GameObject obj = null;

                if (current_scene.Contains("start"))
                    if (_puzzle != "")
                        next_scene = "level_" + _puzzle;
                    else
                        next_scene = "level_square";
                else if (current_scene.Contains("level"))
                    next_scene = "start";

                SceneManager.LoadScene(next_scene);

                /*if (clean_up)
                    DestroyObject (obj);*/

                //Debug.Log("Scene Changed");

            } catch (Exception e) {
                Debug.LogError ("Caught Error changing scene: " + e.Message);
            }
		}

        public void select_level (int level) {
            
            change_scene ( );

        }
        
		public void puzzle_finish() {
			_puzzle_over = DateTime.Now;
			_puzzle_finished = true;
		}

        public void lock_pieces() {
            Dictionary<string, GameObject> pieces_left = PuzzleManager.Instance.get_remaining_pieces();
            foreach (string piece_name in pieces_left.Keys) {
                pieces_left[piece_name].GetComponent<MovePiece>().lock_piece();
                pieces_left[piece_name].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
            }
        }

        public void unlock_pieces() {
            Dictionary<string, GameObject> pieces_left = PuzzleManager.Instance.get_remaining_pieces();
            foreach (string piece_name in pieces_left.Keys) {
                pieces_left[piece_name].GetComponent<MovePiece>().unlock_piece();
                pieces_left[piece_name].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }

        private bool can_play() {
            return (_can_play && !(_robot_connection.get_last_event().Contains("game") && _robot_connection.get_last_event().Contains("ready")));
        }

        public GameModes.GameMode get_playing_mode() {
            return _game_mode;
        }

		public bool unlock_puzzle_pieces(){
            return _unlock_puzzle_pieces;
		}

		public void unset_unlock_puzzle_pieces(){
			_unlock_puzzle_pieces = false;
		}

		public void set_unlock_puzzle_pieces(){
			_unlock_puzzle_pieces = true;
		}

		public void set_game_started(){
			_game_ready = true;
		}

		public void unset_game_started(){
			_game_ready = false;
		}
        
        /*public void start_logging ( ) {

            if(_logger == null){
                string file_name = _child_name + "_"  + DateTime.Now.ToString ("dd-MM-yyyy_HH-mm");
                _logger = new PuzzleLogger(file_name, _child_name);

            }
        }*/

        public void set_started_logging() {
            _started_logging = true;
        }

        public bool started_logging() {
            return _started_logging;
        }

       /*public PuzzleLogger get_logger ( ) {
            return _logger;
        }*/

        public void set_turn_change(bool value) {
            _change_turn = value;
        }

        public bool play_with_rotation() {
            return _rotation;
        }

        public int get_difficulty() {
            return _difficulty_level;
        }

        public string get_puzzle() {
            return _puzzle;
        }

        public string get_pub_topic() {
            return _ros_pub_topic;
        }

        public string get_sub_topic() {
            return _ros_sub_topic;
        }

        public string get_current_player() {
            return _current_player;
        }

        public string get_connection_mode() {
            return _connection_mode;
        }

        public void set_touched_piece_solution(GameObject touched_solution) {
            _touched_piece_solution = true;
            _touched_place = touched_solution;
        }

        public void unset_touched_piece_solution() {
            _touched_piece_solution = false;
        }

        public GameObject get_touched_solution_place() {
            return _touched_place;
        }

        public bool has_solution_touched() {
            return _touched_piece_solution;
        }

        public void set_can_play(bool val) {
            _can_play = val;
        }

        public bool has_game_start() {
            return _game_started;
        }

        public void set_game_started(bool started) {
            _game_started = started;
        }

        public void set_response_game_started(bool response) {
            _response_game_started = response;
        }

    }
}
