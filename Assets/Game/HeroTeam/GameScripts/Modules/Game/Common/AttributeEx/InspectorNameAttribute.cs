using UnityEngine;

namespace GameScripts.HeroTeam
{
  public class InspectorNameAttribute : PropertyAttribute
  {
    public string DisplayName { get; private set; }

    public InspectorNameAttribute(string displayName)
    {
      DisplayName = displayName;
    }
  }

}