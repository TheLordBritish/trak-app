using System.ComponentModel;
using System.Runtime.Serialization;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public enum AgeRatingClassification
    {
        [Description("ESRB")]
        [EnumMember(Value = "ESRB")]
        Esrb,
        
        [Description("PEGI")]
        [EnumMember(Value = "PEGI")]
        Pegi,
        
        [Description("CERO")]
        [EnumMember(Value = "CERO")]
        Cero
    }
}