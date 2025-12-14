using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Services;

namespace Vpassbackend.Scripts
{
    /// <summary>
    /// Direct script to process PDFs from local filesystem and add to knowledge base
    /// Run this independently without API calls
    /// </summary>
    public class DirectPdfProcessor
    {
        public static async Task Main(string[] args)
        {
            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Setup database context
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            using var context = new ApplicationDbContext(optionsBuilder.Options);

            // Setup services
            var httpClient = new HttpClient();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var embeddingLogger = loggerFactory.CreateLogger<OpenAIEmbeddingService>();
            var qdrantLogger = loggerFactory.CreateLogger<QdrantService>();
            var groqLogger = loggerFactory.CreateLogger<GroqService>();
            var chatbotLogger = loggerFactory.CreateLogger<ChatbotService>();
            var pdfLogger = loggerFactory.CreateLogger<PdfKnowledgeService>();

            var embeddingService = new OpenAIEmbeddingService(configuration, httpClient);
            var qdrantService = new QdrantService(configuration, httpClient);
            var groqService = new GroqService(configuration, httpClient);
            var chatbotService = new ChatbotService(context, embeddingService, qdrantService, groqService, chatbotLogger);
            var pdfKnowledgeService = new PdfKnowledgeService(context, chatbotService, pdfLogger);

            // ==============================================
            // CONFIGURE YOUR PDFs HERE
            // ==============================================
            var pdfsToProcess = new List<PdfToProcess>
            {
                new PdfToProcess
                {
                    FilePath = @"PDFs\VehicleManual.pdf",  // Your PDF path
        Title = "Vehicle Manual 2024",
        Category = "manuals",
                    AutoChunk = true,
                    MaxChunkSize = 1000
                }
                // Add more PDFs here...
            };

            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Direct PDF to Knowledge Base Processor              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            int totalDocumentsAdded = 0;
            int totalPdfsProcessed = 0;

            foreach (var pdf in pdfsToProcess)
            {
                Console.WriteLine($"📄 Processing: {pdf.Title}");
                Console.WriteLine($"   File: {pdf.FilePath}");

                if (!File.Exists(pdf.FilePath))
                {
                    Console.WriteLine($"   ❌ File not found: {pdf.FilePath}");
                    Console.WriteLine();
                    continue;
                }

                try
                {
                    // Process PDF and add to database
                    var documentsAdded = await pdfKnowledgeService.ProcessPdfFileAsync(
                        filePath: pdf.FilePath,
                        title: pdf.Title,
                        category: pdf.Category,
                        autoChunk: pdf.AutoChunk,
                        maxChunkSize: pdf.MaxChunkSize
                    );

                    totalDocumentsAdded += documentsAdded;
                    totalPdfsProcessed++;

                    Console.WriteLine($"   ✅ Added {documentsAdded} document chunk(s) to database");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Error: {ex.Message}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine($"📊 Summary:");
            Console.WriteLine($"   PDFs Processed: {totalPdfsProcessed}");
            Console.WriteLine($"   Document Chunks Added: {totalDocumentsAdded}");
            Console.WriteLine();

            // Now index everything to Qdrant
            if (totalDocumentsAdded > 0)
            {
                Console.WriteLine("🔄 Indexing documents to Qdrant Vector Database...");
                try
                {
                    var indexed = await chatbotService.IndexKnowledgeDocumentsAsync();
                    Console.WriteLine($"✅ Successfully indexed {indexed} documents to Qdrant");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Indexing error: {ex.Message}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("✨ Process Complete!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private class PdfToProcess
        {
            public string FilePath { get; set; }
            public string Title { get; set; }
            public string Category { get; set; }
            public bool AutoChunk { get; set; } = true;
            public int MaxChunkSize { get; set; } = 1000;
        }
    }
}
