using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class CreateAssetBundles
{
    public static string Boost = "VoiceBoostTrick";
    public static string Combo = "VoiceCombo";
    public static string Die = "VoiceDie";
    public static string DieFall = "VoiceFallingDamage";
    public static string GetHit = "VoiceGetHit";
    public static string Jump = "VoiceJump";
    public static string Talk = "VoiceTalk";

    [MenuItem ("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles ()
    {

        BuildPipeline.BuildAssetBundles ("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    [MenuItem ("Assets/Prep Audio for Bundle")]
    static void PrepAudioForBundle() {
        List<AudioClip> audio = FindAssetsByType<AudioClip>();
        foreach(AudioClip clip in audio) 
        {
            string path = AssetDatabase.GetAssetPath(clip);
            if(path.Contains(Boost)){
                AssetDatabase.RenameAsset(path, audio.IndexOf(clip) + "_boost");
            } else
            if(path.Contains(Combo)){
                AssetDatabase.RenameAsset(path, audio.IndexOf(clip) + "_combo");
            } else
            if(path.Contains(DieFall)){
                AssetDatabase.RenameAsset(path, audio.IndexOf(clip) + "_falldamage");
            } else
            if(path.Contains(Die)){
                AssetDatabase.RenameAsset(path, audio.IndexOf(clip) + "_die");
            } else
            if(path.Contains(GetHit)){
                AssetDatabase.RenameAsset(path, audio.IndexOf(clip) + "_gethit");
            } else
            if(path.Contains(Jump)){
                AssetDatabase.RenameAsset(path, audio.IndexOf(clip) + "_jump");
            } else
            if(path.Contains(Talk)){
                AssetDatabase.RenameAsset(path, audio.IndexOf(clip) + "_talk");
            }
        }
    }

    public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
	    List<T> assets = new List<T>();
	    string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).ToString().Replace("UnityEngine.", "")));
	    for( int i = 0; i < guids.Length; i++ )
	    {
		    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if( asset != null )
            {
                assets.Add(asset);
            }
        }     
        return assets;
    }
}