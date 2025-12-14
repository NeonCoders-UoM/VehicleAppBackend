# PDF Knowledge Base Integration

## Adding PDF Documents to Chatbot

You can now upload PDF files directly to your chatbot's knowledge base. The system will:
1. Extract text from the PDF
2. Automatically split into chunks (for large documents)
3. Save to database
4. Generate embeddings and index to Qdrant

## Methods to Add PDFs

### Method 1: Upload via API (Recommended)

**Endpoint:** `POST /api/chatbot/upload-pdf`  
**Authorization:** Admin/SuperAdmin only  
**Content-Type:** `multipart/form-data`

#### Using Postman/Thunder Client:

1. Set method to **POST**
2. URL: `https://localhost:7xxx/api/chatbot/upload-pdf`
3. Add Authorization header: `Bearer {your-admin-token}`
4. Go to **Body** tab → Select **form-data**
5. Add fields:

| Key | Type | Value |
|-----|------|-------|
| PdfFile | File | Select your PDF file |
| Title | Text | "Vehicle Service Manual" |
| Category | Text | "manuals" |
| AutoChunk | Text | true |
| MaxChunkSize | Text | 1000 |

6. Click **Send**

#### Using PowerShell:

```powershell
# Get admin token first
$loginUrl = "https://localhost:7xxx/api/auth/login"
$loginBody = @{
    email = "admin@example.com"
    password = "your-password"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginBody -ContentType "application/json" -SkipCertificateCheck
$token = $loginResponse.token

# Upload PDF
$pdfPath = "C:\path\to\your\document.pdf"
$uploadUrl = "https://localhost:7xxx/api/chatbot/upload-pdf"

$headers = @{
    "Authorization" = "Bearer $token"
}

$formData = @{
    PdfFile = Get-Item -Path $pdfPath
    Title = "Vehicle Maintenance Guide"
    Category = "maintenance"
    AutoChunk = "true"
    MaxChunkSize = "1000"
}

Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $headers -Form $formData -SkipCertificateCheck
```

#### Response:
```json
{
  "success": true,
  "documentsAdded": 15,
  "documentsIndexed": 15,
  "message": "Successfully processed PDF and added 15 document(s) to knowledge base. Indexed 15 total documents.",
  "errors": []
}
```

### Method 2: Manual Processing (For Local Files)

If you have PDFs stored on the server, you can process them directly:

```csharp
// In a controller or service
var pdfPath = @"C:\Documents\VehicleManual.pdf";
var (added, indexed) = await _pdfKnowledgeService.ProcessAndIndexPdfAsync(
    filePath: pdfPath,
    title: "Vehicle Maintenance Manual",
    category: "manuals",
    autoChunk: true
);
```

## Understanding Auto-Chunking

### Why Chunk?
- Large PDFs may exceed embedding limits
- Smaller chunks provide more precise context retrieval
- Better semantic search accuracy

### Chunking Parameters:

| Parameter | Default | Description |
|-----------|---------|-------------|
| AutoChunk | true | Enable automatic splitting |
| MaxChunkSize | 1000 | Maximum characters per chunk |
| Overlap | 200 | Characters overlapping between chunks |

### Example:
A 50-page PDF might be split into 30-40 chunks, each containing ~1000 characters with some overlap to maintain context.

## Supported PDF Types

✅ Text-based PDFs (searchable)  
✅ Multi-page documents  
✅ PDFs with tables and formatting  
⚠️ Scanned PDFs (OCR required - not included by default)  
⚠️ Image-heavy PDFs (only text will be extracted)

## Required NuGet Package

Install the iText7 library for PDF processing:

```bash
cd Vpassbackend
dotnet add package itext7
```

Or add to `.csproj`:
```xml
<PackageReference Include="itext7" Version="8.0.2" />
```

## Complete Workflow Example

### 1. Upload PDF
```powershell
# Upload vehicle service manual
Invoke-RestMethod -Uri "https://localhost:7xxx/api/chatbot/upload-pdf" `
  -Method Post `
  -Headers @{"Authorization" = "Bearer $token"} `
  -Form @{
    PdfFile = Get-Item "C:\Manuals\ServiceGuide.pdf"
    Title = "Complete Service Guide"
    Category = "service-manuals"
    AutoChunk = "true"
  } `
  -SkipCertificateCheck
```

### 2. Verify in Database
```sql
SELECT * FROM KnowledgeDocuments 
WHERE Title LIKE '%Service Guide%'
ORDER BY CreatedAt DESC;
```

### 3. Check Qdrant
The documents are automatically indexed after upload. Verify in Qdrant Cloud dashboard or:

```http
POST https://localhost:7xxx/api/chatbot/chat
Content-Type: application/json

{
  "message": "What does the service manual say about oil changes?"
}
```

## Best Practices

1. **Organize by Category**
   - Use meaningful categories: "manuals", "policies", "faqs", "procedures"
   - Helps with filtered searches

2. **Clear Titles**
   - Use descriptive titles: "Brake System Maintenance Manual 2024"
   - Avoid generic names like "Document1.pdf"

3. **Chunk Size**
   - Technical manuals: 800-1000 characters
   - FAQs/Short content: 500-700 characters
   - Detailed guides: 1200-1500 characters

4. **File Size Limits**
   - Recommended: < 10MB per PDF
   - For larger files, split before uploading

5. **Update Regularly**
   - Mark outdated documents as inactive:
   ```sql
   UPDATE KnowledgeDocuments 
   SET IsActive = 0 
   WHERE Title = 'Old Manual 2020';
   ```
   - Then re-index:
   ```http
   POST /api/chatbot/index-knowledge
   ```

## Troubleshooting

### PDF Upload Fails
- Check file size (max 100MB by default in ASP.NET)
- Verify PDF is not corrupted
- Ensure it's a valid PDF file

### No Text Extracted
- PDF might be scanned (image-based)
- Consider using OCR tools first
- Try opening PDF in Adobe Reader to verify text

### Upload Timeout
- For large PDFs, increase timeout in `appsettings.json`:
```json
{
  "Kestrel": {
    "Limits": {
      "MaxRequestBodySize": 104857600,
      "RequestHeadersTimeout": "00:05:00"
    }
  }
}
```

## Advanced: Batch Upload Script

```powershell
# Upload multiple PDFs from a folder
$pdfFolder = "C:\VehicleDocuments"
$uploadUrl = "https://localhost:7xxx/api/chatbot/upload-pdf"

# Get token
$token = "your-jwt-token"
$headers = @{"Authorization" = "Bearer $token"}

# Process all PDFs
Get-ChildItem -Path $pdfFolder -Filter *.pdf | ForEach-Object {
    Write-Host "Uploading: $($_.Name)"
    
    $formData = @{
        PdfFile = Get-Item $_.FullName
        Title = $_.BaseName
        Category = "vehicle-docs"
        AutoChunk = "true"
    }
    
    try {
        $result = Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $headers -Form $formData -SkipCertificateCheck
        Write-Host "✓ Success: Added $($result.documentsAdded) chunks" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Start-Sleep -Seconds 2  # Rate limiting
}
```

## Next Steps

1. Install iText7 package
2. Upload your first PDF via Postman
3. Test chatbot with questions about the PDF content
4. Monitor Qdrant to verify embeddings are stored
5. Adjust chunk sizes based on your content type
