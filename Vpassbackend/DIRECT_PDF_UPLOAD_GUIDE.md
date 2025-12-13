# Quick Start: Upload PDFs Directly to Codebase

## Method 1: Using DirectPdfProcessor Script (Recommended)

### Step 1: Install Required Package
```powershell
cd Vpassbackend
dotnet add package itext7
```

### Step 2: Configure Your PDFs

Edit `Scripts\DirectPdfProcessor.cs` and add your PDF files:

```csharp
var pdfsToProcess = new List<PdfToProcess>
{
    new PdfToProcess
    {
        FilePath = @"C:\MyDocuments\VehicleManual.pdf",
        Title = "Vehicle Service Manual 2024",
        Category = "manuals",
        AutoChunk = true,
        MaxChunkSize = 1000
    },
    new PdfToProcess
    {
        FilePath = @"C:\MyDocuments\FAQ.pdf",
        Title = "Frequently Asked Questions",
        Category = "faq",
        AutoChunk = true,
        MaxChunkSize = 800
    },
    // Add more PDFs here...
};
```

### Step 3: Run the Script

```powershell
cd Vpassbackend
dotnet run --project Scripts\DirectPdfProcessor.cs
```

Or compile and run:
```powershell
dotnet build
cd bin\Debug\net8.0
.\DirectPdfProcessor.exe
```

### What Happens:
```
📄 Processing: Vehicle Service Manual 2024
   File: C:\MyDocuments\VehicleManual.pdf
   ✅ Added 15 document chunk(s) to database

📄 Processing: Frequently Asked Questions
   File: C:\MyDocuments\FAQ.pdf
   ✅ Added 8 document chunk(s) to database

─────────────────────────────────────────────────────────
📊 Summary:
   PDFs Processed: 2
   Document Chunks Added: 23

🔄 Indexing documents to Qdrant Vector Database...
✅ Successfully indexed 23 documents to Qdrant

✨ Process Complete!
```

---

## Method 2: Copy PDFs to Project Folder

### Step 1: Create PDFs Folder
```powershell
cd Vpassbackend
mkdir PDFs
```

### Step 2: Copy Your PDFs
```
Vpassbackend/
├── PDFs/
│   ├── VehicleServiceManual.pdf
│   ├── MaintenanceGuide.pdf
│   ├── FAQ.pdf
│   └── PricingGuide.pdf
```

### Step 3: Update Script to Use Relative Paths

```csharp
var pdfsToProcess = new List<PdfToProcess>
{
    new PdfToProcess
    {
        FilePath = @"PDFs\VehicleServiceManual.pdf",  // Relative path
        Title = "Vehicle Service Manual",
        Category = "manuals"
    },
    new PdfToProcess
    {
        FilePath = @"PDFs\MaintenanceGuide.pdf",
        Title = "Maintenance Guide",
        Category = "maintenance"
    }
};
```

### Step 4: Run Script
```powershell
dotnet run --project Scripts\DirectPdfProcessor.cs
```

---

## Method 3: Bulk Process All PDFs in Folder

Create an auto-processor that handles all PDFs in a directory:

```csharp
// In DirectPdfProcessor.cs Main method
var pdfDirectory = @"C:\VehicleDocuments\PDFs";
var pdfFiles = Directory.GetFiles(pdfDirectory, "*.pdf");

var pdfsToProcess = pdfFiles.Select(filePath => new PdfToProcess
{
    FilePath = filePath,
    Title = Path.GetFileNameWithoutExtension(filePath),
    Category = "general",
    AutoChunk = true,
    MaxChunkSize = 1000
}).ToList();
```

This will automatically process every PDF in the folder!

---

## Complete Workflow Example

### 1. Place PDFs in Project
```
D:\sprint588\Github\VehicleAppBackend\Vpassbackend\PDFs\
├── ServiceManual_2024.pdf
├── MaintenanceSchedule.pdf
└── CustomerFAQ.pdf
```

### 2. Configure appsettings.json
```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-your-actual-key-here"
  },
  "Qdrant": {
    "Url": "https://your-cluster.cloud.qdrant.io",
    "ApiKey": "your-qdrant-key",
    "CollectionName": "vehicle_knowledge_base"
  },
  "Groq": {
    "ApiKey": "gsk_your-groq-key"
  }
}
```

### 3. Run Database Migration
```powershell
cd Vpassbackend
dotnet ef migrations add AddChatbotModels
dotnet ef database update
```

### 4. Process PDFs
```powershell
dotnet run --project Scripts\DirectPdfProcessor.cs
```

### 5. Verify in Database
```sql
SELECT 
    DocumentId, 
    Title, 
    Category, 
    LEFT(Content, 100) AS ContentPreview,
    QdrantId,
    CreatedAt
FROM KnowledgeDocuments
ORDER BY CreatedAt DESC;
```

### 6. Test Chatbot
```powershell
# Start your API
dotnet run

# In another terminal, test the chatbot
curl -X POST "https://localhost:7xxx/api/chatbot/chat" `
  -H "Content-Type: application/json" `
  -d '{
    "message": "What does the service manual say about oil changes?"
  }'
```

---

## Advantages of Direct Upload

✅ **No API calls needed** - Run directly on your machine  
✅ **Batch processing** - Upload multiple PDFs at once  
✅ **Full control** - Edit, preview, version control PDFs  
✅ **Offline capable** - Process PDFs without running the API  
✅ **Repeatable** - Can re-run script to re-index  
✅ **Version control** - PDFs can be committed to Git if needed  

---

## File Structure

```
VehicleAppBackend/
└── Vpassbackend/
    ├── Scripts/
    │   └── DirectPdfProcessor.cs        ← Run this!
    ├── Services/
    │   ├── PdfKnowledgeService.cs       ← PDF processing logic
    │   ├── ChatbotService.cs            ← Indexing logic
    │   ├── OpenAIEmbeddingService.cs    ← Creates embeddings
    │   └── QdrantService.cs             ← Vector storage
    ├── PDFs/                            ← Your PDFs here
    │   ├── Manual1.pdf
    │   └── Manual2.pdf
    ├── appsettings.json                 ← API keys
    └── ARCHITECTURE_OVERVIEW.md         ← How everything works
```

---

## Troubleshooting

### "File not found"
- Check PDF path is correct
- Use absolute path: `C:\Full\Path\To\File.pdf`
- Or use relative from project root: `PDFs\File.pdf`

### "Failed to extract text"
- PDF might be scanned (image-based) - needs OCR
- Try opening in Adobe Reader to verify it has text
- Check PDF is not corrupted

### "OpenAI API error"
- Verify API key in appsettings.json
- Check you have credits in OpenAI account
- Test API key: https://platform.openai.com/api-keys

### "Qdrant connection failed"
- Verify Qdrant URL is correct
- Check API key if using Qdrant Cloud
- For local: ensure Docker is running `docker ps`

---

## Next Steps

1. Read `ARCHITECTURE_OVERVIEW.md` to understand the full system
2. Process your first PDF using DirectPdfProcessor
3. Query the chatbot to test retrieval
4. Monitor Qdrant dashboard to see vectors
5. Check SQL database to see stored documents
