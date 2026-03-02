

using System.Collections;

namespace PF;

/// <summary>
/// Class that manages mappers 
/// </summary>
public class BehaviourMapManager
{
    /// <summary>
    /// Dictionary of all the created mapers <br/>
    /// 
    /// <list type="bullet">
    /// <item>
    /// <c>Key</c>: type of the mapper
    /// </item>
    /// <item>
    /// <c>Value</c>: intance of the BehaviourMap class, that contains a maper of the target type
    /// </item>
    /// </list>
    /// </summary>
    public Dictionary<Type, BehaviourMap> mappers = [];


    /// <summary>
    /// Retrieves or creates a new mapper of a type.
    /// </summary>
    /// <typeparam name="T">Type of the mapper to retrieve/create</typeparam>
    /// <param name="dictionary">The retrieved/created Mapper</param>
    /// <param name="createIfNotFound">Whether or not to create a mapper of type <c>T</c> if not existing.</param>
    /// <returns><c>true</c> if the target mapper was, found and retrieved, <c>false</c> otherwise <br/> Won't return <c>false</c> if <c>createIfNotFound</c> is <c>true</c></returns>
    /// <exception cref="NullReferenceException"></exception>
    public bool GetMapper<T>(out IDictionary dictionary, bool createIfNotFound = true) where T : class
    {
        if (mappers.ContainsKey(typeof(T)))
        {
            dictionary = mappers[typeof(T)].GetMapper();
            return true;
        }

        if(createIfNotFound)
        {
            
            BehaviourMap? map = new BehaviourMap.Map<T>();

            if (map == null)
                throw new NullReferenceException("Couldnt create mapper for class: " + typeof(T).Name);
            else
            {
                mappers.Add(typeof(T), map);
                Logger.Log("Created mapper of: " + typeof(T).Name, Logger.MessageType.Other);

                dictionary = map.GetMapper();
                return true;
            }
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        dictionary = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        return false;
    }
}