using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;
using UnityEngine;

namespace Tangram{

	public class PuzzleManager : MonoBehaviour {

	    private int _placed_pieces = 0;
	    private int _n_total_pieces;
        private int _n_errors = 0;
        private int _puzzle_image = (int)Level_Names.SQUARE;
        private int _level = (int)Difficulty_Levels.EASY;
	    private bool _fireworks_fired = false;
	    private bool _puzzle_finished = false;
        //private bool _piece_moved = false;
        private bool _rotation = false;
	    private Dictionary<string, List<Vector3>> _pieces_pos;
	    private GameObject _pieces;
        private Dictionary<string, GameObject> _pieces_left;
        private Dictionary<string, GameObject> _pieces_solution;

        private static PuzzleManager instance = null;
        private static GameObject _picked_piece = null;
		public static PuzzleManager Instance { get { return instance; } }
        public static GameObject picked_piece { get { return _picked_piece; } set { _picked_piece = value; } }
	    
		// Use this for initialization
	    void Start() {

            if (instance == null) {
				instance = this;

			} else if (instance != this) {
				DestroyObject (gameObject);
			}

            //Initialize pieces position
            //_pieces_pos = Constants.PTRS_POS[GameManager.Instance.get_difficulty_level ( )];
            string level_name = GameManager.Instance.get_puzzle();
            _rotation = GameManager.Instance.play_with_rotation();
            _puzzle_image = Util_Methods.level_name_to_val(level_name);
            _level = Util_Methods.level_name_to_val(level_name);
            _pieces_pos = Constants.PTRS_POS[_puzzle_image];
            _pieces = GameObject.Find("Pieces");
            _pieces_left = new Dictionary<string, GameObject>();
            _pieces_solution = new Dictionary<string, GameObject>();
            GameObject solution = GameObject.Find("Solution");
            int n_pieces = _pieces.transform.childCount;

            switch (GameManager.Instance.get_difficulty()) {

                case (int)Difficulty_Levels.EASY:
                    initialize_easy_level(n_pieces, solution);
                    break;

                case (int)Difficulty_Levels.MEDIUM:
                    initialize_medium_level(n_pieces, solution);
                    break;

                case (int)Difficulty_Levels.HARD:
                    initialize_hard_level(n_pieces, solution, level_name);
                    break;

                default:
                    break;

            }

            _n_total_pieces = n_pieces;
            //Locking pieces while game hasn't started;
            _n_errors = 0;
			GameManager.Instance.lock_pieces ();
	    }

	    // Update is called once per frame
	    void Update() {

	        if (_placed_pieces == _n_total_pieces && !_puzzle_finished) {
	            //GameManager.Instance.puzzle_finish( );
	            puzzle_over( );
	            _puzzle_finished = true;

	        }

	    }
		
	    void puzzle_over() {
	        if (!_fireworks_fired) {
	            Debug.Log("Puzzle Done!!!");
	            _fireworks_fired = true;
                GameManager.Instance.puzzle_finish();
                //GameManager.Instance.get_logger ( ).write_log_line ("Puzzle Done!");
	        }
	    }

	    public void piece_placed(string piece_name) {
	        _placed_pieces += 1;
            _pieces_left.Remove(piece_name);
            GameManager.Instance.set_turn_change(true);
            //Debug.Log("Placed piece: " + piece_name + " Remaining: " + (_n_total_pieces - _placed_pieces));
        }

        private void initialize_easy_level(int n_pieces, GameObject solution) {

            GameObject piece;
            string piece_name;

            for (int idx = 0; idx < n_pieces; idx++) {
                piece = _pieces.transform.GetChild(idx).gameObject;
                piece_name = piece.gameObject.name;
                piece.transform.position = _pieces_pos[piece_name][0];
                if (!_rotation) {
                    piece.transform.eulerAngles = _pieces_pos[piece_name][1];
                    piece.transform.GetChild(0).gameObject.SetActive(false);
                } else {
                    if (_level == (int)Level_Names.BOAT)
                        piece.transform.eulerAngles = new Vector3(0, 180, 0);
                    else
                        piece.transform.eulerAngles = new Vector3(0, 0, 0);
                }

                _pieces_left.Add(piece_name, piece);
                _pieces_solution.Add(piece_name, solution.transform.GetChild(idx).gameObject);
            }

        }

        private void initialize_medium_level(int n_pieces, GameObject solution) {

            GameObject piece;
            GameObject solution_piece;
            string piece_name;

            for (int idx = 0; idx < n_pieces; idx++) {
                piece = _pieces.transform.GetChild(idx).gameObject;
                solution_piece = solution.transform.GetChild(idx).gameObject;
                piece_name = piece.gameObject.name;
                piece.transform.position = _pieces_pos[piece_name][0];
                if (!_rotation) {
                    piece.transform.eulerAngles = _pieces_pos[piece_name][1];
                    piece.transform.GetChild(0).gameObject.SetActive(false);
                }
                else {
                    if (_level == (int)Level_Names.BOAT)
                        piece.transform.eulerAngles = new Vector3(0, 180, 0);
                    else
                        piece.transform.eulerAngles = new Vector3(0, 0, 0);
                }

                _pieces_left.Add(piece_name, piece);
                _pieces_solution.Add(piece_name, solution_piece);
                solution_piece.GetComponent<SpriteRenderer>().enabled = false;
            }

        }

        private void initialize_hard_level(int n_pieces, GameObject solution, string level_name) {

            GameObject piece;
            GameObject solution_piece;
            string piece_name;

            Debug.Log(level_name);

            SpriteRenderer spr = solution.transform.Find("skeleton").GetComponent<SpriteRenderer>();
            spr.sprite = Resources.Load<Sprite>("Sprites/Puzzle_Skeletons/" + level_name + "_outline");

            for (int idx = 0; idx < n_pieces; idx++) {
                piece = _pieces.transform.GetChild(idx).gameObject;
                solution_piece = solution.transform.GetChild(idx).gameObject;
                piece_name = piece.gameObject.name;
                piece.transform.position = _pieces_pos[piece_name][0];
                if (!_rotation) {
                    piece.transform.eulerAngles = _pieces_pos[piece_name][1];
                    piece.transform.GetChild(0).gameObject.SetActive(false);
                }
                else {
                    if (_level == (int)Level_Names.BOAT)
                        piece.transform.eulerAngles = new Vector3(0, 180, 0);
                    else
                        piece.transform.eulerAngles = new Vector3(0, 0, 0);
                }

                _pieces_left.Add(piece_name, piece);
                _pieces_solution.Add(piece_name, solution_piece);
                solution_piece.GetComponent<SpriteRenderer>().enabled = false;
            }

        }

        public void new_error ( ) { 
            _n_errors += 1;
        }

        public int get_n_errors ( ) {
            return _n_errors;
        }

        public int get_n_pieces() {
            return _n_total_pieces;
        }

        public int get_n_remaining_pieces() {
            return _n_total_pieces - _placed_pieces;
        }

        public int get_n_placed_pieces() {
            return _placed_pieces;
        }

        public GameObject get_pieces() {
            return _pieces;
        }

        public void set_n_errors (int errors) {
            _n_errors = errors;
        }

        public Dictionary<string, GameObject> get_remaining_pieces() {
            return _pieces_left;
        }

        public Dictionary<string, GameObject> get_solution_pieces() {
            return _pieces_solution;
        }

        public void set_solution_pieces(Dictionary<string, GameObject> solution) {
            _pieces_solution = solution;
        }
        
        public Dictionary<string, List<Vector3>> get_init_positions() {
            return _pieces_pos;
        }
    }
}
