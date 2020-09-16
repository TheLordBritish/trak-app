using System.Runtime.Serialization;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public enum AgeRating
    {
        [EnumMember(Value = "EVERYONE")]
        Everyone,
        
        [EnumMember(Value = "EVERYONE_TEN_PLUS")]
        EveryoneTenPlus,
        
        [EnumMember(Value = "TEEN")]
        Teen,
        
        [EnumMember(Value = "MATURE")]
        Mature,
        
        [EnumMember(Value = "ADULTS_ONLY")]
        AdultsOnly,
        
        [EnumMember(Value = "RATING_PENDING")]
        Pending
    }
}