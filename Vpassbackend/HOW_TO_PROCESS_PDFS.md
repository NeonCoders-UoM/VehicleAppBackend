# Process PDFs - Quick Instructions

## Step 1: Edit Program.cs

Open `Program.cs` and find these lines near the top:

```csharp
// ============================================================================
// PDF PROCESSOR MODE: Uncomment the lines below to process PDFs, then run: dotnet run
// After processing, comment them out again to run the web API normally
// ============================================================================
// await Vpassbackend.Scripts.DirectPdfProcessor.Main(args);
// return;
// ============================================================================
```

**Uncomment** the two lines:
```csharp
await Vpassbackend.Scripts.DirectPdfProcessor.Main(args);
return;
```

## Step 2: Configure Your PDFs

Open `Scripts/DirectPdfProcessor.cs` and update the PDF list:

```csharp
var pdfsToProcess = new List<PdfToProcess>
{
    new PdfToProcess
    {
        FilePath = @"PDFs\VehicleManual.pdf",  // Your PDF path
        Title = "Vehicle Manual 2024",
        Category = "manuals",
        AutoChunk = true,
        MaxChunkSize = 1000
    },
    // Add more PDFs here...
};
```

## Step 3: Run

```powershell
dotnet run
```

## Step 4: Restore Normal Mode

After processing completes, **comment out** those lines again in Program.cs:

```csharp
// await Vpassbackend.Scripts.DirectPdfProcessor.Main(args);
// return;
```

Then run normally:
```powershell
dotnet run
```

Your API will work as usual, and PDFs are indexed!
