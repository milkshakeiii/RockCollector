using UnityEngine;
using System.Collections.Generic;

public abstract class MapCommand
{
    public abstract bool CheckStillValid(Map map);

    public abstract void Execute(Map map);
}

public class SpawnCreature : MapCommand
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
        spawningBuilding.StartSpawnCreature(creatureType, map);
    }
}

public class ChangeRequestedItemAmount : MapCommand
{
    private string itemName;
    private int amount;
    private Vector2Int buildingPosition;

    public ChangeRequestedItemAmount(string itemName, int amount, Vector2Int buildingPosition)
    {
        this.itemName = itemName;
        this.amount = amount;
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
        Building building = null;
        foreach (Placeable placeable in placeablesAtPosition)
        {
            if (placeable is Building b)
            {
                building = b;
                break;
            }
        }
        ItemType itemType = EntityManager.itemTypes[itemName];
        building.ChangeRequestedItemAmount(itemType, amount);
    }
}

public class BuildBuilding : MapCommand
{
    private int teamNumber;
    private string buildingTypeName;
    private Vector2Int sourceBuildingPosition;
    private Vector2Int buildingPosition;

    public BuildBuilding(int teamNumber, string buildingTypeName, Vector2Int sourceBuildingPosition, Vector2Int buildingPosition)
    {
        this.teamNumber = teamNumber;
        this.buildingTypeName = buildingTypeName;
        this.sourceBuildingPosition = sourceBuildingPosition;
        this.buildingPosition = buildingPosition;
    }

    public override bool CheckStillValid(Map map)
    {
        bool inputMaterialsPresent = true;


        BuildingType buildingType = EntityManager.buildingTypes[buildingTypeName];
        int size = buildingType.GetSize();
        return map.IsBuildable(buildingPosition, size) && inputMaterialsPresent;
    }

    public override void Execute(Map map)
    {
        BuildingType buildingType = EntityManager.buildingTypes[buildingTypeName];
        Building building = new (buildingType, teamNumber);
        map.Add(building, buildingPosition);
    }
}