

using System.IO;
using System.Text;

using Raylib_cs;

namespace PatoframeWork.Rendering;

/// <summary>
/// Main class to handle Sprite Loading and Unloading Logic;
/// </summary>
public static class SpriteManager
{
    public static Dictionary<string, ImageData> LoadedImages { get; private set; } = [];

    public static bool IsDirty{ get; private set; } = false;


    public static Image DefaultImg { get; set; } = Raylib.GenImageColor(1, 1, new Color(128, 128, 255));

    public static Texture2D? DefaultText { get; private set; }


    /// <summary>
    /// Opens the .pfimg at <paramref name="dataFilePath"/>, and loads its images.
    /// </summary>
    
    public static ImageData[] LoadTexturesFromDataFile(string dataFilePath, bool cleanUp)
    {
        List<ImageData> returnImages = [];


        if(cleanUp && LoadedImages.Count > 0) RemoveAllTextures();

        IsDirty = true;

        var data = File.ReadLines(dataFilePath);

        if(data.Any())
        foreach (var line in data)
        {
            var split = line.Split(' ', 4);

            var globalPath1 = GameController.ProjectLoc + split[1];
            var globalPath2 = GameController.ProjectLoc + split[2];


            Console.WriteLine("Reading line: " + line);
            Console.Write(File.Exists(globalPath1));
            Console.Write(split[2] == "null");
            Console.WriteLine(File.Exists(globalPath2));


            if(File.Exists(globalPath1) && (split[2] == "null" || File.Exists(globalPath2)))
            {
                string[] sSize = ["0", "0"];

                if(split.Length == 4) sSize = split[3].Split('.', 2);

                ImageData newImg = new(globalPath1, split[2] == "null" ? "null" : globalPath2, split[0], int.Parse(sSize[0]), int.Parse(sSize[1]));

                if(split.Length == 4)
                Console.WriteLine($"Loaded image at {split[1]}, with id {split[0]}, normal data at {split[2]}, and defined size at {split[3]}");
                else
                Console.WriteLine($"Loaded image at {split[1]}, with id {split[0]} abd normal data at {split[2]}");


                returnImages.Add(newImg);
            }
            else
            {
                if(!File.Exists(globalPath1))
                {
                    LogManager.LogError($"Did not find suitable image at {globalPath1}.");
                } else
                {
                    LogManager.LogError($"Did not find suitable normal map at {globalPath2}. If you did not intend to load a custom normal map for this image, please write the word 'null' at the 3rd argument, in the target .pfimg file ");
                }
            }


        }
        else
        LogManager.LogError("Not found any files when tried to load the Data file at: " + dataFilePath);


        return [.. returnImages];
    }



    public static void LoadAllTextures()
    {
        IsDirty = false;

        DefaultText ??= Raylib.LoadTextureFromImage(DefaultImg);



        foreach (var image in LoadedImages.Values)
        {
            image.loadedTexture = Raylib.LoadTexture(image.texturePath);

            if(image.hasNormal)
            {
                if(image.hasNormal) image.loadedNormal = Raylib.LoadTexture(image.textureNormalPath);
            }
            else image.loadedNormal = (Texture2D)DefaultText;
        }
    }


    // Be VERY carefull with unloadData bool, can cause memory leaks if you lose reference of the loaded images/Textures inside the ImageData
    public static void RemoveTexture(string key, bool unloadData = true)
    {
        if(unloadData)
        {
            
            if(LoadedImages[key].loadedTexture is Texture2D loadedTexture)
            Raylib.UnloadTexture(loadedTexture);
        }

        if(LoadedImages[key].hasNormal)        
        {
            Raylib.UnloadTexture(LoadedImages[key].loadedNormal);
        }
        
        LoadedImages.Remove(key);
    }


    public static void RemoveAllTextures()
    {
        foreach (var sprite in LoadedImages.ToList())
        {
            RemoveTexture(sprite.Key);
        }
    }

    public static void SaveTextureDataFile(Dictionary<string, ImageData> TextureList, string saveTextPath)
    {
        
        if(saveTextPath != "" && File.Exists(saveTextPath))
        {
            var wholeText = new StringBuilder();

            foreach (var image in TextureList)
            {
                wholeText.Append(image.Key);
                wholeText.Append(' ');
                
                var relPath = image.Value.texturePath.Remove(0, GameController.ProjectLoc.Length);
                wholeText.Append(relPath);
                wholeText.Append(' ');

                if(image.Value.hasNormal)
                {
                    var relNormPath = image.Value.textureNormalPath.Remove(0, GameController.ProjectLoc.Length);
                    wholeText.Append(relNormPath);
                } else
                {
                    wholeText.Append("null");
                }

                if(image.Value.SpriteSizeX != image.Value.TextSizeX || image.Value.SpriteSizeY != image.Value.TextSizeY)
                {
                    wholeText.Append(' ');

                    wholeText.Append(image.Value.SpriteSizeX);
                    wholeText.Append('.');
                    wholeText.Append(image.Value.SpriteSizeY);
                }


                wholeText.AppendLine();
            }

            File.WriteAllText(saveTextPath, wholeText.ToString());
            
            
            LogManager.LogSuccess("Wrote all the Image data to " + saveTextPath);
        } else
        LogManager.LogError("Could not find file to write into");
    }
}


public class ImageData
{
    public string ID;

    public string texturePath;
    public Texture2D loadedTexture;

    public string textureNormalPath;

    public Texture2D loadedNormal;

    public bool hasNormal = false;

    public int TextSizeX, TextSizeY;
    public int SpriteSizeX, SpriteSizeY;
    public Dictionary<ulong, Rectangle> SpriteRects;

    public ImageData(string path, string normalPath = "null", string id = "null", int spriteSizeX = 0, int spriteSizeY = 0)
    {
        texturePath = path;
        
        
        if(normalPath != "null")
        {
            hasNormal = true;
            textureNormalPath = normalPath;  
        } 
        else
        {
            hasNormal = false;
            textureNormalPath = "";
        }        
        if(SpriteManager.LoadedImages.ContainsKey(id) || id == "null")
        {
            if(id != "null") 
            {
                ID = GetNewID([.. SpriteManager.LoadedImages.Keys], id); 
            
                LogManager.LogError($"Trying to load a image with an already existing ID ({id}). Changing new Image ID to: {ID}");
            } else
            {
                ID = GetNewID([.. SpriteManager.LoadedImages.Keys]);
            }

        } else
        {
            ID = id;
        }

        SpriteManager.LoadedImages.Add(ID, this);

        

        loadedTexture = Raylib.LoadTexture(texturePath);
        if(hasNormal) loadedNormal = Raylib.LoadTexture(normalPath);

        TextSizeX = loadedTexture.Width;
        TextSizeY = loadedTexture.Height;

        if(spriteSizeX != 0 && spriteSizeY != 0)
        {
            SpriteSizeX = spriteSizeX;
            SpriteSizeY = spriteSizeY;
        } else
        {
            SpriteSizeX = TextSizeX;
            SpriteSizeY = TextSizeY;
        }
        

        LoadRectangles();
    }





    public string GetNewID(List<string> list, string initID = "")
    {
        ulong Lowest = 0;

        for (int i = 0; i < list.Count; i++)
        {
            if( list.Contains(initID + Lowest.ToString())) Lowest ++;
        }

        Console.WriteLine($"Assigned ID {Lowest} to ImageData with path reference: {texturePath}");


        return initID + Lowest.ToString();

    }

    public ulong LoadRectangles()
    {

        var RectList = new Dictionary<ulong, Rectangle>();
        
        int i = 0, v = 0;
        ulong count = 0;

        while((i + 1) * SpriteSizeX <= TextSizeX && (v + 1) * SpriteSizeY <= TextSizeY)
        {
            RectList.Add(count, new Rectangle( i * SpriteSizeX,  v * SpriteSizeY, SpriteSizeX, SpriteSizeY));

            i ++;

            if((i + 1) * SpriteSizeX > TextSizeX )
            {
                i = 0;
                v ++;
            }

            count ++;
        }


        Console.WriteLine($"Found {count} rectangles");

        SpriteRects = RectList;

        return count;
    }
}