using UnityEngine;

public static class TagUtility
{
    public const string groudTag = "Ground";
    public const string cityTag = "City";
    public const string poiTag = "POI";
    public const string shipTag = "Ship";
    public const string playerShipTag = "PlayerShip";
    public static WorldMapTypes GetWorldTypeFromTag(string tag)
    {
        switch (tag)
        {
            case groudTag:
                return WorldMapTypes.Ground;
            case cityTag: 
                return WorldMapTypes.City;
            case poiTag: 
                return WorldMapTypes.POI;
            case shipTag:
                return WorldMapTypes.Ship;
            case playerShipTag:
                return WorldMapTypes.PlayerShip;
            default:
                return WorldMapTypes.OutOfBounds;
        }
    }
}
