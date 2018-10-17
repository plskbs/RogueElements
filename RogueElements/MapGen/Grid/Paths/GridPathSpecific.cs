﻿using System;
using System.Collections.Generic;

namespace RogueElements
{
    [Serializable]
    public class GridPathSpecific<T> : GridPathStartStep<T>
        where T : class, IRoomGridGenContext
    {
        public List<SpecificGridRoomPlan<T>> SpecificRooms;
        public PermissiveRoomGen<T>[][] SpecificVHalls;
        public PermissiveRoomGen<T>[][] SpecificHHalls;

        public GridPathSpecific()
            : base()
        {
            SpecificRooms = new List<SpecificGridRoomPlan<T>>();
        }

        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            if (floorPlan.GridWidth != SpecificVHalls.Length ||
                floorPlan.GridWidth - 1 != SpecificHHalls.Length ||
                floorPlan.GridHeight - 1 != SpecificVHalls[0].Length ||
                floorPlan.GridHeight != SpecificHHalls[0].Length)
                throw new InvalidOperationException("Incorrect hall path sizes.");

            for (int ii = 0; ii < SpecificRooms.Count; ii++)
            {
                SpecificGridRoomPlan<T> chosenRoom = SpecificRooms[ii];
                
                floorPlan.AddRoom(chosenRoom.Bounds.X, chosenRoom.Bounds.Y,
                    chosenRoom.Bounds.Size.X, chosenRoom.Bounds.Size.Y,
                    chosenRoom.RoomGen, chosenRoom.Immutable);
            }

            //place halls
            for (int x = 0; x < SpecificVHalls.Length; x++)
            {
                for (int y = 0; y < SpecificHHalls[0].Length; y++)
                {
                    if (x > 0)
                    {
                        if (SpecificHHalls[x - 1][y] != null)
                            UnsafeAddHall(new Loc(x - 1, y), new Loc(x, y), floorPlan, SpecificHHalls[x - 1][y], GetDefaultGen());
                    }
                    if (y > 0)
                    {
                        if (SpecificVHalls[x][y - 1] != null)
                            UnsafeAddHall(new Loc(x, y - 1), new Loc(x, y), floorPlan, SpecificVHalls[x][y - 1], GetDefaultGen());
                    }
                }
            }
        }

        public void UnsafeAddHall(Loc room1, Loc room2, GridPlan floorPlan, IPermissiveRoomGen hallGen, IRoomGen roomGen)
        {
            floorPlan.SetConnectingHall(room1, room2, hallGen);
            if (floorPlan.GetRoomPlan(room1.X, room1.Y) == null ||
                floorPlan.GetRoomPlan(room2.X, room2.Y) == null)
            {
                floorPlan.Clear();
                throw new InvalidOperationException("Can't create a hall without rooms to connect!");
            }
        }
    }


    public class SpecificGridRoomPlan<T> where T : ITiledGenContext
    {
        public Rect Bounds;
        public bool Immutable;
        public RoomGen<T> RoomGen;

        public SpecificGridRoomPlan(Rect bounds, RoomGen<T> roomGen)
        {
            Bounds = bounds;
            RoomGen = roomGen;
        }

    }
}