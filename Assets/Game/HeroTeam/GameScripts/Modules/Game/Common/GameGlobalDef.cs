namespace GameScripts.HeroTeam
{
    /// <summary>
    /// 资源加载委托
    /// </summary>
    /// <typeparam name="T"> 返回的资源类型 </typeparam>
    /// <param name="resPath"> assetBundle路径/Resource路径 </param>
    /// <returns></returns>
    public delegate T LoadResHandler<T>(string resPath);
}