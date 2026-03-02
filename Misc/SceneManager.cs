namespace PF.Misc;


public class SceneManager
{
    public static SceneManager I
    {
        get
        {
            field ??= new SceneManager();

            return field;
        }
    }


    readonly Dictionary<string, HashSet<Actor>> Scenes = [];

    public void RemoveScene(World world, string scene = "")
    {
        if(Scenes.Count > 0)
        if(String.IsNullOrEmpty(scene))
        {
            foreach (var actor in Scenes[Scenes.Keys.First((res) => true)])
            {
                world.RemoveActor(actor);
            }

            Scenes.Remove(Scenes.Keys.First((res) => true));

        }
        else
        {
            foreach (var actor in Scenes[scene])
            {
                world.RemoveActor(actor);
            }

            Scenes.Remove(scene);
        }
    }

    public void AddActor(Actor actor, string scene = "")
    {
        if(Scenes.Count < 0)
            Scenes.Add("main", []);


        if(String.IsNullOrEmpty(scene))
        {
            Scenes[Scenes.Keys.First((res) => true)].Add(actor);
        }
        else
        {
            Scenes[scene].Add(actor);
        }
    }

}