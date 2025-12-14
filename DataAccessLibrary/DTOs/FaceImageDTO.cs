using System;
using System.Collections.Generic;

namespace DataAccessLibrary.DTOs
{
    public class FaceImageDTO
    {
        public int Id { get; set; }
        public string ImageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageData { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public DateTime UploadDate { get; set; }
        public List<FaceTagDTO> FaceTags { get; set; } = new List<FaceTagDTO>();
    }
}
