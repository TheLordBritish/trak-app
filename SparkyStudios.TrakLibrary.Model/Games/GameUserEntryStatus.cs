using System.ComponentModel;
using System.Runtime.Serialization;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public enum GameUserEntryStatus
    {
        [Description("None")]
        [EnumMember(Value = "NONE")]
        None = -1,

        [Description("Backlog")]
        [EnumMember(Value = "BACKLOG")]
        Backlog = 0,
        
        [Description("Playing")]
        [EnumMember(Value = "IN_PROGRESS")]
        InProgress = 1,
        
        [Description("Done")]
        [EnumMember(Value = "COMPLETED")]
        Completed = 2,
        
        [Description("Dropped")]
        [EnumMember(Value = "DROPPED")]
        Dropped = 3
    }
}