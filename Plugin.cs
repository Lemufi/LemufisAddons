using BepInEx;
using HarmonyLib;
using System.IO;
using System.Reflection;

namespace TestModLethalCompany
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Lemufi.LemufisAddons";
        private const string modName = "Lemufi's Addons";
        private const string modVersion = "1.0.0.0"; // 1.0.0 pour vortex
        private readonly Harmony harmony = new Harmony(modGUID);
        private static Plugin Instance;

        // ---------------------------------------------------------------- // 

        /// <summary>
        /// S'éxecute au lancement du mod
        /// </summary>
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            // Vérification du lancement du MOD
            Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>> MOD LEMUFI'S ADDONS OK");

            // Logique pour ajouter un fichier audio à Audios -------------------------------------------------------------------------------
            ExtractAndCopyAudioFile("TestModLethalCompany.AudioFiles.RaphaelLaCruche.wav", "RaphaelLaCruche.wav");
            ExtractAndCopyAudioFile("TestModLethalCompany.AudioFiles.SpongeBobSquarePantsProductionMusic.wav", "SpongeBobSquarePantsProductionMusic.wav");

            // Patchs a utiliser au lancement -----------------------------------------------------------------------------------------------
            harmony.PatchAll(typeof(BoomboxMusicPatch));
        }

        #region GestionPatchAudio

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

        #endregion
    }
}
