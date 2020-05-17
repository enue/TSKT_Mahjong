using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs
{
    public class ScoreOwner
    {
        public readonly Game game;
        public double score;

        public ScoreOwner(Game game)
        {
            this.game = game;
            score = game.rule.initialScore;
        }
    }
}