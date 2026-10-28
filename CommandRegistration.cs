using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UI.Console;

namespace TrackRepair
{
    /// <summary>
    /// The game's console only scans its own assembly for [ConsoleCommandHandler] classes. This adds the mod's
    /// assembly to the same CommandProcessor, both for a console that already exists and for ones created later.
    /// </summary>
    internal static class CommandRegistration
    {
        private static readonly FieldInfo FProcessor = AccessTools.Field(typeof(ConsoleCommandHandler), "_processor");
        private static readonly HashSet<CommandProcessor> Done = new HashSet<CommandProcessor>();

        internal static void Register(ConsoleCommandHandler handler)
        {
            try
            {
                var processor = FProcessor?.GetValue(handler) as CommandProcessor;
                if (processor == null || !Done.Add(processor)) return;
                processor.RegisterHandlers(typeof(CommandRegistration).Assembly);
                ModEntryPoint.Log.Info("Registered /repairtrack with the console.");
            }
            catch (Exception e)
            {
                ModEntryPoint.Log.Error(e, "Could not register console command: " + e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(ConsoleCommandHandler), "Awake")]
    internal static class ConsoleCommandHandler_Awake_Patch
    {
        private static void Postfix(ConsoleCommandHandler __instance)
        {
            CommandRegistration.Register(__instance);
        }
    }
}
