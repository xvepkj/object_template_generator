using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;

[System.Serializable]
public class GameObj
{
    public string name;
    public Vector3 position;
    public List<GameObj> gameObjs;
}

[System.Serializable]
public class Template
{
    public string name;
    public List<GameObj> gameObjs;
}

[System.Serializable]
public class TemplateList
{
    public List<Template> templates;
}

public class TemplateEditor : EditorWindow
{
    private Button newTemplate;
    private Button saveTemplate;
    private VisualElement list;
    private VisualElement templateView;
    private VisualElement objectProps;
    private VisualElement objectPosition;
    private ListView templateListView;
    private TextField objectName;
    private ListView objectListView; // ListView for GameObjs

    private bool isGUICreated = false;

    private TemplateList currentTemplateList;
    private const string jsonPath = "Assets/Resources/templates.json";

    [MenuItem("Tools/Template Editor")]
    public static void OpenEditorWindow()
    {
        TemplateEditor wnd = GetWindow<TemplateEditor>();
        wnd.titleContent = new GUIContent("Template Editor");
        wnd.maxSize = new Vector2(455, 730);
        wnd.minSize = wnd.maxSize;
        if (!wnd.isGUICreated)
        {
            wnd.CreateGUI();
            wnd.isGUICreated = true;
        }
    }

    private void CreateGUI()
    {
        if (isGUICreated)
        {
            return; // Prevent creating GUI multiple times
        }

        isGUICreated = true;

        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/ObjectTemplateEditor/Resources/UI Documents/TemplateEditorWindow.uxml"
        );
        VisualElement tree = visualTree.Instantiate();
        root.Add(tree);

        newTemplate = root.Q<Button>("New");
        saveTemplate = root.Q<Button>("Save");
        list = root.Q<VisualElement>("List");
        templateView = root.Q<VisualElement>("TemplateView");
        objectProps = root.Q<VisualElement>("ObjectProperties");
        objectPosition = root.Q<VisualElement>("Position");
        templateListView = root.Q<ListView>("TemplateListView");
        objectName = root.Q<TextField>("ObjectName");
        objectListView = root.Q<ListView>("ObjectList"); // ListView for GameObjs

        newTemplate.clickable.clicked += AddTemplate;
        saveTemplate.clickable.clicked += SaveTemplate;

        LoadTemplateList();

        // Subscribe to selection change event
        templateListView.onSelectionChange += OnTemplateSelected;

        // Initialize ObjectListView
        objectListView.makeItem = () => new Label();
        objectListView.bindItem = (element, index) =>
        {
            if (element is Label label)
            {
                if (index >= 0 && index < currentTemplateList.templates[templateListView.selectedIndex].gameObjs.Count)
                {
                    label.text = currentTemplateList.templates[templateListView.selectedIndex].gameObjs[index].name;
                }
                else
                {
                    label.text = "Invalid Index";
                }
            }
        };
    }

    private void OnTemplateSelected(IEnumerable<object> selectedItems)
    {
        // Check if an item is selected
        if (selectedItems != null && selectedItems.Any())
        {
            // Get the selected template index
            int selectedIndex = templateListView.selectedIndex;

            if (selectedIndex >= 0 && selectedIndex < currentTemplateList.templates.Count)
            {
                // Load the selected template data into the TemplateView
                Template selectedTemplate = currentTemplateList.templates[selectedIndex];
                LoadTemplateData(selectedTemplate);

                // Populate ObjectListView with GameObjData
                objectListView.itemsSource = selectedTemplate.gameObjs;
                objectListView.Rebuild(); // Refresh the ObjectListView
            }
        }
    }

    private void LoadTemplateData(Template template)
    {
        // Set the loaded template data in the TemplateView fields
        templateView.Q<TextField>("TemplateName").value = template.name;
        objectPosition.Q<TextField>("x").value = template.gameObjs[objectListView.selectedIndex].position.x.ToString();
        objectPosition.Q<TextField>("y").value = template.gameObjs[objectListView.selectedIndex].position.y.ToString();
        objectPosition.Q<TextField>("z").value = template.gameObjs[objectListView.selectedIndex].position.z.ToString();
    }

    private void LoadTemplateList()
    {
        string jsonText = File.ReadAllText(jsonPath);

        if (string.IsNullOrEmpty(jsonText))
        {
            currentTemplateList = new TemplateList { templates = new List<Template>() };
        }
        else
        {
            currentTemplateList = JsonUtility.FromJson<TemplateList>(jsonText);
        }

        templateListView.itemsSource = currentTemplateList.templates;
        templateListView.makeItem = () => new Label();
        templateListView.bindItem = (element, index) =>
        {
            if (element is Label label)
            {
                if (index >= 0 && index < currentTemplateList.templates.Count)
                {
                    label.text = currentTemplateList.templates[index].name;
                }
                else
                {
                    label.text = "Invalid Index";
                }
            }
        };
        templateListView.Rebuild(); // Refresh the TemplateListView
    }

    private void AddTemplate()
    {
        Template template = new Template
        {
            name = templateView.Q<TextField>("TemplateName").value,
            gameObjs = new List<GameObj>()
        };

        currentTemplateList.templates.Add(template);
        string updatedJson = JsonUtility.ToJson(currentTemplateList);
        File.WriteAllText(jsonPath, updatedJson);

        LoadTemplateList();

        // Clear the TemplateView fields after adding a new template
        templateView.Q<TextField>("TemplateName").value = "";
    }

    private void SaveTemplate()
    {
        // Implement saving individual templates if needed
    }
}
