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

    static string? SaveL = "";
    static string? newSaveName = "SaveData 1";
    static bool OpenFileSelector = false;
    static bool SaveFileSelector = false;


    public static void ImGUIBeh()
    {
        ImGui.ShowDemoWindow();

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

                        SaveL = AppDomain.CurrentDomain.BaseDirectory;

                        if(String.IsNullOrEmpty(SaveL))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }
                                        
                    }
                }
                if(ImGui.BeginMenu("Open Recent"))
                {


                    if(ImGui.MenuItem("Recent/Test/2.png")) 
                    {
                        
                    }
                    if(ImGui.MenuItem("Recent/Test/3.png")) 
                    {
                        
                    }
                    if(ImGui.MenuItem("Recent/Test/4.png")) 
                    {
                        
                    }
                    if(ImGui.MenuItem("Recent/Test/5.png")) 
                    {
                        
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if(ImGui.MenuItem("Save"))
                {
                    if(GameController.I.fileSaveName == "")
                    {
                        SaveL = AppDomain.CurrentDomain.BaseDirectory;

                        if(String.IsNullOrEmpty(SaveL))
                        {
                            throw new FormatException("Could not find a viable path to open the file explorer. Please reset the path to an existing path in the .json save file");
                        }

                        SaveFileSelector = true;

                    } else
                    GameController.I.SaveScene();
                }
                
                if(ImGui.MenuItem("Save AS"))
                {
                    SaveL = AppDomain.CurrentDomain.BaseDirectory;

                    if(String.IsNullOrEmpty(SaveL))
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

<<<<<<< Updated upstream
=======
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
                
                


                PropertyInfo[] behPropertyInfos = beh.GetType().GetProperties();
                FieldInfo[] behFieldInfos = beh.GetType().GetFields();

                
                ImGui.BeginChild("ContextMenuTXT###" +  behC, Vector2.UnitY * (55 * (behPropertyInfos.Length + behPropertyInfos.Length)) - Vector2.UnitX * 40, ImGuiChildFlags.FrameStyle | ImGuiChildFlags.AlwaysAutoResize | ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY);
                ImGui.BeginGroup();

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
            List<string> files = [.. Directory.GetFileSystemEntries(SaveLocation).OrderBy(res => !Directory.Exists(res))];

            if(files.Count > 0)
            {
                foreach (var file in files)
                {
                    bool isDirectory = Directory.Exists(file);

                    if(isDirectory) ImGui.Dummy(Vector2.One * 25);
                    else ImGui.Dummy(Vector2.One * 25 + Vector2.UnitX * 15);
                    ImGui.SameLine();

                    if(isDirectory) ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(Raylib.ColorNormalize(Color.DarkBlue)));
                    else if(fileType != null && Path.GetExtension(file) == fileType || (fileType == null && isDirectory)) ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(Raylib.ColorNormalize(Color.Blue)));
                    else ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(Raylib.ColorNormalize(Color.Gray)));

                    if( ImGui.Button(Path.GetFileName(file)) )
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
>>>>>>> Stashed changes
        Vector2 center = new(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2);
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Vector2.One * 500 + Vector2.UnitX * 300, ImGuiCond.Appearing);
                        
        if (ImGui.BeginPopupModal("Open File", ref OpenFileSelector)) {
            ImGui.SetItemDefaultFocus();
            
            ImGui.BeginGroup();

                if(ImGui.ArrowButton("###ArrowB", ImGuiDir.Up))
                {
                    SaveL = Directory.GetParent(SaveL).FullName;
                }

                ImGui.SameLine();
                if(ImGui.InputText("Select Path", ref SaveL, 100))
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
                    SaveL = Directory.GetParent(SaveL).FullName;
                }

                ImGui.SameLine();
                if(ImGui.InputText("Select Path", ref SaveL, 100))
                {
                    //
                }

                ImGui.Dummy(Vector2.One * 30);

                if(ImGui.InputText("File Name", ref newSaveName, 40))
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
                
                GameController.I.saveLocation = Path.GetDirectoryName(SaveL);
                GameController.I.fileSaveName = newSaveName;

                if(!String.IsNullOrEmpty(newSaveName))
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

            foreach (var entity in GameController.I.entities.Where((res) => res.Value.Parent == 0))
            {   
                DrawRecursiveList(entity.Value);
            }
        ImGui.End();

        #endregion




        ImGui.Begin("Entity Info");
        if(DEBUG_Selected != null) DrawEntityInfo(DEBUG_Selected);
        ImGui.End();


       
    }


    static void DrawEntityInfo(Entity entity)
    {
        if(entity.Active) ImGui.Text(entity.name);
        else ImGui.TextDisabled(entity.name);

        PropertyInfo[] propertyInfos = typeof(Entity).GetProperties();
        FieldInfo[] fieldInfos = typeof(Entity).GetFields();

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
        
        ImGui.SeparatorText("Properties");

        if(entity.Behaviours.Count > 0)
        {
            foreach (var beh in entity.Behaviours)
            {
                
                ImGui.TextColored(new Vector4(Color.Gold.R, Color.Gold.G, Color.Gold.B, Color.Gold.A) / 255, beh.GetType().Name);
                ImGui.BeginGroup();

                PropertyInfo[] behPropertyInfos = beh.GetType().GetProperties();
                FieldInfo[] behFieldInfos = beh.GetType().GetFields();

                foreach (var property in behPropertyInfos)
                {
                    if(property.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0 ) continue;


                    var res = property.GetValue(beh);

                    
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
                                property.SetValue(beh, refInt);
                            }

                        } else if(res is bool boolR)
                        {
                            bool newB = boolR;
                            if(ImGui.Checkbox(property.Name, ref newB))
                            {
                                property.SetValue(beh, newB);
                            }

                        } else if(res is Vector2 v2)
                        {
                            Vector2 refV = v2;
                            if(ImGui.DragFloat2(property.Name + " X", ref refV, v2.Length() / 150))
                            {
                                property.SetValue(beh, refV);
                            }

                        }else if(res is Color colR)
                        {
                            Vector3 newCol = new Vector3(colR.R, colR.G, colR.B) / 255;
                            if(ImGui.ColorEdit3(property.Name, ref newCol))
                            {
                                property.SetValue(beh, new Color(newCol.X, newCol.Y, newCol.Z));
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

                foreach (var field in behFieldInfos)
                {
                    if(field.GetCustomAttributes(typeof(InspectorHideAttribute), false).Length > 0 ) continue;


                    var res = field.GetValue(beh);

                    
                    if(field.GetCustomAttributes(typeof(InspectorHideNull), false).Length > 0 
                    && res == null) continue;

                    bool defaultRender = false;
                    if(field.GetCustomAttributes(typeof(InspectorNonEditableAttribute), false).Length <= 0 )
                    {
                        if(res is int intR)
                        {
                            int refInt = intR;
                            if(ImGui.InputInt(field.Name + " ", ref refInt))
                            {
                                field.SetValue(beh, refInt);
                            }

                        } else if(res is bool boolR)
                        {
                            bool newB = boolR;
                            if(ImGui.Checkbox(field.Name, ref newB))
                            {
                                field.SetValue(beh, newB);
                            }

                        } else if(res is Vector2 v2)
                        {
                            Vector2 refV = v2;
                            if(ImGui.DragFloat2(field.Name + " X", ref refV, v2.Length() / 150))
                            {
                                field.SetValue(beh, refV);
                            }

                        } else if(res is Color colR)
                        {
                            Vector3 newCol = new Vector3(colR.R, colR.G, colR.B) / 255;
                            if(ImGui.ColorEdit3(field.Name, ref newCol))
                            {
                                field.SetValue(beh, new Color(newCol.X, newCol.Y, newCol.Z));
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
                        ImGui.TextColored(new Vector4(.5f, .5f, 1f, 1f), field.Name + "  : ");
                        ImGui.SameLine();
                        ImGui.Text(res?.ToString() ?? "Null"); 
                    }
                    
                }

                ImGui.EndGroup();
                ImGui.Separator();
            }
        
        }
    }

    
<<<<<<< Updated upstream
    static void DrawRecursiveList(Entity REntity)
    {
        if(REntity.childs.Count > 0) 
        {
            if(ImGui.TreeNodeEx(REntity.name, (DEBUG_Selected == REntity ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAllColumns))
            {   
                if(ImGui.IsItemClicked()) DEBUG_Selected = REntity;

                foreach(var ch in REntity.childs)
                {
                    DrawRecursiveList(GameController.I.entities[ch]);
                }

                ImGui.TreePop();
            }
            else if(ImGui.IsItemClicked()) DEBUG_Selected = REntity;

        } else
        {
            if(ImGui.TreeNodeEx(REntity.name, (DEBUG_Selected == REntity ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.Leaf))
            {
                if(ImGui.IsItemClicked()) DEBUG_Selected = REntity;

                ImGui.TreePop();
            }
        }

        
    }


   

    static void DrawFileSelector(bool limitToJson = true)
    {
        

        var files = Directory.GetFileSystemEntries(SaveL).Where(res => (limitToJson && Path.GetExtension(res) == ".json") || Directory.Exists(res)).OrderBy(res => !Directory.Exists(res)).ToList();

        if(files.Count > 0)
        {
            foreach (var file in files)
            {
                if(Directory.Exists(file)) ImGui.Dummy(Vector2.One * 25);
                else ImGui.Dummy(Vector2.One * 25 + Vector2.UnitX * 15);
                ImGui.SameLine();

                if(ImGui.Button(file))
                {
                   
                    GameController.I.saveLocation = Path.GetDirectoryName(file);
                    

                    if(!Directory.Exists(file))
                    {
                        GameController.I.fileSaveName =  Path.GetFileName(file);
                        OpenFileSelector = false;
                        GameController.I.LoadScene();
                    }
                    
                    SaveL = file;
                    Console.WriteLine(GameController.I.saveLocation + " - " + GameController.I.fileSaveName);

                }
            }
            
        }
    }
    


=======


    public static void DrawSelectedPos()
    {
        if(DEBUG_Selected != null) 
        Raylib.DrawRing(DEBUG_Selected.GlobalPosition, 150, 145, 0, 360, 100, Color.Black);
    }


>>>>>>> Stashed changes
#endif
}