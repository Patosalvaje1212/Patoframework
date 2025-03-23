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
        LoadPopus();
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
                    if(GameController.fileSaveName == "")
                    {
                        SaveLocation = AppDomain.CurrentDomain.BaseDirectory;

                        if(String.IsNullOrEmpty(SaveLocation))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }

                        SaveFileSelector = true;

                    } else
                    GameController.SaveScene();
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


        if(OpenFileSelector) ImGui.OpenPopup("Open File");

        if(SaveFileSelector) ImGui.OpenPopup("Save File");

        
        ImGui.Begin("Entities");

            ImGui.PushStyleColor(ImGuiCol.Button, Raylib.ColorNormalize(Color.DarkGreen));
            if(ImGui.SmallButton("Create Entity"))
            {
                var t = new Entity("New Entity");
                DEBUG_Selected = t;
            }
            ImGui.PopStyleColor();

            ImGui.SeparatorText("Entities -- ");

            foreach (var entity in GameController.Entities.ToList().Where((res) => res.Value.Parent == 0))
            {   
                DrawRecursiveList(entity.Value);
            }

        ImGui.End();


        ImGui.Begin("Entity Info");

        if(DEBUG_Selected != null) 
        {
            DrawEntityInfo(DEBUG_Selected);
            
            ImGui.SeparatorText("");
            
            DrawBehaviourSelector();
        }

        ImGui.End();
    }

    #region Draw Entity Info

    // Draw Properties Inspector
    static void DrawEntityInfo(Entity entity)
    {
        ImGui.Text("Entity Name");
        if(entity.Active)
        {
            if(ImGui.InputText("###EntityName", ref entity.Name, 50))
            {
                if(entity.Name == "") entity.Name = " ";
            }
        }
        else ImGui.TextDisabled(entity.Name);

        ImGui.Dummy(Vector2.One * 15);

        PropertyInfo[] propertyInfos = typeof(Entity).GetProperties();
        FieldInfo[] fieldInfos = typeof(Entity).GetFields();

        DrawPropertiesAndFields(entity, null, propertyInfos, fieldInfos, -1);

        if(entity.Behaviours.Count > 0)
        {
            ImGui.Dummy(Vector2.One * 20);
            ImGui.SeparatorText("Properties");

            int behC = 0;
            foreach (var beh in entity.Behaviours.ToList())
            {
                //ImGui.TextColored(new Vector4(Color.Gold.R, Color.Gold.G, Color.Gold.B, Color.Gold.A) / 255, beh.GetType().Name);

                ImGui.PushStyleColor(ImGuiCol.Text, Raylib.ColorNormalize(Color.Gold));
                ImGui.Text(beh.GetType().Name);
                ImGui.PopStyleColor();

                ImGui.PushStyleColor(ImGuiCol.ChildBg, Raylib.ColorNormalize(Color.DarkGray));
                
                ImGui.BeginChild("ContextMenuTXT###" +  behC, Vector2.UnitY * ImGui.GetWindowHeight() / 3 - Vector2.UnitX * 40, ImGuiChildFlags.FrameStyle | ImGuiChildFlags.AlwaysAutoResize | ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY);
                

                ImGui.BeginGroup();

                PropertyInfo[] behPropertyInfos = beh.GetType().GetProperties();
                FieldInfo[] behFieldInfos = beh.GetType().GetFields();

                DrawPropertiesAndFields(null, beh, behPropertyInfos, behFieldInfos, behC);

                ImGui.EndGroup();
                ImGui.EndChild();
                
                ImGui.PopStyleColor();

                if(ImGui.BeginPopupContextItem("ContextMenu###" +  behC))
                {
                    if(ImGui.MenuItem("Duplicate")) 
                    {
                        beh.CloneBehaviour();
                    }

                    if(ImGui.MenuItem("Delete")) 
                    {
                        beh.RemoveBehaviour();
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                behC ++;
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

        return [.. Ordererd];
    }


    #region Draw Properties
    
    
    // Draw Individual Properties
    static void DrawPropertiesAndFields(Entity? entity, Behaviour? beh, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, int BehaviourNumber)
    {
        var Ordered = GetOrderedArray(propertyInfos, fieldInfos); 
        
        for (int i = 0; i < Ordered.Length; i++)
        {
            if(Ordered[i] is PropertyInfo pI) DrawProperty(pI, entity, beh, i, BehaviourNumber);
            else if(Ordered[i] is FieldInfo fI) DrawField(fI, entity, beh, i, BehaviourNumber);
        }
        
    } 


    static void DrawBehaviourSelector()
    {
        var possibleBehaviours = AppDomain.CurrentDomain.GetAssemblies()
		.SelectMany(assembly => assembly.GetTypes())
		.Where(type => type.IsSubclassOf(typeof(Behaviour))).ToList();


        if(ImGui.BeginCombo("Add Behaviours", "", ImGuiComboFlags.PopupAlignLeft))
        {
            
            for (int n = 0; n < possibleBehaviours.Count; n++)
            {
                if (ImGui.Selectable(possibleBehaviours[n].Name, false))
                {
                    DEBUG_Selected?.AddBehaviour(possibleBehaviours[n]);
                }
            }

            ImGui.EndCombo();
        }
        
    }


    static void DrawProperty(PropertyInfo property, Entity? entity, Behaviour? beh, int IdNumber, int BehaviourNumber)
    {
        if(property.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0 ) return;


        var res = property.GetValue(entity == null ? beh : entity);

        
        if(property.GetCustomAttributes(typeof(InspectorHideNullAttribute), false).Length > 0 
        && res == null) return;
        
        
        if(property.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0 )
        {
            ImGui.Text(property.Name);
            ImGui.SameLine();

            if(res is int intR)
            {
                if(ImGui.InputInt("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref intR))
                {
                    property.SetValue(entity == null ? beh : entity, intR);
                }

            } else  if(res is bool boolR)
            {
                if(ImGui.Checkbox("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref boolR))
                {
                    property.SetValue(entity == null ? beh : entity, boolR);
                }

            } else if(res is Vector2 v2)
            {
                if(ImGui.DragFloat2("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref v2))
                {
                    property.SetValue(entity == null ? beh : entity, v2);
                }

            } else if(res is string str)
            {
                if (ImGui.InputText("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref str, 100))
                {
                    property.SetValue(entity == null ? beh : entity, str);
                }

            } else if(res is Enum newEnm)
            {

                if(ImGui.BeginCombo("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), property.GetValue(entity == null ? beh : entity)?.ToString()))
                {
                    foreach (var item in Enum.GetValues(newEnm.GetType()))
                    {
                        if(ImGui.Selectable(item.ToString(), item == property.GetValue(entity == null ? beh : entity)))
                        {
                            property.SetValue(entity == null ? beh : entity, item);
                        }
                    }

                    ImGui.EndCombo();
                }

            } else if(res is ulong newUl)
            {
                int newIntC = Convert.ToInt32(newUl);

                if (ImGui.InputInt("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref newIntC, 0))
                {
                    if(newIntC < 0 ) newIntC = 0;

                    if(property.Name == "Parent" && entity != null)
                        entity.SetParent(Convert.ToUInt64(newIntC));
                    else
                        property.SetValue(entity == null ? beh : entity, Convert.ToUInt64(newIntC));
                }

            }else
            {
                ImGui.SameLine();
                ImGui.Text(res?.ToString() ?? "Null"); 
            }

        }else
        {
            ImGui.SameLine();
            ImGui.Text(res?.ToString() ?? "Null"); 
        }
    }
    static void DrawField(FieldInfo property, Entity? entity, Behaviour? beh, int IdNumber, int BehaviourNumber)
    {
        if(property.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0 ) return;


        var res = property.GetValue(entity == null ? beh : entity);

        
        if(property.GetCustomAttributes(typeof(InspectorHideNullAttribute), false).Length > 0 
        && res == null) return;
        
        if(property.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0 )
        {
            ImGui.Text(property.Name);

            if(res is int intR)
            {
                if(ImGui.InputInt("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref intR))
                {
                    property.SetValue(entity == null ? beh : entity, intR);
                }

            } else  if(res is bool boolR)
            {
                if(ImGui.Checkbox("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref boolR))
                {
                    property.SetValue(entity == null ? beh : entity, boolR);
                }

            } else if(res is Vector2 v2)
            {
                if(ImGui.DragFloat2("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref v2))
                {
                    property.SetValue(entity == null ? beh : entity, v2);
                }

            } else if(res is string str)
            {
                if (ImGui.InputText("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref str, 100))
                {
                    property.SetValue(entity == null ? beh : entity, str);
                }

            } else if(res is Color col)
            {
                Vector4 newCol = Raylib.ColorNormalize(col);
                if (ImGui.ColorEdit4("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref newCol))
                {
                    property.SetValue(entity == null ? beh : entity, Raylib.ColorFromNormalized(newCol));
                }

            } else if(res is float newF)
            {
                if (ImGui.DragFloat("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref newF))
                {
                    property.SetValue(entity == null ? beh : entity, newF);
                }

            } else if(res is Enum newEnm)
            {

                if(ImGui.BeginCombo("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), property.GetValue(entity == null ? beh : entity)?.ToString()))
                {
                    foreach (var item in Enum.GetValues(newEnm.GetType()))
                    {
                        if(ImGui.Selectable(item.ToString(), item == property.GetValue(entity == null ? beh : entity)))
                        {
                            property.SetValue(entity == null ? beh : entity, item);
                        }
                    }

                    ImGui.EndCombo();
                }

            } else if(res is ulong newUl)
            {
                int newIntC = Convert.ToInt32(newUl);

                if (ImGui.InputInt("###" + property.Name + IdNumber.ToString() + BehaviourNumber.ToString(), ref newIntC, 0))
                {
                    if(newIntC < 0 ) newIntC = 0;

                    if(property.Name == "Parent" && entity != null)
                        entity.SetParent(Convert.ToUInt64(newIntC));
                    else
                        property.SetValue(entity == null ? beh : entity, Convert.ToUInt64(newIntC));
                }

            } else
            {
                ImGui.SameLine();
                ImGui.Text(res?.ToString() ?? "Null"); 
            }

        }else
        {
            ImGui.SameLine();
            ImGui.Text(res?.ToString() ?? "Null"); 
        }
    }

    #endregion
    #endregion
    
    // Draw Entity List
    static void DrawRecursiveList(Entity REntity)
    {
        if(REntity.Childs.Count > 0) 
        {
            if(ImGui.TreeNodeEx($"{REntity.Name}###{REntity.Id}", (DEBUG_Selected == REntity ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth))
            {   
                if(ImGui.IsItemClicked(ImGuiMouseButton.Left)) DEBUG_Selected = REntity;

                if (ImGui.BeginPopupContextItem("ContextMenu###" + REntity.Id))
                {
                    if(ImGui.MenuItem("Duplicate")) 
                    {
                        REntity.Duplicate();
                    }

                    if(ImGui.MenuItem("Delete")) 
                    {
                        REntity.Delete();
                    }

                    if(ImGui.MenuItem("Delete -- Chain")) 
                    {
                        REntity.Delete(true, true);
                    }


                    ImGui.EndPopup();
                }

                foreach(var ch in REntity.Childs)
                {
                    DrawRecursiveList(GameController.Entities[ch]);
                }

                ImGui.TreePop();
            }
            else 
            {
                if(ImGui.IsItemClicked(ImGuiMouseButton.Left)) DEBUG_Selected = REntity;
            
                if (ImGui.BeginPopupContextItem("ContextMenu###" + REntity.Id))
                {
                    if(ImGui.MenuItem("Duplicate")) 
                    {
                        REntity.Duplicate();
                    }

                    if(ImGui.MenuItem("Delete")) 
                    {
                        REntity.Delete();
                    }

                    if(ImGui.MenuItem("Delete -- Chain")) 
                    {
                        REntity.Delete(true, true);
                    }


                    ImGui.EndPopup();
                }
            }

        } else
        {
            if(ImGui.TreeNodeEx($"{REntity.Name}###{REntity.Id}", (DEBUG_Selected == REntity ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanFullWidth))
            {
                if(ImGui.IsItemClicked(ImGuiMouseButton.Left)) DEBUG_Selected = REntity;
                
                if (ImGui.BeginPopupContextItem("ContextMenu###" + REntity.Id))
                {
                    if(ImGui.MenuItem("Duplicate")) 
                    {
                        REntity.Duplicate();
                    }

                    if(ImGui.MenuItem("Delete")) 
                    {
                        REntity.Delete();
                    }

                    if(ImGui.MenuItem("Delete -- Chain")) 
                    {
                        REntity.Delete(true, true);
                    }


                    ImGui.EndPopup();
                }
                        
                
                ImGui.TreePop();
            }
        }
    }


    #region Draw Selectors
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

                    if(isDirectory) ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(Raylib.ColorNormalize(Color.Gray)));
                    else ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(Raylib.ColorNormalize(Color.Blue)));

                    if(ImGui.Button(Path.GetFileName(file)))
                    {
                    
                        DestPath = Path.GetDirectoryName(file) + "/";
                        

                        if(!isDirectory)
                        {
                            FileName = Path.GetFileName(file);

                            ImGui.EndGroup();
                            ImGui.PopStyleColor();

                            return true;
                        }
                        
                        SaveLocation = file;
                    }
                    ImGui.PopStyleColor();
                }
                
            }
        } else
        throw new FileLoadException("Error while loading the Folder data. Please check the Engine has access to the requested folder");

        ImGui.EndGroup();

        if(fileType == null)
        {
            
            ImGui.Separator();

            ImGui.Dummy(Vector2.One * 10 + Vector2.UnitX * 150);
            ImGui.SameLine();
            ImGui.BeginGroup();
            if(ImGui.Button("Select folder"))
            {
                DestPath = Path.GetDirectoryName(SaveLocation) + "/";
                FileName = SaveName + ".json";

                ImGui.EndGroup();
                
                return true;
            }
            ImGui.EndGroup();
        }

        DestPath = null;
        FileName = null;

        return false;
    }
    
    #endregion
    static void LoadPopus()
    {
       


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


        // Open file popup
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
                GameController.SaveLocation = filePath;
                GameController.fileSaveName = fileName;
                OpenFileSelector = false;

                GameController.LoadScene();
            }
                
            

            ImGui.EndGroup();

            ImGui.EndPopup();
        }


        // Save file popup
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
                ImGui.SameLine();
                if(ImGui.InputText("File Name", ref SaveName, 40))
                {
                    
                }

                ImGui.Dummy(Vector2.One * 30);


            ImGui.EndGroup();

            ImGui.SeparatorText("Files:");

            string? filePath = null;
            string? fileName = null;
            if(DrawFileSelector(null, ref filePath, ref fileName)) 
            {
                GameController.SaveLocation = filePath;
                GameController.fileSaveName = fileName;
                SaveFileSelector = false;

                GameController.SaveScene();
            }

            ImGui.EndPopup();
        }
    }

    
#endif
}