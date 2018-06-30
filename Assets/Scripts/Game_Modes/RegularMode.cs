using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Tangram.GameModes {
    public class RegularMode : GameMode {

        private bool _rotation = false;
        private int _turn_warning = 0;
        private DateTime _robot_play_time;
        private string _current_player = "";
        private string _current_piece_name = "";
        private Text _current_player_display;
        

        public RegularMode(bool rotation, GameObject pieces, GameObject solution) {
            _rotation = rotation;
            _current_player_display = GameObject.Find("CurrentPlayer").GetComponent<Text>();
        }
        
        public void robot_turn() {
            _current_player_display.text = "Robot";
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0)) {
                _turn_warning++;
                Debug.Log("Robot Turn!");
            } else if (DateTime.Now >= _robot_play_time) {
                if(!_rotation) 
                    move_piece();
                else {
                    rotate_move_piece();
                }
            }
            
        }

        public void player_turn() {
            _current_player_display.text = Util_Methods.capitalize_word(_current_player);
        }

        public void update_turn_info(string player, string piece, int turn_time, DateTime start) {
            _current_player = player;
            _current_piece_name = piece;
            if (_current_player.ToLower().Contains("robot")) {
                _robot_play_time = start.AddSeconds(turn_time);
            } else
                _robot_play_time = start;
        }

        private void move_piece() {
            GameObject piece = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name];
            GameObject piece_solution = PuzzleManager.Instance.get_solution_pieces()[_current_piece_name];

            piece.GetComponent<MovePiece>().piece_position(piece_solution.transform.position);
            piece.GetComponent<MovePiece>().set_position_change(true);
            piece.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        private void rotate_piece() {
            GameObject piece = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name];
            GameObject piece_solution = PuzzleManager.Instance.get_solution_pieces()[_current_piece_name];

            piece.GetComponent<MovePiece>().rotation_objective(piece_solution.transform.rotation);
            piece.GetComponent<MovePiece>().set_rotation_change(true);
            piece.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        private void rotate_move_piece() {
            GameObject piece = PuzzleManager.Instance.get_remaining_pieces()[_current_piece_name];
            GameObject piece_solution = PuzzleManager.Instance.get_solution_pieces()[_current_piece_name];

            if (piece.transform.rotation == piece_solution.transform.rotation) {
                move_piece();
            } else {
                rotate_piece();
            }

        }

    }
}
