using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[HarmonyPatch(typeof(BoomboxItem), "StartMusic")]
public static class BoomboxMusicPatch
{
    static bool Prefix(BoomboxItem __instance, bool startMusic, ref bool __state)
    {
        Debug.Log("[BoomboxMusicPatch] Prefix called. startMusic: " + startMusic);

        if (startMusic)
        {
            __state = true;
            string audioFolderPath = Path.Combine(Paths.BepInExRootPath, "Audio");
            __instance.StartCoroutine(LoadAndPlayRandomAudio(__instance.boomboxAudio, audioFolderPath));

            return false; // Empêche l'exécution de la méthode originale
        }
        else
        {
            __state = false;
            return true; // Continue avec la méthode originale pour l'arrêt de la musique
        }
    }

    static void Postfix(BoomboxItem __instance, bool startMusic, bool __state)
    {
        Debug.Log("[BoomboxMusicPatch] Postfix called. __state: " + __state);

        if (__state)
        {
            Debug.Log("[BoomboxMusicPatch] Music started in Prefix. Skipping Postfix actions.");
            return;
        }

        Debug.Log("[BoomboxMusicPatch] Resetting to original music clip.");
        __instance.boomboxAudio.clip = __instance.musicAudios[UnityEngine.Random.Range(0, __instance.musicAudios.Length)];
    }

    /// <summary>
    /// sélectionne un fichier audio au hasard dans le dossier et utilise UnityWebRequest pour le charger et le jouer.
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    private static IEnumerator LoadAndPlayRandomAudio(AudioSource audioSource, string folderPath)
    {
        string[] files = GetAllAudioFiles(folderPath);
        if (files == null || files.Length == 0)
        {
            yield break; // Sortir si aucun fichier n'est trouvé
        }

        string selectedFile = files[UnityEngine.Random.Range(0, files.Length)];
        Debug.Log("[BoomboxMusicPatch] Fichier sélectionné : " + selectedFile);

        AudioType audioType = DetermineAudioType(selectedFile);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + selectedFile, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[BoomboxMusicPatch] Erreur lors du chargement du fichier audio : {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// récupère tous les fichiers avec les extensions .wav ou .mp3 dans le dossier spécifié.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    private static string[] GetAllAudioFiles(string folderPath)
    {
        try
        {
            string[] files = Directory.GetFiles(folderPath, "*.*")
                .Where(file => file.ToLower().EndsWith("wav") || file.ToLower().EndsWith("mp3")).ToArray();
            if (files.Length == 0)
            {
                Debug.LogError("[BoomboxMusicPatch] Aucun fichier audio trouvé dans : " + folderPath);
                return null;
            }
            return files;
        }
        catch (Exception ex)
        {
            Debug.LogError("[BoomboxMusicPatch] Erreur lors de la récupération des fichiers audio : " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// utilise l'extension du fichier pour déterminer le AudioType à utiliser lors du chargement du clip.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static AudioType DetermineAudioType(string filePath)
    {
        if (filePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            return AudioType.MPEG;
        }
        else if (filePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            return AudioType.WAV;
        }
        // Ajoute ici d'autres formats si nécessaire
        return AudioType.UNKNOWN;
    }
}
