using System.Numerics;
using System.Reflection;


using PatoframeWork;
using PatoframeWork.Rendering;

using ImGuiNET;
using Raylib_cs;
using System.Diagnostics;

namespace PatoframeWork;

public static class InspectorVisual
{
    
#if DEBUG

    static bool DEBUG_ColorStyle = false;

    static Entity? DEBUG_Selected;

    static string? SaveLocation = "";
    static string? SaveName = "SaveData 1";
    static bool OpenFileSelector = false;
    static bool SaveFileSelector = false;


    public static void ImGUIBeh()
    {
        ImGui.ShowDemoWindow();

        // Draw Main Menu
        if(ImGui.BeginMainMenuBar())
        {
            if(ImGui.BeginMenu("File"))
            {
                
                if(ImGui.MenuItem("New")) {}
                if(ImGui.MenuItem("Open"))
                {
                    if(!ImGui.IsPopupOpen("Open File"))
                    {
                        OpenFileSelector = true;

                        SaveLocation = AppDomain.CurrentDomain.BaseDirectory;

                        if(String.IsNullOrEmpty(SaveLocation))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }
                                        
                    }
                }
                if(ImGui.BeginMenu("Open Recent"))
                {
                    
                    // TODO

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if(ImGui.MenuItem("Save"))
                {
                    if(GameController.I.fileSaveName == "")
                    {
                        SaveLocation = AppDomain.CurrentDomain.BaseDirectory;

                        if(String.IsNullOrEmpty(SaveLocation))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }

                        SaveFileSelector = true;

                    } else
                    GameController.I.SaveScene();
                }
                
                if(ImGui.MenuItem("Save As"))
                {
                    SaveLocation = AppDomain.CurrentDomain.BaseDirectory;

                    if(String.IsNullOrEmpty(SaveLocation))
                    {
                        throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                    }
                    SaveFileSelector = true;
                }

                ImGui.EndMenu();
            }

            
            
            if(ImGui.MenuItem("Tool"))
            {}

            if(ImGui.BeginMenu("View"))
            {
                ImGui.BeginGroup();

                
                if(ImGui.Checkbox("Toggle Dark Mode", ref DEBUG_ColorStyle))
                {

                    if(DEBUG_ColorStyle) 
                    {
                        ImGui.StyleColorsLight();
                    } else
                    {
                        ImGui.StyleColorsDark();
                    }
                }

                    
                ImGui.EndGroup();
                
                if(ImGui.MenuItem("Reset Camera"))
                {
                    CameraManager.I.ResetCamera();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        
        }

        #region OpenFile Popup

        if(OpenFileSelector) ImGui.OpenPopup("Open File");

        Vector2 center = new(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2);
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Vector2.One * 500 + Vector2.UnitX * 300, ImGuiCond.Appearing);
                        
        if (ImGui.BeginPopupModal("Open File", ref OpenFileSelector)) {
            ImGui.SetItemDefaultFocus();
            
            ImGui.BeginGroup();

                if(ImGui.ArrowButton("###ArrowB", ImGuiDir.Up))
                {
                    if(SaveLocation != null && Directory.GetParent(SaveLocation) is DirectoryInfo info)
                    {
                        if(info != null)  SaveLocation = info.FullName;
                    }
                }

                ImGui.SameLine();
                if(ImGui.InputText("Select Path", ref SaveLocation, 100))
                {
                    //
                }

            ImGui.EndGroup();

            ImGui.SeparatorText("Files:");

            ImGui.BeginGroup();
            DrawFileSelector();
                
            

            ImGui.EndGroup();

            ImGui.EndPopup();
        }

        #endregion
        #region SaveFile Popup

        if(SaveFileSelector) ImGui.OpenPopup("Save File");

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Vector2.One * 500 + Vector2.UnitX * 300, ImGuiCond.Appearing);
                        
        if (ImGui.BeginPopupModal("Save File", ref SaveFileSelector))
        {
            ImGui.SetItemDefaultFocus();
            
            ImGui.BeginGroup();

                if(ImGui.ArrowButton("###ArrowB", ImGuiDir.Up))
                {
                    if(SaveLocation != null && Directory.GetParent(SaveLocation) is DirectoryInfo info)
                    {
                        if(info != null)  SaveLocation = info.FullName;
                    }
                }

                ImGui.SameLine();
                if(ImGui.InputText("Select Path", ref SaveLocation, 100))
                {
                    //
                }

                ImGui.Dummy(Vector2.One * 30);

                if(ImGui.InputText("File Name", ref SaveName, 40))
                {
                    
                }

                ImGui.Dummy(Vector2.One * 130);


            ImGui.EndGroup();

            ImGui.SeparatorText("Files:");

            ImGui.BeginGroup();
            DrawFileSelector(false);

           
            

            ImGui.EndGroup();

            ImGui.Separator();

            ImGui.BeginGroup();
            if(ImGui.Button("Save Data"))
            {
                
                GameController.I.SaveLocation = Path.GetDirectoryName(SaveLocation);
                GameController.I.fileSaveName = SaveName;

                if(!String.IsNullOrEmpty(SaveName))
                {
                    SaveFileSelector = false;

                    GameController.I.SaveScene();
                } else
                {
                    ImGui.OpenPopup("Cant be empty");
                }

                
            }
            ImGui.EndGroup();

            ImGui.EndPopup();


            if(ImGui.BeginPopup("Cant be empty", ImGuiWindowFlags.ChildMenu))
            {
                ImGui.SetItemDefaultFocus();
                ImGui.BeginGroup();
                    if(ImGui.Button("OK"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                ImGui.EndGroup();

                ImGui.EndPopup();
            }
        }


        ImGui.Begin("Entities");

            foreach (var entity in GameController.I.Entities.Where((res) => res.Value.Parent == 0))
            {   
                DrawRecursiveList(entity.Value);
            }
        ImGui.End();

        #endregion


        ImGui.Begin("Entity Info");
        if(DEBUG_Selected != null) DrawEntityInfo(DEBUG_Selected);
        ImGui.End();
    }

    // Draw Properties Inspector
    static void DrawEntityInfo(Entity entity)
    {
        if(entity.Active) ImGui.Text(entity.Name);
        else ImGui.TextDisabled(entity.Name);

        PropertyInfo[] propertyInfos = typeof(Entity).GetProperties();
        FieldInfo[] fieldInfos = typeof(Entity).GetFields();

        DrawPropertiesAndFields(entity, propertyInfos, fieldInfos);

        ImGui.SeparatorText("Properties");

        if(entity.Behaviours.Count > 0)
        {
            foreach (var beh in entity.Behaviours)
            {
                
                ImGui.TextColored(new Vector4(Color.Gold.R, Color.Gold.G, Color.Gold.B, Color.Gold.A) / 255, beh.GetType().Name);
                ImGui.BeginGroup();

                PropertyInfo[] behPropertyInfos = beh.GetType().GetProperties();
                FieldInfo[] behFieldInfos = beh.GetType().GetFields();

                DrawPropertiesAndFields(entity, behPropertyInfos, behFieldInfos);

                ImGui.EndGroup();
                ImGui.Separator();
            }
        
        }
    }

    // Draw Individual Properties
    static void DrawPropertiesAndFields(Entity entity, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos)
    {

        foreach (var property in propertyInfos)
        {
            if(property.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0 ) continue;


            var res = property.GetValue(entity);

            
            if(property.GetCustomAttributes(typeof(InspectorHideNull), false).Length > 0 
            && res == null) continue;

            bool defaultRender = false;
            if(property.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0 )
            {
                if(res is int intR)
                {
                    int refInt = intR;
                    if(ImGui.InputInt(property.Name + " ", ref refInt))
                    {
                        property.SetValue(entity, refInt);
                    }

                } else  if(res is bool boolR)
                {
                    bool newB = boolR;
                    if(ImGui.Checkbox(property.Name, ref newB))
                    {
                        property.SetValue(entity, newB);
                    }

                } else if(res is Vector2 v2)
                {
                    Vector2 refV = v2;
                    if(ImGui.DragFloat2(property.Name + " X", ref refV, v2.Length() / 150))
                    {
                        property.SetValue(entity, refV);
                    }

                }else
                {
                    defaultRender = true;
                }

            } else
            {
                defaultRender = true;
            }

            if(defaultRender)
            {
                ImGui.TextColored(new Vector4(.5f, .5f, 1f, 1f), property.Name + "  : ");
                ImGui.SameLine();
                ImGui.Text(res?.ToString() ?? "Null"); 
            }

            
            
        }

        foreach (var field in fieldInfos)
        {
            if(field.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0 ) continue;


            var res = field.GetValue(entity);

            
            if(field.GetCustomAttributes(typeof(InspectorHideNull), false).Length > 0 
            && res == null) continue;

            bool defaultRender = false;
            if(field.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0 )
            {
                if(res is int intR)
                {
                    int refInt = intR;
                    if(ImGui.InputInt(field.Name + "", ref refInt))
                    {
                        field.SetValue(entity, refInt);
                    }

                } else  if(res is bool boolR)
                {
                    bool newB = boolR;
                    if(ImGui.Checkbox(field.Name, ref newB))
                    {
                        field.SetValue(entity, newB);
                    }

                } else if(res is Vector2 v2)
                {
                    Vector2 refV = v2;
                    if(ImGui.DragFloat2(field.Name + " X", ref refV, v2.Length() / 150))
                    {
                        field.SetValue(entity, refV);
                    }

                }else
                {
                    defaultRender = true;
                }

            } else
            {
                defaultRender = true;
            }

            if(defaultRender)
            {
                ImGui.TextColored(new Vector4(.5f, .5f, 1f, 1f), field.Name + "  : ");
                ImGui.SameLine();
                ImGui.Text(res?.ToString() ?? "Null"); 
            }
            
        }
        
    } 

    
    // Draw Entity List
    static void DrawRecursiveList(Entity REntity)
    {
        if(REntity.Childs.Count > 0) 
        {
            if(ImGui.TreeNodeEx(REntity.Name, (DEBUG_Selected == REntity ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAllColumns))
            {   
                if(ImGui.IsItemClicked()) DEBUG_Selected = REntity;

                foreach(var ch in REntity.Childs)
                {
                    DrawRecursiveList(GameController.I.Entities[ch]);
                }

                ImGui.TreePop();
            }
            else if(ImGui.IsItemClicked()) DEBUG_Selected = REntity;

        } else
        {
            if(ImGui.TreeNodeEx(REntity.Name, (DEBUG_Selected == REntity ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.Leaf))
            {
                if(ImGui.IsItemClicked()) DEBUG_Selected = REntity;

                ImGui.TreePop();
            }
        }

        
    }


   
    // Draw File Selector (Choose only folder /// Choose only Json)
    static void DrawFileSelector(bool limitToJson = true)
    {
        
        
        List<string> files;

        if(SaveLocation != null)
        {
            files = [.. Directory.GetFileSystemEntries(SaveLocation).Where(res => (limitToJson && Path.GetExtension(res) == ".json") || Directory.Exists(res)).OrderBy(res => !Directory.Exists(res))];

            if(files.Count > 0)
            {
                foreach (var file in files)
                {
                    if(Directory.Exists(file)) ImGui.Dummy(Vector2.One * 25);
                    else ImGui.Dummy(Vector2.One * 25 + Vector2.UnitX * 15);
                    ImGui.SameLine();

                    if(ImGui.Button(file))
                    {
                    
                        GameController.I.SaveLocation = Path.GetDirectoryName(file);
                        

                        if(!Directory.Exists(file))
                        {
                            GameController.I.fileSaveName =  Path.GetFileName(file);
                            OpenFileSelector = false;
                            GameController.I.LoadScene();
                        }
                        
                        SaveLocation = file;
                        Console.WriteLine(GameController.I.SaveLocation + " - " + GameController.I.fileSaveName);

                    }
                }
                
            }
        } else
        throw new FileLoadException("Error while loading the Folder data. Please check the Engine has access to the requested folder");
        
    }
    


#endif
}