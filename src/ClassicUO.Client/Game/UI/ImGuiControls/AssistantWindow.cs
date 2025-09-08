using ImGuiNET;
using System;
using System.Collections.Generic;
using ClassicUO.Game.UI.Gumps;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class AssistantWindow : SingletonImGuiWindow<AssistantWindow>
    {
        private readonly List<TabItem> _tabs;
        private int _selectedTabIndex = -1;
        private AssistantWindow() : base("Assistant")
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;
            _tabs = new List<TabItem>();

            AddTab("Organizer", DrawOrganizer, OrganizerWindow.Show, () => OrganizerWindow.Instance?.Dispose() );
            AddTab("Bandage Agent", DrawBandageAgent, BandageAgentWindow.Show, () => BandageAgentWindow.Instance?.Dispose() );
        }

        public void SelectTab(AssistantGump.PAGE page)
        {
            switch (page)
            {
                case AssistantGump.PAGE.None:
                    break;
                case AssistantGump.PAGE.AutoLoot:
                    break;
                case AssistantGump.PAGE.AutoSell:
                    break;
                case AssistantGump.PAGE.AutoBuy:
                    break;
                case AssistantGump.PAGE.MobileGraphicFilter:
                    break;
                case AssistantGump.PAGE.SpellBar:
                    break;
                case AssistantGump.PAGE.HUD:
                    break;
                case AssistantGump.PAGE.SpellIndicator:
                    break;
                case AssistantGump.PAGE.JournalFilter:
                    break;
                case AssistantGump.PAGE.TitleBar:
                    break;
                case AssistantGump.PAGE.DressAgent:
                    break;
                case AssistantGump.PAGE.BandageAgent:
                    _selectedTabIndex = 1;
                    break;
                case AssistantGump.PAGE.FriendsList:
                    break;
                case AssistantGump.PAGE.Organizer:
                    _selectedTabIndex = 0;
                    break;
            }
        }

        public void AddTab(string title, Action drawContent, Action showFullWindow, Action dispose)
        {
            _tabs.Add(new TabItem { Title = title, DrawContent = drawContent, ShowFullWindow = showFullWindow, Dispose = dispose});
        }

        public void RemoveTab(int index)
        {
            if (index >= 0 && index < _tabs.Count)
            {
                _tabs.RemoveAt(index);
                if (_selectedTabIndex >= _tabs.Count)
                    _selectedTabIndex = Math.Max(0, _tabs.Count - 1);
            }
        }

        public void ClearTabs()
        {
            _tabs.Clear();
            _selectedTabIndex = 0;
        }

        public override void DrawContent()
        {
            if (_tabs.Count == 0)
            {
                ImGui.Text("No tabs available");
                return;
            }

            // Draw tab bar
            if (ImGui.BeginTabBar("TabMenuTabs", ImGuiTabBarFlags.Reorderable))
            {
                for (int i = 0; i < _tabs.Count; i++)
                {
                    var tab = _tabs[i];
                    if (ImGui.BeginTabItem(tab.Title))
                    {
                        _selectedTabIndex = i;
                        tab.DrawContent?.Invoke();

                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            if (tab.ShowFullWindow != null)
                            {
                                tab.ShowFullWindow.Invoke();
                                RemoveTab(i);
                            }
                        }

                        ImGui.EndTabItem();
                    }
                }
                ImGui.EndTabBar();
            }
        }

        private void DrawOrganizer() => OrganizerWindow.GetInstance()?.DrawContent();
        private void DrawBandageAgent() => BandageAgentWindow.GetInstance()?.DrawContent();

        public override void Dispose()
        {
            base.Dispose();
            foreach (var tab in _tabs)
                tab.Dispose?.Invoke();
            ClearTabs();
        }

        private class TabItem
        {
            public string Title { get; set; }
            public Action DrawContent { get; set; }
            public Action ShowFullWindow { get; set; }
            public Action Dispose { get; set; }
        }
    }
}
