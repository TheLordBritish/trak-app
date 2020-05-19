using System.Runtime.Serialization;

namespace Sparky.TrakApp.Model.Games
{
    public enum GameUserEntryStatus
    {
        [EnumMember(Value = "WISH_LIST")]
        WishList,
        
        [EnumMember(Value = "BACKLOG")]
        Backlog,
        
        [EnumMember(Value = "IN_PROGRESS")]
        InProgress,
        
        [EnumMember(Value = "COMPLETED")]
        Completed,
        
        [EnumMember(Value = "DROPPED")]
        Dropped
    }
}