namespace PointOfNoReturn.Data.Models
{
    public class LoggedUser
    {
        public string? Id { get; set; }

        public int? PeerId { get; set; }

        public string? Token { get; set; }

        public Player? User { get; set; }
    }
}