using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json;


namespace PatoframeWork;

public class Entity : ISerializable
{
    [InspectorHide]
    public string name = "Entity";

    public ulong Id { get; set; }

    public ulong Parent;

    [InspectorHide]
    public List<ulong> childs = [];


    public Vector2 LocalPosition;

    [InspectorHide]
    public List<Behaviour> Behaviours = [];


    
    
    [InspectorNonEditable, JsonIgnore]
    public Vector2 GlobalPosition
    {
        get
        {   
            return (Parent != 0 ? GameController.I.entities[Parent].GlobalPosition : Vector2.Zero) + LocalPosition;
        }
        set
        {
            LocalPosition = Parent != 0 ? value - GlobalPosition : value;
        }
    }

    private bool receiveUpdates;
    public bool ReceiveUpdates 
    {
        get => receiveUpdates;

        set
        {
            if(value && !receiveUpdates)
            GameController.I.Update += SelfUpdate;
            if(!value && receiveUpdates)
            GameController.I.Update -= SelfUpdate;

            receiveUpdates = value;
        }   
    }

    bool active = true;
    public bool Active
    {
        get
        {
            return Parent != 0 ? GameController.I.entities[Parent].Active && active : active;
        }
        set
        {
            if(value != active) 
            {
                active = value;

                if(value) OnLoad();
                else OnUnload();
            }
        }
    }

    public virtual void SelfUpdate()
    {
        if(!Active || !ReceiveUpdates) return; 

        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].UpdateEffect();
        }
    }

    public Entity()
    {
        Id = GetLowestID();

        Setup();
    }

    public Entity(string name)
    {
        this.name = name;

        Id = GetLowestID();
        
        Setup();
    }

    public Entity(Entity parent)
    {
        this.SetParent(parent);

        Id = GetLowestID();
        
        Setup();
    }

    void Setup()
    {
        GameController.I.entities.Add(Id, this);

        OnLoad();
    }


    [OnDeserialized]
    void Setup(StreamingContext sc)
    {
        OnLoad();
    }

    public void Delete()
    {
        if(GameController.I.entities.ContainsKey(Id))
        {
            for (int i = 0; i < childs.Count; i++)
            {
                GameController.I.entities[childs[i]].SetParent(null);
            }

            for (int i = 0; i < Behaviours.Count; i++)
            {
                Behaviours[i].RemoveBehaviour();
            }
            GameController.I.entities.Remove(Id);
        }
        else throw new InvalidDataException("Tried to delete a non-existing Entity");
        
        OnDelete();
    }

    protected virtual void OnLoad()
    {
        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].owner = this;
            Behaviours[i].OnAdd();
        }
    }
    protected virtual void OnUnload() 
    {
        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].OnRemove();
        }
    }
    protected virtual void OnDelete()
    {
        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].OnRemove();
        }
    }

    #region Utilities

    public void SetParent(Entity? newParent)
    {
        if(newParent != null)
        {
<<<<<<< Updated upstream
            newParent.childs.Add(this.Id);
            Parent = newParent.Id;
=======
            if(GameController.Entities.ContainsKey(newParent.Id))
            {
                if(!IsMyChild(newParent) && newParent != this)
                {
                    newParent.Childs.Add(this.Id);
                    Parent = newParent.Id;

                } else
                ErrorManager.LogError("Cannot set a child of an Entity as its Parent");
            } 
            else ErrorManager.LogError("Did not find Entity with ID: " + newParent);
            
>>>>>>> Stashed changes
        } else
        {
            Parent = 0;
        }
        
    }

    public void SetParent(Entity? newParent, bool notifyParent)
    {
        if(newParent != null)
        {
            if(notifyParent) newParent.childs.Add(this.Id);
            Parent = newParent.Id;
        } else
        {
            Parent = 0;
        }
        
    }

    public static ulong GetLowestID()
    {
        ulong Lowest = 1;

        List<ulong> List = [.. GameController.I.entities.Keys.Order()];
        for (int i = 0; i < List.Count; i++)
        {
            if(Lowest == List[i]) Lowest ++;
        }

        return Lowest;
    }

    public bool IsMyChild(Entity entity)
    {
        if(childs.Contains(entity.Id))
        {
            return true;
        } else
        {
            for (int i = 0; i < childs.Count; i++)
            {
                if(GameController.I.entities[childs[i]].IsMyChild(entity)) return true;
            }

            return false;
        }
    }

     public bool IsMyChild(ulong entityId)
    {
        if(childs.Contains(Id))
        {
            return true;
        } else
        {
            for (int i = 0; i < childs.Count; i++)
            {
                if(GameController.I.entities[childs[i]].IsMyChild(entityId)) return true;
            }

            return false;
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("name", name);
        info.AddValue("Parent", Parent);
        info.AddValue("Active", Active);
        info.AddValue("ReceiveUpdates", ReceiveUpdates);

        info.AddValue("childs", childs, typeof(List<ulong>));
    }

    #endregion
}