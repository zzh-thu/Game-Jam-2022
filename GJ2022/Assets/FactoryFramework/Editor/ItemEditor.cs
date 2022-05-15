using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FactoryFramework;

namespace FactoryFramework.Editor
{
    [CustomEditor(typeof(Item))]
    public class ItemEditor : UnityEditor.Editor
    {
        private Item _item;
        private VisualElement _visualElement;
        private VisualTreeAsset _visualTreeAsset;

        private void OnEnable()
        {
            
            _visualElement = new VisualElement();

            _visualTreeAsset = EditorUtils.LoadFirstAssetByFilter<VisualTreeAsset>("ItemEditor");

            StyleSheet stylesheet = EditorUtils.LoadFirstAssetByFilter<StyleSheet>("FactoryFramework");
            _visualElement.styleSheets.Add(stylesheet);

            //DisplayItemData();
        }

        public override VisualElement CreateInspectorGUI()
        {
            _item = (Item)target;

            _visualElement.Clear();

            _visualTreeAsset.CloneTree(_visualElement);

            _visualElement = DisplayItemData();

            return _visualElement;
        }

        VisualElement DisplayItemData()
        {
            // display Icon
            VisualElement iconDisplayField = _visualElement.Query<VisualElement>(name:"2D-icon").First();
            iconDisplayField.style.backgroundImage = _item.icon ? _item.icon.texture : null;
            // handle icon change
            ObjectField iconField = _visualElement.Query<ObjectField>("icon").First();
            iconField.objectType = typeof(Sprite);
            iconField.value = _item.icon;
            iconField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                _item.icon = (Sprite)e.newValue;
                iconDisplayField.style.backgroundImage = _item.icon.texture; // _item.icon ? _item.icon.texture : null;
                EditorUtility.SetDirty(_item);
            });
            // display 3d preview
            VisualElement previewDisplayField = _visualElement.Query<VisualElement>(name: "3D-preview").First();
            previewDisplayField.style.backgroundImage = _item.icon ? AssetPreview.GetAssetPreview(_item.prefab) : null;
            //handle field change
            ObjectField prefabField = _visualElement.Query<ObjectField>(name: "prefab").First();
            prefabField.objectType = typeof(GameObject);
            prefabField.allowSceneObjects = false;
            prefabField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                Debug.Log("TEST HERE");
                _item.prefab = (GameObject)e.newValue;
                previewDisplayField.style.backgroundImage = AssetPreview.GetAssetPreview(_item.prefab);
                EditorUtility.SetDirty(_item);
            });

            var descriptionField = _visualElement.Query<TextField>(name: "item-description").First();

            

            return _visualElement;
        }

    }
}