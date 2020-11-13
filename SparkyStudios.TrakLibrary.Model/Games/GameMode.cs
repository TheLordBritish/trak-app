using System.ComponentModel;
using System.Runtime.Serialization;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public enum GameMode
    {
        [Description("Single-player")]
        [EnumMember(Value = "SINGLE_PLAYER")]
        SinglePlayer,
        
        [Description("Multi-player")]
        [EnumMember(Value = "MULTI_PLAYER")]
        MultiPlayer,
        
        [Description("Co-op")]
        [EnumMember(Value = "COOPERATIVE")]
        Cooperative,
        
        [Description("Virtual Reality")]
        [EnumMember(Value = "VIRTUAL_REALITY")]
        VirtualReality
    }
}