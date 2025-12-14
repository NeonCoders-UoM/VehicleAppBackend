namespace Vpassbackend.DTOs
{
    public class PdfUploadDto
    {
        public IFormFile PdfFile { get; set; }
        public string Title { get; set; }
        public string Category { get; set; } = "general";
        public bool AutoChunk { get; set; } = true;
        public int MaxChunkSize { get; set; } = 1000;
    }

    public class PdfUploadResponseDto
    {
        public bool Success { get; set; }
        public int DocumentsAdded { get; set; }
        public int DocumentsIndexed { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
