# RAG Chatbot Setup Guide

## Overview

This chatbot uses a RAG (Retrieval-Augmented Generation) pipeline with:

- **OpenAI** text-embedding-3-small for embeddings
- **Qdrant** for vector search
- **Groq** Llama 3.3 70B for LLM responses

## Architecture Flow

```
User Question
    ↓
Embed using OpenAI (text-embedding-3-small)
    ↓
Qdrant Search (Cosine similarity, Top K selection)
    ↓
Return relevant chunks
    ↓
Groq Llama 3.3 uses context to generate response
```

## Prerequisites

### 1. API Keys Required

- **OpenAI API Key**: Get from https://platform.openai.com/api-keys
- **Groq API Key**: Get from https://console.groq.com/keys
- **Qdrant**: Can run locally or use cloud

### 2. Install Qdrant Vector Database

#### Option A: Docker (Recommended)

```bash
docker pull qdrant/qdrant
docker run -p 6333:6333 qdrant/qdrant
```

#### Option B: Qdrant Cloud

Sign up at https://cloud.qdrant.io/ and get your cluster URL and API key

## Configuration

### 1. Update appsettings.json

Replace the placeholder values with your actual API keys:

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-xxxxxxxxxxxxx"
  },
  "Qdrant": {
    "Url": "http://localhost:6333", // or your Qdrant Cloud URL
    "ApiKey": "", // Leave empty for local, add for cloud
    "CollectionName": "vehicle_knowledge_base"
  },
  "Groq": {
    "ApiKey": "gsk_xxxxxxxxxxxxx"
  }
}
```

### 2. Create Database Migration

Run the following commands in Package Manager Console or terminal:

```bash
# Navigate to project directory
cd Vpassbackend

# Create migration
dotnet ef migrations add AddChatbotModels

# Update database
dotnet ef database update
```

Or use the batch file:

```bash
.\update_database.bat
```

## Seeding Knowledge Base

### 1. Add Knowledge Documents

Insert documents into the `KnowledgeDocuments` table:

```sql
INSERT INTO KnowledgeDocuments (Title, Content, Category, IsActive, CreatedAt, UpdatedAt)
VALUES
('Service Appointment Booking', 'To book a service appointment, navigate to the Appointments page...', 'appointments', 1, GETUTCDATE(), GETUTCDATE()),
('Vehicle Registration', 'You can register your vehicle by going to the Vehicles section...', 'vehicles', 1, GETUTCDATE(), GETUTCDATE()),
('Payment Methods', 'We accept credit cards, debit cards, and digital wallets...', 'payments', 1, GETUTCDATE(), GETUTCDATE());
```

### 2. Index Documents to Qdrant

After adding documents, call the indexing endpoint:

```http
POST https://localhost:7XXX/api/chatbot/index-knowledge
Authorization: Bearer {admin_token}
```

This will:

- Generate embeddings for all documents using OpenAI
- Store vectors in Qdrant with metadata
- Enable semantic search

## API Endpoints

### 1. Chat Endpoint

**POST** `/api/chatbot/chat`

Request:

```json
{
  "message": "How do I book an appointment?",
  "sessionId": "optional-session-id",
  "customerId": 123, // optional
  "userId": 456 // optional
}
```

Response:

```json
{
  "response": "To book an appointment...",
  "sessionId": "generated-or-provided-session-id",
  "conversationId": 1,
  "retrievedContext": ["Context chunk 1", "Context chunk 2"],
  "contextChunksUsed": 2
}
```

### 2. Get Conversation History

**GET** `/api/chatbot/conversation/{sessionId}`

Response:

```json
{
  "conversationId": 1,
  "sessionId": "abc123",
  "createdAt": "2025-12-05T10:00:00Z",
  "lastMessageAt": "2025-12-05T10:05:00Z",
  "messages": [
    {
      "messageId": 1,
      "role": "user",
      "content": "How do I book an appointment?",
      "createdAt": "2025-12-05T10:00:00Z"
    },
    {
      "messageId": 2,
      "role": "assistant",
      "content": "To book an appointment...",
      "createdAt": "2025-12-05T10:00:05Z"
    }
  ]
}
```

### 3. Index Knowledge Base (Admin Only)

**POST** `/api/chatbot/index-knowledge`
Requires Admin/SuperAdmin role

### 4. Health Check

**GET** `/api/chatbot/health`

## Testing the Chatbot

### 1. Start Qdrant

```bash
docker run -p 6333:6333 qdrant/qdrant
```

### 2. Run the Application

```bash
dotnet run
```

### 3. Index Sample Knowledge

Use Swagger or Postman to call the index endpoint with admin credentials.

### 4. Test Chat

Send a test message:

```bash
curl -X POST https://localhost:7XXX/api/chatbot/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "How do I register my vehicle?"
  }'
```

## Customization

### Adjust Search Parameters

In `ChatbotService.cs`, modify:

- **topK**: Number of context chunks to retrieve (default: 5)
- **scoreThreshold**: Minimum similarity score (default: 0.6)

### Customize System Prompt

In `GroqService.cs`, modify the `defaultSystemPrompt` to change the chatbot's behavior.

### Add Filters

Pass filters to Qdrant search:

```csharp
var filters = new Dictionary<string, object>
{
    { "category", "appointments" }
};
```

## Troubleshooting

### Qdrant Connection Error

- Ensure Qdrant is running on port 6333
- Check firewall settings
- Verify URL in appsettings.json

### OpenAI Rate Limits

- Monitor your API usage at https://platform.openai.com/usage
- Consider implementing caching for embeddings

### Groq API Errors

- Check API key validity
- Verify model name: `llama-3.3-70b-versatile`
- Monitor rate limits

## Performance Optimization

1. **Cache Embeddings**: Store embeddings to avoid regenerating
2. **Batch Processing**: Use batch embedding for multiple documents
3. **Index Optimization**: Regularly update Qdrant indices
4. **Context Window**: Limit conversation history to recent messages

## Security Notes

- Store API keys in environment variables or Azure Key Vault for production
- Implement rate limiting on chat endpoint
- Add authentication for sensitive operations
- Sanitize user inputs before processing

## Next Steps

1. Add more knowledge documents specific to your vehicle service system
2. Implement feedback mechanism to improve responses
3. Add analytics to track common questions
4. Consider implementing streaming responses for better UX
5. Add multi-language support if needed
