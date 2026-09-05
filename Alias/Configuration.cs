using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace Alias;

[Serializable]
public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 1;

    public List<AliasEntry> AliasList = new();

    [NonSerialized] private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pi) {
        pluginInterface = pi;
    }

    public void Save() {
        pluginInterface?.SavePluginConfig(this);
    }
}
