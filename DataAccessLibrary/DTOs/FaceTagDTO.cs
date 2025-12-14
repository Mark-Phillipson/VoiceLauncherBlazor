namespace DataAccessLibrary.DTOs
{
    public class FaceTagDTO
    {
        public int Id { get; set; }
        public int FaceImageId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
