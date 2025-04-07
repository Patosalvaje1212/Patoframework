using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using PatoframeWork.Inspector;


namespace PatoframeWork;

public class Entity
{
    /// <summary>
    /// Shown name of the Entity.
    /// </summary>
    [InspectorHide]
    public string Name = "Entity";

    /// <summary>
    /// Inmutable Id of an Entity.
    /// </summary>
    /// <remarks>
    /// Every Id is unique to that Entity. Use GameController.FindEntity(])to find an Entity by its Id
    /// </remarks>

    [InspectorShowOrder(6), InspectorNonEditable]
    public ulong Id { get; private set; }

    /// <summary>
    /// Id of the Parent Entity.
    /// </summary>
    /// <remarks>
    /// is 0 if has no Parent
    /// </remarks>
    public ulong Parent { get; private set; }


    /// <summary>
    /// Collection of the Ids of the Child Entities.
    /// </summary>
    [InspectorHide]
    public List<ulong> Childs { get; private set; } = [];

    /// <summary>
    /// Local position of the Entity, relative to its Parent ( if it has any ).
    /// </summary>
    [InspectorShowOrder(3)]
    public Vector2 LocalPosition;


    /// <summary>
    /// Collection of all the Behaviours an entity has. 
    /// </summary>
    /// <remarks>
    /// Use FindBehaviours to get an specific one
    /// </remarks>
    [InspectorHide]
    public List<Behaviour> Behaviours { get; private set; }= [];


    
    /// <summary>
    /// Position of the Entity in relation to all its Parent Entities.
    /// </summary>
    /// <remarks>
    /// If no Parent Entities exist, its equal to LocalPosition 
    /// </remarks>
    [InspectorNonEditable, JsonIgnore, InspectorShowOrder(2)]
    public Vector2 GlobalPosition
    {
        get
        {   
            return (Parent != 0 ? GameController.FindEntity(Parent).GlobalPosition : Vector2.Zero) + LocalPosition;
        }
        set
        {
            LocalPosition = Parent != 0 ? value - GlobalPosition : value;
        }
    }

    private bool receiveUpdates;

    /// <summary>
    /// If enabled, the Behaviours of this Entity get updated each frame.
    /// </summary>
    [InspectorShowOrder(2)]
    public bool ReceiveUpdates 
    {
        get => receiveUpdates;

        set
        {
            if(value && !receiveUpdates)
            GameController.Update += SelfUpdate;
            if(!value && receiveUpdates)
            GameController.Update -= SelfUpdate;

            receiveUpdates = value;
        }   
    }

    private bool active = true;
    /// <summary>
    /// If disabled, this Entity and its Behaviours will act like if they didnt exist.
    /// </summary>
    [InspectorShowOrder(5)]
    public bool Active
    {
        get
        {
            return Parent != 0 ? GameController.FindEntity(Parent).Active && active : active;
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

    /// <summary>
    /// Its called once each frame, override to control how often the Behaviours of this Entity get Updated.
    /// </summary>
    /// <remarks>
    /// By default, it doesnt get called if the Entity is unactive, or if ReceiveUpdates is set to false
    /// </remarks>
    public virtual void SelfUpdate()
    {
        if(!Active || !ReceiveUpdates) return; 

        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].UpdateEffect();
        }
    }

    /// <summary>
    /// This constructor initializes a new Entity.
    /// </summary>
    public Entity()
    {
        Setup();
    }

    /// <summary>
    /// This constructor initializes a new Entitywith  
    /// <paramref name="name"/> as its name.
    /// </summary>
    public Entity(string name)
    {
        this.Name = name;
        
        Setup();
    }

    /// <summary>
    /// This constructor initializes a new Entity, and sets its Parent to
    /// <paramref name="parent"/>.
    /// </summary>
    public Entity(Entity parent)
    {
        this.SetParent(parent);

        Setup();
    }

    void Setup()
    {
        Id = GameController.GetLowestFreeID();

        GameController.AddEntity(this);

        OnLoad();
    }


    [OnDeserialized]
    void Setup(StreamingContext sc)
    {

        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].Owner = this;
            Behaviours[i].OnAdd();
        }

        OnLoad();
    }

    /// <summary>
    /// Remove the Entity from the Entity List.
    /// </summary>
    public void Delete(bool isInstant = false, bool DeleteChilds = false)
    {
        if(!isInstant) OnDelete();

        if(GameController.TryFindEntity(Id) is not null)
        {
            this.SetParent(null);
            
            if(DeleteChilds)
            {
                for (int i = 0; i < Childs.Count; i++)
                {
                    GameController.FindEntity(Childs[i]).Delete(isInstant, true);
                }


            } else
            {
                for (int i = 0; i < Childs.Count; i++)
                {
                    GameController.FindEntity(Childs[i]).SetParent(null);
                }
            }
            

            for (int i = 0; i < Behaviours.Count; i++)
            {
                Behaviours[i].RemoveBehaviour();
            }

            GameController.RemoveEntity(Id);
        }
        else throw new InvalidDataException("Tried to delete a non-existing Entity");
    }

    /// <summary>
    /// Override this method to set custom behaviour when this object gets Loaded/Instantiated/Enabled into a scene.
    /// </summary>
    protected virtual void OnLoad() {}

    /// <summary>
    /// Override this method to set custom behaviour when this object gets disabled into a scene.
    /// </summary>
    protected virtual void OnUnload() {}

    /// <summary>
    /// Override this method to set custom behaviour when this object gets Deleted.
    /// </summary>
    protected virtual void OnDelete() {}

    #region Utilities


    /// <summary>
    /// Changes the current Parent of the Entity to <paramref name="newParent"/>.
    /// </summary>
    /// <remarks>
    /// If <paramref name="newParent"/> is null, it unparents the Entity
    /// </remarks>
    public void SetParent(Entity? newParent)
    {
        if(Parent != 0) 
                GameController.FindEntity(Parent).Childs.Remove(Id);

        if(newParent != null)
        {
            if(!IsMyChild(newParent) && newParent != this)
            {
                newParent.Childs.Add(this.Id);
                Parent = newParent.Id;

            } else
            LogManager.LogError("Cannot set a child of an Entity as its Parent");
            
        } else
        { 
            Parent = 0;
        }
        
    }

    /// <summary>
    /// Changes the current Parent of the Entity with ID <paramref name="newParentID"/>.
    /// </summary>
    /// <remarks>
    /// If <paramref name="newParentID"/> is 0, it unparents the Entity.
    /// </remarks>
    public void SetParent(ulong newParentID)
    {
        if(Parent != 0) 
            GameController.FindEntity(Parent).Childs.Remove(Id);

        if(newParentID != 0)
        {
            if(!IsMyChild(newParentID) && newParentID != Id)
            {
                GameController.FindEntity(newParentID).Childs.Add(this.Id);
                Parent = newParentID;

            } else
            LogManager.LogError("Cannot set a child of an Entity as its Parent");
            
        } else
        {
            
            Parent = 0;
        }

    }

    /// <summary>
    /// Changes the current Parent of the Entity to <paramref name="newParent"/>, without updating the Parent about it.
    /// </summary>
    public void SetParentNoNotify(ulong newParent)
    {
        if(newParent != 0)
        {
            if(GameController.TryFindEntity(newParent) != null)
                Parent = newParent;
            else
                throw new ArgumentNullException(nameof(newParent), $"The Entity with ID {newParent} could not be found.");

        } else
        {
            throw new ArgumentNullException(nameof(newParent), "You cannot use SetParentNoNotify to unset the parent of an Entity, use SetParent(null) instead");
        }
        
    }


    /// <summary>
    /// Checks if the Entity <paramref name="entity"/>, is a child of the current Entity.
    /// <paramref name="SearchRecursively"/> makes it search recursively in the childs of childs.
    /// </summary>
    public bool IsMyChild(Entity entity, bool SearchRecursively = true)
    {
        if(Childs.Contains(entity.Id))
        {
            return true;
        } else
        {
            if(SearchRecursively)
            for (int i = 0; i < Childs.Count; i++)
            {
                if(GameController.FindEntity(Childs[i]).IsMyChild(entity)) return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Checks if the Entity <paramref name="entityId"/>, is a child of the current Entity.
    /// <paramref name="SearchRecursively"/> makes it search recursively in the childs of childs.
    /// </summary>
    public bool IsMyChild(ulong entityId, bool SearchRecursively = true)
    {
        if(Childs.Contains(Id))
        {
            return true;
        } else
        {
            if(SearchRecursively)
            for (int i = 0; i < Childs.Count; i++)
            {
                if(GameController.FindEntity(Childs[i]).IsMyChild(entityId)) return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Creates an exact copy of the current entity ( with a different ID ).
    /// </summary>
    /// <remarks>
    /// Copies the Behaviours and Child Entities as well
    /// </remarks>
    public Entity Duplicate()
    {
        Entity newEnt = new(Name)
        {
            Active = this.active,
            ReceiveUpdates = this.receiveUpdates,
            LocalPosition = this.LocalPosition
        };

        newEnt.SetParent(Parent);

        var InitList = Behaviours.ToList();

        for (int i = 0; i < InitList.Count; i++)
        {
            var newBeh = InitList[i].CloneBehaviour();

            newBeh.SwitchOwner(newEnt);
        }
        
        var InitChilds = Childs.ToList();

        for(int i = 0; i < InitChilds.Count; i++)
        {
            var childEnt = GameController.FindEntity(InitChilds[i]).Duplicate();

            childEnt.SetParent(newEnt.Id);
        }


        return newEnt;
    }

    #endregion
}