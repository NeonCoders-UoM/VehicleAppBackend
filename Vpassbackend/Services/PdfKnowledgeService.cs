using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    /// <summary>
    /// Service for processing PDF files and adding them to the knowledge base
    /// </summary>
    public class PdfKnowledgeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ChatbotService _chatbotService;
        private readonly ILogger<PdfKnowledgeService> _logger;

        public PdfKnowledgeService(
            ApplicationDbContext context,
            ChatbotService chatbotService,
            ILogger<PdfKnowledgeService> logger)
        {
            _context = context;
            _chatbotService = chatbotService;
            _logger = logger;
        }

        /// <summary>
        /// Extract text from PDF file
        /// </summary>
        public string ExtractTextFromPdf(string filePath)
        {
            try
            {
                using (PdfReader pdfReader = new PdfReader(filePath))
                using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                {
                    var text = new System.Text.StringBuilder();

                    for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
                    {
                        var strategy = new SimpleTextExtractionStrategy();
                        string pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);
                        text.AppendLine(pageText);
                    }

                    return text.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Extract text from PDF stream (for uploaded files)
        /// </summary>
        public string ExtractTextFromPdfStream(Stream pdfStream)
        {
            try
            {
                using (PdfReader pdfReader = new PdfReader(pdfStream))
                using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                {
                    var text = new System.Text.StringBuilder();

                    for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
                    {
                        var strategy = new SimpleTextExtractionStrategy();
                        string pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);
                        text.AppendLine(pageText);
                    }

                    return text.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF stream");
                throw;
            }
        }

        /// <summary>
        /// Split text into chunks (for large documents)
        /// </summary>
        public List<string> SplitIntoChunks(string text, int maxChunkSize = 1000, int overlap = 200)
        {
            var chunks = new List<string>();
            var sentences = text.Split(new[] { ". ", ".\n", ".\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            var currentChunk = new System.Text.StringBuilder();
            
            foreach (var sentence in sentences)
            {
                if (currentChunk.Length + sentence.Length > maxChunkSize && currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    
                    // Add overlap from previous chunk
                    var words = currentChunk.ToString().Split(' ');
                    currentChunk.Clear();
                    if (words.Length > 20)
                    {
                        currentChunk.Append(string.Join(" ", words.TakeLast(20)));
                        currentChunk.Append(" ");
                    }
                }
                
                currentChunk.Append(sentence);
                currentChunk.Append(". ");
            }
            
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }
            
            return chunks;
        }

        /// <summary>
        /// Process PDF and add to knowledge base
        /// </summary>
        public async Task<int> ProcessPdfFileAsync(
            string filePath, 
            string title, 
            string category = "general",
            bool autoChunk = true,
            int maxChunkSize = 1000)
        {
            try
            {
                // Extract text
                var fullText = ExtractTextFromPdf(filePath);
                
                if (string.IsNullOrWhiteSpace(fullText))
                {
                    _logger.LogWarning("No text extracted from PDF: {FilePath}", filePath);
                    return 0;
                }

                int documentsAdded = 0;

                if (autoChunk && fullText.Length > maxChunkSize)
                {
                    // Split into chunks
                    var chunks = SplitIntoChunks(fullText, maxChunkSize);
                    
                    _logger.LogInformation("Split PDF into {Count} chunks", chunks.Count);

                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var document = new KnowledgeDocument
                        {
                            Title = $"{title} (Part {i + 1}/{chunks.Count})",
                            Content = chunks[i],
                            Category = category,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.KnowledgeDocuments.Add(document);
                        documentsAdded++;
                    }
                }
                else
                {
                    // Add as single document
                    var document = new KnowledgeDocument
                    {
                        Title = title,
                        Content = fullText,
                        Category = category,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.KnowledgeDocuments.Add(document);
                    documentsAdded = 1;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Added {Count} documents from PDF to knowledge base", documentsAdded);

                return documentsAdded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF file: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Process uploaded PDF stream and add to knowledge base
        /// </summary>
        public async Task<int> ProcessPdfStreamAsync(
            Stream pdfStream,
            string title,
            string category = "general",
            bool autoChunk = true,
            int maxChunkSize = 1000)
        {
            try
            {
                // Extract text
                var fullText = ExtractTextFromPdfStream(pdfStream);

                if (string.IsNullOrWhiteSpace(fullText))
                {
                    _logger.LogWarning("No text extracted from uploaded PDF");
                    return 0;
                }

                int documentsAdded = 0;

                if (autoChunk && fullText.Length > maxChunkSize)
                {
                    // Split into chunks
                    var chunks = SplitIntoChunks(fullText, maxChunkSize);

                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var document = new KnowledgeDocument
                        {
                            Title = $"{title} (Part {i + 1}/{chunks.Count})",
                            Content = chunks[i],
                            Category = category,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.KnowledgeDocuments.Add(document);
                        documentsAdded++;
                    }
                }
                else
                {
                    var document = new KnowledgeDocument
                    {
                        Title = title,
                        Content = fullText,
                        Category = category,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.KnowledgeDocuments.Add(document);
                    documentsAdded = 1;
                }

                await _context.SaveChangesAsync();
                return documentsAdded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded PDF");
                throw;
            }
        }

        /// <summary>
        /// Process PDF and automatically index to Qdrant
        /// </summary>
        public async Task<(int documentsAdded, int indexed)> ProcessAndIndexPdfAsync(
            string filePath,
            string title,
            string category = "general",
            bool autoChunk = true)
        {
            var documentsAdded = await ProcessPdfFileAsync(filePath, title, category, autoChunk);
            var indexed = await _chatbotService.IndexKnowledgeDocumentsAsync();
            
            return (documentsAdded, indexed);
        }
    }
}
