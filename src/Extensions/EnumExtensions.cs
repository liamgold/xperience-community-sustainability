using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace XperienceCommunity.Sustainability.Extensions;

public static class EnumExtensions
{
    public static string? GetDisplayName(this Enum enumValue)
    {
        var memberInfo = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault();

        return memberInfo?.GetCustomAttribute<DisplayAttribute>()?.GetName();
    }
}
