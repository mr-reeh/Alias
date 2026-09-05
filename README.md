# Alias

Set up your own command aliases for the chat box. Type a short alias and have
it silently rewritten into a different command before it's sent - e.g. alias
`hw` to `housing` so `/hw enter` becomes `/housing enter`.

Run `/alias` in game to open the config window.

## Features

- Simple input -> output alias list, toggleable per entry.
- Reorder aliases with up/down arrow buttons.
- "Alt Mode" resends the command through the chat box instead of injecting it
  directly, for the handful of commands that don't behave well with direct
  injection.
- Aliases don't work inside macros, by design - use the original command
  there instead.

## Installing

Add this repo to Dalamud's custom plugin repositories
(Settings -> Experimental -> Custom Plugin Repositories):

```
https://raw.githubusercontent.com/mr-reeh/Alias/main/pluginmaster.json
```

Then install "Alias" from the plugin installer as normal.

## Building

Uses `Dalamud.NET.Sdk` (15.0.0), same as my other plugins - no extra NuGet
packages needed. Open `Alias.sln` and build, or:

```
dotnet build --configuration Release Alias/Alias.csproj
```

## Credit & license

This started as the "Command Alias" tweak from
[SimpleTweaks](https://github.com/Caraxi/SimpleTweaksPlugin), pulled out into
its own plugin and rewritten to stand alone. Licensed AGPL-3.0 to match, since
it carries over SimpleTweaks code directly.

Built with AI assistance (Claude), same as my other plugins.
