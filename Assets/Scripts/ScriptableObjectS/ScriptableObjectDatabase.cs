using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectS
{
    [CreateAssetMenu(fileName = "ScriptableObjectDatabase", menuName = "Database/ScriptableObject Database")]
    public class ScriptableObjectDatabase<T> : ScriptableObject where T : ScriptableObject
    {
        [SerializeField]
        private List<T> objects = new List<T>();

        /// <summary>
        /// Returns a list of all stored ScriptableObjects.
        /// </summary>
        public List<T> GetAllObjects()
        {
            return objects;
        }

        /// <summary>
        /// Adds a ScriptableObject to the database if it's not already added.
        /// </summary>
        public void AddObject(T obj)
        {
            if (!objects.Contains(obj))
            {
                objects.Add(obj);
            }
        }

        /// <summary>
        /// Removes a ScriptableObject from the database.
        /// </summary>
        public void RemoveObject(T obj)
        {
            objects.Remove(obj);
        }

        /// <summary>
        /// Checks if a specific ScriptableObject exists in the database.
        /// </summary>
        public bool Contains(T obj)
        {
            return objects.Contains(obj);
        }
    }
}