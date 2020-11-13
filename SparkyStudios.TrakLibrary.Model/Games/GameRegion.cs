using System.Runtime.Serialization;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public enum GameRegion
    {
        [EnumMember(Value = "NORTH_AMERICA")]
        NorthAmerica,

        [EnumMember(Value = "PAL")]
        Pal,
        
        [EnumMember(Value = "JAPAN")]
        Japan,
    }
}