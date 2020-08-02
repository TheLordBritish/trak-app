using System.Diagnostics.CodeAnalysis;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class Genre : HateoasResource
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public long? Version { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}