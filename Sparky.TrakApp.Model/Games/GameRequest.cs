using System;

namespace Sparky.TrakApp.Model.Games
{
    public class GameRequest
    {
        public long Id { get; set; }
        
        public string Title { get; set; }
        
        public string Notes { get; set; }
        
        public bool Completed { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        public long UserId { get; set; }
        
        public long Version { get; set; }
    }
}