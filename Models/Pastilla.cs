namespace AVUS.Models
{
    public class Pastilla
    {
        public int pastilla_id { get; set; }
        public int avu_id { get; set; }
        public string nombre { get; set; }
        public string? dosis { get; set; }
        public TimeSpan? hora { get; set; }
    }
}