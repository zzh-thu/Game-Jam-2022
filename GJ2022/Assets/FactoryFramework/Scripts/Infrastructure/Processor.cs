using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FactoryFramework
{
    public class Processor : Building, IInput, IOutput
    {
        public int numInputs = 1;
        public int numOutputs = 1;
        private Dictionary<Item, int> _inputs = new Dictionary<Item,int>();
        private Dictionary<Item, int> _outputs = new Dictionary<Item, int>();
        public Recipe recipe;

        public Recipe[] validRecipes = new Recipe[0];

        private IEnumerator currentRoutine;

        public override void ProcessLoop()
        {
            if (CanStartProduction())
            {
                currentRoutine = MakeOutput();
                StartCoroutine(currentRoutine);
            }
        }

        public void ClearInternalStorage()
        {
            _inputs = new Dictionary<Item, int>();
        }
        public bool AssignRecipe(Recipe recipe, bool clearStorage =false)
        {
            this.recipe = recipe;
            if (clearStorage)
                ClearInternalStorage();
            return true;
        }

        private bool CanStartProduction()
        {
            // need a recipe to make!
            if (recipe == null)
            {
                if (_inputs.Keys.Count == 0) return false;
                // we can try to find a recipe
                Recipe[] matchedRecipes = RecipeFinder.FilterRecipes(_inputs.Keys.ToArray(), numOutputs, validRecipes);
                if (matchedRecipes.Length > 0)
                {
                    AssignRecipe(matchedRecipes[0]);
    
                } else
                    return false;
            }
            // cannot start a new production cycle while one is running
            if (currentRoutine != null) return false;
            //check for outputs being full
            foreach (Item item in recipe.OutputItems)
            {
                _outputs.TryGetValue(item, out int amount);
                if (amount >= item.itemData.maxStack) return false;
            }
            // check that we have enough input ingredients
            for (int i = 0; i < recipe.inputs.Length; i++)
            {
                Item item = recipe.InputItems[i];
                _inputs.TryGetValue(item, out int amount);
                if (amount < recipe.inputs[i].amount) return false;
            }
            return true;
        }
        private void ConsumeInputIngredients()
        {
            for (int i = 0; i < recipe.inputs.Length; i++)
            {
                Item item = recipe.InputItems[i];
                _inputs[item] -= recipe.inputs[i].amount;
            }
        }
        private void CreateOutputProducts()
        {
            for (int i = 0; i < recipe.outputs.Length; i++)
            {
                Item item = recipe.OutputItems[i];
                if (_outputs.ContainsKey(item))
                {
                    _outputs[item] = Mathf.Min(_outputs[item] + recipe.outputs[i].amount, item.itemData.maxStack);
                } else
                {
                    _outputs.Add(item, 1);
                }
            }
        }

        public bool CanGiveOutput(Item filter = null)
        {
            if (filter != null)
            {
                _outputs.TryGetValue(filter, out int amount);
                if (amount > 0) return true;
            } else
            {
                foreach (KeyValuePair<Item, int> availableOutput in _outputs)
                {
                    if (availableOutput.Value > 0) return true;
                }

            }
            return false;
        }
        public Item OutputType() {
            foreach (KeyValuePair<Item, int> availableOutput in _outputs)
            {
                if (availableOutput.Value > 0) return availableOutput.Key;
            }
            return null;
        }
        public Item GiveOutput(Item filter = null)
        {

            if (filter != null)
            {
                _outputs.TryGetValue(filter, out int amount);
                if (amount > 0)
                {
                    _outputs[filter] -= 1;
                    return filter;
                }
            }
            else
            {
                foreach (KeyValuePair<Item, int> availableOutput in _outputs)
                {
                    if (availableOutput.Value > 0)
                    {
                        _outputs[availableOutput.Key] -= 1;
                        return availableOutput.Key;
                    }
                }

            }
            return null;
        }

        public void TakeInput(Item item)
        {
            if (_inputs.ContainsKey(item))
                _inputs[item] += 1;
            else
                _inputs.Add(item, 1);
        }
        public bool CanTakeInput(Item item)
        {
            if (item == null) return false;
            
            if (_inputs.ContainsKey(item))
            {
                return _inputs[item] < item.itemData.maxStack;
            } else
            {
                return _inputs.Keys.Count < numInputs;
            }
        }

        IEnumerator MakeOutput()
        {
            ConsumeInputIngredients();
            float _t = 0f;
            while (_t < recipe.tickCost)
            {
                //FIXME custom tick?
                _t += Time.deltaTime;
                yield return null;
            }
            CreateOutputProducts();

            // do we need to un-assign the current recipe? Check if we can make any more
            bool isEmpty = true;
            foreach(KeyValuePair<Item,int> pair in _inputs)
            {
                isEmpty &= pair.Value == 0;
            }
            if (isEmpty)
            {
                AssignRecipe(null,true);
            }

            currentRoutine = null;
        }

    }
}