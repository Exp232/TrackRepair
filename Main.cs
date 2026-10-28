using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Railroader.Modding;
using UI.Console;

namespace TrackRepair
{
    /// <summary>Entry point: the game creates this from Info.json and calls OnModLoad once at start-up.</summary>
    public sealed class ModEntryPoint : IModEntryPoint
    {
        internal static IModLogger Log;
        private Harmony _harmony;

        public void OnModLoad(IModContext context)
        {
            Log = context.Log;
            string dir = context.ModDirectory;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string file = Path.Combine(dir, new AssemblyName(args.Name).Name + ".dll");
                return File.Exists(file) ? Assembly.LoadFrom(file) : null;
            };
            Start();
        }

        // Kept in its own method so Harmony is only touched after the resolver above is in place.
        private void Start()
        {
            try
            {
                _harmony = new Harmony("exp232.trackrepair");
                _harmony.PatchAll(typeof(ModEntryPoint).Assembly);

                // The console may already exist (mod loaded after the scene), so register with it now as well.
                foreach (var handler in UnityEngine.Object.FindObjectsOfType<ConsoleCommandHandler>(true))
                    CommandRegistration.Register(handler);

                Log.Info("TrackRepair loaded. Use /repairtrack in the console.");
            }
            catch (Exception e)
            {
                Log.Error(e, "TrackRepair failed to start: " + e.Message);
            }
        }

        public void OnModUnload()
        {
            _harmony?.UnpatchAll("exp232.trackrepair");
        }
    }
}
