

using System.IO;
using System.Text;

using Raylib_cs;

namespace PatoframeWork.Rendering;

/// <summary>
/// Main class to handle Sprite Loading and Unloading Logic;
/// </summary>
public static class SpriteManager
{
    public static Dictionary<ulong, ImageData> LoadedImages { get; private set; } = [];

    public static bool IsDirty{ get; private set; } = false;


    public static Image DefaultImg { get; set; } = Raylib.GenImageColor(1, 1, new Color(128, 128, 255));

    public static Texture2D? DefaultText { get; private set; }


    /// <summary>
    /// Searches a .pfdata file in <paramref name="folderPath"/>, and loads its data.
    /// </summary>
    
    public static void LoadTextureFolder(string folderPath, bool cleanUp = true)
    {

        if(cleanUp && LoadedImages.Count > 0) UnloadAllImages();

        IsDirty = true;



        var Files = Directory.GetFiles(GameController.ProjectLoc + folderPath);
        
        Console.WriteLine($"Reading dir: {GameController.ProjectLoc + folderPath} . Detected {Files.Length} files");

        if(Files.Length > 0)
        {
            foreach (var file in Files.Where(res => Path.GetExtension(res) == ".pfdata"))
            {
                var textLine =  File.ReadAllLines(file);
                
                foreach (var line in textLine)
                {
                    string[] contents = line.Split(" ", 4);

                    string imagePath = GameController.ProjectLoc + contents[0];

                    var Texture = Raylib.LoadImage(imagePath);

                    string normalPath = GameController.ProjectLoc + contents[1];

                    Console.Write($"Image at: {contents[0]}, with attached Normal Image at: {contents[1]} --");

                    Image Normal;

                    bool hasNormal = !String.IsNullOrWhiteSpace(normalPath) && contents[1] != "null";
                    
                    if(hasNormal)
                        Normal = Raylib.LoadImage(normalPath);
                    else
                        Normal = DefaultImg;

                    string[] tSize = contents[2].Split(".", 2);
                    int tSizeX = Int32.Parse(tSize[0]);
                    int tSizeY = Int32.Parse(tSize[1]);


                    string[] sSize = contents[3].Split(".", 2);
                    int sSizeX = Int32.Parse(sSize[0]);
                    int sSizeY = Int32.Parse(sSize[1]);

                    var RectList = new Dictionary<ulong, Rectangle>();
                    
                    int i = 0, v = 0;
                    ulong count = 0;

                    while((i + 1) * sSizeX <= tSizeX && (v + 1) * sSizeY <= tSizeY)
                    {
                        RectList.Add(count, new Rectangle( i * sSizeX,  v * sSizeY, sSizeX, sSizeY));

                        i ++;

                        if((i + 1) * sSizeX > tSizeX )
                        {
                            i = 0;
                            v ++;
                        }

                        count ++;
                    }
                    

                    Console.WriteLine($"Found {count} rectangles");



                    var newT = new ImageData(Texture, tSizeX, tSizeY, RectList);
        
                    LoadedImages.Add(ImageData.GetLowestID([.. LoadedImages.Keys]), newT);

                    if(hasNormal)
                    {
                        newT.imageNormal = Normal;
                    } else
                    {
                        newT.imageNormal = null;
                    }

                    newT.hasNormal = hasNormal;


                }
            }
        }
        else
        ErrorManager.LogError("Not found any files when tried to load the Texture Folder");
    }



    public static void LoadAllTextures()
    {
        IsDirty = false;

        DefaultText ??= Raylib.LoadTextureFromImage(DefaultImg);



        foreach (var image in LoadedImages.Values)
        {
            if(image.loadedTexture != null)
            Raylib.LoadTextureFromImage(image.image);

            if(image.loadedNormal != null && image.imageNormal is Image normalToLoad)
            {
                if(image.hasNormal) image.loadedNormal = Raylib.LoadTextureFromImage(normalToLoad);
                else image.loadedNormal = DefaultText;
            }
        }
    }


    public static void UnloadImage(ulong key)
    {
        Raylib.UnloadImage(LoadedImages[key].image);

        if(LoadedImages[key].loadedTexture is Texture2D loadedTexture)
        Raylib.UnloadTexture(loadedTexture);

        if(LoadedImages[key].imageNormal != null && LoadedImages[key].loadedNormal is Texture2D loadedNormal)
        Raylib.UnloadTexture(loadedNormal);

        LoadedImages.Remove(key);
    }


    public static void UnloadAllImages()
    {
        foreach (var sprite in LoadedImages.ToList())
        {
            UnloadImage(sprite.Key);
        }
    }
}


public class ImageData(Image newImage, int SizeX, int SizeY,Dictionary<ulong, Rectangle> SpriteRectangles)
{
    public Image image = newImage;

    public Texture2D? loadedTexture = null;

    public Image? imageNormal = null;

    public Texture2D? loadedNormal = null;

    public bool hasNormal = true;

    public int TextSizeX = SizeX, TextSizeY = SizeY;
    public Dictionary<ulong, Rectangle> SpriteRects = SpriteRectangles; 

    public static ulong GetLowestID(List<ulong> list)
    {
        ulong Lowest = 0;

        List<ulong> List = [.. list.Order()];

        for (int i = 0; i < List.Count; i++)
        {
            if(Lowest == List[i]) Lowest ++;
        }

        Console.WriteLine($"With ID: {Lowest}");


        return Lowest;

    }
}