using System;
using System.Linq;

namespace Alias;

// Ported from SimpleTweaksPlugin's Tweaks/CommandAlias.cs (AliasEntry class).
public class AliasEntry {
    // Commands that should never be overwritten by an alias, since aliasing them
    // could lock you out of the tools you'd need to fix things.
    public static readonly string[] NoOverwrite = ["xlplugins", "xlsettings", "xldclose", "xldev", "alias"];

    public bool Enabled = true;
    public string Input = string.Empty;
    public string Output = string.Empty;
    public bool Resend;

    [NonSerialized] public bool Delete;
    [NonSerialized] public int UniqueId;

    public bool IsValid() {
        if (NoOverwrite.Contains(Input)) return false;
        if (Input.Contains(' ')) return false;
        return !(string.IsNullOrWhiteSpace(Input) || string.IsNullOrWhiteSpace(Output));
    }
}
