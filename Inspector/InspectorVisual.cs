using System.Numerics;
using System.Reflection;


using PatoframeWork;
using PatoframeWork.Rendering;

using ImGuiNET;
using Raylib_cs;
using System.Collections;
using PatoframeWork.Inspector;
using rlImGui_cs;
using System.Text;
using Newtonsoft.Json;

namespace Patoframework.Inspector;
using static ImGui;

public static class InspectorVisual
{

#if DEBUG

    static bool DEBUG_ColorStyle = false;

    static Entity? DEBUG_Selected;

    static string SaveLocation = GameController.ProjectLoc;
    static string SaveName = "SaveData 1";
    static bool OpenFileSelector = false;
    static bool SaveFileSelector = false;
    static bool ShowFileImporter = false;

    //Sprite Manager imports
    static string saveTextPath = "";
    static Dictionary<string, ImageData> DroppedImages = [];

    static Vector2 test = Vector2.Zero;

    static string searchEnding = "";
    static int searchMode = 0;

    


    public static void ImGUIBeh()
    {
        
        LoadPopups();
        ShowDemoWindow();

        // Draw Main Menu
        if (BeginMainMenuBar())
        {
            // FILE

            if (BeginMenu("File"))
            {
                if (MenuItem("New")) { }
                if (MenuItem("Open"))
                {
                    if (!IsPopupOpen("Open File"))
                    {
                        SaveLocation = GameController.ProjectLoc;

                        if (string.IsNullOrEmpty(SaveLocation))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }

                        searchEnding = ".json";
                        searchMode = 0;
                        
                        OpenFileSelector = true;
                    }
                }
                if (BeginMenu("Open Recent"))
                {

                    // TODO

                    EndMenu();
                }

                Separator();

                if (MenuItem("Save"))
                {
                    if (File.Exists(GameController.SaveLocation))
                    {
                        SaveLocation = GameController.ProjectLoc;

                        if (string.IsNullOrEmpty(SaveLocation))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }

                        searchEnding = ".json";
                        searchMode = 0;

                        SaveFileSelector = true;

                    }
                    else
                    {
                        
                        GameController.SaveScene(SaveName);
                    }
                }

                if (MenuItem("Save As"))
                {
                    SaveLocation = GameController.ProjectLoc;

                    if (string.IsNullOrEmpty(SaveLocation))
                    {
                        throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                    }

                    searchEnding = ".json";
                    searchMode = 0;

                    SaveFileSelector = true;
                }

                EndMenu();
            }

            // TOOL

            if (BeginMenu("Tool"))
            {
                if (BeginMenu("Texture Utilities"))
                {
                    if(MenuItem("Open Sprite Importer"))
                    {
                        ShowFileImporter = true;
                        if(!Path.Exists(GameController.ProjectLoc + "/Resources/Images/ImageData.pfimg"))
                        {
                            File.Create(GameController.ProjectLoc + "/Resources/Images/ImageData.pfimg");
                        }
                    }

                    Separator();

                    if(MenuItem("Load Textures from file"))
                    {
                        searchMode = 1;
                        searchEnding = ".pfimg";

                        SaveLocation = GameController.ProjectLoc;

                        if (string.IsNullOrEmpty(SaveLocation))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }

                        OpenFileSelector = true;
                    }


                    if(MenuItem("Save Textures to file"))
                    {
                        searchMode = 1;
                        searchEnding = ".pfimg";

                        SaveLocation = GameController.ProjectLoc;

                        if (string.IsNullOrEmpty(SaveLocation))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }

                        SaveFileSelector = true;
                    }

                    Separator();

                    if(MenuItem("Unload all textures"))
                    {
                        SpriteManager.RemoveAllTextures();
                    }

                    EndMenu();
                }

                EndMenu();
            }

            // VIEW / MISC

            if (BeginMenu("View"))
            {
                BeginGroup();


                if (Checkbox("Toggle Dark Mode", ref DEBUG_ColorStyle))
                {

                    if (DEBUG_ColorStyle)
                    {
                        StyleColorsLight();
                    }
                    else
                    {
                        StyleColorsDark();
                    }
                }


                EndGroup();
                Separator();

                if (MenuItem("Reset Camera"))
                {
                    CameraManager.I.ResetCamera();
                }

                EndMenu();
            }

            EndMainMenuBar();

        }


        if (OpenFileSelector) OpenPopup("Open File");

        if (SaveFileSelector) OpenPopup("Save File");


        DrawEntityInspector();
        
        SetNextWindowSize(Vector2.One * 300, ImGuiCond.FirstUseEver);
        SetNextWindowPos(Vector2.One * 100, ImGuiCond.FirstUseEver);
        
        Begin("Entity Info");

        if (DEBUG_Selected != null)
        {
            DrawEntityInfo(DEBUG_Selected);

            SeparatorText("");

            DrawBehaviourSelector();
        }
        End();

        
        
        if(ShowFileImporter) DrawFileImporter();

        DrawLoadedImageList();

        DrawSavedEntities();
        
    }

    #region Draw Entity Info

    // Draw Properties Inspector
    static void DrawEntityInfo(Entity entity)
    {
        Text("Entity Name");
        if (entity.Active)
        {
            if (InputText("###EntityName", ref entity.Name, 50))
            {
                if (entity.Name == "") entity.Name = " ";
            }
        }
        else TextDisabled(entity.Name);

        Dummy(Vector2.One * 15);

        PropertyInfo[] propertyInfos = typeof(Entity).GetProperties();
        FieldInfo[] fieldInfos = typeof(Entity).GetFields();

        DrawPropertiesAndFields(entity, null, propertyInfos, fieldInfos, -1);

        if (entity.Behaviours.Count > 0)
        {
            Dummy(Vector2.One * 20);
            SeparatorText("Properties");

            int behC = 0;
            foreach (var beh in entity.Behaviours.ToList())
            {
                //TextColored(new Vector4(Color.Gold.R, Color.Gold.G, Color.Gold.B, Color.Gold.A) / 255, beh.GetType().Name);

                PushStyleColor(ImGuiCol.Text, Raylib.ColorNormalize(Color.Gold));
                Text(beh.GetType().Name);
                PopStyleColor();

                PushStyleColor(ImGuiCol.ChildBg, Raylib.ColorNormalize(Color.DarkGray));




                PropertyInfo[] behPropertyInfos = beh.GetType().GetProperties();
                FieldInfo[] behFieldInfos = beh.GetType().GetFields();


                BeginChild("ContextMenuTXT###" + behC, Vector2.UnitY * (55 * (behPropertyInfos.Length + behPropertyInfos.Length)) - Vector2.UnitX * 40, ImGuiChildFlags.FrameStyle | ImGuiChildFlags.AlwaysAutoResize | ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY);
                BeginGroup();

                DrawPropertiesAndFields(null, beh, behPropertyInfos, behFieldInfos, behC);

                EndGroup();
                EndChild();

                PopStyleColor();

                if (BeginPopupContextItem("ContextMenu###" + behC))
                {
                    if (MenuItem("Duplicate"))
                    {
                        beh.CloneBehaviour();
                    }

                    if (MenuItem("Delete"))
                    {
                        beh.RemoveBehaviour();
                    }

                    EndMenu();
                }

                Separator();

                behC++;
            }
        }
    }


    static void DrawEntityInspector()
    {

        SetNextWindowSize(Vector2.One * 300, ImGuiCond.FirstUseEver);
        SetNextWindowPos(Vector2.One * 100 + Vector2.UnitX * 400, ImGuiCond.FirstUseEver);

        Begin("Entities");

        PushStyleColor(ImGuiCol.Button, Raylib.ColorNormalize(Color.DarkGreen));
        if (SmallButton("Create Entity"))
        {
            var t = new Entity("New Entity");
            DEBUG_Selected = t;

            t.GlobalPosition = CameraManager.I.Cam.Target;
        }
        PopStyleColor();

        SeparatorText("Entities -- ");
        BeginChild("EntititesList", new Vector2(-1, -1), ImGuiChildFlags.AlwaysAutoResize | ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY);

        foreach (var entity in GameController.GetAllEntities().Where((res) => res.Parent == 0))
        {
            DrawRecursiveList(entity);
        }
        EndChild();

        if(BeginDragDropTarget())
        {
            var data = AcceptDragDropPayload("EntityID");

            unsafe
            {
                if(data.NativePtr != null)
                {
                    ulong dataPtr = ((ulong*)data.Data.ToPointer())[0];

                    GameController.FindEntity(dataPtr).SetParent(0);
                }
            }
            
            EndDragDropTarget();
        }

        End();

    }

    static object?[] GetOrderedArray(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos)
    {
        var L = new ArrayList(propertyInfos);
        L.AddRange(fieldInfos);

        var Ordererd = L.ToArray().OrderByDescending(res =>
        {
            if (res is PropertyInfo pI) return pI.GetCustomAttribute<InspectorShowOrderAttribute>(false)?.order ?? 0;
            if (res is FieldInfo fI) return fI.GetCustomAttribute<InspectorShowOrderAttribute>(false)?.order ?? 0;

            return 0;
        });

        return [.. Ordererd];
    }


    #region Draw Properties


    // Draw Individual Properties
    static void DrawPropertiesAndFields(Entity? entity, Behaviour? beh, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, int BehaviourNumber)
    {
        var Ordered = GetOrderedArray(propertyInfos, fieldInfos);

        for (int i = 0; i < Ordered.Length; i++)
        {
            if (Ordered[i] is PropertyInfo pI) DrawProperty(pI, entity, beh, i, BehaviourNumber);
            else if (Ordered[i] is FieldInfo fI) DrawField(fI, entity, beh, i, BehaviourNumber);
        }

    }


    static void DrawBehaviourSelector()
    {
        var possibleBehaviours = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.IsSubclassOf(typeof(Behaviour))).ToList();


        if (BeginCombo("Add Behaviours", "", ImGuiComboFlags.PopupAlignLeft))
        {

            for (int n = 0; n < possibleBehaviours.Count; n++)
            {
                if (Selectable(possibleBehaviours[n].Name, false))
                {
                    DEBUG_Selected?.AddBehaviour(possibleBehaviours[n]);
                }
            }

            EndCombo();
        }

    }


    static void DrawProperty(PropertyInfo property, Entity? entity, Behaviour? beh, int IdNumber, int BehaviourNumber)
    {
        if (property.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0) return;


        var res = property.GetValue(entity == null ? beh : entity);


        if (property.GetCustomAttributes(typeof(InspectorHideNullAttribute), false).Length > 0
        && res == null) return;

        if (property.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0)
        {
            Text(property.Name);
            SameLine();

            if (res is int intR)
            {
                if (InputInt("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref intR))
                {
                    property.SetValue(entity == null ? beh : entity, intR);
                }

            }
            else if (res is bool boolR)
            {
                if (Checkbox("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref boolR))
                {
                    property.SetValue(entity == null ? beh : entity, boolR);
                }

            }
            else if (res is Vector2 v2)
            {
                if (DragFloat2("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref v2))
                {
                    property.SetValue(entity == null ? beh : entity, v2);
                }

            }
            else if (res is string str)
            {
                if (InputText("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref str, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    property.SetValue(entity == null ? beh : entity, str);
                }

            }
            else if (res is Enum newEnm)
            {

                if (BeginCombo("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), property.GetValue(entity == null ? beh : entity)?.ToString()))
                {
                    foreach (var item in Enum.GetValues(newEnm.GetType()))
                    {
                        if (Selectable(item.ToString(), item == property.GetValue(entity == null ? beh : entity)))
                        {
                            property.SetValue(entity == null ? beh : entity, item);
                        }
                    }

                    EndCombo();
                }

            }
            else if (res is ulong newUl)
            {
                int newIntC = Convert.ToInt32(newUl);

                if (InputInt("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref newIntC, 0))
                {
                    if (newIntC < 0) newIntC = 0;

                    if (property.Name == "Parent" && entity != null)
                        entity.SetParent(Convert.ToUInt64(newIntC));
                    else
                        property.SetValue(entity == null ? beh : entity, Convert.ToUInt64(newIntC));
                }

            }
            else
            {
                SameLine();
                Text(res?.ToString() ?? "Null");
            }

        }
        else
        {
            SameLine();
            Text(res?.ToString() ?? "Null");
        }

        if(property.GetCustomAttributes(typeof(InspectorReceiveDropAttribute), false).Length > 0 && 
        property.GetCustomAttributes(typeof(InspectorReceiveDropAttribute), false)[0] is InspectorReceiveDropAttribute insAttrib)
        {
            if(BeginDragDropTarget())
            {

                var data = AcceptDragDropPayload(insAttrib.receiveData);


                unsafe
                {
                    try
                    {
                        if(data.NativePtr != null)
                        {
                            MethodInfo? method = typeof(InspectorVisual)?.GetMethod(nameof(AssignPropertiesData))?.MakeGenericMethod(property.PropertyType);
                            
                            object?[] parms = [data, property, entity == null ? beh : entity];
                            method?.Invoke(null, parms);
                        }
                    }
                    catch
                    {
                        LogManager.LogError("Could not convert data to " + property.PropertyType.Name);
                    }

                }
                

                EndDragDropTarget();
            }
        }
    }


    public static unsafe void AssignPropertiesData<T>(ImGuiPayloadPtr data, PropertyInfo property, object? target) where T : class
    {
        
        T dataPtr = ((T*)data.Data.ToPointer())[0];

        property.SetValue(target, dataPtr);
    
    }

    public static unsafe void AssignFieldsData<T>(ImGuiPayloadPtr data, FieldInfo property, object? target) where T : class
    {
        
        T dataPtr = ((T*)data.Data.ToPointer())[0];

        property.SetValue(target, dataPtr);
    
    }
    static void DrawField(FieldInfo property, Entity? entity, Behaviour? beh, int IdNumber, int BehaviourNumber)
    {
        if (property.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0) return;


        var res = property.GetValue(entity == null ? beh : entity);


        if (property.GetCustomAttributes(typeof(InspectorHideNullAttribute), false).Length > 0
        && res == null) return;

        if (property.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0)
        {
            Text(property.Name);

            if (res is int intR)
            {
                if (InputInt("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref intR))
                {
                    property.SetValue(entity == null ? beh : entity, intR);
                }

            }
            else if (res is bool boolR)
            {
                if (Checkbox("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref boolR))
                {
                    property.SetValue(entity == null ? beh : entity, boolR);
                }

            }
            else if (res is Vector2 v2)
            {
                if (DragFloat2("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref v2))
                {
                    property.SetValue(entity == null ? beh : entity, v2);
                }

            }
            else if (res is string str)
            {
                if (InputText("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref str, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    property.SetValue(entity == null ? beh : entity, str);
                }

            }
            else if (res is Color col)
            {
                Vector4 newCol = Raylib.ColorNormalize(col);
                if (ColorEdit4("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref newCol))
                {
                    property.SetValue(entity == null ? beh : entity, Raylib.ColorFromNormalized(newCol));
                }

            }
            else if (res is float newF)
            {
                if (DragFloat("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref newF))
                {
                    property.SetValue(entity == null ? beh : entity, newF);
                }

            }
            else if (res is Enum newEnm)
            {

                if (BeginCombo("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), property.GetValue(entity == null ? beh : entity)?.ToString()))
                {
                    foreach (var item in Enum.GetValues(newEnm.GetType()))
                    {
                        if (Selectable(item.ToString(), item == property.GetValue(entity == null ? beh : entity)))
                        {
                            property.SetValue(entity == null ? beh : entity, item);
                        }
                    }

                    EndCombo();
                }

            }
            else if (res is ulong newUl)
            {
                int newIntC = Convert.ToInt32(newUl);

                if (InputInt("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref newIntC, 0))
                {
                    if (newIntC < 0) newIntC = 0;

                    if (property.Name == "Parent" && entity != null)
                        entity.SetParent(Convert.ToUInt64(newIntC));
                    else
                        property.SetValue(entity == null ? beh : entity, Convert.ToUInt64(newIntC));
                }

            }
            else
            {
                SameLine();
                Text(res?.ToString() ?? "Null");
            }

        }
        else
        {
            SameLine();
            Text(res?.ToString() ?? "Null");
        }

        if(property.GetCustomAttributes(typeof(InspectorReceiveDropAttribute), false).Length > 0 && 
        property.GetCustomAttributes(typeof(InspectorReceiveDropAttribute), false)[0] is InspectorReceiveDropAttribute insAttrib)
        {
            if(BeginDragDropTarget())
            {

                var data = AcceptDragDropPayload(insAttrib.receiveData);


                unsafe
                {
                    try
                    {
                        if(data.NativePtr != null)
                        {
                            MethodInfo? method = typeof(InspectorVisual)?.GetMethod(nameof(AssignFieldsData))?.MakeGenericMethod(property.FieldType);;
                            
                            object?[] parms = [data, property, entity == null ? beh : entity];
                            method?.Invoke(null, parms);
                        }
                    }
                    catch
                    {
                        LogManager.LogError("Could not convert data to " + property.FieldType.Name);
                    }

                }
                

                EndDragDropTarget();
            }
        }

    }

    #endregion
    #endregion

    // Draw Entity List
    static void DrawRecursiveList(Entity REntity)
    {
        PushID(REntity.ID.ToString());
        if (REntity.Childs.Count > 0)
        {
            if (TreeNodeEx($"{REntity.Name}###{REntity.ID}", (DEBUG_Selected == REntity ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (IsItemClicked(ImGuiMouseButton.Left)) DEBUG_Selected = REntity;

                if (BeginPopupContextItem("ContextMenu###" + REntity.ID))
                {
                    if (MenuItem("Duplicate"))
                    {
                        var ent = REntity.Duplicate();

                        DEBUG_Selected = ent;
                    }

                    if (MenuItem("Delete"))
                    {
                        REntity.Delete();
                        DEBUG_Selected = null;
                    }

                    if (MenuItem("Delete -- Chain"))
                    {
                        REntity.Delete(true, true);
                        DEBUG_Selected = null;
                    }


                    EndPopup();
                }

                if (BeginDragDropSource())
                {
                    unsafe
                    {
                        ulong data = REntity.ID;
                        SetDragDropPayload("EntityID", (IntPtr)(&data), sizeof(ulong));
                    }
                    
                    Text(Path.GetFileName("Entity ID: " + REntity.ID));

                    EndDragDropSource();
                }

                if(BeginDragDropTarget())
                {
                    var data = AcceptDragDropPayload("EntityID");

                    unsafe
                    {
                        if(data.NativePtr != null)
                        {
                            ulong dataPtr = ((ulong*)data.Data.ToPointer())[0];

                            GameController.FindEntity(dataPtr).SetParent(REntity);
                        }

                    }
                    

                    EndDragDropTarget();
                }

                foreach (var ch in REntity.Childs.ToList())
                {
                    DrawRecursiveList(GameController.FindEntity(ch));
                }

                TreePop();
            }
            else
            {
                if (IsItemClicked(ImGuiMouseButton.Left)) DEBUG_Selected = REntity;

                if (BeginPopupContextItem("ContextMenu###" + REntity.ID))
                {
                    if (MenuItem("Duplicate"))
                    {
                        REntity.Duplicate();
                    }

                    if (MenuItem("Delete"))
                    {
                        REntity.Delete();
                    }

                    if (MenuItem("Delete -- Chain"))
                    {
                        REntity.Delete(true, true);
                    }


                    EndPopup();
                }

                if (BeginDragDropSource())
                {
                    unsafe
                    {
                        ulong data = REntity.ID;
                        SetDragDropPayload("EntityID", (IntPtr)(&data), sizeof(ulong));
                    }
                    
                    Text(Path.GetFileName("Entity ID: " + REntity.ID));

                    EndDragDropSource();
                }

                if(BeginDragDropTarget())
                {
                    var data = AcceptDragDropPayload("EntityID");

                    unsafe
                    {
                        if(data.NativePtr != null)
                        {
                            ulong dataPtr = ((ulong*)data.Data.ToPointer())[0];

                            GameController.FindEntity(dataPtr).SetParent(REntity);
                        }

                    }
                    

                    EndDragDropTarget();
                }
            }

            

        }
        else
        {
            if (TreeNodeEx($"{REntity.Name}###{REntity.ID}", (DEBUG_Selected == REntity ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanFullWidth))
            {
                if (IsItemClicked(ImGuiMouseButton.Left)) DEBUG_Selected = REntity;

                if (BeginPopupContextItem("ContextMenu###" + REntity.ID))
                {
                    if (MenuItem("Duplicate"))
                    {
                        REntity.Duplicate();
                    }

                    if (MenuItem("Delete"))
                    {
                        REntity.Delete();
                        DEBUG_Selected = null;
                    }

                    if (MenuItem("Delete -- Chain"))
                    {
                        REntity.Delete(true, true);
                        DEBUG_Selected = null;
                    }


                    EndPopup();
                }


                TreePop();
            }

            if (BeginDragDropSource())
            {
                unsafe
                {
                    ulong data = REntity.ID;
                    SetDragDropPayload("EntityID", (IntPtr)(&data), sizeof(ulong));
                }
                
                Text(Path.GetFileName("Entity ID: " + REntity.ID));

                EndDragDropSource();
            }

            if(BeginDragDropTarget())
            {
                var data = AcceptDragDropPayload("EntityID");

                unsafe
                {
                    if(data.NativePtr != null)
                    {
                        ulong dataPtr = ((ulong*)data.Data.ToPointer())[0];

                        GameController.FindEntity(dataPtr).SetParent(REntity);
                    }

                }
                

                EndDragDropTarget();
            }
        }

        

        PopID();
    }


    #region Draw Selectors
    // Draw File Selector (Choose only folder /// Choose only Json)
    static bool DrawFileSelector(string? fileType, ref string FilePath)
    {
        BeginChild("FileInspector", new Vector2(-30, -80), ImGuiChildFlags.AlwaysAutoResize | ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.Borders);
        
        List<string> files = [.. Directory.GetFileSystemEntries(SaveLocation).OrderBy(res => !Directory.Exists(res))];

        if (files.Count > 0)
        {            
            foreach (var file in files)
            {
                bool isDirectory = Directory.Exists(file);

                // Different padding for directories and regular files
                if (isDirectory) Dummy(Vector2.One * 25);
                else Dummy(Vector2.One * 25 + Vector2.UnitX * 15);
                SameLine();

                //Diferent colors based on the type of file we are searching for
                if (isDirectory) PushStyleColor(ImGuiCol.Button, ColorConvertFloat4ToU32(Raylib.ColorNormalize(Color.DarkBlue)));
                else if (fileType != null && Path.GetExtension(file) == fileType) PushStyleColor(ImGuiCol.Button, ColorConvertFloat4ToU32(Raylib.ColorNormalize(Color.Blue)));
                else PushStyleColor(ImGuiCol.Button, ColorConvertFloat4ToU32(Raylib.ColorNormalize(Color.Gray)));
                   
                
                //On clicked File/Directory
                if (Button(Path.GetFileName(file)))
                {
                    

                    if (!isDirectory && fileType == Path.GetExtension(file))
                    {
                        FilePath = Path.GetFullPath(file);
                        PopStyleColor();
                        EndChild();

                        return true;
                    } else if(isDirectory)
                    {
                        FilePath = Path.GetFullPath(file);
                    }
                }
                PopStyleColor();
            }
        }
        

        EndChild();

        if (fileType == null)
        {
            Separator();

            Dummy(Vector2.One * 10 + Vector2.UnitX * (GetWindowWidth() - 150));
            SameLine();
            BeginGroup();
            if (Button("Select folder"))
            {
                EndGroup();

                return true;
            }
            EndGroup();
        }

        return false;
    }

    #endregion
    static void LoadPopups()
    {
        if (BeginPopup("Cant be empty", ImGuiWindowFlags.ChildMenu))
        {
            SetItemDefaultFocus();
            BeginGroup();
            if (Button("OK"))
            {
                CloseCurrentPopup();
            }
            EndGroup();

            EndPopup();
        }

        if(OpenFilePopup())
        {
            if(searchMode == 0)
            {
                GameController.LoadScene(SaveLocation);
            }
            else
            if(searchMode == 1)
            {
                SpriteManager.LoadTexturesFromDataFile(SaveLocation, false);
            }
        }

        if(SaveFilePopup())
        {
            if(searchMode == 0)
            {
                GameController.SaveScene(SaveName, SaveLocation);
            }
            else
            if(searchMode == 1)
            {
                var newFile = File.Create(SaveLocation + "/" + SaveName + ".pfimg");
                newFile.Close();

                SpriteManager.SaveTextureDataFile(SpriteManager.LoadedImages, SaveLocation + "/" + SaveName + ".pfimg");
            }
        }
    }

    
    static bool OpenFilePopup()
    {
        bool selected = false;

        Vector2 center = new(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2);
        SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        SetNextWindowSize(Vector2.One * 500 + Vector2.UnitX * 300, ImGuiCond.Appearing);

        if (BeginPopupModal("Open File", ref OpenFileSelector))
        {
            SetItemDefaultFocus();

            BeginGroup();

            if (ArrowButton("###ArrowB", ImGuiDir.Up))
            {
                if (SaveLocation != null && Directory.GetParent(SaveLocation) is DirectoryInfo info)
                {
                    if (info != null) SaveLocation = info.FullName;
                }
            }

            SameLine();
            if (InputText("Select Path", ref SaveLocation, 100))
            {
                //
            }

            EndGroup();

            SeparatorText("Files:");

            BeginGroup();

            if (DrawFileSelector(searchEnding, ref SaveLocation))
            {   
                OpenFileSelector = false;
                selected = true;
            }

            EndGroup();

            EndPopup();
        }

        return selected;
    }

    static bool SaveFilePopup()
    {
        bool selected = false;

        Vector2 center = new(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2);
        SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        SetNextWindowSize(Vector2.One * 500 + Vector2.UnitX * 300, ImGuiCond.Appearing);

        // Save file popup
        SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        SetNextWindowSize(Vector2.One * 500 + Vector2.UnitX * 300, ImGuiCond.Appearing);

        if (BeginPopupModal("Save File", ref SaveFileSelector))
        {
            SetItemDefaultFocus();

            BeginGroup();

            if (ArrowButton("###ArrowB", ImGuiDir.Up))
            {
                SaveLocation ??= "./";

                if (Directory.GetParent(SaveLocation) is DirectoryInfo info)
                {
                    if (info != null) SaveLocation = info.FullName;
                }
            }

            SameLine();
            if (InputText("Select Path", ref SaveLocation, 100))
            {
                //
            }
            Dummy(Vector2.One * 30);
            SameLine();
            if (InputText("File Name", ref SaveName, 40))
            {
                if(SaveName == "") SaveName = "Data";
            }

            Dummy(Vector2.One * 30);


            EndGroup();

            SeparatorText("Files:");

            if (DrawFileSelector(null, ref SaveLocation))
            {
                selected = true;
                SaveFileSelector = false;
            }

            EndPopup();
        }

        return selected;
    }

    #region File Importer

    static void  DrawFileImporter()
    {
        SetNextWindowSize(Vector2.One * 500, ImGuiCond.Once);
        SetNextWindowPos(Vector2.One * 500, ImGuiCond.Once);

        Begin("Sprite Importer", ref ShowFileImporter, ImGuiWindowFlags.NoCollapse );

            TextColored(Raylib.ColorNormalize(Color.LightGray), "Drop a file to load it");


            if(saveTextPath != "")
            {
                Text("Writing into: ");
                SameLine();

                var relPath = Path.GetRelativePath(GameController.ProjectLoc, saveTextPath);

                TextColored(Raylib.ColorNormalize(Color.SkyBlue), relPath);
            }
            

            if(Raylib.IsFileDropped() && GetIO().WantCaptureMouse)
            {
                var files = Raylib.GetDroppedFiles();
        
                foreach (var file in files)
                {
                    Console.WriteLine("Dropped: " + file);

                    if(Path.GetExtension(file) == ".pfimg")
                    {


                        saveTextPath = file;

                        var addedImages = SpriteManager.LoadTexturesFromDataFile(file, false);

                        foreach (var image in addedImages)
                        {
                            DroppedImages.Add(image.ID, image);
                        }

                    }
                    else
                    {
                        var newImg = new ImageData(file);
                        DroppedImages.Add(newImg.ID, newImg);
                    }
                }


                var unloadF = Raylib.LoadDroppedFiles();

                Raylib.UnloadDroppedFiles(unloadF);
            }

            int lenght = 0;

            BeginChild("GeneralArea", new Vector2(-1, - 50), ImGuiChildFlags.Borders);

            foreach (var image in DroppedImages.ToList())
            {
                BeginChild(image.Key, new Vector2(200, 200), ImGuiChildFlags.Borders);
                
                PushID(image.Key);

                var relPath = Path.GetRelativePath(saveTextPath, image.Value.texturePath);

                TextColored(Raylib.ColorNormalize(Color.Gold), "Loaded ID: " + image.Key);

                Text("Loaded Image at:");
                TextColored(Raylib.ColorNormalize(Color.SkyBlue), relPath);

                var s = new Vector2(image.Value.TextSizeX, image.Value.TextSizeY);
                
                s = Raymath.Vector2Normalize(s);

                PushStyleColor(ImGuiCol.Button, Raylib.ColorNormalize(Color.Blank));
                PushStyleColor(ImGuiCol.ButtonHovered, Raylib.ColorNormalize(Raylib.ColorAlpha(Color.Blue, .2f)));
                RlImGui.ImageButtonSize("Butt"+relPath, image.Value.loadedTexture, s*200);
                PopStyleColor();
                PopStyleColor();

                if (BeginDragDropSource())
                {
                    unsafe
                    {
                        string data = image.Key;
                        SetDragDropPayload("LoadingTextureDragData", (IntPtr)(&data), (uint)sizeof(string));
                    }
                    
                    Text(Path.GetFileName(relPath));

                    EndDragDropSource();
                }


                if(BeginDragDropTarget())
                {

                    var data = AcceptDragDropPayload("LoadingTextureDragData");


                    unsafe
                    {
                        if(data.NativePtr != null)
                        {
                            string dataPtr = ((string*)data.Data.ToPointer())[0];

                            image.Value.textureNormalPath = DroppedImages[dataPtr].texturePath;
                            image.Value.loadedNormal = Raylib.LoadTexture(DroppedImages[dataPtr].texturePath);
                            image.Value.hasNormal = true;

                            SpriteManager.RemoveTexture(dataPtr);
                            DroppedImages.Remove(dataPtr);
                        }

                    }
                    

                    EndDragDropTarget();
                }

                if(image.Value.hasNormal)
                {
                    Text("Attached normal:");
                    RlImGui.ImageSize(image.Value.loadedNormal, s * 100);
                }

                Text("Sprite size:");


                InputInt("###Int1", ref image.Value.SpriteSizeX, 0);
                InputInt("###Int2", ref image.Value.SpriteSizeY, 0);
        

                Dummy(new Vector2(140, 30));
                Dummy(new Vector2(140, 30));
                SameLine();
                if(Button("X", new Vector2(20, 20)))
                {
                    SpriteManager.RemoveTexture(image.Key);
                    DroppedImages.Remove(image.Key);
                }

                PopID();
                EndChild();


                lenght += 200;

                if(lenght + 200 < GetWindowWidth())
                {
                    SameLine();
                } else
                {
                    lenght = 0;
                }

            }

            Dummy(Vector2.One);

            EndChild();

            if(Button("Save Changes"))
            {
                SpriteManager.SaveTextureDataFile(DroppedImages, saveTextPath);

                foreach (var loadedImg in DroppedImages)
                {
                    SpriteManager.RemoveTexture(loadedImg.Key);
                }

                DroppedImages = [];

                saveTextPath = "";
            }
            
            
            
        End();
    }

    #endregion

    

    static void DrawLoadedImageList()
    {
        SetNextWindowSize(Vector2.One * 400, ImGuiCond.FirstUseEver);
        SetNextWindowPos(Vector2.One * 100 + Vector2.UnitY * 400, ImGuiCond.FirstUseEver);

        Begin("Loaded Image List", ImGuiWindowFlags.AlwaysVerticalScrollbar);
        
        int lenght = 0;

        foreach (var image in SpriteManager.LoadedImages.ToList())
        {
            BeginChild(image.Key, new Vector2(100, 100), ImGuiChildFlags.Borders);
                
                PushID(image.Key);

                TextColored(Raylib.ColorNormalize(Color.Gold), "Loaded ID: " + image.Key);

                var s = new Vector2(image.Value.TextSizeX, image.Value.TextSizeY);
                
                s = Raymath.Vector2Normalize(s);

                PushStyleColor(ImGuiCol.Button, Raylib.ColorNormalize(Color.Blank));
                PushStyleColor(ImGuiCol.ButtonHovered, Raylib.ColorNormalize(Raylib.ColorAlpha(Color.Blue, .2f)));
                RlImGui.ImageButtonSize("Butt", image.Value.loadedTexture, s*100);
                PopStyleColor();
                PopStyleColor();

                if (BeginDragDropSource())
                {
                    unsafe
                    {
                        string data = image.Key;
                        SetDragDropPayload("TextureDragData", (IntPtr)(&data), (uint)sizeof(string));
                    }
                    
                    Text(Path.GetFileName(image.Key));

                    EndDragDropSource();
                }



                PopID();
            EndChild();


            lenght += 100;

            if(lenght + 100 < GetWindowWidth())
            {
                SameLine();
            } else
            {
                lenght = 0;
            }


        }

        End();
    }

    static void DrawSavedEntities()
    {
        SetNextWindowSize(Vector2.One * 300, ImGuiCond.FirstUseEver);
        SetNextWindowPos(Vector2.One * 800 + Vector2.UnitX * 200, ImGuiCond.FirstUseEver);

        Begin("Saved Entities");
        
        Text("Drag in to save Entities. Drag out to instantiate them in the world");

        foreach (var entity in PrefabLoader.savedEntities)
        {
            if(BeginChild(entity.Name, new Vector2(200), ImGuiChildFlags.Borders))
            {
                Text("Origin ID:");
                Text(entity.ID.ToString());

                Button(entity.Name);

                
                EndChild();
            }
            
        }


        End();
    }


    public static void DrawSelectedPos()
    {
        if (DEBUG_Selected != null)
            Raylib.DrawCircle((int)DEBUG_Selected.GlobalPosition.X, -(int)DEBUG_Selected.GlobalPosition.Y, 10, Raylib.ColorAlpha(Color.White, 0.4f));
    }

    public static void ClickAndDrag()
    {
        CameraManager.I.freeRoam = !GetIO().WantCaptureMouse;
        
        if(Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.S))
        {
            Console.WriteLine("Saving");
            SaveFileSelector = true;
            OpenPopup("Open File");
        }

        if(GetIO().WantCaptureMouse) return;

        if(Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            var diff = Raylib.GetMouseDelta();
            diff = (diff / 10) * GameController.PixelRatio;

            CameraManager.I.Cam.Target = CameraManager.I.Cam.Target - diff;
        }

        if(Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.IsKeyDown(KeyboardKey.LeftControl))
        {
            var diff = Raylib.GetMousePosition() * GameController.PixelRatio / 10;

            var MousePos = diff + (CameraManager.I.Cam.Target - CameraManager.I.Cam.Offset);

            var ent = GameController.GetAllRenderers().Where(res => Raymath.Vector2Distance(res.Owner.GlobalPosition, MousePos) < res.Size).OrderByDescending(res => Raymath.Vector2Distance(res.Owner.GlobalPosition, MousePos)).FirstOrDefault((RendererBehaviour?)null);

            DEBUG_Selected = ent?.Owner ?? null;
        } else
        if(Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            if(DEBUG_Selected != null)
            {
                var diff = Raylib.GetMouseDelta();
                diff = (diff / 10) * GameController.PixelRatio;

                DEBUG_Selected.GlobalPosition += diff;
            }
        }
    }

#endif
}