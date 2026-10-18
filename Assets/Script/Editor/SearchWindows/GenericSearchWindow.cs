using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
public abstract class GenericSearchWindow<T> : EditorWindow
{
    protected const int DEFAULT_WIDTH = 300;
    protected const int DEFAULT_HEIGHT = 400;

    protected static Styles m_styles;
    public abstract string searchWindowTitle { get; }
    protected string m_windowTitle;
    public bool inSubmenu = false;

    public Vector2 menuScrollPos;
    protected int mouseOverIndex = 0;
    protected int selectionIndex = 0;

    public bool useSearch = false;
    protected string searchText = "";
    protected bool hasSearchText
    {
        get { return !string.IsNullOrEmpty(searchText); }
    }

    protected List<SearchableMenuItem> _treeItems;

    private List<SearchableMenuItem> _displayedItems;
    protected List<SearchableMenuItem> displayedItems
    {
        get { if (_displayedItems == null) _displayedItems = _treeItems; return _displayedItems; }
        set { _displayedItems = value; }
    }

    protected object[] data;
    public Action<T> OnMenuItemSelected;
    public Action<T, object[]> OnMenuItemSelected2;
    private Rect lastRectMouseOver;

    //............................................
    // Helper Classes
    //............................................

    protected class SearchableMenuItem
    {

        public string title;
        public string tooltip;
        public T item;
        public bool isParent;
        public bool isOption => item != null;

        public SearchableMenuItem parent;
        public List<SearchableMenuItem> children = new List<SearchableMenuItem>();

        public SearchableMenuItem(string title, bool isParent, SearchableMenuItem parent, string tooltip = "none")
        {
            this.title = title;
            this.isParent = isParent;
            this.tooltip = tooltip;
            this.parent = parent;
        }

        public SearchableMenuItem(string title, bool isParent, string tooltip = "none")
        {
            this.title = title;
            this.isParent = isParent;
            this.tooltip = tooltip;
        }

        public SearchableMenuItem(string title, T item, SearchableMenuItem parent, string tooltip = "none")
        {
            this.title = title;
            this.item = item;
            this.parent = parent;
            this.tooltip = tooltip;
        }
    }

    public class Styles
    {
        public GUIStyle searchTextField = "SearchTextField";
        public GUIStyle searchCancelButton = "SearchCancelButton";
        public GUIStyle searchCancelButtonEmpty = "SearchCancelButtonEmpty";

        public GUIStyle background = "grey_border";
        public GUIStyle componentItem = new GUIStyle("PR Label");
        public GUIStyle highlightedItem = new GUIStyle("PR Label");
        public GUIStyle header = new GUIStyle("In BigTitle");
        public GUIStyle leftArrow = "AC LeftArrow";
        public GUIStyle rightArrow = "AC RightArrow";
        public GUIStyle window = GUI.skin.window;
        public GUIStyle centralLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);

        public Styles()
        {
            highlightedItem.normal = highlightedItem.active;
            highlightedItem.hover = highlightedItem.active;
            highlightedItem.stretchWidth = true;

            componentItem.stretchWidth = true;

            centralLabel.normal.textColor = Color.black;
        }
    }

    //.................................................
    // Initilize
    //.................................................

    protected void DisplayInternal(Vector2 pos)
    {
        var buttonRect = new Rect(pos.x, pos.y, 1, 1);
        var windowSize = new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT);

        wantsMouseMove = true;
        _treeItems = new List<SearchableMenuItem>();
        selectionIndex = 0;
        InitializeMenu();

        DisplayRootItems();
        ShowAsDropDown(buttonRect, windowSize);
    }

    public abstract void InitializeMenu();

    protected SearchableMenuItem AddParentItem(string itemTitle, SearchableMenuItem parent)
    {
        var item = new SearchableMenuItem(itemTitle, true, parent, GetParentTooltip(itemTitle));
        parent.children.Add(item);
        _treeItems.Add(item);
        return item;
    }

    protected SearchableMenuItem AddRootItem(string itemTitle)
    {
        var treeItem = _treeItems.FirstOrDefault(x => x.title == itemTitle);
        if (treeItem == null)
        {
            treeItem = new SearchableMenuItem(itemTitle, true, GetParentTooltip(itemTitle));
            _treeItems.Add(treeItem);
        }
        return treeItem;
    }

    protected void AddMenuItem(T item, string itemTitle, SearchableMenuItem parent)
    {
        if (parent.children.Any(searchableMenuItem => searchableMenuItem.Equals(item)))
        {
            return;
        }

        var searchableItem = new SearchableMenuItem(itemTitle, item, parent, GetMenuItemTooltip(item));
        parent.children.Add(searchableItem);
        _treeItems.Add(searchableItem);
    }
    protected void AddRootMenuItem(T item, string itemTitle)
    {
        var searchableItem = new SearchableMenuItem(itemTitle, item, null, GetMenuItemTooltip(item));
        searchableItem.isParent = true;
        _treeItems.Add(searchableItem);
    }

    //.................................................
    // Main
    //.................................................

    void OnGUI()
    {
        CreateStyles();

        DrawSearchGUI();
        DrawTitleGUI();

        if (hasSearchText)
        {
            useSearch = true;
            inSubmenu = false;
        }
        else if (!hasSearchText && useSearch)
        {
            useSearch = false;
            m_windowTitle = searchWindowTitle;
            DisplayRootItems();
        }

        DrawMenuContent();

        DrawItemTooltip();

        HandleUserInput();
    }

    void OnDestroy()
    {
        if (_treeItems != null)
            _treeItems.Clear();
        if (displayedItems != null)
            displayedItems.Clear();
        OnMenuItemSelected = null;
    }

    private void HandleUserInput()
    {
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.KeyUp:
                {
                    if (evt.keyCode == KeyCode.Escape)
                    {
                        if (useSearch) // clear search
                        {
                            searchText = "";
                            selectionIndex = 0;
                            mouseOverIndex = 0;
                        }
                        else // close window
                        {
                            Close();
                        }

                        evt.Use();
                    }
                    else if (evt.keyCode == KeyCode.RightArrow) // go to submenu
                    {
                        if (displayedItems[selectionIndex].isParent == true)
                        {
                            displayedItems = _treeItems[selectionIndex].children;
                            m_windowTitle = _treeItems[selectionIndex].title;
                            inSubmenu = true;
                            selectionIndex = 0;
                            mouseOverIndex = 0;
                        }
                    }
                    else if (evt.keyCode == KeyCode.LeftArrow) // go to mainmenu
                    {
                        if (displayedItems.Count == 0 || displayedItems[selectionIndex].isParent == false)
                        {
                            displayedItems = _treeItems.Where(t => t.isParent).ToList();
                            m_windowTitle = searchWindowTitle;
                            inSubmenu = false;
                            selectionIndex = 0;
                            mouseOverIndex = 0;
                        }
                    }
                    else if (evt.keyCode == KeyCode.UpArrow) // item up
                    {
                        selectionIndex -= 1;
                        selectionIndex = Math.Max(0, selectionIndex);
                    }
                    else if (evt.keyCode == KeyCode.DownArrow) // item down
                    {
                        selectionIndex += 1;
                        selectionIndex = Math.Min(selectionIndex, displayedItems.Count - 1);
                    }
                    else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    {
                        if (displayedItems[selectionIndex].isParent == false)
                        {
                            ItemSelectedCallback(displayedItems[selectionIndex].item);
                        }
                    }
                    Repaint();
                }
                break;
            case EventType.MouseMove:
                {
                    selectionIndex = mouseOverIndex;
                    Repaint();
                }
                break;
        }
    }

    //.................................................
    // GUI drawings
    //.................................................

    private void DrawTitleGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(searchWindowTitle, m_styles.header, GUILayout.Height(32), GUILayout.Width(position.width));
        if (inSubmenu == true)
        {
            var arrowRect = GUILayoutUtility.GetLastRect();
            if (GUI.Button(arrowRect, GUIContent.none, GUIStyle.none)) // exit submenu
            {
                inSubmenu = false;
                m_windowTitle = searchWindowTitle;
                DisplayRootItems();
                selectionIndex = 0;
                mouseOverIndex = 0;
            }
            EditorGUIUtility.AddCursorRect(arrowRect, MouseCursor.Link);

            arrowRect.width = 16;
            arrowRect.height = 16;
            arrowRect.x += 8;
            arrowRect.y += 8;
            GUI.Box(arrowRect, GUIContent.none, m_styles.leftArrow);
        }
        else if (hasSearchText)
        {
            m_windowTitle = "Search Results";
        }
        else
        {
            inSubmenu = false;
            m_windowTitle = searchWindowTitle;
        }

        GUILayout.EndHorizontal();
    }

    private void DrawSearchGUI()
    {
        GUI.Label(new Rect(0, 0, position.width, position.height), GUIContent.none, m_styles.background);

        GUILayout.Space(7f);
        var searchRect = GUILayoutUtility.GetRect(10f, 20f);
        searchRect.x += 8f;
        searchRect.width -= 16f;
        EditorGUI.FocusTextInControl("ItemSearch");
        GUI.SetNextControlName("ItemSearch");

        var regen = DrawSearchField(searchRect, ref searchText);

        if (regen)
            GenerateSearchResults(searchText);

    }

    private void DisplayRootItems()
    {
        displayedItems = _treeItems.Where(t => t.isParent && t.parent == null).ToList();
    }

    private void ItemSelectedCallback(T menuItem)
    {
        if (OnMenuItemSelected != null)
            OnMenuItemSelected(menuItem);
        if (OnMenuItemSelected2 != null)
            OnMenuItemSelected2(menuItem, data);
        Close();
    }

    private void DrawMenuContent()
    {
        menuScrollPos = EditorGUILayout.BeginScrollView(menuScrollPos);

        foreach (var item in displayedItems)
        {
            GUIStyle buttonStyle = (displayedItems.IndexOf(item) == selectionIndex) ? m_styles.highlightedItem : m_styles.componentItem;

            bool hasChildren = item.isParent && item.children != null && item.children.Count != 0;

            if (GUILayout.Button(item.title, buttonStyle, GUILayout.Height(20)))
            {
                if (hasChildren) // enter submenu
                {
                    selectionIndex = 0;
                    mouseOverIndex = 0;
                    inSubmenu = true;
                    m_windowTitle = item.title;
                    displayedItems = item.children;
                }
                else
                {
                    ItemSelectedCallback(item.item);
                }
                break;
            }

            var itemRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(itemRect, MouseCursor.Link);

            if (hasChildren)
            {
                itemRect.width = 16;
                itemRect.height = 16;
                itemRect.x = position.width - 24;
                GUI.Box(itemRect, GUIContent.none, m_styles.rightArrow);
            }

            if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                lastRectMouseOver = GUILayoutUtility.GetLastRect();
                mouseOverIndex = displayedItems.IndexOf(item);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private static bool DrawSearchField(Rect position, ref string text)
    {
        var searchRect = position;
        searchRect.width -= 15f;
        var startText = text;
        text = GUI.TextField(searchRect, startText ?? "", m_styles.searchTextField);

        var searchCancelRect = position;
        searchCancelRect.x += position.width - 15f;
        searchCancelRect.width = 15f;
        var cancelStyle = (string.IsNullOrEmpty(text)) ? m_styles.searchCancelButtonEmpty : m_styles.searchCancelButton;
        if (GUI.Button(searchCancelRect, GUIContent.none, cancelStyle) && text != "")
        {
            text = "";
            GUIUtility.keyboardControl = 0;
        }
        return startText != text;
    }

    private void DrawItemTooltip()
    {
        if (displayedItems.Count == 0 || selectionIndex > displayedItems.Count - 1) return;

        var tooltipStr = displayedItems[selectionIndex].tooltip;

        EditorGUILayout.LabelField(tooltipStr, m_styles.centralLabel, GUILayout.Height(50));
    }

    //.................................................
    // Helper
    //.................................................

    private void GenerateSearchResults(string text)
    {
        var result = new List<SearchableMenuItem>();
        //foreach (var item in _treeItems.Where(t => t.isParent).SelectMany(p => p.children).Where(x => x.isParent == false).ToList())
        foreach (var item in _treeItems.Where(t => t.isOption).ToList())
        {
            var patternWords = text.Split(' ').Where(p => !string.IsNullOrEmpty(p)).ToArray();
            var elementName = item.title.Replace(" ", string.Empty);
            var match = true;

            for (int i = 0; i < patternWords.Length; i++)
            {
                var pattern = patternWords[i];
                if (!elementName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                result.Add(item);
            }
        }
        displayedItems = result;
    }

    private void CreateStyles()
    {
        if (m_styles == null) m_styles = new Styles();
    }

    protected string GetMenuItemTooltip(T menuItem)
    {
        var result = string.Empty;

        return (string.IsNullOrEmpty(result)) ? "Missing tooltip" : result;
    }

    protected string GetParentTooltip(string treeName)
    {
        var result = string.Empty;

        return (string.IsNullOrEmpty(result)) ? "Missing tooltip" : result;
    }
}
