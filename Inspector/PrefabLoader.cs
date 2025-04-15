using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace PatoFramework.Inspector;



public static class PrefabLoader
{

    public static string loadedFilePath {get; private set;} = "";
    public static HashSet<Entity> savedEntities {get; private set;} = [];

    static JsonSerializerSettings settings = new()
    {
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        Formatting = Formatting.None,
    };

    public static void SetLoadedPrefabsFilePath(string path)
    {
        if(File.Exists(path) && Path.GetExtension(path) == ".pfdata")
        {
            loadedFilePath = path;

        }else
        if(File.Exists(GameController.ProjectLoc + path) && Path.GetExtension(GameController.ProjectLoc + path) == ".pfdata")
        {
            loadedFilePath = GameController.ProjectLoc + path;
        } else
        {
            LogManager.LogError("Could not find preload entity data file. Resorting to default");
            path = GameController.ProjectLoc + "PreloadedEntities.pfdata";

            if(File.Exists(path) && Path.GetExtension(path) == ".pfdata")
            {
                loadedFilePath = path;
            } else
            {
                LogManager.LogError("Could not find preload entity data file at default position. Cancelling operation...");
            }
        }
    }

    public static void LoadPrefabsFile(string path)
    {
        bool success = true;
        if(File.Exists(path) && Path.GetExtension(path) == ".pfdata")
        {
            loadedFilePath = path;

        }else
        if(File.Exists(GameController.ProjectLoc + path) && Path.GetExtension(GameController.ProjectLoc + path) == ".pfdata")
        {
            loadedFilePath = GameController.ProjectLoc + path;
        } else
        {
            LogManager.LogError("Could not find preload entity data file. Resorting to default");
            path = GameController.ProjectLoc + "PreloadedEntities.pfdata";

            if(File.Exists(path) && Path.GetExtension(path) == ".pfdata")
            {
                loadedFilePath = path;
            } else
            {
                LogManager.LogError("Could not find preload entity data file at default position. Cancelling operation...");
                success = false;
            }
        }
        
        if(success)
        {
            List<string> content = [.. File.ReadAllLines(loadedFilePath)];

            foreach (var line in content)
            {
                if(content.IndexOf(line) == 0) continue;

                Entity? newObj = JsonConvert.DeserializeObject<Entity>(line, settings);

                if(newObj != null)
                {
                    savedEntities.Add(newObj);
                    newObj.Active = false;
                } else
                LogManager.LogError("Could not preload entity from file");
            }
        }
    }

    public static Entity? PreloadEntityAtIndex(int index, string path = "")
    {
        bool success = true;
        if(File.Exists(path) && Path.GetExtension(path) == ".pfdata")
        {
            loadedFilePath = path;

        }else
        if(File.Exists(GameController.ProjectLoc + path) && Path.GetExtension(GameController.ProjectLoc + path) == ".pfdata")
        {
            loadedFilePath = GameController.ProjectLoc + path;
        } else
        {
            LogManager.LogError("Could not find preload entity data file. Resorting to default");
            path = GameController.ProjectLoc + "PreloadedEntities.pfdata";

            if(File.Exists(path) && Path.GetExtension(path) == ".pfdata")
            {
                loadedFilePath = path;
            } else
            {
                LogManager.LogError("Could not find preload entity data file at default position. Cancelling operation...");
                success = false;
            }
        }

        if(success)
        {
            var content = File.ReadAllLines(loadedFilePath);

            var newObj = JsonConvert.DeserializeObject<Entity>(content[index], settings);
            if(newObj != null)
            {
                savedEntities.Add (newObj);
                newObj.Active = false;
                return newObj;
            }
            else
            LogManager.LogError("Could not preload entity from file");
        }

        return null;
    }

    public static void SavePrefabsFile(string path = "")
    {
        bool success = true;

        if(path == "")
        {
            if(loadedFilePath == "")
            {
                if(!File.Exists(GameController.ProjectLoc + "PreloadedEntities.pfdata"))
                {
                    File.Create(GameController.ProjectLoc + "PreloadedEntities.pfdata");
                }

                path = GameController.ProjectLoc + "PreloadedEntities.pfdata";
                loadedFilePath = path;
            }
        } else
        if(File.Exists(path) && Path.GetExtension(path) == ".pfdata")
        {
            loadedFilePath = path;

        }else
        if(File.Exists(GameController.ProjectLoc + path) && Path.GetExtension(GameController.ProjectLoc + path) == ".pfdata")
        {
            loadedFilePath = GameController.ProjectLoc + path;
        } else
        {
            LogManager.LogError("Could not find preload entity data file. Resorting to default");
            path = GameController.ProjectLoc + "PreloadedEntities.pfdata";

            if(File.Exists(path) && Path.GetExtension(path) == ".pfdata")
            {
                loadedFilePath = path;
            } else
            {
                LogManager.LogError("Could not find preload entity data file at default position. Cancelling operation...");
                success = false;
            }
        }

        if(success)
        {
            var builder = new StringBuilder(savedEntities.Count);

            foreach (var entity in savedEntities.OrderBy(res => res.ID))
            {
                var serEntity = JsonConvert.SerializeObject(entity, settings);

                builder.AppendLine();
                builder.Append(serEntity);
            }
        }

    }

    public static void UnloadAllSavedEntities()
    {
        foreach (var entity in savedEntities.ToList())
        {
            UnloadSavedEntity(entity.ID);
        }
    }
    public static void UnloadSavedEntity(ulong ID)
    {
        savedEntities.RemoveWhere(res => res.ID == ID);
    }
}