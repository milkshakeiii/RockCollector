using UnityEngine;
using System.Collections.Generic;

public abstract class MapCommand
{
    public abstract bool CheckStillValid(Map map);

    public abstract void Execute(Map map);
}

public class  SpawnCreature : MapCommand
{
    private string creatureTypeName;
    private Vector2Int buildingPosition;
    
    public SpawnCreature(string creatureTypeName, Vector2Int buildingPosition)
    {
        this.creatureTypeName = creatureTypeName;
        this.buildingPosition = buildingPosition;
    }

    public override bool CheckStillValid(Map map)
    {
        List<Placeable> placeablesAtPosition = map.PlaceablesAt(buildingPosition);
        foreach (Placeable placeable in placeablesAtPosition)
        {
            if (placeable is Building)
            {
                return true;
            }
        }
        return false;
    }

    public override void Execute(Map map)
    {
        List<Placeable> placeablesAtPosition = map.PlaceablesAt(buildingPosition);
        Building spawningBuilding = null;
        foreach (Placeable placeable in placeablesAtPosition)
        {
            if (placeable is Building building)
            {
                spawningBuilding = building;
                break;
            }
        }
        CreatureType creatureType = EntityManager.creatureTypes[creatureTypeName];
        spawningBuilding.StartSpawnCreature(creatureType);
    }
}
