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

        public bool Consumed { get; private set; }

        public BeforeRoundStart(Game game)
        {
            this.game = game;
        }

        public AfterDraw StartRound(params TileType[][] initialPlayerTilesByCheat)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            return game.StartRound(initialPlayerTilesByCheat);
        }
    }
}
