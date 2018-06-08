using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tangram{

	public class GameManager : MonoBehaviour {

		private string _parameters_text_file = "params_file.txt";
        private string _child_name = "Miguel";
        private string _unlocked_piece = "";
		private DateTime _puzzle_over;
		private bool _puzzle_finished = false;
		private bool _unlock_puzzle_pieces = false;
		private bool _game_started = false;
        private bool _started_logging = false;

        //private static PuzzleLogger _logger = null;
		private static GameManager _instance = null;
		public static GameManager Instance { get { return _instance; } }

		//Basic Game Settings
		private static int _difficulty_level = (int)Difficulty_Levels.EASY;
        private static string _puzzle = "square";
        private static string _play_mode = "regular";
        private static bool _rotation = false;
        private static int _n_players = 1;
        private static List<string> _player_names;

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

			} else if (_instance != this){
                gameObject.SetActive (false);
				return;
			}

		}

		/*void Awake(){

		}*/

		// Update is called once per frame
		void Update () {

			if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.LandscapeRight)
				Screen.orientation = ScreenOrientation.LandscapeRight;
			else
				Screen.orientation = ScreenOrientation.LandscapeLeft;

            
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
                            bool correct_parse = false;

							switch(words[0]){

								case "robot_ip":
								break;

								case "ros_ip":
								break;

								case "robot_port":
								break;

								case "ros_port":
								break;

								case "ros_topic_pub":
								break;

                                case "ros_topic_sub":
                                break;

                                case "topic_type":
								break;

                                case "on_experiment":
                                break;

                                default:
                                Debug.Log ("Unrecognized input argument: " + words[0]);
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
                                _difficulty_level = Util_Methods.level_name_to_val(arg_value);
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
                                if (arg_value == "tangram") { 
                                    _puzzle = "square";
                                } else if (arg_value == "") {
                                    System.Random r = new System.Random();
                                    Array vals = Enum.GetValues(typeof(Level_Names));
                                    _puzzle = Util_Methods.level_val_to_name(r.Next((int)vals.GetValue(0), (int)vals.GetValue(vals.Length - 1)));
                                } else
                                    _puzzle = arg_value;
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

                        case "-game_mode":
                            
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
                                _player_names.Add(command_args[i + 2]);
                                Debug.LogError("Invalid number of players, considering only 1 player.");
                            }else {
                                for(int j = 0; j < _n_players; j++) {
                                    _player_names.Add(command_args[i + 2 + j]);
                                }
                            }
                            i += _n_players;
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

		void quit_game(){
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
            OnQuit ( );
			#else
			Application.Quit();
			#endif
		}

		void OnQuit(){
			//_ros_bridge.close_connection ();
            /*_robot_connection.close_socket ( );
            if (_logger != null)
                _logger.close_logger ( );*/
		}

		public void change_scene(){
            try { 

                Debug.Log ("Change Scene");
			    string current_scene = Application.loadedLevelName;
                string next_scene = "level_select";
                bool clean_up = false;
                GameObject obj = null;
                
			    Application.LoadLevel(next_scene);
                
                if (clean_up)
                    DestroyObject (obj);

                Debug.Log("Scene Changed");

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

        private void lock_pieces()
        {
            GameObject pieces = PuzzleManager.Instance.get_pieces();
            for (int i = 0; i < PuzzleManager.Instance.get_n_pieces(); i++)
            {
                GameObject piece = pieces.transform.GetChild(i).gameObject;
                if (piece.name != _unlocked_piece)
                    piece.GetComponent<MovePiece>().lock_piece();
            }
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
			_game_started = true;
		}

		public void unset_game_started(){
			_game_started = false;
		}
        
        public void change_child_name (string name) {
            _child_name = name;
        }

        public string get_child_name (string name) {
            return _child_name;
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
	}
}
