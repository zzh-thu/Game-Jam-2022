using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FactoryFramework
{
    public class Building : LogisticComponent
    {
        public UnityEvent<Building> OnBuildingDestroyed;

        private void Update()
        {
            this.ProcessLoop();
        }

        protected Recipe[] GetAllRecipes()
        {
            return Resources.LoadAll<Recipe>("");
        }

        private void OnDestroy()
        {
            OnBuildingDestroyed?.Invoke(this);
        }
    }
}