using System;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.Shell;

namespace Alias;

public sealed class Plugin : IDalamudPlugin {
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/alias";

    public Configuration Configuration { get; }
    public readonly WindowSystem WindowSystem = new("Alias");
    private ConfigWindow ConfigWindow { get; }

    // Reproduces SimpleTweaksPlugin's Tweaks/CommandAlias.cs hook, but standing on its own
    // (no TweakSystem) using Dalamud's own IGameInteropProvider.
    private readonly Hook<ShellCommandModule.Delegates.ExecuteCommandInner>? processChatInputHook;
    private readonly Stopwatch resendSafety = Stopwatch.StartNew();

    public Plugin() {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) {
            HelpMessage = "Open the Alias configuration window.",
        });

        try {
            unsafe {
                processChatInputHook = GameInteropProvider.HookFromAddress<ShellCommandModule.Delegates.ExecuteCommandInner>(
                    ShellCommandModule.Addresses.ExecuteCommandInner.Value,
                    ProcessChatInputDetour);
                processChatInputHook.Enable();
            }
        } catch (Exception ex) {
            Log.Error(ex, "Failed to hook ShellCommandModule.ExecuteCommandInner - aliases will not function.");
        }
    }

    public void Dispose() {
        processChatInputHook?.Dispose();

        CommandManager.RemoveHandler(CommandName);

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;

        WindowSystem.RemoveAllWindows();
    }

    private void OnCommand(string command, string args) => ToggleConfigUi();

    public void ToggleConfigUi() => ConfigWindow.Toggle();

    // Ported from SimpleTweaksPlugin's Tweaks/CommandAlias.cs (ProcessChatInputDetour).
    private unsafe void ProcessChatInputDetour(ShellCommandModule* shellCommandModule, Utf8String* message, UIModule* uiModule) {
        try {
            if (message->GetCharAt(0) == '/') {
                var inputString = message->ToString();
                var splitString = inputString.Split(' ');
                if (splitString.Length > 0 && splitString[0].Length >= 2) {
                    var alias = Configuration.AliasList.FirstOrDefault(a => {
                        if (!a.Enabled) return false;
                        if (!a.IsValid()) return false;
                        return splitString[0] == $"/{a.Input}";
                    });

                    if (alias != null) {
                        var commandExtra = inputString[(alias.Input.Length + 1)..];
                        if (commandExtra.StartsWith(' ')) commandExtra = commandExtra[1..];
                        var newStr = alias.Output.Contains(' ') ? $"/{alias.Output}{commandExtra}" : $"/{alias.Output} {commandExtra}";

                        if (newStr.Length <= 500) {
                            if (alias.Resend) {
                                if (resendSafety.ElapsedMilliseconds >= 1000) {
                                    resendSafety.Restart();
                                    ChatHelper.SendMessage(newStr);
                                } else {
                                    ChatGui.PrintError("[Alias] Something went wrong... You seem to have a command loop");
                                }

                                return;
                            }

                            var str = Utf8String.FromString(newStr);
                            processChatInputHook!.Original(shellCommandModule, str, uiModule);
                            str->Dtor(true);
                            return;
                        }

                        ChatGui.PrintError("[Alias] Command alias result is longer than the maximum of 500 characters. The command could not be executed.");
                    }
                }
            }
        } catch (Exception ex) {
            Log.Error(ex, "Error in Command Alias chat processing.");
        }

        processChatInputHook!.Original(shellCommandModule, message, uiModule);
    }
}
