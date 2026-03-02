

using System.Text.Json.Nodes;
using PF.Tilemap;


namespace PF;


public static class PrefabHolder
{
    private static readonly Dictionary<string, Action<Actor, JsonNode>> Prefabs = [];

    public static void AddPrefab(string name, Action<Actor, JsonNode> action) => Prefabs.Add(name, action);
    public static void RemovePrefab(string name) => Prefabs.Remove(name);

    public static List<Actor> InstantiatePrefabs(World world, Tiledmap tm)
    {
        List<Actor> createdPrefabs = [];
        foreach (var prefab in Prefabs)
        {
            createdPrefabs.AddRange(tm.CreateActorsByName(world, prefab.Key, prefab.Value));
        }

        return createdPrefabs;
    }
}