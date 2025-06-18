public class PathUtils
{


    /// <summary>
    /// 获取Spine资源路径
    /// </summary>
    /// <param name="spineName"> 002? </param>
    /// <returns></returns>
    public static string GetSpineABPath(string spineName)
    {
        return $"Game/HeroTeam/GameResources/Spine/BC_{spineName}/{spineName}_SkeletonData.asset";
    }

    /// <summary>
    /// 获取SpineManager配置资源路径
    /// </summary>
    /// <returns></returns>
    public static string GetSpineManagerConfigABPath()
    {
        return "Game/HeroTeam/GameResources/Configs/SpineManagerLODConfig.asset";
    }

    /// <summary>
    /// 获取预制体的资源路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetPrefabABPath(string path)
    {
        return "Game/HeroTeam/GameResources/Prefabs/" + path;
    }

    /// <summary>
    /// 获取人物预制体资源路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetCharacterABPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return defaultCharacterPrefabPath;
        return "Game/HeroTeam/GameResources/Prefabs/Game/Characters/" + path;
    }
    private const string defaultCharacterPrefabPath = "Game/HeroTeam/GameResources/Prefabs/Game/Characters/SpineCharacter.prefab";

}
