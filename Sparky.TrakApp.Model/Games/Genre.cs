using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Model.Games
{
    public class Genre : HateoasResource
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public long version { get; set; }
    }
}