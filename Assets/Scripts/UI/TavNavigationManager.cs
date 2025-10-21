using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabNavigationManager : MonoBehaviour
{
    [System.Serializable]
    public class TabPage
    {
        public string tabName;
        public Button tabButton;
        public GameObject tabContent;
        public bool startActive = false;
    }

    [Header("Tab Configuration")]
    public List<TabPage> tabs = new();
    public Color activeTabColor = new(0.2f, 0.6f, 1.0f);
    public Color inactiveTabColor = new(0.5f, 0.5f, 0.5f);

    [Header("Optional Animation")]
    public bool useAnimation = true;
    public float tabTransitionTime = 0.3f;

    private TabPage currentActiveTab;
    private readonly Dictionary<string, TabPage> tabLookup = new();

    void Start()
    {
        SetupTabs();
    }

    void SetupTabs()
    {
        // Build lookup table for quick access
        tabLookup.Clear();

        // Setup button listeners and initial states
        foreach (TabPage tab in tabs)
        {
            // Add to lookup
            tabLookup[tab.tabName] = tab;

            // Set initial state to inactive
            tab.tabContent.SetActive(false);

            // Set color of button to inactive
            ColorBlock colors = tab.tabButton.colors;
            colors.normalColor = inactiveTabColor;
            tab.tabButton.colors = colors;

            // Add click listener
            tab.tabButton.onClick.AddListener(() => ActivateTab(tab.tabName));
        }

        // Activate the default tab
        TabPage defaultTab = tabs.Find(t => t.startActive);
        if (defaultTab != null)
        {
            ActivateTab(defaultTab.tabName);
        }
        else if (tabs.Count > 0)
        {
            ActivateTab(tabs[0].tabName);
        }
    }

    public void ActivateTab(string tabName)
    {
        if (!tabLookup.ContainsKey(tabName))
        {
            Debug.LogError($"Tab '{tabName}' not found!");
            return;
        }

        // Deactivate current active tab if exists
        if (currentActiveTab != null)
        {
            // Change button color
            ColorBlock colors = currentActiveTab.tabButton.colors;
            colors.normalColor = inactiveTabColor;
            currentActiveTab.tabButton.colors = colors;

            if (useAnimation)
            {
                // Fade out current tab
                if (!currentActiveTab.tabContent.TryGetComponent<CanvasGroup>(out var group))
                {
                    group = currentActiveTab.tabContent.AddComponent<CanvasGroup>();
                }
                StartCoroutine(FadeOut(group, tabTransitionTime));
            }
            else
            {
                // Just hide it
                currentActiveTab.tabContent.SetActive(false);
            }
        }

        // Activate new tab
        TabPage newTab = tabLookup[tabName];
        currentActiveTab = newTab;

        // Change button color
        ColorBlock newColors = newTab.tabButton.colors;
        newColors.normalColor = activeTabColor;
        newTab.tabButton.colors = newColors;

        if (useAnimation)
        {
            // Show and fade in new tab
            newTab.tabContent.SetActive(true);
            if (!newTab.tabContent.TryGetComponent<CanvasGroup>(out var group))
            {
                group = newTab.tabContent.AddComponent<CanvasGroup>();
            }
            StartCoroutine(FadeIn(group, tabTransitionTime));
        }
        else
        {
            // Just show it
            newTab.tabContent.SetActive(true);
        }

        // Fire event that tab changed
        OnTabChanged(tabName);
    }

    private System.Collections.IEnumerator FadeOut(CanvasGroup group, float time)
    {
        float startTime = Time.time;
        float endTime = startTime + time;

        group.alpha = 1;

        while (Time.time < endTime)
        {
            float normalizedTime = (Time.time - startTime) / time;
            group.alpha = 1 - normalizedTime;
            yield return null;
        }

        group.alpha = 0;
        group.gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator FadeIn(CanvasGroup group, float time)
    {
        float startTime = Time.time;
        float endTime = startTime + time;

        group.alpha = 0;
        group.gameObject.SetActive(true);

        while (Time.time < endTime)
        {
            float normalizedTime = (Time.time - startTime) / time;
            group.alpha = normalizedTime;
            yield return null;
        }

        group.alpha = 1;
    }

    // Method to allow other scripts to listen for tab changes
    public System.Action<string> onTabChanged;

    private void OnTabChanged(string tabName)
    {
        onTabChanged?.Invoke(tabName);
    }

    // Method to get the current active tab
    public string GetActiveTabName()
    {
        return currentActiveTab?.tabName;
    }

    // Method to check if a specific tab is currently active
    public bool IsTabActive(string tabName)
    {
        return currentActiveTab != null && currentActiveTab.tabName == tabName;
    }
}