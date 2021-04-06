using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/EXPORT", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}