﻿// <copyright file="RoomGenBlocked.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace RogueElements
{
    [Serializable]
    public class RoomGenBlocked<T> : PermissiveRoomGen<T>
        where T : ITiledGenContext
    {
        public RoomGenBlocked()
        {
        }

        public RoomGenBlocked(ITile blockTerrain, RandRange width, RandRange height, RandRange blockWidth, RandRange blockHeight)
        {
            this.BlockTerrain = blockTerrain;
            this.Width = width;
            this.Height = height;
            this.BlockWidth = blockWidth;
            this.BlockHeight = blockHeight;
        }

        protected RoomGenBlocked(RoomGenBlocked<T> other)
        {
            this.BlockTerrain = other.BlockTerrain.Copy();
            this.Width = other.Width;
            this.Height = other.Height;
            this.BlockWidth = other.BlockWidth;
            this.BlockHeight = other.BlockHeight;
        }

        public RandRange Width { get; set; }

        public RandRange Height { get; set; }

        public RandRange BlockWidth { get; set; }

        public RandRange BlockHeight { get; set; }

        public ITile BlockTerrain { get; set; }

        public override RoomGen<T> Copy() => new RoomGenBlocked<T>(this);

        public override Loc ProposeSize(IRandom rand)
        {
            return new Loc(this.Width.Pick(rand), this.Height.Pick(rand));
        }

        public override void DrawOnMap(T map)
        {
            for (int x = 0; x < this.Draw.Size.X; x++)
            {
                for (int y = 0; y < this.Draw.Size.Y; y++)
                    map.SetTile(new Loc(this.Draw.X + x, this.Draw.Y + y), map.RoomTerrain.Copy());
            }

            GenContextDebug.DebugProgress("Room Rect");

            Loc blockSize = new Loc(Math.Min(this.BlockWidth.Pick(map.Rand), this.Draw.Size.X - 2), Math.Min(this.BlockHeight.Pick(map.Rand), this.Draw.Size.Y - 2));
            Loc blockStart = new Loc(this.Draw.X + map.Rand.Next(1, this.Draw.Size.X - blockSize.X - 1), this.Draw.Y + map.Rand.Next(1, this.Draw.Size.Y - blockSize.Y - 1));
            for (int x = 0; x < blockSize.X; x++)
            {
                for (int y = 0; y < blockSize.Y; y++)
                    map.SetTile(new Loc(blockStart.X + x, blockStart.Y + y), this.BlockTerrain.Copy());
            }

            GenContextDebug.DebugProgress("Block Rect");

            // hall restrictions
            this.SetRoomBorders(map);
        }
    }
}
