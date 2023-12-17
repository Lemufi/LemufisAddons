

// LemufisAddons, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6e7115eb172495e8
// BoomboxMusicPatch
using System;
using System.Collections;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

[HarmonyPatch(typeof(BoomboxItem), "StartMusic")]
public static class BoomboxMusicPatch
{
    private static bool Prefix(BoomboxItem __instance, bool startMusic, ref bool __state)
    {
        Debug.Log("[BoomboxMusicPatch] Prefix called. startMusic: " + startMusic);
        if (startMusic)
        {
            __state = true;
            string folderPath = Path.Combine(Paths.BepInExRootPath, "Audio");
            ((MonoBehaviour)(object)__instance).StartCoroutine(LoadAndPlayRandomAudio(__instance.boomboxAudio, folderPath));
            return false;
        }
        __state = false;
        return true;
    }

    private static void Postfix(BoomboxItem __instance, bool startMusic, bool __state)
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

    private static IEnumerator LoadAndPlayRandomAudio(AudioSource audioSource, string folderPath)
    {
        string[] files = GetAllAudioFiles(folderPath);
        if (files == null || files.Length == 0)
        {
            yield break;
        }
        string selectedFile = files[UnityEngine.Random.Range(0, files.Length)];
        Debug.Log("[BoomboxMusicPatch] Fichier sélectionné : " + selectedFile);
        AudioType audioType = DetermineAudioType(selectedFile);
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + selectedFile, audioType);
        try
        {
            yield return www.SendWebRequest();
            if ((int)www.result == 2 || (int)www.result == 3)
            {
                Debug.LogError("[BoomboxMusicPatch] Erreur lors du chargement du fichier audio : " + www.error);
                yield break;
            }
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }
        finally
        {
            ((IDisposable)www)?.Dispose();
        }
    }

    private static string[] GetAllAudioFiles(string folderPath)
    {
        try
        {
            string[] array = (from file in Directory.GetFiles(folderPath, "*.*")
                              where file.ToLower().EndsWith("wav") || file.ToLower().EndsWith("mp3")
                              select file).ToArray();
            if (array.Length == 0)
            {
                Debug.LogError("[BoomboxMusicPatch] Aucun fichier audio trouvé dans : " + folderPath);
                return null;
            }
            return array;
        }
        catch (Exception ex)
        {
            Debug.LogError("[BoomboxMusicPatch] Erreur lors de la récupération des fichiers audio : " + ex.Message);
            return null;
        }
    }

    private static AudioType DetermineAudioType(string filePath)
    {
        if (filePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            return AudioType.MPEG;
        }
        if (filePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            return AudioType.WAV;
        }
        return AudioType.UNKNOWN;
    }
}
