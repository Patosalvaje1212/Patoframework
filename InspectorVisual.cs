using System.Numerics;
using System.Reflection;


using PatoframeWork;
using PatoframeWork.Rendering;

using ImGuiNET;
using Raylib_cs;
using System.Diagnostics;
using System.Collections;

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
            
            string? filePath = null;
            string? fileName = null;
            if(DrawFileSelector(".json", ref filePath, ref fileName)) 
            {
                GameController.I.SaveLocation = filePath;
                GameController.I.fileSaveName = fileName;
                GameController.I.LoadScene();
                OpenFileSelector = false;
            }
                
            

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

            string? filePath = null;
            string? fileName = null;
            if(DrawFileSelector(null, ref filePath, ref fileName)) 
            {
                GameController.I.SaveLocation = filePath;
                GameController.I.SaveScene();
                SaveFileSelector = false;
            }
                            


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
        if(entity.Active)
        {
            if(ImGui.InputText("Name:", ref entity.Name, 50))
            {
                if(entity.Name == "") entity.Name = " ";
            }
        }
        else ImGui.TextDisabled(entity.Name);

        PropertyInfo[] propertyInfos = typeof(Entity).GetProperties();
        FieldInfo[] fieldInfos = typeof(Entity).GetFields();

        DrawPropertiesAndFields(entity, null, propertyInfos, fieldInfos);

        ImGui.SeparatorText("Properties");

        if(entity.Behaviours.Count > 0)
        {
            foreach (var beh in entity.Behaviours)
            {
                
                ImGui.TextColored(new Vector4(Color.Gold.R, Color.Gold.G, Color.Gold.B, Color.Gold.A) / 255, beh.GetType().Name);
                ImGui.BeginGroup();

                PropertyInfo[] behPropertyInfos = beh.GetType().GetProperties();
                FieldInfo[] behFieldInfos = beh.GetType().GetFields();

                DrawPropertiesAndFields(null, beh, behPropertyInfos, behFieldInfos);

                ImGui.EndGroup();
                ImGui.Separator();
            }
        
        }
    }

    static object?[] GetOrderedArray(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos)
    {
        var L = new ArrayList(propertyInfos);
        L.AddRange(fieldInfos);

        var Ordererd = L.ToArray().OrderByDescending(res => 
        {
            if(res is PropertyInfo pI) return pI.GetCustomAttribute<InspectorShowOrderAttribute>(false)?.order ?? 0;
            if(res is FieldInfo fI) return fI.GetCustomAttribute<InspectorShowOrderAttribute>(false)?.order ?? 0;

            return 0;
        } );

        return Ordererd.ToArray();
    }

    // Draw Individual Properties
    static void DrawPropertiesAndFields(Entity? entity, Behaviour? beh, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos)
    {
        int switchItem = 0;

        var Ordered = GetOrderedArray(propertyInfos, fieldInfos); 
        
        for (int i = 0; i < Ordered.Length; i++)
        {
            if(Ordered[i] is PropertyInfo pI) DrawProperty(pI, entity, beh);
            else if(Ordered[i] is FieldInfo fI) DrawField(fI, entity, beh);
        }
        
    } 


    static void DrawProperty(PropertyInfo property, Entity? entity, Behaviour? beh)
    {
        if(property.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0 ) return;


        var res = property.GetValue(entity == null ? beh : entity);

        
        if(property.GetCustomAttributes(typeof(InspectorHideNullAttribute), false).Length > 0 
        && res == null) return;
        
        bool defaultRender = false;
        if(property.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0 )
        {
            if(res is int intR)
            {
                if(ImGui.InputInt(property.Name + " ", ref intR))
                {
                    property.SetValue(entity == null ? beh : entity, intR);
                }

            } else  if(res is bool boolR)
            {
                if(ImGui.Checkbox(property.Name, ref boolR))
                {
                    property.SetValue(entity == null ? beh : entity, boolR);
                }

            } else if(res is Vector2 v2)
            {
                if(ImGui.DragFloat2(property.Name + " X", ref v2, v2.Length() / 150))
                {
                    property.SetValue(entity == null ? beh : entity, v2);
                }

            } else if(res is string str)
            {
                if (ImGui.InputText(property.Name, ref str, 100))
                {
                    property.SetValue(entity == null ? beh : entity, str);
                }

            } else
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
    
    static void DrawField(FieldInfo property, Entity? entity, Behaviour? beh)
    {
        if(property.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0 ) return;


        var res = property.GetValue(entity == null ? beh : entity);

        
        if(property.GetCustomAttributes(typeof(InspectorHideNullAttribute), false).Length > 0 
        && res == null) return;
        
        bool defaultRender = false;
        if(property.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0 )
        {
            if(res is int intR)
            {
                if(ImGui.InputInt(property.Name + " ", ref intR))
                {
                    property.SetValue(entity == null ? beh : entity, intR);
                }

            } else  if(res is bool boolR)
            {
                if(ImGui.Checkbox(property.Name, ref boolR))
                {
                    property.SetValue(entity == null ? beh : entity, boolR);
                }

            } else if(res is Vector2 v2)
            {
                if(ImGui.DragFloat2(property.Name + " X", ref v2, v2.Length() / 150))
                {
                    property.SetValue(entity == null ? beh : entity, v2);
                }

            } else if(res is string str)
            {
                if (ImGui.InputText(property.Name, ref str, 100))
                {
                    property.SetValue(entity == null ? beh : entity, str);
                }

            } else if(res is Color col)
            {
                Vector4 newCol = Raylib.ColorNormalize(col);
                if (ImGui.ColorEdit4(property.Name, ref newCol))
                {
                    property.SetValue(entity == null ? beh : entity, Raylib.ColorFromNormalized(newCol));
                }

            } else if(res is float newF)
            {
                if (ImGui.DragFloat(property.Name, ref newF))
                {
                    property.SetValue(entity == null ? beh : entity, newF);
                }

            } else
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
    static bool DrawFileSelector(string? fileType, ref string? DestPath, ref string? FileName)
    {
        ImGui.BeginGroup();
        if(SaveLocation != null)
        {
            List<string> files = [.. Directory.GetFileSystemEntries(SaveLocation).Where(res => (fileType != null && Path.GetExtension(res) == fileType) || Directory.Exists(res)).OrderBy(res => !Directory.Exists(res))];

            if(files.Count > 0)
            {
                foreach (var file in files)
                {
                    bool isDirectory = Directory.Exists(file);

                    if(isDirectory) ImGui.Dummy(Vector2.One * 25);
                    else ImGui.Dummy(Vector2.One * 25 + Vector2.UnitX * 15);
                    ImGui.SameLine();

                    if(ImGui.ColorButton(file, Raylib.ColorNormalize(isDirectory ? Color.Gray : Color.Blue)))
                    {
                    
                        DestPath = Path.GetDirectoryName(file);
                        

                        if(!isDirectory)
                        {
                            FileName =  Path.GetFileName(file);

                            return true;
                        }
                        
                        SaveLocation = file;
                    }
                }
                
            }
        } else
        throw new FileLoadException("Error while loading the Folder data. Please check the Engine has access to the requested folder");

        ImGui.EndGroup();

        if(fileType == null)
        {
            ImGui.Separator();

            ImGui.BeginGroup();
            if(ImGui.Button("Select folder"))
            {
                DestPath = Path.GetDirectoryName(SaveLocation);
                FileName = null;
                
                return true;
            }
            ImGui.EndGroup();
        }

        DestPath = null;
        FileName = null;

        return false;
    }
    


#endif
}