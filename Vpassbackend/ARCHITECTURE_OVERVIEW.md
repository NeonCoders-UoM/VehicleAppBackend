# Complete RAG Chatbot Architecture & Data Flow

## 🏗️ System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         USER INTERACTION                                 │
│  User asks: "How do I book a service appointment?"                      │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      CHATBOT CONTROLLER                                  │
│  POST /api/chatbot/chat                                                 │
│  Receives question + optional sessionId/customerId                      │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      CHATBOT SERVICE (Orchestrator)                      │
│  - Get/Create conversation session                                      │
│  - Coordinate the RAG pipeline                                          │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                    ┌────────────┴────────────┐
                    │                         │
                    ▼                         ▼
         ┌──────────────────┐    ┌──────────────────────┐
         │  SQL DATABASE    │    │  RAG PIPELINE        │
         │  (Local Storage) │    │  (Vector Search)     │
         └──────────────────┘    └──────────────────────┘
                                          │
                    ┌─────────────────────┼─────────────────────┐
                    │                     │                     │
                    ▼                     ▼                     ▼
         ┌─────────────────┐  ┌──────────────────┐  ┌─────────────────┐
         │  OpenAI         │  │  Qdrant Vector   │  │  Groq LLM       │
         │  Embedding API  │  │  Database        │  │  (Llama 3.3)    │
         │  (Cloud)        │  │  (Cloud/Local)   │  │  (Cloud)        │
         └─────────────────┘  └──────────────────┘  └─────────────────┘
```

---

## 📊 Detailed Process Flow

### PHASE 1: PDF Upload & Processing

```
┌──────────────────────────────────────────────────────────────────┐
│  1. PDF FILE (Local or Uploaded)                                │
│     Example: VehicleServiceManual.pdf (2MB, 50 pages)           │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  2. PDF EXTRACTION (PdfKnowledgeService)                        │
│     - Uses iText7 library                                       │
│     - Extracts raw text page by page                            │
│     - Result: ~15,000 characters of text                        │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  3. TEXT CHUNKING (if AutoChunk = true)                        │
│     - Splits text into smaller pieces                           │
│     - Default: 1000 characters per chunk                        │
│     - Adds 200 character overlap between chunks                 │
│     - Result: 15 chunks created                                 │
│                                                                  │
│     Example Chunks:                                             │
│     Chunk 1: "To book a service appointment, visit..."          │
│     Chunk 2: "...visit the appointments page. Select..."        │
│     Chunk 3: "...Select your service center and..."             │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  4. SAVE TO SQL DATABASE (Local)                                │
│     Database: VehiclePassportAppNew                             │
│     Table: KnowledgeDocuments                                   │
│                                                                  │
│     Each chunk saved as:                                        │
│     ┌────────────────────────────────────────────────┐          │
│     │ DocumentId: 1                                  │          │
│     │ Title: "Vehicle Service Manual (Part 1/15)"   │          │
│     │ Content: "To book a service appointment..."   │          │
│     │ Category: "manuals"                            │          │
│     │ QdrantId: NULL (not indexed yet)               │          │
│     │ IsActive: true                                 │          │
│     │ CreatedAt: 2025-12-05 10:30:00                │          │
│     └────────────────────────────────────────────────┘          │
│                                                                  │
│     Total: 15 rows inserted                                     │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
                   ✅ PDF processed and stored locally
```

---

### PHASE 2: Indexing to Qdrant (Vector Database)

```
┌──────────────────────────────────────────────────────────────────┐
│  5. TRIGGER INDEXING                                            │
│     POST /api/chatbot/index-knowledge                           │
│     OR DirectPdfProcessor.cs script                             │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  6. FETCH DOCUMENTS FROM SQL                                    │
│     Query: SELECT * FROM KnowledgeDocuments WHERE IsActive=1    │
│     Result: 15 documents retrieved                              │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                   ┌─────────┴──────────┐
                   │  For Each Document │
                   └─────────┬──────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  7. GENERATE EMBEDDING (OpenAI API - Cloud)                     │
│     Model: text-embedding-3-small                               │
│     Input: "To book a service appointment, visit..."            │
│                                                                  │
│     Process:                                                     │
│     1. Send text to OpenAI API                                  │
│     2. OpenAI converts text to 1536-dimensional vector          │
│     3. Each dimension is a float number                         │
│                                                                  │
│     Output: [0.023, -0.891, 0.445, ..., 0.129]                 │
│            (1536 numbers representing semantic meaning)         │
│                                                                  │
│     Cost: ~$0.00002 per document                                │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  8. STORE IN QDRANT (Vector Database - Cloud or Local)         │
│     Collection: vehicle_knowledge_base                          │
│                                                                  │
│     Point Structure:                                            │
│     ┌──────────────────────────────────────────────┐            │
│     │ id: "1"                                      │            │
│     │ vector: [0.023, -0.891, 0.445, ..., 0.129] │            │
│     │         (1536 dimensions)                   │            │
│     │ payload: {                                   │            │
│     │   "title": "Vehicle Service Manual (1/15)", │            │
│     │   "content": "To book a service...",        │            │
│     │   "category": "manuals",                    │            │
│     │   "documentId": 1                            │            │
│     │ }                                            │            │
│     └──────────────────────────────────────────────┘            │
│                                                                  │
│     Repeat for all 15 documents                                 │
│     Result: 15 vectors stored in Qdrant                         │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  9. UPDATE SQL DATABASE                                         │
│     UPDATE KnowledgeDocuments SET QdrantId='1' WHERE Id=1      │
│     Links SQL record to Qdrant vector                           │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
              ✅ All documents indexed and searchable
```

---

### PHASE 3: User Query Processing (RAG Pipeline)

```
┌──────────────────────────────────────────────────────────────────┐
│  10. USER SENDS QUESTION                                        │
│      "How do I book a service appointment?"                     │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  11. CREATE/GET CONVERSATION (SQL Database)                     │
│      Check if sessionId exists in ChatConversations table       │
│      If not, create new conversation:                           │
│      ┌────────────────────────────────────────┐                 │
│      │ ConversationId: 123                    │                 │
│      │ SessionId: "abc-def-456"               │                 │
│      │ CustomerId: 789 (optional)             │                 │
│      │ CreatedAt: 2025-12-05 14:30:00        │                 │
│      │ IsActive: true                         │                 │
│      └────────────────────────────────────────┘                 │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  12. EMBED USER QUESTION (OpenAI API)                           │
│      Input: "How do I book a service appointment?"              │
│      Output: [0.112, -0.334, 0.667, ..., 0.221]                │
│             (1536-dimensional vector)                           │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  13. SEARCH QDRANT (Cosine Similarity)                          │
│      Compare question vector with all document vectors          │
│                                                                  │
│      Qdrant calculates similarity:                              │
│      Document 1: 0.89 (89% similar) ← High match!              │
│      Document 5: 0.85 (85% similar) ← Good match               │
│      Document 8: 0.78 (78% similar) ← Decent match             │
│      Document 2: 0.45 (45% similar) ← Low match                │
│      Document 11: 0.32 (32% similar) ← Low match               │
│                                                                  │
│      Filter: score_threshold = 0.6 (60%)                        │
│      Select: Top 5 results (topK = 5)                           │
│                                                                  │
│      Retrieved Documents:                                       │
│      1. "To book a service appointment, visit..." (89%)         │
│      2. "Select your preferred service center..." (85%)         │
│      3. "Choose available time slot from..." (78%)              │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  14. RETRIEVE CONVERSATION HISTORY (SQL Database)               │
│      Query: SELECT * FROM ChatMessages                          │
│             WHERE ConversationId = 123                          │
│             ORDER BY CreatedAt                                  │
│                                                                  │
│      Previous messages (if any):                                │
│      User: "What services do you offer?"                        │
│      Bot: "We offer oil changes, brake service..."             │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  15. GENERATE RESPONSE (Groq LLM - Cloud)                       │
│      Model: llama-3.3-70b-versatile                            │
│                                                                  │
│      Input to LLM:                                              │
│      ┌──────────────────────────────────────────┐               │
│      │ System Prompt:                           │               │
│      │ "You are a helpful assistant for         │               │
│      │  vehicle service. Use this context:      │               │
│      │                                           │               │
│      │  [Context 1]                              │               │
│      │  To book a service appointment, visit...  │               │
│      │                                           │               │
│      │  [Context 2]                              │               │
│      │  Select your preferred service center...  │               │
│      │                                           │               │
│      │  [Context 3]                              │               │
│      │  Choose available time slot from..."      │               │
│      │                                           │               │
│      │ User: "How do I book an appointment?"    │               │
│      └──────────────────────────────────────────┘               │
│                                                                  │
│      LLM Processing:                                            │
│      - Reads all context chunks                                 │
│      - Understands user question                                │
│      - Generates natural response based on context              │
│                                                                  │
│      Output:                                                    │
│      "To book a service appointment, you can visit the          │
│       appointments page and select your preferred service       │
│       center. Then choose an available time slot that works     │
│       for you. You'll receive a confirmation email once your    │
│       appointment is confirmed."                                │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  16. SAVE CONVERSATION (SQL Database)                           │
│      Insert 2 new rows in ChatMessages:                         │
│                                                                  │
│      Message 1 (User):                                          │
│      ┌────────────────────────────────────────┐                 │
│      │ MessageId: 456                         │                 │
│      │ ConversationId: 123                    │                 │
│      │ Role: "user"                           │                 │
│      │ Content: "How do I book...?"           │                 │
│      │ CreatedAt: 2025-12-05 14:30:15        │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│      Message 2 (Assistant):                                     │
│      ┌────────────────────────────────────────┐                 │
│      │ MessageId: 457                         │                 │
│      │ ConversationId: 123                    │                 │
│      │ Role: "assistant"                      │                 │
│      │ Content: "To book a service..."        │                 │
│      │ CreatedAt: 2025-12-05 14:30:17        │                 │
│      └────────────────────────────────────────┘                 │
│                                                                  │
│      Update conversation:                                       │
│      UPDATE ChatConversations                                   │
│      SET LastMessageAt = NOW()                                  │
│      WHERE ConversationId = 123                                 │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  17. RETURN RESPONSE TO USER                                    │
│      {                                                          │
│        "response": "To book a service appointment...",          │
│        "sessionId": "abc-def-456",                              │
│        "conversationId": 123,                                   │
│        "retrievedContext": [                                    │
│          "To book a service appointment, visit...",             │
│          "Select your preferred service center...",             │
│          "Choose available time slot from..."                   │
│        ],                                                       │
│        "contextChunksUsed": 3                                   │
│      }                                                          │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
                      ✅ User receives answer
```

---

## 🗄️ Database Architecture

### LOCAL SQL DATABASE (VehiclePassportAppNew)

#### Table: KnowledgeDocuments
Stores the actual document text and metadata

```sql
CREATE TABLE KnowledgeDocuments (
    DocumentId INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(500) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,        -- The actual text
    Category NVARCHAR(100),                 -- e.g., "manuals", "faq"
    QdrantId NVARCHAR(100),                -- Links to Qdrant vector
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Example Data:
-- DocumentId | Title                    | Content              | QdrantId
-- 1          | Service Manual (1/15)    | "To book a..."       | "1"
-- 2          | Service Manual (2/15)    | "Select center..."   | "2"
-- 3          | Maintenance Guide (1/8)  | "Oil change..."      | "3"
```

#### Table: ChatConversations
Tracks user chat sessions

```sql
CREATE TABLE ChatConversations (
    ConversationId INT PRIMARY KEY IDENTITY(1,1),
    SessionId NVARCHAR(100) NOT NULL UNIQUE,
    CustomerId INT NULL,                    -- Optional: authenticated user
    UserId INT NULL,                        -- Optional: staff user
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    LastMessageAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1,
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Example Data:
-- ConversationId | SessionId     | CustomerId | CreatedAt
-- 123            | abc-def-456   | 789        | 2025-12-05 14:30:00
-- 124            | xyz-123-999   | NULL       | 2025-12-05 15:00:00
```

#### Table: ChatMessages
Stores all messages in conversations

```sql
CREATE TABLE ChatMessages (
    MessageId INT PRIMARY KEY IDENTITY(1,1),
    ConversationId INT NOT NULL,
    Role NVARCHAR(50) NOT NULL,             -- "user" or "assistant"
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (ConversationId) REFERENCES ChatConversations(ConversationId)
);

-- Example Data:
-- MessageId | ConversationId | Role       | Content
-- 456       | 123            | user       | "How do I book...?"
-- 457       | 123            | assistant  | "To book a service..."
-- 458       | 123            | user       | "What about pricing?"
-- 459       | 123            | assistant  | "Our pricing starts..."
```

---

### QDRANT VECTOR DATABASE (Cloud or Local)

#### Collection: vehicle_knowledge_base

**Storage Structure:**
```json
{
  "collection_name": "vehicle_knowledge_base",
  "vector_size": 1536,
  "distance": "Cosine",
  
  "points": [
    {
      "id": "1",
      "vector": [0.023, -0.891, 0.445, ..., 0.129],  // 1536 floats
      "payload": {
        "title": "Vehicle Service Manual (Part 1/15)",
        "content": "To book a service appointment, visit the appointments page...",
        "category": "manuals",
        "documentId": 1
      }
    },
    {
      "id": "2",
      "vector": [0.156, -0.234, 0.778, ..., 0.445],
      "payload": {
        "title": "Vehicle Service Manual (Part 2/15)",
        "content": "Select your preferred service center from the dropdown...",
        "category": "manuals",
        "documentId": 2
      }
    }
    // ... 13 more vectors
  ]
}
```

**How Qdrant Works:**

1. **Indexing:** Creates optimized index structure (HNSW algorithm)
2. **Searching:** 
   - Takes query vector [0.112, -0.334, 0.667, ...]
   - Compares with all stored vectors using cosine similarity
   - Returns closest matches ranked by similarity score
3. **Filtering:** Can filter by payload (e.g., category="manuals")

---

## 🔄 Data Flow Summary

### When Uploading PDF:
```
PDF File 
  → Extract Text (iText7)
  → Split into Chunks
  → Save to SQL (KnowledgeDocuments table)
  → Generate Embeddings (OpenAI API)
  → Store Vectors in Qdrant
  → Update SQL with QdrantId
```

### When User Asks Question:
```
User Question
  → Create/Get Session (SQL: ChatConversations)
  → Embed Question (OpenAI API)
  → Search Similar Docs (Qdrant vector search)
  → Get Chat History (SQL: ChatMessages)
  → Generate Answer (Groq LLM with context)
  → Save Q&A (SQL: ChatMessages)
  → Return Response to User
```

### Storage Locations:

| Data Type | Where Stored | Purpose |
|-----------|--------------|---------|
| Document Text | SQL Server (local) | Source of truth, editable |
| Document Vectors | Qdrant (cloud/local) | Fast semantic search |
| Conversations | SQL Server (local) | Chat history, tracking |
| Messages | SQL Server (local) | Full conversation logs |

---

## 💰 Cost & Performance

### API Costs (Approximate):
- **OpenAI Embedding**: $0.00002 per document (~$0.30 per 15,000 docs)
- **Groq LLM**: Free tier available, then ~$0.10 per million tokens
- **Qdrant Cloud**: Free tier: 1GB storage, then $0.50/GB/month

### Performance:
- **Embedding Generation**: ~200ms per document
- **Qdrant Search**: ~10-50ms for similarity search
- **LLM Response**: ~1-3 seconds
- **Total Response Time**: ~2-5 seconds

---

## 🎯 Key Takeaways

1. **Two Databases Working Together:**
   - SQL = Stores actual data, conversations, metadata
   - Qdrant = Stores vectors for fast semantic search

2. **Why Both?**
   - SQL: Easy to edit, query, manage traditional data
   - Qdrant: Specialized for vector similarity search (AI)

3. **RAG Pipeline:**
   - Retrieval: Find relevant info (Qdrant)
   - Augmented: Add context to prompt
   - Generation: LLM creates answer (Groq)

4. **Everything is Linked:**
   - SQL DocumentId ← → Qdrant vector id
   - Maintains consistency between both systems
