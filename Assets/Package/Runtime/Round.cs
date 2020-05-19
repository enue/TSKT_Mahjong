using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class Round
    {
        public readonly Game game;
        public readonly WallTile wallTile;
        public readonly DeadWallTile deadWallTile = new DeadWallTile();
        public readonly Player[] players = new Player[4];
        public readonly TileType roundWind;
        public readonly int dealer;
        public Player Dealer => players[dealer];
        public int CountKan => players.Sum(_ => _.hand.melds.Count(x => x.槓子));
        public readonly List<Tile> totalDiscardedTiles = new List<Tile>();

        public Round(Game game, TileType prevailingWind, int dealer,
            params TileType[][] initialPlayerTilesByCheat)
        {
            this.game = game;
            roundWind = prevailingWind;
            this.dealer = dealer;
            wallTile = new WallTile(game.rule.redTile);

            var winds = new TileType[] { TileType.東, TileType.南, TileType.西, TileType.北 };

            for (int i = 0; i < players.Length; ++i)
            {
                var wind = winds[(i - dealer + winds.Length) % winds.Length];
                var player = new Player(game.scoreOwners[i], this, i, wind);
                players[i] = player;
            }

            if (initialPlayerTilesByCheat != null)
            {
                for (int i = 0; i < initialPlayerTilesByCheat.Length; ++i)
                {
                    if (initialPlayerTilesByCheat[i] != null)
                    {
                        foreach (var it in initialPlayerTilesByCheat[i])
                        {
                            var t = wallTile.tiles.FirstOrDefault(_ => _.type == it);
                            if (t != null)
                            {
                                wallTile.tiles.Remove(t);
                                players[i].hand.tiles.Add(t);
                            }
                        }
                    }
                }
            }

            foreach (var player in players)
            {
                while (player.hand.tiles.Count < 13)
                {
                    var t = wallTile.tiles[0];
                    wallTile.tiles.RemoveAt(0);
                    player.hand.tiles.Add(t);
                }
                player.hand.Sort();
            }

            for (int i = 0; i < DeadWallTile.Count; ++i)
            {
                var t = wallTile.tiles[0];
                wallTile.tiles.RemoveAt(0);
                deadWallTile.tiles.Add(t);
            }
            deadWallTile.OpenDora();
        }

        public Serializables.Round ToSerializable()
        {
            return new Serializables.Round(this);
        }

        public AfterDraw Start()
        {
            return Dealer.Draw();
        }

        Tile DrawFromDeadWallTile(Player player)
        {
            var t = deadWallTile.Draw();
            player.hand.tiles.Add(t);

            if (wallTile.tiles.Count > 0)
            {
                var wall = wallTile.tiles[0];
                wallTile.tiles.RemoveAt(0);
                deadWallTile.tiles.Add(wall);
            }
            return t;
        }

        public AfterDraw ExecuteClosedQuad(Player player, TileType tileType)
        {
            player.hand.BuildClosedQuad(tileType);

            var t = DrawFromDeadWallTile(player);
            deadWallTile.OpenDora();
            return new AfterDraw(player, t, 嶺上: true, openDoraAfterDiscard: false);
        }

        public AfterDraw ExecuteAddedOpenQuad(Player player, Tile tile)
        {
            var meld = player.hand.melds.Find(_ => _.tileFroms.All(x => x.tile.type == tile.type));
            meld.tileFroms.Add((tile, player));

            player.hand.tiles.Remove(tile);

            var drawTile = DrawFromDeadWallTile(player);
            return new AfterDraw(player, drawTile, 嶺上: true, openDoraAfterDiscard: true);
        }

        public AfterDraw ExecuteOpenQuad(Player player, Player discardPlayer)
        {
            var discardPile = discardPlayer.discardPile;
            var discardTile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            var meld = new Meld();
            player.hand.melds.Add(meld);
            meld.tileFroms.Add((discardTile, discardPlayer));
            for (int i = 0; i < 3; ++i)
            {
                var tile = player.hand.tiles.First(_ => _.type == discardTile.type);
                player.hand.tiles.Remove(tile);
                meld.tileFroms.Add((tile, player));
            }
            var drawTile = DrawFromDeadWallTile(player);
            return new AfterDraw(player, drawTile, 嶺上: true, openDoraAfterDiscard: true);
        }
    }
}
