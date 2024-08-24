using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Gameplay
{
    /// <summary>
    /// Update the game state based on the user input.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="userInput"></param>
    /// <returns> True if the update advanced the game state, false otherwise. </returns>
    /// <exception cref="System.Exception"></exception>
    public static bool Update(Board board, UserInput userInput)
    {
        // get player and player position
        Vector2Int playerPosition = board.GetPlayerPosition();
        if (playerPosition.x == -1)
        {
            throw new System.Exception("Player not found");
        }
        Player player = board.tiles[playerPosition.x, playerPosition.y].placeable as Player;
        // if there are not enough inputs yet, do nothing
        if (player.InputsRequiredCount() > userInput.keys.Count)
        {
            return false;
        }
        if (player.InputsRequiredCount() < userInput.keys.Count)
        {
            throw new System.Exception("Too many inputs");
        }

        Vector2Int movementTarget = playerPosition;
        {
            // in the single input case, movementTarget is the direction of the input
            if (userInput.keys.Count == 1)
            {
                movementTarget += KeyToDirection(userInput.keys[0]);
            }
            // in the double input case, we need to check if the inputs are valid
            else
            {
                // for WA, WD, SA, SD, AW, AS, DW, DS, the player should have a pogostick
                List<List<KeyCode>> pogostickInputs = new()
                {
                    new List<KeyCode> { KeyCode.W, KeyCode.A },
                    new List<KeyCode> { KeyCode.W, KeyCode.D },
                    new List<KeyCode> { KeyCode.S, KeyCode.A },
                    new List<KeyCode> { KeyCode.S, KeyCode.D },
                    new List<KeyCode> { KeyCode.A, KeyCode.W },
                    new List<KeyCode> { KeyCode.A, KeyCode.S },
                    new List<KeyCode> { KeyCode.D, KeyCode.W },
                    new List<KeyCode> { KeyCode.D, KeyCode.S },
                };
                if (pogostickInputs.Contains(userInput.keys))
                {
                    bool hasPogostick = false;
                    foreach (Powerup powerup in player.powerups)
                    {
                        if (powerup is Pogostick)
                        {
                            hasPogostick = true;
                            break;
                        }
                    }
                    if (!hasPogostick)
                    {
                        return false;
                    }

                    // if the player has a pogostick, the movement target is the first input times 2 plus the second input
                    Vector2Int firstInput = KeyToDirection(userInput.keys[0]) * 2;
                    Vector2Int secondInput = KeyToDirection(userInput.keys[1]);
                    movementTarget += firstInput + secondInput;
                }

                // for WW, AA, SS, DD, the player should have rollerblades
                List<List<KeyCode>> rollerbladesInputs = new()
                {
                    new List<KeyCode> { KeyCode.W, KeyCode.W },
                    new List<KeyCode> { KeyCode.A, KeyCode.A },
                    new List<KeyCode> { KeyCode.S, KeyCode.S },
                    new List<KeyCode> { KeyCode.D, KeyCode.D },
                };
                if (rollerbladesInputs.Contains(userInput.keys))
                {
                    bool hasRollerblades = false;
                    foreach (Powerup powerup in player.powerups)
                    {
                        if (powerup is Rollerblades)
                        {
                            hasRollerblades = true;
                            break;
                        }
                    }
                    if (!hasRollerblades)
                    {
                        return false;
                    }

                    // if the player has rollerblades, the player moves in the direction of the inputs until they are about to hit a non-empty tile
                    Vector2Int direction = KeyToDirection(userInput.keys[0]);
                    while (true)
                    {
                        Vector2Int nextPosition = movementTarget + direction;
                        if (nextPosition.x < 0 || nextPosition.x >= board.tiles.GetLength(0) || nextPosition.y < 0 || nextPosition.y >= board.tiles.GetLength(1))
                        {
                            break;
                        }
                        if (board.tiles[nextPosition.x, nextPosition.y].placeable != null || board.tiles[nextPosition.x, nextPosition.y].IsBlocking())
                        {
                            break;
                        }
                        movementTarget = nextPosition;
                    }
                    // if this would mean the player would not move, however, the player still tries to move one step in the direction of the input
                    if (movementTarget == playerPosition)
                    {
                        movementTarget += direction;
                    }
                }
            }
        } // determine movementTarget

        int playerMiningStrength = 1;
    }

    private static Vector2Int KeyToDirection(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.W => Vector2Int.up,
            KeyCode.A => Vector2Int.left,
            KeyCode.S => Vector2Int.down,
            KeyCode.D => Vector2Int.right,
            _ => throw new System.Exception("Invalid key"),
        };
    }
}

public class UserInput
{
    public List<KeyCode> keys;
}

public class Board
{
    public Tile[,] tiles;

    public Board(int width, int height)
    {
        tiles = new Tile[width, height];
    }

    public Vector2Int GetPlayerPosition()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y].placeable is Player)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
}