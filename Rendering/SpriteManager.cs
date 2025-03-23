

using System.IO;
using System.Text;

using Raylib_cs;

namespace PatoframeWork.Rendering;

public static class SpriteManager
{

    public static Dictionary<ulong, ImageData> LoadedImages = [];

    public static bool isDirty = false;

    public static void LoadTextureFolder(string folderPath)
    {
        isDirty = true;

        var Files = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPath));
        
        Console.WriteLine("Reading dir: " + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPath) + " -- " + Files.Length);

        if(Files.Length > 0)
        {
            foreach (var file in Files.Where(res => Path.GetExtension(res) == ".pfdata"))
            {
                var textLine =  File.ReadAllLines(Path.GetFullPath(file)); 
                
                foreach (var line in textLine)
                {
                    string[] contents = line.Split(" ", 3);

                    string imagePath = contents[0];

                    var Texture = Raylib.LoadImage(imagePath);
                    
                    string[] tSize = contents[1].Split(".", 2);
                    int tSizeX = Int32.Parse(tSize[0]);
                    int tSizeY = Int32.Parse(tSize[1]);

                    string[] sSize = contents[2].Split(".", 2);
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
        
                    LoadedImages.Add(ImageData.GetLowestID(LoadedImages.Keys.ToList()), newT);

                    //Console.WriteLine($"{LoadedImages[0]} images loaded");


                }
            }
        }
        else
        ErrorManager.LogError("Not found any files when tried to load the Texture Folder");
    }



    public static void LoadAllTextures()
    {
        isDirty = false;


        foreach (var image in LoadedImages.Values)
        {
            if(image.loadedTexture != null)
            Raylib.LoadTextureFromImage(image.image);
        }
    }


    public static void UnloadImage(ulong key)
    {
        Raylib.UnloadImage(LoadedImages[key].image);

        if(LoadedImages[key].loadedTexture is Texture2D loadedText)
        Raylib.UnloadTexture(loadedText);

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

    public int TextSizeX = SizeX, TextSizeY = SizeY;
    public Dictionary<ulong, Rectangle> SpriteRects = SpriteRectangles; 

    public static ulong GetLowestID(List<ulong> list)
    {
        ulong Lowest = 0;

        List<ulong> List = list.Order().ToList();

        for (int i = 0; i < List.Count; i++)
        {
            if(Lowest == List[i]) Lowest ++;
        }

        Console.WriteLine($"Returned ID: {Lowest}");


        return Lowest;

    }
}