# Alias

A Dalamud plugin containing exclusively the "Command Alias" feature from
[SimpleTweaks](https://github.com/Caraxi/SimpleTweaksPlugin), plus one
addition: reorder entries in the list with up/down arrow buttons.

## What it does

Type `/alias` in chat to open the config window. Set up an alias (e.g. input
`hw`, output `housing`) and typing `/hw enter` will be silently rewritten to
`/housing enter` before it's sent. No leading slash in the "Input" field,
aliases don't work inside macros (by design), and "Alt Mode" resends the
command through the chat box instead of injecting it directly - useful for
the handful of commands that don't behave well with direct injection.

## Project layout

- `Plugin.cs` - entry point; registers `/alias` and hooks
  `ShellCommandModule.ExecuteCommandInner` to intercept and rewrite chat input.
- `Configuration.cs` - persisted list of aliases.
- `AliasEntry.cs` - one alias row (input, output, enabled, alt mode).
- `ConfigWindow.cs` - the ImGui config UI, including the reorder buttons.
- `ChatHelper.cs` - helper for "Alt Mode" resending.
- `Alias.yaml` - plugin manifest metadata (name/author/description/punchline/
  repo url) picked up by Dalamud.NET.Sdk at build time.

The hook and chat-processing logic is a direct port of SimpleTweaks'
`Tweaks/CommandAlias.cs`, rewritten to stand alone without their
TweakSystem/attribute framework. The reorder buttons are the one new feature.

## License

This is licensed **AGPL-3.0**, matching SimpleTweaksPlugin's license, since
`Plugin.cs`, `ChatHelper.cs`, and `AliasEntry.cs` are ports of SimpleTweaks
code and AGPL-3.0 is copyleft - any distributed work built on it needs to
carry the same license. Attribution is noted in the source files and here in
the README; keep both if you fork this further.

## Building

Uses `Dalamud.NET.Sdk` (15.0.0), same as most current Dalamud plugins - no
extra NuGet packages needed. Open `Alias.sln` (or `Alias/Alias.csproj`) and
build with your usual toolchain, or:

```
dotnet build --configuration Release Alias/Alias.csproj
```

To test locally, load it as a dev plugin the way you normally do (point
Dalamud's dev plugin location at the build output folder, or drop it in your
`devPlugins` directory).

## Releasing v0.1.0.0 on GitHub

1. Update `Alias.yaml`'s `repo_url` to your actual repo (currently a
   placeholder: `https://github.com/mr-reeh/Alias`) - update `author` too if
   `mr-reeh` isn't right for this repo.
2. Push this to a new GitHub repo, commit everything including
   `images/icon.png` (currently a simple placeholder - swap it for your own
   art if you want).
3. Tag the release and push the tag:
   ```
   git tag v0.1.0.0
   git push origin v0.1.0.0
   ```
4. `.github/workflows/build.yml` will build on that tag push, download a
   Dalamud dev build to compile against, and attach the packaged zip to a
   GitHub Release automatically. It's a generic starting point - adjust the
   .NET version or Dalamud download step if your other plugins' CI does this
   differently.
5. If you want this installable via a personal plugin repo (like your other
   plugins), you'll still need a `pluginmaster.json`/`repo.json` listing this
   release's zip URL, DalamudApiLevel, etc. - happy to put that together too
   if you point me at how your existing repo (for Milk Meter / Flash / etc.)
   is structured.
