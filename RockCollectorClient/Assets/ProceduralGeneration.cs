using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGeneration
{
    public static Board GenerateBoard()
    {
        // Generate a board with random dimensions
        int width = Random.Range(5, 10);
        int height = Random.Range(5, 10);

        Board board = new Board(width, height);

        // Fill the board with random tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                board.tiles[x, y] = Tile.RandomTile();
            }
        }

        // place the player and exit
        int entranceX = Random.Range(0, width);
        int entranceY = Random.Range(0, height);
        board.tiles[entranceX, entranceY].placeable = new Player();

        int exitX = Random.Range(0, width);
        int exitY = Random.Range(0, height);
        while (exitX == entranceX && exitY == entranceY)
        {
            // Make sure the exit is not placed on top of the player
            exitX = Random.Range(0, width);
            exitY = Random.Range(0, height);
        }
        board.tiles[exitX, exitY].placeable = new Exit();

        return board;
    }
}