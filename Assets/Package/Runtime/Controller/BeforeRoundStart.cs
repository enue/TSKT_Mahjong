using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class BeforeRoundStart : IController
    {
        public Round Round => null;
        public readonly Game game;

        public BeforeRoundStart(Game game)
        {
            this.game = game;
        }

        public AfterDraw StartRound(params TileType[][] initialPlayerTilesByCheat)
        {
            return game.StartRound(initialPlayerTilesByCheat);
        }
    }
}
