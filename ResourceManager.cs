
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json;
using Raylib_cs;

namespace PF;

/// <summary>
/// Singleton class for loading Textures, Audio and Text from files.
/// </summary>
/// <remarks>
/// For loading directly from a file path, call <c>Init(nint, bool)</c> with <c>true</c> <br/>
/// Its source path is <c>Content/</c>.
/// </remarks>
public class ResourceManager : IDisposable
{
    public static LoadHelper Load
    {
        get
        {
            field ??= new();

            return field;
        }
    }

    public static SaveHelper Save
    {
        get
        {
            field ??= new();

            return field;
        }
    }

    public class LoadHelper
    {
        private readonly Dictionary<string, Texture2D> loadedTextures = [];
        private readonly Dictionary<string, Image> loadedImages = [];
        private readonly Dictionary<string, Wave> loadedAudios = [];
        private readonly Dictionary<string, string> loadedTexts = [];
        private nint renderer;

        private string fileDataPath = "data.pf";


        bool initiated = false;
        bool loadFromPath = false;


        /// <summary>
        /// Initializes the <c>ResourceManager</c> with a Renderer. Call this only once (or when switching renderers)
        /// </summary>
        /// <param name="rend">Renderer to use when creating textures</param>
        /// <param name="loadFromPath">Enable if you are using this without the engine, or reading the data from a file</param>
        public void Init(nint rend, bool loadFromPath)
        {
            renderer = rend;
            initiated = true;

            this.loadFromPath = loadFromPath;
        }

        #region Image
        public Image LoadImage(string name, string? basePath = null)
        {
            StreamReader d;

            if(loadFromPath)
            {    
                string gPath = Path.GetFullPath(name, Directory.GetCurrentDirectory());
                d = new StreamReader(gPath);
            }
            else
            {
                d = GetDataReader(basePath ?? fileDataPath, name);
            }

            MemoryStream m = new ();
            d.BaseStream.CopyTo(m);
            byte[] b = m.ToArray();

            var im = Raylib.LoadImageFromMemory(Path.GetExtension(name), b);
            
            loadedImages.Add(name, im);

            return im;
        }
        
        public Image GetImage(string name) => loadedImages.ContainsKey(name) ? loadedImages[name] : LoadImage(name);

        public void UnloadImage(string name)
        {
            Logger.Log("Unloaded Image with name: " + name);

            Raylib.UnloadImage(loadedImages[name]);
            loadedImages.Remove(name);
        }


        public void UnLoadAllImages()
        {
            foreach (var texture in loadedImages.Values)
            {
                Raylib.UnloadImage(texture);
            }

            loadedImages.Clear();
        }
        #endregion

        #region Texture
        public Texture2D LoadTextureFromImage(string name, bool deleteAfter = false)
        {
            Texture2D t = Raylib.LoadTextureFromImage(loadedImages[name]);

            loadedTextures.Add(name, t);

            if(deleteAfter)
                UnloadImage(name);

            return t;
        }
        public Texture2D LoadTextureFromImage(Image img, string? name = null)
        {
            Texture2D t = Raylib.LoadTextureFromImage(img);

            if(name != null) loadedTextures.Add(name, t);

            return t;
        }

        public Texture2D GetTexture(string name) => loadedTextures.ContainsKey(name) ? loadedTextures[name] : LoadTextureFromImage(GetImage(name), name);

        public void UnloadTexture(string name)
        {
            Logger.Log("Unloaded Texture with name: " + name);

            Raylib.UnloadTexture(loadedTextures[name]);
            loadedTextures.Remove(name);
        }

        public void UnLoadAllTextures()
        {
            foreach (var texture in loadedTextures.Values)
            {
                Raylib.UnloadTexture(texture);
            }

            loadedTextures.Clear();
        }
        #endregion
    
        #region Audio
        public Wave LoadAudio(string name, string? basePath = null)
        {

            StreamReader d;

            if(loadFromPath)
            {    
                string gPath = Path.GetFullPath(name, Directory.GetCurrentDirectory());
                d = new StreamReader(gPath);
            }
            else
            {
                d = GetDataReader(basePath ?? fileDataPath, name);
            }

            MemoryStream m = new ();
            d.BaseStream.CopyTo(m);
            byte[] b = m.ToArray();

            var au = Raylib.LoadWaveFromMemory(Path.GetExtension(name), b);
            
            loadedAudios.Add(name, au);

            return au;
        }

        public Wave GetAudio(string name) => loadedAudios.ContainsKey(name) ? loadedAudios[name] : LoadAudio(name);


        public void UnloadAudio(string name)
        {
            Logger.Log("Unloaded Audio with name: " + name);

            Raylib.UnloadWave(loadedAudios[name]);
            loadedAudios.Remove(name);
        }

        /// <summary>
        /// Frees all loaded SDL Audios. 
        /// </summary>
        public void UnLoadAllAudios()
        {
            foreach (var audio in loadedAudios.Values)
            {
                Raylib.UnloadWave(audio);
            }

            loadedAudios.Clear();
        }
        #endregion
        
        #region Sound
        #endregion
        /// <summary>
        /// Loads a text file into memory.
        /// </summary>
        /// <param name="name">Name to assign to the Text</param>
        /// <param name="dataFile"></param>
        /// <returns>Text that the loaded file contains.</returns>
        public string LoadText(string name, string? dataFile = null)
        {

            StreamReader d;
            if(loadFromPath)
            {    
                string gPath = Path.GetFullPath(name, Directory.GetCurrentDirectory()); 
                d = new StreamReader(gPath);
            }
            else
            {
                d = GetDataReader(dataFile ?? fileDataPath, name);
            }


            string result = d.ReadToEnd();

            loadedTexts.Add(name, result);

            return result;
        }

        /// <summary>
        /// Retrieves a text, or creates it if its not found.
        /// </summary>
        /// <param name="name">Name to search the text by or to assign when loading it, if not found</param>
        /// <returns>Text that the loaded file contains.</returns>
        public string GetText(string name) => loadedTexts.ContainsKey(name) ? loadedTexts[name] : LoadText(name);

        /// <summary>
        /// Unloads a text.
        /// </summary>
        /// <remarks>
        /// Its usually not needed, as string generally do not use much memory.
        /// </remarks>
        /// <param name="name">The text to unload</param>
        public void UnloadText(string name)
        {
            Logger.Log("Unloaded Text with name: " + name);

            loadedTexts.Remove(name);
        }

        /// <summary>
        /// Unloads all loaded text from memory
        /// </summary>
        /// <remarks>
        /// Its usually not needed, as string generally do not use much memory.
        /// </remarks>
        public void UnLoadAllTexts()
        {
            loadedTexts.Clear();
        }

        private StreamReader GetDataReader(string dataFile, string name)
        {
            var t = System.IO.Compression.ZipFile.OpenRead(dataFile).Entries.FirstOrDefault(res => res.Name.Equals(name, StringComparison.InvariantCulture))
                    ?? throw new FileNotFoundException($"Couldn't find saved data file {name} inside {dataFile} data file.");
            return new(t.Open());
        }
    }
    
    public class SaveHelper
    {
        JsonSerializerSettings settings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include
        };
        public async Task<string> SerializeData(object data, bool encode = false)
        {
            string serialized = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
            return encode ? EncodeData(serialized) : serialized;
        }


        MemoryStream saveVirtualFile
        {
            get
            {
                return field ?? throw new NullReferenceException("Tried to save data without first Calling 'InitializeSaveDataFile'. ");
            }
            set;
        }
        ZipArchive saveFile
        {
            get
            {
                return field ?? throw new NullReferenceException("Tried to save data without first Calling 'InitializeSaveDataFile'. ");
            }
            set;
        }

        public void InitializeSaveDataFile(out MemoryStream saveVirtualFile, out ZipArchive saveFile)
        {
            saveVirtualFile = new MemoryStream();
            saveFile = new ZipArchive(saveVirtualFile, ZipArchiveMode.Create, true);
        }
        public void InitializeSaveDataFile()
        {
            InitializeSaveDataFile(out MemoryStream saveVirtualFile, out ZipArchive saveFile);
            this.saveVirtualFile = saveVirtualFile;
            this.saveFile = saveFile;
        }

        public void AddDataToSaveFile(ZipArchive saveFile, string data, string name)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            using var file = new MemoryStream(bytes);

            var archiveEntry = saveFile.CreateEntry(name, CompressionLevel.Fastest);
            using var entryStream = archiveEntry.Open();
            file.CopyTo(entryStream);
        }

        public void AddDataToSaveFile(string data, string name)
        {
            AddDataToSaveFile(saveFile, data, name);
        }

        public async Task SerializeAndAddDataToSaveFile(ZipArchive saveFile, object data, string name, bool encode = false)
        {
            string serialized = await SerializeData(data);

            AddDataToSaveFile(saveFile, serialized, name);
        }

        public async Task SerializeAndAddDataToSaveFile(object data, string name, bool encode = false)
        {
            await SerializeAndAddDataToSaveFile(saveFile, data, name);   
        }


        public async Task WriteSaveFileToDisk(ZipArchive saveFile, MemoryStream saveVirtualFile, string destPath)
        {
            await saveFile.DisposeAsync();
            
            using var fileStream = new FileStream("data.pf", FileMode.Create, FileAccess.Write);
            saveVirtualFile.Seek(0, SeekOrigin.Begin);
            
            await saveVirtualFile.CopyToAsync(fileStream);
            
            await saveVirtualFile.DisposeAsync();

            Logger.Log($"Saved data to {Path.GetFullPath(destPath)} successfully.");
        }

        public async Task WriteSaveFileToDisk(string destPath)
        {
            await WriteSaveFileToDisk(saveFile, saveVirtualFile, destPath);
        }

        public static string EncodeData(string data)
        {
            var bytes = data.GetUTF8Bytes();

            var s = Convert.ToBase64String(bytes);
            
            var b = Encoding.UTF8.GetBytes(s);

            var nb = new byte[b.Length];

            for (int i = 0; i < b.Length; i++)
            {
                nb[i] = b[b.Length -i -1];
            }
            
            
            return Encoding.UTF8.GetString(nb);
        }

        public static string DeEncodeData(string data)
        {
            var b = data.GetUTF8Bytes();

            var nb = new byte[b.Length];

            for (int i = 0; i < b.Length; i++)
            {
                nb[i] = b[b.Length -i -1];
            }


            var s = Convert.FromBase64String(Encoding.UTF8.GetString(nb));
            
            
            return Encoding.UTF8.GetString(s);
        }
    }
    
    private bool disposed;

    /// <summary>
    /// Unloads all the data loaded from this manager.
    /// </summary>
    public void Dispose()
    {
        if(!disposed)
        {
            disposed = true;

            Load.UnLoadAllAudios();
            Load.UnLoadAllTextures();
        }
    }

    ~ResourceManager()
    {
        Dispose();
    }
}