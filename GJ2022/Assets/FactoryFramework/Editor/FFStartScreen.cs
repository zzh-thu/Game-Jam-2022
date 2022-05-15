using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using FactoryFramework;

[InitializeOnLoad]
public class FFStartup
{
    private static readonly string ChangeLogGUID = "9c554adf21d87ee40a93a2930401b3a6";
    static FFStartup()
    {

        var changelog = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(ChangeLogGUID));
        if (changelog != null && changelog.text.StartsWith("~"))
        {
            FFStartScreen.Init();
            string newText = changelog.text.Substring(1);
            File.WriteAllText(AssetDatabase.GUIDToAssetPath(ChangeLogGUID), newText);
            EditorUtility.SetDirty(changelog);
        } 

    }
}

public class FFStartScreen : EditorWindow
{
    [MenuItem("Window/FactoryFramework/Start Screen", false, 1997)]
    public static void Init()
    {
        // code just used to find guids\
        //string t = AssetDatabase.AssetPathToGUID("Assets/FactoryFramework/ChangeLog.txt");
        //Debug.Log(t);
        //string t = AssetDatabase.AssetPathToGUID("Assets/FactoryFramework/URP_demo.unitypackage");
        //Debug.Log(t);
        //string hdrp = AssetDatabase.AssetPathToGUID("Assets/FactoryFramework/HDRP_demo.unitypackage");
        //Debug.Log(hdrp);

        FFStartScreen window = GetWindow<FFStartScreen>("Factory Framework");
        window.minSize = new Vector2(300, 250);
        window.maxSize = new Vector2(300, 250);
        window.Show(true);

        Debug.Log(window);
    }

    private static readonly string ChangeLogGUID = "9c554adf21d87ee40a93a2930401b3a6";
    private static readonly string URPPackageGUID = "baafab9a553055b4b9258d0990c7b8ad";
    private static readonly string HDRPPackageGUID = "5ed41b97f47b79240a60d7b3e8f45aae";

    private static readonly string OnlineDocumentationURL = "https://docs.google.com/document/d/1U2zVDWSDcqG6s1sXl5qliNWZ3GclAbTwtYuxOxIfalg/edit?usp=sharing";
    private static readonly string AssetStoreURL = "https://u3d.as/2NNm";
    private static readonly string TwitterURL = "https://twitter.com/CtrlAltBees";
    private static readonly string DiscordURL = "https://discord.gg/9tnKg9XPpV";

  
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/FactoryFramework/Editor/FFStartScreen.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // handle Render Pipeline Imports
        var urpButton = root.Q<Button>("urp");
        urpButton.RegisterCallback<ClickEvent>((evt) =>
        {
            ImportSample(urpButton.text, URPPackageGUID);
        });
        var hdrpButton = root.Q<Button>("hdrp");
        hdrpButton.RegisterCallback<ClickEvent>((evt) =>
        {
            ImportSample(hdrpButton.text, HDRPPackageGUID);
        });

        // online documentation link(s)
        var onlineDocsButton = root.Q<Button>("docs");
        onlineDocsButton.RegisterCallback<ClickEvent>((evt) => Application.OpenURL(OnlineDocumentationURL));

            //settings open
        var settingsButton = root.Q<Button>("settings");
        settingsButton.RegisterCallback<ClickEvent>((evt) => SettingsService.OpenProjectSettings("Project/Factory Framework"));

        // community links
        var twitterLink = root.Q<Button>("twitter");
        twitterLink.RegisterCallback<ClickEvent>((evt) => Application.OpenURL(TwitterURL));
        var discordLink = root.Q<Button>("discord");
        discordLink.RegisterCallback<ClickEvent>((evt) => Application.OpenURL(DiscordURL));

        // get current version
        var changelog = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(ChangeLogGUID));
        Regex r = new Regex(@"v(\d+\.\d+\.\d)", RegexOptions.IgnoreCase);
        Match m = r.Match(changelog.text);
        Label currentVersion = root.Q<Label>("current-version");
        currentVersion.text = m.Value.ToString();

        // download version
        Label assetStoreLink = root.Q<Label>("asset-store-link");
        assetStoreLink.RegisterCallback<ClickEvent>((evt) =>
            {
                Application.OpenURL(AssetStoreURL);
            });
    }

    void ImportSample(string pipeline, string guid)
    {
        if (EditorUtility.DisplayDialog("Import Compatible Render Pipeline Assets", "Import the samples for " + pipeline + ", make sure the pipeline is properly installed before importing the samples.\n\nContinue?", "Yes", "No"))
        {
            AssetDatabase.ImportPackage(AssetDatabase.GUIDToAssetPath(guid), false);
        }
    }
}
