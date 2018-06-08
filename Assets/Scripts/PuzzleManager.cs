using System;
using System.Collections;
using System.Collections.Generic;
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
        private bool _piece_moved = false;
        private bool _rotation = false;
	    private Dictionary<string, List<Vector3>> _pieces_pos;
	    private GameObject _pieces;

		private static PuzzleManager instance = null;
		public static PuzzleManager Instance { get { return instance; } }
	    
		// Use this for initialization
	    void Start() {

            if (instance == null) {
				instance = this;

			} else if (instance != this) {
				DestroyObject (gameObject);
			}

            //Initialize pieces position
            //_pieces_pos = Constants.PTRS_POS[GameManager.Instance.get_difficulty_level ( )];
            _pieces_pos = Constants.PTRS_POS[_puzzle_image];
            _pieces = GameObject.Find("Pieces");
			GameObject piece;
			int n_pieces = _pieces.transform.childCount;
            string piece_name;
			for (int idx = 0; idx < n_pieces; idx++) {
				piece = _pieces.transform.GetChild(idx).gameObject;
                piece_name = piece.gameObject.name;
                piece.transform.position = _pieces_pos[piece_name][0];
                if (!_rotation)
                    piece.transform.eulerAngles = _pieces_pos[piece_name][1];
                else
                    piece.transform.eulerAngles = new Vector3(0, 0, 0);


            }

			_n_total_pieces = n_pieces;

            //Locking pieces while game hasn't started;
            _n_errors = 0;
			//lock_pieces ();
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
                //GameManager.Instance.get_logger ( ).write_log_line ("Puzzle Done!");
	        }
	    }

	    public void piece_placed(string piece_name) {
	        _placed_pieces += 1;
            Debug.Log("Placed piece: " + piece_name + " Remaining: " + (_n_total_pieces - _placed_pieces));
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

        public GameObject get_pieces() {
            return _pieces;
        }

        public void set_n_errors (int errors) {
            _n_errors = errors;
        }

	}
}
