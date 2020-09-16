using System.Runtime.Serialization;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public enum BarcodeType
    {
        [EnumMember(Value = "EAN_13")]
        Ean13,
        
        [EnumMember(Value = "UPC_A")]
        UpcA
    }
}