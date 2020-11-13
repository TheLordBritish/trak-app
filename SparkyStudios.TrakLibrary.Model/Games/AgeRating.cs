using System.ComponentModel;
using System.Runtime.Serialization;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public enum AgeRating
    {
        [Description("Everyone")]
        [EnumMember(Value = "EVERYONE")]
        Everyone,
        
        [Description("Everyone 10+")]
        [EnumMember(Value = "EVERYONE_TEN_PLUS")]
        EveryoneTenPlus,
        
        [Description("Teen")]
        [EnumMember(Value = "TEEN")]
        Teen,
        
        [Description("Mature")]
        [EnumMember(Value = "MATURE")]
        Mature,
        
        [Description("Adults Only")]
        [EnumMember(Value = "ADULTS_ONLY")]
        AdultsOnly,
        
        [Description("Rating Pending")]
        [EnumMember(Value = "RATING_PENDING")]
        Pending
    }
}