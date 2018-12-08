﻿using System;
using System.Collections.Generic;

namespace RogueElements
{
    [Serializable]
    public class ConnectGridBranchStep<T> : GridPlanStep<T> where T : class, IRoomGridGenContext
    {
        public IRandPicker<PermissiveRoomGen<T>> GenericHalls;
        public int ConnectPercent;

        public ConnectGridBranchStep()
        {
            GenericHalls = new SpawnList<PermissiveRoomGen<T>>();
        }

        public ConnectGridBranchStep(int connectPercent)
            : this()
        {
            ConnectPercent = connectPercent;
        }

        public override void ApplyToPath(IRandom rand, GridPlan floorPlan)
        {
            List<LocRay4> endBranches = new List<LocRay4>();
            for (int ii = 0; ii < floorPlan.RoomCount; ii++)
            {
                GridRoomPlan roomPlan = floorPlan.GetRoomPlan(ii);
                if (roomPlan.Bounds.Size == new Loc(1))
                {
                    List<int> adjacents = floorPlan.GetAdjacentRooms(ii);
                    if (adjacents.Count == 1)
                        endBranches.Add(new LocRay4(roomPlan.Bounds.Start));
                }
            }

            List<List<LocRay4>> candBranchPoints = new List<List<LocRay4>>();
            for (int nn = 0; nn < endBranches.Count; nn++)
            {
                LocRay4 chosenBranch = endBranches[nn];

                while (chosenBranch.Loc != new Loc(-1))
                {
                    List<LocRay4> connectors = new List<LocRay4>();
                    List<LocRay4> candBonds = new List<LocRay4>();
                    for (int ii = 0; ii < DirExt.DIR4_COUNT; ii++)
                    {
                        if ((Dir4)ii != chosenBranch.Dir)
                        {
                            if (floorPlan.GetHall(new LocRay4(chosenBranch.Loc, (Dir4)ii)) != null)
                                connectors.Add(new LocRay4(chosenBranch.Loc, (Dir4)ii));
                            else
                            {
                                Loc loc = chosenBranch.Loc + ((Dir4)ii).GetLoc();
                                if (Collision.InBounds(floorPlan.GridWidth, floorPlan.GridHeight, loc)
                                    && floorPlan.GetRoomIndex(loc) > -1)
                                    candBonds.Add(new LocRay4(chosenBranch.Loc, (Dir4)ii));
                            }
                        }
                    }

                    if (connectors.Count == 1)
                    {
                        if (candBonds.Count > 0)
                        {
                            candBranchPoints.Add(candBonds);
                            chosenBranch = new LocRay4(new Loc(-1));
                        }
                        else
                            chosenBranch = new LocRay4(connectors[0].Traverse(1), connectors[0].Dir.Reverse());
                    }
                    else
                        chosenBranch = new LocRay4(new Loc(-1));
                }
            }

            //compute a goal amount of terminals to connect
            //this computation ignores the fact that some terminals may be impossible
            RandBinomial randBin = new RandBinomial(candBranchPoints.Count, ConnectPercent);
            int connectionsLeft = randBin.Pick(rand);

            while (candBranchPoints.Count > 0 && connectionsLeft > 0)
            {
                //choose random point to connect
                int randIndex = rand.Next(candBranchPoints.Count);
                List<LocRay4> candBonds = candBranchPoints[randIndex];
                LocRay4 chosenDir = candBonds[rand.Next(candBonds.Count)];
                //connect
                Loc chosenDest = chosenDir.Traverse(1);
                floorPlan.SetConnectingHall(chosenDir.Loc, chosenDest, GenericHalls.Pick(rand));
                candBranchPoints.RemoveAt(randIndex);
                GenContextDebug.DebugProgress("Connected Branch");
                connectionsLeft--;
                //check to see if connection destination was also a candidate,
                //counting this as a double if so
                for (int ii = candBranchPoints.Count - 1; ii >= 0; ii--)
                {
                    if (candBranchPoints[ii][0].Loc == chosenDest)
                    {
                        candBranchPoints.RemoveAt(ii);
                        connectionsLeft--;
                    }
                }
            }

        }


    }
}