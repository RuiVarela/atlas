using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlas
{
    public enum PieceState
    {
        STABLE = 0,
        INSTABLE = 1,
        REMOVABLE = 2,
    }

    public enum GameState
    {
        RUNNING = 0,
        LEVELINTRO = 1,
        MENU = 2,
        MISSION_END = 3,
    }
}
