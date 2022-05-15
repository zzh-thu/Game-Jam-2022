using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace FactoryFramework.Editor
{
    [CustomEditor(typeof(Recipe))]
    public class RecipeEditor : UnityEditor.Editor
    {
        private Recipe _recipe;
        private VisualElement _visualElement;
        private VisualTreeAsset _visualTreeAsset;

        private VisualTreeAsset _ingredientTreeAsset;

        private void OnEnable()
        {

            _visualElement = new VisualElement();

            _visualTreeAsset = EditorUtils.LoadFirstAssetByFilter<VisualTreeAsset>("RecipeEditor");
            _ingredientTreeAsset = EditorUtils.LoadFirstAssetByFilter<VisualTreeAsset>("RecipeIngredientEditor");

            StyleSheet stylesheet = EditorUtils.LoadFirstAssetByFilter<StyleSheet>("FactoryFramework");
            _visualElement.styleSheets.Add(stylesheet);

            DisplayRecipeData();
        }

        public override VisualElement CreateInspectorGUI()
        {
            _recipe = (Recipe)target;

            if (_recipe.inputs == null)
                _recipe.inputs = new ItemStack[1];
            if(_recipe.outputs == null)
                _recipe.outputs = new ItemStack[1];

            _visualElement.Clear();

            _visualTreeAsset.CloneTree(_visualElement);

            _visualElement = DisplayRecipeData();

            return _visualElement;
        }

        VisualElement DisplayRecipeData()
        {
            if (_recipe == null) return _visualElement;

            Button addInputButton = _visualElement.Q<Button>(name: "add-input-button");
            addInputButton?.RegisterCallback<ClickEvent>(e =>
            {
                var newItems = _recipe.inputs.ToList();
                ItemStack istack = new ItemStack();
                newItems.Add(istack);
                _recipe.inputs = newItems.ToArray();
                EditorUtility.SetDirty(_recipe);
                RefreshListView();
            });

            Button addOutputButton = _visualElement.Q<Button>(name: "add-output-button");
            addOutputButton?.RegisterCallback<ClickEvent>(e =>
            {
                var newItems = _recipe.outputs.ToList();
                ItemStack istack = new ItemStack();
                newItems.Add(istack);
                _recipe.outputs = newItems.ToArray();
                EditorUtility.SetDirty(_recipe);
                RefreshListView();
            });

            if(addInputButton != null) addInputButton.style.visibility = _recipe.inputs.ToList().Count == 0 ? Visibility.Visible : Visibility.Hidden;
            if(addOutputButton != null) addOutputButton.style.visibility = _recipe.outputs.ToList().Count == 0 ? Visibility.Visible : Visibility.Hidden;

            Func<VisualElement> makeItem = () => _ingredientTreeAsset.CloneTree();
            Action<VisualElement, int> bindInputItem = (e, i) =>
            {
                if (_recipe == null || i >= _recipe.inputs.Count()) return;

                var icon = e.Q<VisualElement>(name: "ingredient-icon");
                icon.style.backgroundImage = (_recipe.inputs[i].item != null && _recipe.inputs[i].item.icon != null) ? _recipe.inputs[i].item.icon.texture : null;

                var itemField = e.Q<ObjectField>(name: "item-reference");
                itemField.objectType = typeof(Item);
                itemField.value = _recipe.inputs[i].item;
                itemField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
                {
                    ItemStack istack = _recipe.inputs[i];
                    istack.item = (Item)e.newValue;
                    icon.style.backgroundImage = (istack.item != null ? istack.item.icon : null != null) ? istack.item.icon.texture : null;
                    _recipe.inputs[i] = istack;
                    EditorUtility.SetDirty(_recipe);
                });

                var amountField = e.Q<IntegerField>();
                amountField.value = _recipe.inputs[i].amount;
                amountField.RegisterCallback<ChangeEvent<int>>(e =>
                {
                    ItemStack istack = _recipe.inputs[i];
                    istack.amount = (int)e.newValue;
                    _recipe.inputs[i] = istack;
                    EditorUtility.SetDirty(_recipe);
                });

                Button insertBtn = e.Q<Button>(name: "insert");
                insertBtn.RegisterCallback<ClickEvent>(e =>
                {
                    var newItems = _recipe.inputs.ToList();
                    ItemStack istack = new ItemStack();
                    newItems.Insert(i+1,istack);
                    _recipe.inputs = newItems.ToArray();
                    EditorUtility.SetDirty(_recipe);
                    RefreshListView();
                });

                Button deleteBtn = e.Q<Button>(name: "delete");
                deleteBtn.RegisterCallback<ClickEvent>(e =>
                {
                    var newItems = _recipe.inputs.ToList();
                    newItems.RemoveAt(i);
                    _recipe.inputs = newItems.ToArray();
                    EditorUtility.SetDirty(_recipe);
                    RefreshListView();
                   
                });
            };

            Action<VisualElement, int> bindOutputItem = (e, i) =>
            {
                if (_recipe == null || i >= _recipe.outputs.Count()) return;

                var icon = e.Q<VisualElement>(name: "ingredient-icon");
                icon.style.backgroundImage = (_recipe.outputs[i].item != null && _recipe.outputs[i].item.icon != null) ? _recipe.outputs[i].item.icon.texture : null;

                var itemField = e.Q<ObjectField>(name: "item-reference");
                itemField.objectType = typeof(Item);
                itemField.value = _recipe.outputs[i].item;
                itemField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
                {
                    ItemStack istack = _recipe.outputs[i];
                    istack.item = (Item)e.newValue;
                    icon.style.backgroundImage = (istack.item != null && istack.item.icon != null) ? istack.item.icon.texture : null;
                    _recipe.outputs[i] = istack;
                    EditorUtility.SetDirty(_recipe);
                });

                var amountField = e.Q<IntegerField>();
                amountField.value = _recipe.outputs[i].amount;
                amountField.RegisterCallback<ChangeEvent<int>>(e =>
                {
                    ItemStack istack = _recipe.outputs[i];
                    istack.amount = (int)e.newValue;
                    _recipe.outputs[i] = istack;
                    EditorUtility.SetDirty(_recipe);
                });

                Button insertBtn = e.Q<Button>(name: "insert");
                insertBtn.RegisterCallback<ClickEvent>(e =>
                {
                    var newItems = _recipe.outputs.ToList();
                    ItemStack istack = new ItemStack();
                    newItems.Insert(i + 1, istack);
                    _recipe.outputs = newItems.ToArray();
                    EditorUtility.SetDirty(_recipe);
                    RefreshListView();
                });

                Button deleteBtn = e.Q<Button>(name: "delete");
                deleteBtn.RegisterCallback<ClickEvent>(e =>
                {
                    var newItems = _recipe.outputs.ToList();
                    newItems.RemoveAt(i);
                    _recipe.outputs = newItems.ToArray();
                    EditorUtility.SetDirty(_recipe);
                    RefreshListView();

                });
            };

            var inputListView = _visualElement.Q<ListView>(name:"input-items");
            inputListView.makeItem = makeItem;
            inputListView.bindItem = bindInputItem;
            inputListView.itemsSource = _recipe != null ? _recipe.inputs : new ItemStack[0];
            inputListView.selectionType = SelectionType.None;
            inputListView.itemHeight = 65;
            inputListView.style.height = (_recipe != null ? _recipe.inputs.Count() : 1) * 65;
            inputListView.Refresh();

            var outputListView = _visualElement.Q<ListView>(name: "output-items");
            outputListView.makeItem = makeItem;
            outputListView.bindItem = bindOutputItem;
            outputListView.itemsSource = _recipe != null ? _recipe.outputs : new ItemStack[0];
            outputListView.selectionType = SelectionType.None;
            outputListView.itemHeight = 65;
            outputListView.style.height = (_recipe != null ? _recipe.outputs.Count() : 1) * 65;
            outputListView.Refresh();

            return _visualElement;
        }

        private void RefreshListView()
        {
            var inputView = _visualElement.Q<ListView>(name: "input-items");
            inputView.itemsSource = _recipe.inputs;
            inputView.Refresh();
            inputView.style.height = _recipe.inputs.Length * 65;

            var outputView = _visualElement.Q<ListView>(name: "output-items");
            outputView.itemsSource = _recipe.outputs;
            outputView.Refresh();
            outputView.style.height = _recipe.outputs.Length * 65;

            Button addInputButton = _visualElement.Q<Button>(name: "add-input-button");
            Button addOutputButton = _visualElement.Q<Button>(name: "add-output-button");
            addInputButton.style.visibility = _recipe.inputs.ToList().Count == 0 ? Visibility.Visible : Visibility.Hidden;
            addOutputButton.style.visibility = _recipe.outputs.ToList().Count == 0 ? Visibility.Visible : Visibility.Hidden;
        }
    }
}