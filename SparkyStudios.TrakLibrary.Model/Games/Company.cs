using System;
using System.Diagnostics.CodeAnalysis;
using SparkyStudios.TrakLibrary.Model.Response;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class Company : HateoasResource
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public DateTime FoundedDate { get; set; }

        public long? Version { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}