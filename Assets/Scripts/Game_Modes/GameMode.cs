using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangram.GameModes {
    public interface GameMode {

        void robot_turn();

        void player_turn();

        void update_turn_info(string player, string piece);

    }
}
