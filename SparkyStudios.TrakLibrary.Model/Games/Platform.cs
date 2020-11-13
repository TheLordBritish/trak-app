using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SparkyStudios.TrakLibrary.Model.Response;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class Platform : HateoasResource
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public long? Version { get; set; }

        public IEnumerable<GameReleaseDate> ReleaseDates { get; set; }
    }
}