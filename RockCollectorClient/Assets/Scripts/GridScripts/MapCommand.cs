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
    private Vector2Int builtBuildingPosition;

    public BuildBuilding(int teamNumber, string buildingTypeName, Vector2Int sourceBuildingPosition, Vector2Int buildingPosition)
    {
        this.teamNumber = teamNumber;
        this.buildingTypeName = buildingTypeName;
        this.sourceBuildingPosition = sourceBuildingPosition;
        this.builtBuildingPosition = buildingPosition;
    }

    private Building GetSourceBuilding(Map map)
    {
        List<Placeable> placeablesAtPosition = map.PlaceablesAt(sourceBuildingPosition);
        foreach (Placeable placeable in placeablesAtPosition)
        {
            if (placeable is Building building)
            {
                return building;
            }
        }
        return null;
    }

    public override bool CheckStillValid(Map map)
    {
        Building sourceBuilding = GetSourceBuilding(map);
        if (sourceBuilding == null)
        {
            return false;
        }
        bool inputMaterialsPresent = sourceBuilding.BuildBuildingInputMaterialsPresent(EntityManager.buildingTypes[buildingTypeName], map);

        BuildingType buildingType = EntityManager.buildingTypes[buildingTypeName];
        int size = buildingType.GetSize();
        return map.IsBuildable(builtBuildingPosition, size) && inputMaterialsPresent;
    }

    public override void Execute(Map map)
    {
        // Remove input materials
        Building sourceBuilding = GetSourceBuilding(map);
        List<ItemType> inputTypes = EntityManager.buildingTypes[buildingTypeName].GetConstructionItemTypes();
        List<int> inputAmounts = EntityManager.buildingTypes[buildingTypeName].GetConstructionItemAmounts();
        for (int i = 0; i < inputTypes.Count; i++)
        {
            sourceBuilding.SpendStoredItems(inputTypes[i], inputAmounts[i], map);
        }

        BuildingType buildingType = EntityManager.buildingTypes[buildingTypeName];
        Building building = new (buildingType, teamNumber);
        // New buildings start at 10% health
        building.TakeDamage(Mathf.CeilToInt(building.GetMaxHealth() * 0.9f));
        map.Add(building, builtBuildingPosition);
    }
}

public class MakeTransportRoute : MapCommand
{
    private Vector2Int sourceBuildingPosition;
    private Vector2Int destinationBuildingPosition;

    public MakeTransportRoute(Vector2Int sourceBuildingPosition, Vector2Int destinationBuildingPosition)
    {
        this.sourceBuildingPosition = sourceBuildingPosition;
        this.destinationBuildingPosition = destinationBuildingPosition;
    }

    public override bool CheckStillValid(Map map)
    {
        bool sourceFound = false;
        bool destinationFound = false;
        List<Placeable> placeablesAtSource = map.PlaceablesAt(sourceBuildingPosition);
        foreach (Placeable placeable in placeablesAtSource)
        {
            if (placeable is Building)
            {
                sourceFound = true;
                break;
            }
        }
        List<Placeable> placeablesAtDestination = map.PlaceablesAt(destinationBuildingPosition);
        foreach (Placeable placeable in placeablesAtDestination)
        {
            if (placeable is Building)
            {
                destinationFound = true;
                break;
            }
        }
        return sourceFound && destinationFound;
    }

    public override void Execute(Map map)
    {
        Building sourceBuilding = null;
        List<Placeable> placeablesAtPosition = map.PlaceablesAt(sourceBuildingPosition);
        foreach (Placeable placeable in placeablesAtPosition)
        {
            if (placeable is Building building)
            {
                sourceBuilding = building;
                break;
            }
        }
        Building destinationBuilding = null;
        placeablesAtPosition = map.PlaceablesAt(destinationBuildingPosition);
        foreach (Placeable placeable in placeablesAtPosition)
        {
            if (placeable is Building building)
            {
                destinationBuilding = building;
                break;
            }
        }
        if (sourceBuilding == null || destinationBuilding == null)
        {
            throw new System.Exception("Source or destination building not found");
        }
        map.AddTransportRoute(sourceBuilding, destinationBuilding);
    }
}