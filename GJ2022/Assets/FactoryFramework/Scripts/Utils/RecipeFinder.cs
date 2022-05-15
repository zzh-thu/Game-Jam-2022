using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace FactoryFramework {
    public class RecipeFinder
    {
        internal static Recipe[] _recipes;
        public static Recipe[] Recipes
        {
            get
            {
                if (_recipes == null)
                {
                    _recipes = Resources.LoadAll<Recipe>("");
                }
                return _recipes;
            }
        }
        public static Recipe[] FilterRecipes(Item[] inputs, int numOutputs=-1, Recipe[] whitelist=null)
        {
            // find recipes that match the given inputs and outputs
            Recipe[] recipes = Recipes.Where(r => r.InputItems.Except(inputs).Count()==0 && (numOutputs == -1 || r.OutputItems.Length == numOutputs)).ToArray(); 
            if (whitelist != null && whitelist.Length > 0)
                recipes = recipes.Where(r => whitelist.Contains(r)).ToArray();
            return recipes;
        }

    } 
}
