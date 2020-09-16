using System.ComponentModel;
using System.Runtime.Serialization;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public enum GameUserEntryStatus
    {
        [Description("None")]
        [EnumMember(Value = "NONE")]
        None,

        [Description("Backlog")]
        [EnumMember(Value = "BACKLOG")]
        Backlog,
        
        [Description("Playing")]
        [EnumMember(Value = "IN_PROGRESS")]
        InProgress,
        
        [Description("Done")]
        [EnumMember(Value = "COMPLETED")]
        Completed,
        
        [Description("Dropped")]
        [EnumMember(Value = "DROPPED")]
        Dropped
    }
}