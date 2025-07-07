using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectS
{
    public class ScriptableObjectScannerRuntime
    {
        /// <summary>
        /// Finds all ScriptableObject instances of a specific type T in the Resources folder at runtime.
        /// </summary>
        /// <typeparam name="T">The type of ScriptableObject to find.</typeparam>
        /// <returns>A list of ScriptableObject instances of type T.</returns>
        public static List<T> FindAllInstances<T>() where T : ScriptableObject
        {
            // Load all ScriptableObjects of type T from the Resources folder
            T[] objects = Resources.LoadAll<T>("");
            return new List<T>(objects);
        }

    }
}
