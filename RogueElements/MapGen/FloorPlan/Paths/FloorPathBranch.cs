﻿using System;
using System.Collections.Generic;

namespace RogueElements
{
    [Serializable]
    public class FloorPathBranch<T> : FloorPathStartStepGeneric<T>
        where T : class, IFloorPlanGenContext
    {
        public RandRange FillPercent;
        public int HallPercent;
        public RandRange BranchRatio;
        public bool NoForcedBranches;

        //maintains a separate grid based on markovs
        //anything in the actual chain can link to the generic rooms/halls as a last resort

        public FloorPathBranch()
            : base()
        { }

        public override void ApplyToPath(IRandom rand, FloorPlan floorPlan)
        {
            for (int ii = 0; ii < 10; ii++)
            {
                //always clear before trying
                floorPlan.Clear();

                int tilesToOpen = floorPlan.DrawRect.Area * FillPercent.Pick(rand) / 100;
                if (tilesToOpen < 1)
                    tilesToOpen = 1;
                int addBranch = BranchRatio.Pick(rand);
                int tilesLeft = tilesToOpen;

                //choose a room
                IRoomGen room = PrepareRoom(rand, floorPlan, false);
                //place in a random location
                room.SetLoc(new Loc(rand.Next(floorPlan.DrawRect.Left, floorPlan.DrawRect.Right - room.Draw.Width + 1),
                    rand.Next(floorPlan.DrawRect.Top, floorPlan.DrawRect.Bottom - room.Draw.Height + 1)));
                floorPlan.AddRoom(room, false);

                tilesLeft -= room.Draw.Area;

                //repeat this process until the requisite room amount is met
                int pendingBranch = 0;
                while (tilesLeft > 0)
                {
                    ExpansionResult terminalResult = expandPath(rand, floorPlan, false);
                    ExpansionResult branchResult = new ExpansionResult();
                    if (terminalResult.Area > 0)
                    {
                        tilesLeft -= terminalResult.Area;
                        //add branch PER ROOM when we add over the min threshold
                        for (int jj = 0; jj < terminalResult.Rooms; jj++)
                        {
                            if (floorPlan.RoomCount + floorPlan.HallCount- terminalResult.Rooms + jj + 1 > 2)
                                pendingBranch += addBranch;
                        }
                    }
                    else if (NoForcedBranches)
                        break;
                    else
                        pendingBranch = 100;
                    while (pendingBranch >= 100 && tilesLeft > 0)
                    {
                        branchResult = expandPath(rand, floorPlan, true);
                        if (branchResult.Area == 0)
                            break;
                        pendingBranch -= 100;
                        //if we add any more than one room, that also counts as a branchable node
                        pendingBranch += (branchResult.Rooms - 1) * addBranch;
                        tilesLeft -= branchResult.Area;
                    }
                    if (terminalResult.Area == 0 && branchResult.Area == 0)
                        break;
                }

                if (tilesLeft <= 0)
                    break;
            }
        }

        private ExpansionResult expandPath(IRandom rand, FloorPlan floorPlan, bool branch)
        {
            ListPathBranchExpansion expansion = ChooseRoomExpansion(rand, floorPlan, branch);

            if (expansion == null)
                return new ExpansionResult();

            int tilesCovered = 0;
            int roomsAdded = 0;

            RoomHallIndex from = expansion.From;
            if (expansion.Hall != null)
            {
                floorPlan.AddHall(expansion.Hall, from);
                from = new RoomHallIndex(floorPlan.HallCount-1,true);
                tilesCovered += expansion.Hall.Draw.Area;
                roomsAdded++;
            }

            floorPlan.AddRoom(expansion.Room, false, from);
            tilesCovered += expansion.Room.Draw.Area;
            roomsAdded++;
            //report the added area coverage
            return new ExpansionResult(tilesCovered, roomsAdded);
        }

        public virtual ListPathBranchExpansion ChooseRoomExpansion(IRandom rand, FloorPlan floorPlan, bool branch)
        {
            List<RoomHallIndex> availableExpansions = GetPossibleExpansions(floorPlan, branch);

            if (availableExpansions.Count == 0)
                return null;

            for (int ii = 0; ii < 30; ii++)
            {
                //choose the next room to add to
                RoomHallIndex firstExpandFrom = availableExpansions[rand.Next(availableExpansions.Count)];
                RoomHallIndex expandFrom = firstExpandFrom;
                IRoomGen roomFrom = floorPlan.GetRoomHall(firstExpandFrom).Gen;

                //choose the next room to add
                //choose room size/fulfillables
                //note: by allowing halls to be picked as extensions, we run the risk of adding dead-end halls
                //halls should always terminate at rooms?
                //this means... doubling up with hall+room?
                bool addHall = (rand.Next(100) < HallPercent);
                IRoomGen hall = null;
                if (addHall)
                {
                    hall = PrepareRoom(rand, floorPlan, true);

                    //randomly choose a perimeter to assign this to
                    SpawnList<Loc> possibleHallPlacements = new SpawnList<Loc>();
                    for (int dd = 0; dd < DirExt.DIR4_COUNT; dd++)
                        AddLegalPlacements(possibleHallPlacements, floorPlan, expandFrom, roomFrom, hall, (Dir4)dd);

                    //at this point, all possible factors for whether a placement is legal or not is accounted for
                    //therefor just pick one
                    if (possibleHallPlacements.Count == 0)
                        continue;

                    //randomly choose one
                    Loc hallCandLoc = possibleHallPlacements.Pick(rand);
                    //set location
                    hall.SetLoc(hallCandLoc);

                    //change the roomfrom for the upcoming room
                    expandFrom = new RoomHallIndex(-1, false);
                    roomFrom = hall;
                }

                IRoomGen room = PrepareRoom(rand, floorPlan, false);

                //randomly choose a perimeter to assign this to
                SpawnList<Loc> possiblePlacements = new SpawnList<Loc>();
                for (int dd = 0; dd < DirExt.DIR4_COUNT; dd++)
                    AddLegalPlacements(possiblePlacements, floorPlan, expandFrom, roomFrom, room, (Dir4)dd);

                //at this point, all possible factors for whether a placement is legal or not is accounted for
                //therefor just pick one
                if (possiblePlacements.Count > 0)
                {
                    //randomly choose one
                    Loc candLoc = possiblePlacements.Pick(rand);
                    //set location
                    room.SetLoc(candLoc);
                    return new ListPathBranchExpansion(firstExpandFrom, room, (IPermissiveRoomGen)hall);
                }
            }

            return null;
        }


        public List<RoomHallIndex> GetPossibleExpansions(FloorPlan floorPlan, bool branch)
        {
            List<RoomHallIndex> availableExpansions = new List<RoomHallIndex>();
            for (int ii = 0; ii < floorPlan.RoomCount; ii++)
            {
                RoomHallIndex listHall = new RoomHallIndex(ii, false);
                List<RoomHallIndex> adjacents = floorPlan.GetRoomHall(listHall).Adjacents;
                if ((adjacents.Count <= 1) != branch)
                    availableExpansions.Add(listHall);
            }
            for (int ii = 0; ii < floorPlan.HallCount; ii++)
            {
                RoomHallIndex listHall = new RoomHallIndex(ii, true);
                List<RoomHallIndex> adjacents = floorPlan.GetRoomHall(listHall).Adjacents;
                if ((adjacents.Count <= 1) != branch)
                    availableExpansions.Add(listHall);
            }
            return availableExpansions;
        }

        public virtual void AddLegalPlacements(SpawnList<Loc> possiblePlacements, FloorPlan floorPlan, RoomHallIndex indexFrom, IRoomGen roomFrom, IRoomGen room, Dir4 expandTo)
        {
            bool vertical = expandTo.ToAxis() == Axis4.Vert;
            //this scaling factor equalizes the chances of long sides vs short sides
            int reverseSideMult = vertical ? roomFrom.Draw.Width * room.Draw.Width : roomFrom.Draw.Height * room.Draw.Height;

            Range side = roomFrom.Draw.GetSide(expandTo.ToAxis());
            //subtract the room's original size, not the inflated trialrect size
            side.Min -= (vertical ? room.Draw.Size.X : room.Draw.Size.Y) - 1;

            Rect tryRect = room.Draw;
            //expand in every direction
            //this will create a one-tile buffer to check for collisions
            tryRect.Inflate(1, 1);
            int currentScalar = side.Min;
            while (currentScalar < side.Max)
            {
                //compute the location
                Loc trialLoc = roomFrom.GetEdgeRectLoc(expandTo, room.Draw.Size, currentScalar);
                tryRect.Start = trialLoc + new Loc(-1, -1);
                //check for collisions (not counting the rectangle from)
                List<RoomHallIndex> collisions = floorPlan.CheckCollision(tryRect);

                //find the first tile in which no collisions will be found
                int maxCollideScalar = currentScalar;
                bool collided = false;
                foreach (RoomHallIndex collision in collisions)
                {
                    if (collision != indexFrom)
                    {
                        IRoomGen collideRoom = floorPlan.GetRoomHall(collision).Gen;
                        //this is the point at which the new room will barely touch the collided room
                        //the +1 at the end will move it into the safe zone
                        maxCollideScalar = Math.Max(maxCollideScalar, vertical ? collideRoom.Draw.Right : collideRoom.Draw.Bottom);
                        collided = true;
                    }
                }

                //if no collisions were hit, do final checks and add the room
                if (!collided)
                {
                    Loc locTo = roomFrom.GetEdgeRectLoc(expandTo, room.Draw.Size, currentScalar);
                    //must be within the borders of the floor!
                    if (floorPlan.DrawRect.Contains(new Rect(locTo, room.Draw.Size)))
                    {
                        //check the border match and if add to possible placements
                        int chanceTo = FloorPlan.GetBorderMatch(roomFrom, room, locTo, expandTo);
                        if (chanceTo > 0)
                            possiblePlacements.Add(locTo, chanceTo * reverseSideMult);
                    }
                }

                currentScalar = maxCollideScalar + 1;
            }
        }
        


        public virtual RoomGen<T> PrepareRoom(IRandom rand, FloorPlan floorPlan, bool isHall)
        {
            RoomGen<T> room = null;
            if (!isHall) //choose a room
                room = GenericRooms.Pick(rand).Copy();
            else // chose a hall
                room = GenericHalls.Pick(rand).Copy();

            //decide on acceptable border/size/fulfillables
            Loc size = room.ProposeSize(rand);
            if (size.X > floorPlan.DrawRect.Width)
                size.X = floorPlan.DrawRect.Width;
            if (size.Y > floorPlan.DrawRect.Height)
                size.Y = floorPlan.DrawRect.Height;
            room.PrepareSize(rand, size);
            return room;
        }
        
    }

    public struct ExpansionResult
    {
        public int Area;
        public int Rooms;

        public ExpansionResult(int area, int rooms)
        {
            Area = area;
            Rooms = rooms;
        }
    }

    public class ListPathBranchExpansion
    {
        public RoomHallIndex From;
        public IPermissiveRoomGen Hall;
        public IRoomGen Room;

        public ListPathBranchExpansion(RoomHallIndex from, IRoomGen room, IPermissiveRoomGen hall)
        {
            From = from;
            Room = room;
            Hall = hall;
        }
    }

    
}