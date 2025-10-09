using System;
using ImGuiNET;
using ClassicUO.Configuration;
using System.Numerics;
using ClassicUO.Network;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class GeneralWindow : SingletonImGuiWindow<GeneralWindow>
    {
        private readonly Profile _profile = ProfileManager.CurrentProfile;
        private int _objectMoveDelay;
        private bool _highlightObjects;
        private bool _showNames;
        private ushort _turnDelay;
        private GeneralWindow() : base("General Tab")
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;
            _objectMoveDelay = _profile.MoveMultiObjectDelay;
            _highlightObjects = _profile.HighlightGameObjects;
            _showNames = _profile.NameOverheadToggled;
            _turnDelay = _profile.TurnDelay;
        }

        public override void DrawContent()
        {
            if (_profile == null)
            {
                ImGui.Text("Profile not loaded");
                return;
            }

            ImGui.Spacing();

            if (ImGui.BeginTabBar("##GeneralTabs", ImGuiTabBarFlags.None))
            {
                if (ImGui.BeginTabItem("Options"))
                {
                    DrawOptionsTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Info"))
                {
                    DrawInfoTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("HUD"))
                {
                    HudWindow.GetInstance()?.DrawContent();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Spell Bar"))
                {
                    SpellBarWindow.GetInstance()?.DrawContent();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Title Bar"))
                {
                    TitleBarWindow.GetInstance()?.DrawContent();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Spell Indicators"))
                {
                    SpellIndicatorWindow.GetInstance()?.DrawContent();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Friends List"))
                {
                    FriendsListWindow.GetInstance()?.DrawContent();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }


        private void DrawOptionsTab()
        {
            // Group: Visual Config
            ImGui.BeginGroup();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(ImGuiTheme.Colors.BaseContent, "Visual Config");

            if (ImGui.Checkbox("Highlight game objects", ref _highlightObjects))
            {
                _profile.HighlightGameObjects = _highlightObjects;
            }

            if (ImGui.Checkbox("Show Names", ref _showNames))
            {
                _profile.NameOverheadToggled = _showNames;
            }
                ImGuiComponents.Tooltip("Toggle the display of names above characters and NPCs in the game world.");
            ImGui.EndGroup();

            ImGui.SameLine();

            // Group: Delay Config
            ImGui.BeginGroup();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(ImGuiTheme.Colors.BaseContent, "Delay Config");

            int tempTurnDelay = _turnDelay;

            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("Turn Delay", ref tempTurnDelay, 0, 150, " %d ms"))
            {
                if (tempTurnDelay < 0) tempTurnDelay = 0;
                if (tempTurnDelay > ushort.MaxValue) tempTurnDelay = 100;

                _turnDelay = (ushort)tempTurnDelay;
                _profile.TurnDelay = _turnDelay;
            }
            ImGui.SetNextItemWidth(150);
            if (ImGui.InputInt("Object Delay", ref _objectMoveDelay, 50, 100))
            {
                _objectMoveDelay = Math.Clamp(_objectMoveDelay, 0, 3000);

                _profile.MoveMultiObjectDelay = _objectMoveDelay;
            }
            ImGui.EndGroup();
        }

        private readonly string _version = "TazUO Version: " + CUOEnviroment.Version; //Pre-cache to prevent reading var and string concatenation every frame
        private uint _lastObject = 0;
        private string _lastObjectString = "Last Object:";
        private void DrawInfoTab()
        {
            if (World.Instance != null)
            {
                if (_lastObject != World.Instance.LastObject)
                {
                    _lastObject = World.Instance.LastObject;
                    _lastObjectString = "Last Object: " + _lastObject;
                }
            }

            ImGui.Text("Ping: " + AsyncNetClient.Socket.Statistics.Ping + "ms");
            ImGui.Spacing();
            ImGui.Text("FPS: " + CUOEnviroment.CurrentRefreshRate);
            ImGui.Spacing();
            ImGui.Text(_lastObjectString);
            if(ImGui.IsItemClicked())
            {
                SDL3.SDL.SDL_SetClipboardText(_lastObject.ToString());
                GameActions.Print("Copied last object to clipboard.", 62);
            }
            ImGui.Spacing();
            ImGui.Text(_version);
        }

    }
}
