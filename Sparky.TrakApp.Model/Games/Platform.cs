using System;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Model.Games
{
    public class Platform : HateoasResource
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? ReleaseDate { get; set; }
        
        public long Version { get; set; }
    }
}