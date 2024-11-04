using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public GameObject timefishSpawnerPrefab;
    public GameObject seaweedPrefab;
    public GameObject smallDecorRockPrefab;
    public GameObject largeDecorRockPrefab;
    public GameObject coralDecorationPrefab;
    public GameObject iceDecorationPrefab;
    public GameObject stalactiteDecorationPrefab;
    public GameObject cavePlanetDecorationPrefab;

    public Sprite leftCornerRock;
    public Sprite rightCornerRock;
    public List<Sprite> edgeRocks;
    public List<Sprite> topEdgeRocks;
    public List<Sprite> fillerRocks;

    public Sprite leftCornerIce;
    public Sprite rightCornerIce;
    public Sprite leftInnerCornerIce;
    public Sprite rightInnerCornerIce;
    public List<Sprite> edgeIce;
    public List<Sprite> topEdgeIce;
    public List<Sprite> fillerIce;

    public List<Sprite> coralSprites;
    public List<Sprite> caveSprites;

    public float terrainSpacing = 0.319f;
    private int lastSeed = 0;

    public int GetLastSeed()
    {
        return lastSeed;
    }

    public IEnumerator Generate(int seed)
    {
        System.Random random = new(seed);

        yield return null;

        int width = random.Next(150, 400);
        int height = random.Next(150, 400);
        Debug.Log("Width: " + width + " Height: " + height);

        bool[,] rockGrid = new bool[width, height];
        bool[,] iceGrid = new bool[width, height];
        bool[,] coralGrid = new bool[width*2, height*2];
        bool[,] caveGrid = new bool[width * 2, height * 2];

        // and a corresponding grid to keep track of danger levels,
        // which will be used to spawn timefish and other dangers
        float[,] dangerGrid = new float[width, height];

        // create horizontal and vertical imaginary lines to divide the terrain into sectors
        int numberOfHorizontalLines = Mathf.Max(Mathf.RoundToInt(2f + (float)NormalDistribution(random)), 1);
        int numberOfVerticalLines = random.Next(1, 3);
        List<int> horizontalLines = new();
        List<int> verticalLines = new();
        // make sure the first horizontal line is low
        horizontalLines.Add(random.Next(0, height / 3));
        for (int i = 1; i < numberOfHorizontalLines; i++)
        {
            horizontalLines.Add(random.Next(0, height));
        }
        for (int i = 0; i < numberOfVerticalLines; i++)
        {
            verticalLines.Add(random.Next(0, width));
        }
        int secondLowestHorizontalLine = int.MaxValue;
        foreach (int line in horizontalLines)
        {
            secondLowestHorizontalLine = Math.Min(secondLowestHorizontalLine, line);
        }
        horizontalLines.Add(height);
        verticalLines.Add(width);
        horizontalLines.Add(0);
        verticalLines.Add(0);
        horizontalLines.Sort();
        verticalLines.Sort();
        List<Vector2Int> sectorStarts = new();
        List<Vector2Int> sectorSizes = new();
        for (int i = 0; i < horizontalLines.Count - 1; i++)
        {
            for (int j = 0; j < verticalLines.Count - 1; j++)
            {
                int sectorStartX = verticalLines[j];
                int sectorStartY = horizontalLines[i];
                int sectorWidth = verticalLines[j + 1] - verticalLines[j];
                int sectorHeight = horizontalLines[i + 1] - horizontalLines[i];
                sectorStarts.Add(new Vector2Int(sectorStartX, sectorStartY));
                sectorSizes.Add(new Vector2Int(sectorWidth, sectorHeight));
            }
        }

        int edgeCaveChance = Math.Clamp(Mathf.RoundToInt((float)Math.Abs(NormalDistribution(random) * 20 + 15)), 0, 100);
        bool falseBottom = true;
        // determine the terrain for each sector
        for (int i = 0; i < sectorStarts.Count; i++)
        {
            int sectorWidth = sectorSizes[i].x;
            int sectorHeight = sectorSizes[i].y;
            int sectorStartX = sectorStarts[i].x;
            int sectorStartY = sectorStarts[i].y;
            Debug.Log("Sector start: " + sectorStartX + ", " + sectorStartY);
            Debug.Log("Sector size: " + sectorWidth + ", " + sectorHeight);
            if ((sectorStartX == 0 || sectorStartX == width - sectorWidth) && random.Next(100) < edgeCaveChance)
            {
                IEnumerator sectorEnumator = MakeCaveSector(random, sectorWidth * 2, sectorHeight * 2, sectorStartX * 2, sectorStartY * 2, caveGrid);
                while (sectorEnumator.MoveNext())
                {
                    yield return null;
                }
            }
            else if ((!falseBottom && sectorStartY == 0) || falseBottom && (sectorStartY == secondLowestHorizontalLine) && sectorStartY != 0)
            {
                Debug.Log("mountain sector: " + sectorStartX + ", " + sectorStartY);
                IEnumerator sectorEnumator = MakeMountainSector(random, sectorWidth, sectorHeight, sectorStartX, sectorStartY, rockGrid, 3, 100, 100, 100);
                while (sectorEnumator.MoveNext())
                {
                    yield return null;
                }
            }
            else if (sectorStartY == 0 && falseBottom)
            {
                IEnumerator sectorEnumator = MakeCaveSector(random, sectorWidth * 2, sectorHeight * 2, sectorStartX * 2, sectorStartY * 2, caveGrid);
                while (sectorEnumator.MoveNext())
                {
                    yield return null;
                }
            }
            else if (sectorStartY == height - sectorHeight)
            {
                IEnumerator sectorEnumator = MakeInvertedMountainSector(random, sectorWidth, sectorHeight, sectorStartX, sectorStartY, iceGrid);
                while (sectorEnumator.MoveNext())
                {
                    yield return null;
                }
            }
            else
            {
                IEnumerator sectorEnumator = MakeCoralSector(random, sectorWidth*2, sectorHeight*2, sectorStartX*2, sectorStartY*2, coralGrid);
                while (sectorEnumator.MoveNext())
                {
                    yield return null;
                }
            }
        }

        // make the left, right, and bottom edges solid
        for (int x = 0; x < width; x++)
        {
            rockGrid[x, 0] = true;
        }
        for (int y = 0; y < height; y++)
        {
            rockGrid[0, y] = true;
            rockGrid[width - 1, y] = true;
        }

        // make sure there's space for the return ship
        // make sure the terrain grid is false for a rectangle at the top center
        for (int x = width / 2 - 5; x < width / 2 + 5; x++)
        {
            for (int y = height - 40; y < height; y++)
            {
                iceGrid[x, y] = false;
            }
        }

        // each grid cell covers 4 sprites with 4 rotations
        Sprite[,] sprites = new Sprite[width * 2, height * 2];
        int[,] rotations = new int[width * 2, height * 2];
        IEnumerator rockSpritesEnumerator = DecideRockSpritesAndRotations(random, rockGrid, sprites, rotations);
        while (rockSpritesEnumerator.MoveNext())
        {
            yield return null;
        }

        IEnumerator iceSpritesEnumerator = DecideIceSpritesAndRotations(random, iceGrid, sprites, rotations);
        while (iceSpritesEnumerator.MoveNext())
        {
            yield return null;
        }

        IEnumerator coralSpritesEnumerator = DecideCoralSpritesAndRotations(random, coralGrid, sprites, rotations);
        while (coralSpritesEnumerator.MoveNext())
        {
            yield return null;
        }

        IEnumerator caveSpritesEnumerator = DecideCaveSpritesAndRotations(random, caveGrid, sprites, rotations);
        while (caveSpritesEnumerator.MoveNext())
        {
            yield return null;
        }

        bool[,] hasTimefishSpawner = new bool[width, height];
        // each tile that is not solid has a 1 in ~8000 chance of having a timefish spawner
        // that chance is 1 in ~1200 instead if there is an adjacent rock or ~600 for adjacent coral tile
        int openWaterSpawnChance = random.Next(3000, 12000);
        int rockSpawnChance = random.Next(900, 1400);
        int coralSpawnChance = random.Next(300, 800);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!rockGrid[x, y] && random.Next(0, openWaterSpawnChance) == 0)
                {
                    hasTimefishSpawner[x, y] = true;
                }
                bool adjacentRock = false;
                bool adjacentCoral = false;
                List<Vector2Int> neighborOffsets = new()
                {
                    new Vector2Int(1, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1)
                };
                foreach (Vector2Int offset in neighborOffsets)
                {
                    Vector2Int neighbor = new Vector2Int(x + offset.x, y + offset.y);
                    if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height)
                    {
                        if (rockGrid[neighbor.x, neighbor.y])
                        {
                            adjacentRock = true;
                        }
                        if (coralGrid[neighbor.x*2, neighbor.y*2])
                        {
                            adjacentCoral = true;
                        }
                    }
                }
                if (adjacentRock && random.Next(0, rockSpawnChance) == 0)
                {
                    hasTimefishSpawner[x, y] = true;
                }
                if (adjacentCoral && random.Next(0, coralSpawnChance) == 0)
                {
                    hasTimefishSpawner[x, y] = true;
                }
            }
        }

        // spawn the GameObjects
        IEnumerator spawnEnumerator = SpawnGameObjects(random, sprites, rotations, hasTimefishSpawner);
        while (spawnEnumerator.MoveNext())
        {
            yield return null;
        }

        // TODO spawn timefish


        lastSeed = seed;
    }

    private IEnumerator MakeInvertedMountainSector(System.Random random, int sectorWidth, int sectorHeight, int sectorStartX, int sectorStartY, bool[,] terrainGrid)
    {
        IEnumerator mountainEnumerator = MakeMountainSector(random, sectorWidth, sectorHeight, sectorStartX, sectorStartY, terrainGrid, 6, 50, 100, 50);
        while (mountainEnumerator.MoveNext())
        {
            yield return null;
        }
        FlipSector(sectorWidth, sectorHeight, sectorStartX, sectorStartY, terrainGrid);
    }

    private IEnumerator MakeMountainSector(
        System.Random random,
        int sectorWidth,
        int sectorHeight,
        int sectorStartX,
        int sectorStartY,
        bool[,] terrainGrid,
        int mountainCount,
        int maximumSpacing,
        int maximumWidth,
        int maximumHeight)
    {
        int mountainStartX = sectorStartX + random.Next(0, maximumSpacing); ;
        for (int i = 0; i < mountainCount; i++)
        { // make mountainCount many mountains
            // if we're already past the width, break
            if (mountainStartX >= sectorStartX + sectorWidth)
            {
                break;
            }

            int mountainStartY = sectorStartY;
            int mountainEndX = Mathf.Min(mountainStartX + random.Next(30, maximumWidth), sectorStartX + sectorWidth - 1);
            int mountainEndY = Mathf.Min(mountainStartY + random.Next(30, maximumHeight), sectorStartY + sectorHeight - 1);
            int mountainWidth = mountainEndX - mountainStartX;
            int mountainCenter = random.Next(mountainStartX + mountainWidth / 4, mountainEndX - mountainWidth / 4);
            IEnumerator mountainEnumerator = AddMountain(random, mountainStartX, mountainStartY, mountainEndX, mountainEndY, mountainCenter, terrainGrid);
            while (mountainEnumerator.MoveNext())
            {
                yield return null;
            }
            mountainStartX = mountainStartX + random.Next(30, maximumSpacing);
        }
    }

    private void FlipSector(int sectorWidth, int sectorHeight, int sectorStartX, int sectorStartY, bool[,] terrainGrid)
    {
        bool[,] sectorCopy = new bool[sectorWidth, sectorHeight];
        for (int x = 0; x < sectorWidth; x++)
        {
            for (int y = 0; y < sectorHeight; y++)
            {
                sectorCopy[x, y] = terrainGrid[sectorStartX + x, sectorStartY + y];
            }
        }
        for (int x = 0; x < sectorWidth; x++)
        {
            for (int y = 0; y < sectorHeight; y++)
            {
                terrainGrid[sectorStartX + x, sectorStartY + y] = sectorCopy[x, sectorHeight - y - 1];
            }
        }
    }

    private IEnumerator AddMountain(System.Random random, int startX, int startY, int endX, int endY, int center, bool[,] grid)
    {

        int currentX = startX;
        int currentY = startY;
        // starting at the bottom left corner, create a mountain contour
        int conservativeHeight = endY - 7;
        while (currentX < endX)
        {
            if (currentY < endY && currentY >= startY)
            {
                //Debug.Log("xIndex: " + currentX + " yIndex: " + currentY);
                grid[currentX, currentY] = true;
            }
            currentX++;
            float slope = (float)(conservativeHeight - currentY) / (center - currentX);
            if (currentX >= center)
            {
                slope = (float)(-currentY) / (endX - currentX);
            }
            int jitter = random.Next(-1, 2);
            int jitter2 = random.Next(-1, 2);
            int Ychange = Mathf.RoundToInt(slope + jitter + jitter2);
            currentY += Ychange;
            yield return null;
        }

        // fill in the rest of the rocks one column at a time
        for (int x = startX; x < endX; x++)
        {
            bool fill = false;
            for (int y = endY - 1; y >= startY; y--)
            {
                if (grid[x, y])
                {
                    fill = true;
                }
                grid[x, y] = fill;
            }
            yield return null;
        }
    }

    private IEnumerator MakeCoralSector(System.Random random, int sectorWidth, int sectorHeight, int sectorStartX, int sectorStartY, bool[,] coralGrid)
    {   
        for (int x = sectorStartX; x < sectorStartX + sectorWidth; x++)
        {
            for (int y = sectorStartY; y < sectorStartY + sectorHeight; y++)
            {
                if (Mathf.PerlinNoise(x / 10f, y / 10f) > 0.5f)
                {
                    coralGrid[x, y] = true;
                }
            }
        }
        for (int x = sectorStartX; x < sectorStartX + sectorWidth; x++)
        {
            for (int y = sectorStartY; y < sectorStartY + sectorHeight; y++)
            {
                int distanceFromLeft = x - sectorStartX;
                int distanceFromRight = sectorStartX + sectorWidth - x;
                int distanceFromBottom = y - sectorStartY;
                int distanceFromTop = sectorStartY + sectorHeight - y;
                int closestEdge = Mathf.Min(distanceFromLeft, distanceFromRight, distanceFromBottom, distanceFromTop);
                float edgeNearnessFactor = (1 / (float)closestEdge) * 0.3f;
                if (Mathf.PerlinNoise(x / 100f, y / 100f) < 0.7f + edgeNearnessFactor)
                {
                    coralGrid[x, y] = false;
                }
            }
        }
        yield return null;
    }
    
    // This will also add holes in the sector above it unless it is already a topmost sector
    private IEnumerator MakeCaveSector(System.Random random, int sectorWidth, int sectorHeight, int sectorStartX, int sectorStartY, bool[,] caveGrid)
    {
        for (int x = sectorStartX; x < sectorStartX + sectorWidth; x++)
        {
            for (int y = sectorStartY; y < sectorStartY + sectorHeight; y++)
            {
                if (Mathf.PerlinNoise(x / 10f, y / 10f) > 0.5f)
                {
                    caveGrid[x, y] = true;
                }
            }
        }
        yield return null;
    }

    private IEnumerator DecideRockSpritesAndRotations(System.Random random, bool[,] grid, Sprite[,] sprites, int[,] rotations)
    {
        IEnumerator rockStyleEnumerator = DecideRockStyleSpritesAndRotations(
            random, grid, sprites, rotations, edgeRocks, topEdgeRocks, fillerRocks, leftCornerRock, rightCornerRock, null, null);
        while (rockStyleEnumerator.MoveNext())
        {
            yield return null;
        }
    }

    private IEnumerator DecideIceSpritesAndRotations(System.Random random, bool[,] iceGrid, Sprite[,] sprites, int[,] rotations)
    {
        IEnumerator iceStyleEnumerator = DecideRockStyleSpritesAndRotations(
            random, iceGrid, sprites, rotations, edgeIce, topEdgeIce, fillerIce, leftCornerIce, rightCornerIce, leftInnerCornerIce, rightInnerCornerIce);
        while (iceStyleEnumerator.MoveNext())
        {
            yield return null;
        }
    }

    private IEnumerator DecideRockStyleSpritesAndRotations(
        System.Random random,
        bool[,] grid,
        Sprite[,] sprites,
        int[,] rotations,
        List<Sprite> useEdgeRocks,
        List<Sprite> useTopRocks,
        List<Sprite> useFillerRocks,
        Sprite useLeftCornerRock,
        Sprite useRightCornerRock,
        Sprite useLeftInnerCornerRock,
        Sprite useRightInnerCornerRock
    )
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        // set filler, edge, corner
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid[x, y])
                {
                    continue;
                }
                bool upLeftOccupied = x > 0 && y < height - 1 && grid[x - 1, y + 1];
                bool upOccupied = y < height - 1 && grid[x, y + 1];
                bool upRightOccupied = x < width - 1 && y < height - 1 && grid[x + 1, y + 1];
                bool rightOccupied = x < width - 1 && grid[x + 1, y];
                bool downRightOccupied = x < width - 1 && y > 0 && grid[x + 1, y - 1];
                bool downOccupied = y > 0 && grid[x, y - 1];
                bool downLeftOccupied = x > 0 && y > 0 && grid[x - 1, y - 1];
                bool leftOccupied = x > 0 && grid[x - 1, y];

                bool topLeftCornerFree = !upLeftOccupied && !upOccupied && !leftOccupied;
                bool topRightCornerFree = !upRightOccupied && !upOccupied && !rightOccupied;
                bool bottomRightCornerFree = !downRightOccupied && !downOccupied && !rightOccupied;
                bool bottomLeftCornerFree = !downLeftOccupied && !downOccupied && !leftOccupied;

                bool topLeftCornerOccupied = upLeftOccupied && upOccupied && leftOccupied;
                bool topRightCornerOccupied = upRightOccupied && upOccupied && rightOccupied;
                bool bottomRightCornerOccupied = downRightOccupied && downOccupied && rightOccupied;
                bool bottomLeftCornerOccupied = downLeftOccupied && downOccupied && leftOccupied;

                bool topLeftInnerCorner = !upLeftOccupied && upOccupied && leftOccupied;
                bool topRightInnerCorner = !upRightOccupied && upOccupied && rightOccupied;
                bool bottomRightInnerCorner = !downRightOccupied && downOccupied && rightOccupied;
                bool bottomLeftInnerCorner = !downLeftOccupied && downOccupied && leftOccupied;

                // based on the grid neighbors, set 4 sprites per grid cell
                Sprite topLeft = useFillerRocks[random.Next(useFillerRocks.Count)];
                Sprite topRight = useFillerRocks[random.Next(useFillerRocks.Count)];
                Sprite bottomRight = useFillerRocks[random.Next(useFillerRocks.Count)];
                Sprite bottomLeft = useFillerRocks[random.Next(useFillerRocks.Count)];
                // begin with random rotations
                int topLeftRotation = random.Next(0, 4) * 90;
                int topRightRotation = random.Next(0, 4) * 90;
                int bottomRightRotation = random.Next(0, 4) * 90;
                int bottomLeftRotation = random.Next(0, 4) * 90;
                if (topLeftCornerFree)
                {
                    topLeft = useLeftCornerRock;
                    topLeftRotation = 0;
                }
                if (topRightCornerFree)
                {
                    topRight = useRightCornerRock;
                    topRightRotation = 270;
                }
                if (bottomRightCornerFree)
                {
                    bottomRight = useLeftCornerRock;
                    bottomRightRotation = 180;
                }
                if (bottomLeftCornerFree)
                {
                    bottomLeft = useRightCornerRock;
                    bottomLeftRotation = 90;
                }
                if (upOccupied && !leftOccupied)
                {
                    topLeft = useEdgeRocks[random.Next(useEdgeRocks.Count)];
                    topLeftRotation = 90;
                }
                if (rightOccupied && !upOccupied)
                {
                    topRight = useTopRocks[random.Next(useTopRocks.Count)];
                    topRightRotation = 0;
                }
                if (downOccupied && !rightOccupied)
                {
                    bottomRight = useEdgeRocks[random.Next(useEdgeRocks.Count)];
                    bottomRightRotation = 270;
                }
                if (leftOccupied && !downOccupied)
                {
                    bottomLeft = useEdgeRocks[random.Next(edgeRocks.Count)];
                    bottomLeftRotation = 180;
                }
                if (downOccupied && !leftOccupied)
                {
                    bottomLeft = useEdgeRocks[random.Next(useEdgeRocks.Count)];
                    bottomLeftRotation = 90;
                }
                if (leftOccupied && !upOccupied)
                {
                    topLeft = useTopRocks[random.Next(useTopRocks.Count)];
                    topLeftRotation = 0;
                }
                if (upOccupied && !rightOccupied)
                {
                    topRight = useEdgeRocks[random.Next(useEdgeRocks.Count)];
                    topRightRotation = 270;
                }
                if (rightOccupied && !downOccupied)
                {
                    bottomRight = useEdgeRocks[random.Next(useEdgeRocks.Count)];
                    bottomRightRotation = 180;
                }
                if (topLeftInnerCorner && useLeftInnerCornerRock != null)
                {
                    topLeft = useLeftInnerCornerRock;
                    topLeftRotation = 0;
                }
                if (topRightInnerCorner && useRightInnerCornerRock != null)
                {
                    topRight = useRightInnerCornerRock;
                    topRightRotation = 0;
                }
                if (bottomRightInnerCorner && useLeftInnerCornerRock != null)
                {
                    bottomRight = useLeftInnerCornerRock;
                    bottomRightRotation = 180;
                }
                if (bottomLeftInnerCorner && useRightInnerCornerRock != null)
                {
                    bottomLeft = useRightInnerCornerRock;
                    bottomLeftRotation = 180;
                }
                sprites[x * 2, y * 2] = bottomLeft;
                sprites[x * 2 + 1, y * 2] = bottomRight;
                sprites[x * 2, y * 2 + 1] = topLeft;
                sprites[x * 2 + 1, y * 2 + 1] = topRight;
                rotations[x * 2, y * 2] = bottomLeftRotation;
                rotations[x * 2 + 1, y * 2] = bottomRightRotation;
                rotations[x * 2, y * 2 + 1] = topLeftRotation;
                rotations[x * 2 + 1, y * 2 + 1] = topRightRotation;
            }
            yield return null;
        }
    }

    private IEnumerator DecideCoralSpritesAndRotations(System.Random random, bool[,] coralGrid, Sprite[,] sprites, int[,] rotations)
    {
        int width = coralGrid.GetLength(0);
        int height = coralGrid.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!coralGrid[x, y])
                {
                    continue;
                }
                // set the sprite to a random coral sprite
                sprites[x, y] = coralSprites[random.Next(coralSprites.Count)];
                // set the rotation to a random multiple of 90 degrees
                rotations[x, y] = random.Next(0, 4) * 90;
            }
        }
        yield return null;
    }

    private IEnumerator DecideCaveSpritesAndRotations(System.Random random, bool[,] caveGrid, Sprite[,] sprites, int[,] rotations)
    {
        int width = caveGrid.GetLength(0);
        int height = caveGrid.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!caveGrid[x, y])
                {
                    continue;
                }
                // set the sprite to a random cave sprite
                sprites[x, y] = caveSprites[random.Next(caveSprites.Count)];
                // set the rotation to a random multiple of 90 degrees
                rotations[x, y] = random.Next(0, 4) * 90;
            }
        }
        yield return null;
    }

    private IEnumerator SpawnGameObjects(System.Random random, Sprite[,] sprites, int[,] rotations, bool[,] hasTimefishSpawner)
    {
        float seaweedChance = random.Next(10, 50) / 100f;
        float decorRockChance = random.Next(10, 40) / 100f;
        float coralDecorationChance = random.Next(10, 60) / 100f;
        float iceDecorationChance = random.Next(30, 90) / 100f;
        float stalactiteDecorationChance = random.Next(10, 50) / 100f;
        float cavePlantDecorationChance = random.Next(30, 90) / 100f;
        HashSet<Vector2Int> squaresWithCoralDecorations = new();
        HashSet<Vector2Int> squaresWithCaveDecorations = new();

        int width = sprites.GetLength(0);
        int height = sprites.GetLength(1);
        // spawn GameObjects with sprites
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (sprites[x, y] != null)
                {
                    GameObject terrainObject = new(sprites[x, y].name);
                    terrainObject.transform.position = new Vector3((-width / 2 + x) * terrainSpacing, (-height + y) * terrainSpacing, 0);
                    SpriteRenderer renderer = terrainObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprites[x, y];
                    renderer.sortingOrder = 0;
                    // also add a collider if this is not a filler rock or ice
                    if (!fillerRocks.Contains(sprites[x, y]) && !fillerIce.Contains(sprites[x, y]))
                        terrainObject.AddComponent<BoxCollider2D>();
                    // layer is "Terrain"
                    terrainObject.layer = 9;
                    // set the rotation
                    terrainObject.transform.Rotate(Vector3.forward, rotations[x, y]);

                    // chance to add a decoration if this is a top edge rock
                    if (topEdgeRocks.Contains(sprites[x, y]))
                    {
                        if (random.NextDouble() < seaweedChance)
                        {
                            // instantiate seaweed prefab
                            GameObject seaweed = Instantiate(seaweedPrefab, terrainObject.transform);
                            seaweed.GetComponent<RockDecoration>().Initialize(random);
                        }
                        else if (random.NextDouble() < decorRockChance)
                        {
                            bool flatArea = true;
                            if (!topEdgeRocks.Contains(sprites[x-1, y]) || !topEdgeRocks.Contains(sprites[x+1, y]))
                            {
                                flatArea = false;
                            }
                            if (random.NextDouble() < 0.25 && flatArea)
                            {
                                GameObject largeRock = Instantiate(largeDecorRockPrefab, terrainObject.transform);
                                largeRock.GetComponent<RockDecoration>().Initialize(random);
                                // large rock should always be behind the terrain
                                largeRock.GetComponent<SpriteRenderer>().sortingOrder = random.Next(-200, -87);
                            }
                            else
                            {
                                GameObject smallRock = Instantiate(smallDecorRockPrefab, terrainObject.transform);
                                smallRock.GetComponent<RockDecoration>().Initialize(random);
                            }
                        }
                    }

                    // chance to add a coral decoration if this is a coral sprite
                    if (coralSprites.Contains(sprites[x, y]))
                    {
                        // only proceed if an orthogonally adjacent square is empty
                        List<Vector2Int> emptyNeighbors = new List<Vector2Int>();
                        List<Vector2Int> neighborOffsets = new List<Vector2Int>
                        {
                            new Vector2Int(1, 0),
                            new Vector2Int(-1, 0),
                            new Vector2Int(0, 1),
                            new Vector2Int(0, -1)
                        };
                        foreach (Vector2Int offset in neighborOffsets)
                        {
                            Vector2Int neighbor = new Vector2Int(x + offset.x, y + offset.y);
                            if (sprites[neighbor.x, neighbor.y] == null && !squaresWithCoralDecorations.Contains(neighbor))
                            {
                                emptyNeighbors.Add(neighbor);
                            }
                        }
                        if (emptyNeighbors.Count > 0 && random.NextDouble() < coralDecorationChance)
                        {
                            Vector2Int newCoralDecorationPosition = emptyNeighbors[random.Next(emptyNeighbors.Count)];
                            GameObject coralDecoration = Instantiate(coralDecorationPrefab, terrainObject.transform);
                            coralDecoration.GetComponent<SpriteRenderer>().sortingOrder = random.Next(100, 200);
                            Vector2Int baseDirection = new Vector2Int(x - newCoralDecorationPosition.x, y - newCoralDecorationPosition.y);
                            coralDecoration.GetComponent<CoralDecoration>().Initialize(random, baseDirection);

                            squaresWithCoralDecorations.Add(newCoralDecorationPosition);
                        }
                    }

                    // chance to add an ice decoration if this is an edge or corner ice sprite and the square below is empty
                    bool squareBelowEmpty = y > 0 && sprites[x, y - 1] == null;
                    if (edgeIce.Contains(sprites[x, y]) || sprites[x, y] == leftCornerIce || sprites[x, y] == rightCornerIce && squareBelowEmpty)
                    {
                        if (random.NextDouble() < iceDecorationChance)
                        {
                            GameObject iceDecoration = Instantiate(iceDecorationPrefab, terrainObject.transform);
                            iceDecoration.GetComponent<IcicleDecoration>().Initialize(random);
                            // ice decoration should always be behind the terrain
                            iceDecoration.GetComponent<SpriteRenderer>().sortingOrder = random.Next(-200, -87);
                        }
                    }

                    // chance to add a stalactite decoration if this is a cave sprite and the square above or below is empty
                    bool squareAboveEmpty = y < height - 1 && sprites[x, y + 1] == null;
                    if (caveSprites.Contains(sprites[x, y]) && random.NextDouble() < stalactiteDecorationChance && (squareAboveEmpty || squareBelowEmpty))
                    {
                        GameObject stalactiteDecoration = Instantiate(stalactiteDecorationPrefab, terrainObject.transform);
                        stalactiteDecoration.GetComponent<StalactiteDecoration>().Initialize(random, squareBelowEmpty);
                        // stalactite decoration should always be behind the terrain
                        stalactiteDecoration.GetComponent<SpriteRenderer>().sortingOrder = random.Next(-200, -87);
                        if (squareBelowEmpty)
                        {
                            squaresWithCaveDecorations.Add(new Vector2Int(x, y - 1));
                        }
                        else
                        {
                            squaresWithCaveDecorations.Add(new Vector2Int(x, y + 1));
                        }
                    }

                    // chance to add a cave planet decoration if this is a cave sprite
                    if (caveSprites.Contains(sprites[x, y]) && x > 1 && x < width - 2 && y > 1 && y < height - 2)
                    {
                        // only proceed if an orthogonally adjacent square is empty
                        List<Vector2Int> emptyNeighbors = new List<Vector2Int>();
                        List<Vector2Int> neighborOffsets = new List<Vector2Int>
                        {
                            new Vector2Int(1, 0),
                            new Vector2Int(-1, 0),
                            new Vector2Int(0, 1),
                            new Vector2Int(0, -1)
                        };
                        foreach (Vector2Int offset in neighborOffsets)
                        {
                            Vector2Int neighbor = new Vector2Int(x + offset.x, y + offset.y);
                            if (sprites[neighbor.x, neighbor.y] == null && !squaresWithCaveDecorations.Contains(neighbor))
                            {
                                emptyNeighbors.Add(neighbor);
                            }
                        }
                        if (emptyNeighbors.Count > 0 && random.NextDouble() < cavePlantDecorationChance)
                        {
                            Vector2Int newCaveDecorationPosition = emptyNeighbors[random.Next(emptyNeighbors.Count)];
                            GameObject coralDecoration = Instantiate(cavePlanetDecorationPrefab, terrainObject.transform);
                            coralDecoration.GetComponent<SpriteRenderer>().sortingOrder = random.Next(100, 200);
                            Vector2Int baseDirection = new Vector2Int(x - newCaveDecorationPosition.x, y - newCaveDecorationPosition.y);
                            coralDecoration.GetComponent<CoralDecoration>().Initialize(random, baseDirection);

                            squaresWithCaveDecorations.Add(newCaveDecorationPosition);
                        }
                    }
                }
            }
            yield return null;
        }

        // spawn the timefish spawners
        int undoubledWidth = width / 2;
        int undoubledHeight = height / 2;
        float minX = (-width / 2) * terrainSpacing;
        float maxX = (width / 2) * terrainSpacing;
        float minY = (-height) * terrainSpacing;
        float maxY = 0;
        for (int x = 0; x < undoubledWidth; x++)
        {
            for (int y = 0; y < undoubledHeight; y++)
            {
                if (hasTimefishSpawner[x, y])
                {
                    GameObject timefishSpawner = Instantiate(timefishSpawnerPrefab);
                    timefishSpawner.transform.position = new Vector3((-undoubledWidth / 2 + x) * terrainSpacing * 2, (-undoubledHeight + y) * terrainSpacing * 2, 0);
                    timefishSpawner.GetComponent<TimefishSpawner>().Initialize(random, minX, maxX, minY, maxY);
                    yield return null;
                }
            }
        }
    }

    /// <summary>
    /// Use the random number generator to generate a number from a normal distribution
    /// </summary>
    /// <param name="random"></param>
    /// <returns>Number from a normal distribution with mean 0 and standard deviation 1</returns>
    public static double NormalDistribution(System.Random random)
    {
        float u1 = (float)random.NextDouble();
        float u2 = (float)random.NextDouble();
        double z0 = Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Cos(2 * Mathf.PI * u2);
        return z0;
    }

    public static double MultimodalDistribution(System.Random random, List<int> percentileDividors, List<double> maxima, List<double> sigmas, bool rerandomize)
    {
        int percentile = random.Next(0, 100);
        if (rerandomize)
        {
            random = new System.Random();
        }
        for (int i = 0; i < percentileDividors.Count; i++)
        {
            if (percentile < percentileDividors[i])
            {
                return NormalDistribution(random) * sigmas[i] + maxima[i];
            }
        }
        return NormalDistribution(random) * sigmas[^1] + maxima[^1];
    }
}

public class TerrainGrid
{

}