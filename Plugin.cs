using BepInEx;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TestModLethalCompany
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Lemufi.TestMod";
        private const string modName = "LC Tutorial Mod";
        private const string modVersion = "1.0.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        private static Plugin Instance;

        // ---------------------------------------------------------------- // 

        /// <summary>
        /// S'exécute au lancement du mod
        /// </summary>
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>> MOD LEMUFI'S ADDONS OK");

            // Fichiers audio a ajouter au mod
            ExtractAndCopyAudioFile("LemufisAddons.AudioFiles.RaphaelLaCruche.wav", "RaphaelLaCruche.wav");
            ExtractAndCopyAudioFile("LemufisAddons.AudioFiles.SpongeBobSquarePantsProductionMusic.wav", "SpongeBobSquarePantsProductionMusic.wav");

            // Patch a utiliser au lancement
            harmony.PatchAll(typeof(BoomboxMusicPatch));
        }

        private void ExtractAndCopyAudioFile(string resourceName, string fileName)
        {
            string targetPath = Path.Combine(Paths.BepInExRootPath, "Audio");
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            string filePath = Path.Combine(targetPath, fileName);

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    Logger.LogError("Resource not found: " + resourceName);
                    return;
                }

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }

            Logger.LogInfo($"Fichier audio copié: {filePath}");
        }
    }
}
