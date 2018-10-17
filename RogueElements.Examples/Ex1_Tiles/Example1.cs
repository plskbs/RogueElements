﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueElements.Examples.Ex1_Tiles
{
    public static class Example1
    {
        public static void Run()
        {
            string title = "1: A Static Map Example";
            MapGen<MapGenContext> layout = new MapGen<MapGenContext>();




            //Initialize a 30x25 blank map full of Wall tiles
            InitTilesStep<MapGenContext> startStep = new InitTilesStep<MapGenContext>();
            startStep.Width = 30;
            startStep.Height = 25;
            layout.GenSteps.Add(new GenPriority<GenStep<MapGenContext>>(startStep));

            //Draw a specific array of tiles onto the map at offset X2,Y3
            SpecificTilesStep<MapGenContext> drawStep = new SpecificTilesStep<MapGenContext>(new Loc(2, 3));
            string[] level = {
                            ".........................",
                            ".........................",
                            "...........#.............",
                            "....###...###...###......",
                            "...#.#.....#.....#.#.....",
                            "...####...###...####.....",
                            "...#.#############.#.....",
                            "......##.......##........",
                            "......#..#####..#........",
                            "......#.#######.#........",
                            "...#.##.#######.##.#.....",
                            "..#####.###.###.#####....",
                            "...#.##.#######.##.#.....",
                            "......#.#######.#........",
                            "......#..#####..#........",
                            "......##.......##........",
                            "...#.#############.#.....",
                            "...####...###...####.....",
                            "...#.#.....#.....#.#.....",
                            "....###...###...###......",
                            "...........#............."
                        };
            drawStep.Tiles = new ITile[level[0].Length][];
            for (int xx = 0; xx < level[0].Length; xx++)
            {
                drawStep.Tiles[xx] = new ITile[level.Length];
                for (int yy = 0; yy < level.Length; yy++)
                {
                    int id = Map.WALL_TERRAIN_ID;
                    if (level[yy][xx] == '.')
                        id = Map.ROOM_TERRAIN_ID;
                    drawStep.Tiles[xx][yy] = new Tile(id);
                }
            }
            layout.GenSteps.Add(new GenPriority<GenStep<MapGenContext>>(drawStep));




            //Run the generator and print
            MapGenContext context = layout.GenMap(MathUtils.Rand.NextUInt64());
            Print(context.Map, title);
        }

        public static void Print(Map map, string title)
        {
            int oldLeft = Console.CursorLeft;
            int oldTop = Console.CursorTop;
            Console.SetCursorPosition(0, 0);
            StringBuilder topString = new StringBuilder("");
            string turnString = title;
            topString.Append(String.Format("{0,-82}", turnString));
            topString.Append('\n');
            for (int i = 0; i < map.Width + 1; i++)
                topString.Append("=");
            topString.Append('\n');

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Loc loc = new Loc(x, y);
                    char tileChar = ' ';
                    Tile tile = map.Tiles[x][y];
                    if (tile.ID <= 0)//wall
                        tileChar = '#';
                    else if (tile.ID == 1)//floor
                        tileChar = '.';
                    else
                        tileChar = '?';
                    topString.Append(tileChar);
                }
                topString.Append('\n');
            }
            Console.Write(topString.ToString());
        }
    }
}