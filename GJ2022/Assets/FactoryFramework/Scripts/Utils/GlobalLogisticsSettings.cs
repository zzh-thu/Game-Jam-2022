using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEditor.UIElements;
#endif

namespace FactoryFramework
{
    // new type of settings object
    public class GlobalLogisticsSettings : ScriptableObject
    {
        public const string conveyorLogisticsSetttingsPath = "Assets/FactoryFramework/Resources/Settings/ConveyorLogisticsSettings.asset";

        //Scale of the belt mesh
        public float BELT_SCALE = 0.35f;
        //Spacing between items on belts
        public float BELT_SPACING = 1f;

        //How many belt segments fit in one unity unit
        public float BELT_SEGMENTS_PER_UNIT = 4f;
        //Turn radius for horizontal belt arcs. This also controls the control point spacing for spline paths
        public float BELT_TURN_RADIUS = 0.75f;
        //Turn radius for vertical belt arcs
        public float BELT_RAMP_RADIUS = 0.65f;
        //Belts whose start and end y positions differ by more than this value will have vertical arcs as well
        public float BELT_VERTICAL_TOLERANCE = 5f;
        // show all of the debug logs 
        public bool SHOW_DEBUG_LOGS = false;

        //Solver types
        public enum PathSolveType {SMART, SPLINE, SEGMENT };
        //Current solver type
        public PathSolveType PATHTYPE = PathSolveType.SMART;

        public static GlobalLogisticsSettings instance;
        internal static GlobalLogisticsSettings GetOrCreateSettings()
        {
#if UNITY_EDITOR
            string path = FileUtil.GetProjectRelativePath(conveyorLogisticsSetttingsPath);
            GlobalLogisticsSettings settings = AssetDatabase.LoadAssetAtPath<GlobalLogisticsSettings>(conveyorLogisticsSetttingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GlobalLogisticsSettings>();
                settings.BELT_SPACING = 1f;
                settings.BELT_SEGMENTS_PER_UNIT = 4f;
                settings.BELT_TURN_RADIUS = 0.5f;
                settings.BELT_RAMP_RADIUS = 0.5f;
                settings.BELT_VERTICAL_TOLERANCE = 0.1f;
                settings.PATHTYPE = PathSolveType.SMART;
                Directory.CreateDirectory(Path.GetDirectoryName(conveyorLogisticsSetttingsPath));
                AssetDatabase.CreateAsset(settings, conveyorLogisticsSetttingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#else
            var settings = Resources.LoadAll<GlobalLogisticsSettings>("Settings").First(); 
#endif
            return settings;
        }
#if UNITY_EDITOR
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }

#if UNITY_EDITOR


    // Register a SettingsProvider using UIElements
    static class conveyorLogisticsSettingsUIElementsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateConveyorLogisticsSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Factory Framework", SettingsScope.Project)
            {
                label = "Factory Framework",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = GlobalLogisticsSettings.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    
                    var title = new Label()
                    {
                        text = "Factory Framework Settings"
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement()
                    {
                        style =
                        {
                        flexDirection = FlexDirection.Column
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    properties.Add(new PropertyField(settings.FindProperty("BELT_SPACING"), "Item Spacing on Belts"));
                    properties.Add(new PropertyField(settings.FindProperty("BELT_SCALE"), "Scale of belt meshes"));
                    properties.Add(new PropertyField(settings.FindProperty("BELT_SEGMENTS_PER_UNIT"), "Generated mesh segments per Unity unit"));
                    properties.Add(new PropertyField(settings.FindProperty("BELT_TURN_RADIUS"), "Turn radius for belts"));
                    properties.Add(new PropertyField(settings.FindProperty("BELT_RAMP_RADIUS"), "Vertical turn radius for SmartPath belts"));
                    properties.Add(new PropertyField(settings.FindProperty("BELT_VERTICAL_TOLERANCE"), "SmartPath vertical tolerance"));
                    properties.Add(new PropertyField(settings.FindProperty("PATHTYPE"), "Solver type for paths"));

                    rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Item Spacing on Belt" })
            };
            return provider;
        }
    }
#endif
}
