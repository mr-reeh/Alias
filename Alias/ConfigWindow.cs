using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace Alias;

public class ConfigWindow : Window {
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin) : base("Alias Config###AliasConfig") {
        this.plugin = plugin;
        Size = new Vector2(680, 400);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(680, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public override void Draw() {
        var config = plugin.Configuration;
        var change = false;

        ImGui.TextWrapped("Add a list of command aliases. Do not start the command with '/'.\nThese aliases, by design, do not work inside macros - use the original command there instead.");
        ImGui.Separator();

        ImGui.Columns(6);
        var s = ImGui.GetIO().FontGlobalScale;
        ImGui.SetColumnWidth(0, 60 * s);
        ImGui.SetColumnWidth(1, 150 * s);
        ImGui.SetColumnWidth(2, 150 * s);
        ImGui.SetColumnWidth(3, 55 * s);
        ImGui.SetColumnWidth(4, 60 * s);

        ImGui.Text("Enabled");
        ImGui.NextColumn();
        ImGui.Text("Input Command");
        ImGui.NextColumn();
        ImGui.Text("Output Command");
        ImGui.NextColumn();
        ImGui.Text("Alt\nMode");
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Use an alternative method to send the alias.");
        }

        ImGui.NextColumn();
        ImGui.Text("Reorder");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.Separator();

        var list = config.AliasList;

        for (var i = 0; i < list.Count; i++) {
            var aliasEntry = list[i];
            if (aliasEntry.UniqueId == 0) {
                aliasEntry.UniqueId = list.Max(a => a.UniqueId) + 1;
            }

            ImGui.Separator();
            ImGui.PushID(aliasEntry.UniqueId);

            if (aliasEntry.IsValid()) {
                change |= ImGui.Checkbox("###aliasToggle", ref aliasEntry.Enabled);
            } else {
                ImGui.Text("Invalid");
            }

            ImGui.NextColumn();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
            ImGui.Text("/");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-5);
            change |= ImGui.InputText("###aliasInput", ref aliasEntry.Input, 500);
            ImGui.PopStyleVar();
            ImGui.NextColumn();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
            ImGui.Text("/");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-5);
            change |= ImGui.InputText("###aliasOutput", ref aliasEntry.Output, 500);
            ImGui.PopStyleVar();
            ImGui.NextColumn();

            ImGui.Checkbox("###altMode", ref aliasEntry.Resend);
            ImGui.NextColumn();

            // Reorder buttons - the one feature this plugin adds on top of the original tweak.
            using (ImRaii.Disabled(i == 0)) {
                if (ImGui.ArrowButton("###moveUp", ImGuiDir.Up)) {
                    (list[i - 1], list[i]) = (list[i], list[i - 1]);
                    change = true;
                }
            }

            ImGui.SameLine();

            using (ImRaii.Disabled(i == list.Count - 1)) {
                if (ImGui.ArrowButton("###moveDown", ImGuiDir.Down)) {
                    (list[i + 1], list[i]) = (list[i], list[i + 1]);
                    change = true;
                }
            }

            ImGui.NextColumn();

            if (AliasEntry.NoOverwrite.Contains(aliasEntry.Input)) {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), $"'/{aliasEntry.Input}' is a protected command.");
            } else if (string.IsNullOrEmpty(aliasEntry.Input)) {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Input must not be empty.");
            } else if (string.IsNullOrEmpty(aliasEntry.Output)) {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Output must not be empty.");
            } else if (aliasEntry.Input.StartsWith("/")) {
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "Don't include the '/'");
            } else if (aliasEntry.Input.Contains(' ')) {
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "Input Command cannot contain a space.");
            }

            ImGui.NextColumn();

            if (string.IsNullOrWhiteSpace(aliasEntry.Input) && string.IsNullOrWhiteSpace(aliasEntry.Output)) {
                aliasEntry.Delete = true;
            }

            ImGui.PopID();
        }

        if (list.Count > 0 && list.RemoveAll(a => a.Delete) > 0) {
            change = true;
        }

        ImGui.Separator();

        var newEntry = new AliasEntry { UniqueId = list.Count == 0 ? 1 : list.Max(a => a.UniqueId) + 1 };
        var addNew = false;

        ImGui.PushID(newEntry.UniqueId);
        ImGui.Text("New:");
        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);
        addNew |= ImGui.InputText("###newAliasInput", ref newEntry.Input, 500);
        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);
        addNew |= ImGui.InputText("###newAliasOutput", ref newEntry.Output, 500);
        ImGui.NextColumn();
        ImGui.PopID();

        if (addNew) {
            list.Add(newEntry);
            change = true;
        }

        ImGui.Columns(1);

        if (change) {
            plugin.Configuration.Save();
        }
    }
}
