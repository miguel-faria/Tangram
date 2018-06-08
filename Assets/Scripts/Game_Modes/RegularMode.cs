using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangram.GameModes {
    public class RegularMode : GameMode {

        private bool _robot_play = true;
        private int _n_players = 1;
        private string _current_player = "";
        private List<string> _playing_order;
        private int _n_turns = 0;

        public RegularMode(bool robot, int n_players, List<string> player_names) {
            _robot_play = robot;
            _n_players = n_players;
            Random r = new Random();
            if(_n_players > 1) {
                List<string> names = new List<string>(player_names);
                int idx;
                if (_robot_play)
                    names.Add("robot");

                while (names.Count > 0) {
                    idx = r.Next(0, names.Count - 1);
                    _playing_order.Add(names[idx]);
                    names.RemoveAt(idx);
                }
            } else {
                if (_robot_play) {
                    if (r.Next(0, 1) > 0) {
                        _playing_order.Add("robot");
                        _playing_order.Add(player_names[0]);
                    } else {
                        _playing_order.Add(player_names[0]);
                        _playing_order.Add("robot");
                    }
                }
                else
                    _playing_order.Add(player_names[0]);
            }
        }

        public void Manage_Game() {

            if (_n_turns == 0) {

            } else if (_n_turns > 0) {

            }

        }

        private void Robot_Turn() {

        }

        private void Player_Turn() {

        }

    }
}
