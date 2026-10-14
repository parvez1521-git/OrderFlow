from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional
from knowledge_store import store

app = FastAPI(
    title="OrderFlow Knowledge & RAG Service",
    description="Python microservice for document ingestion, vector retrieval, and policy Q&A",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class DocumentCreate(BaseModel):
    title: str
    content: str
    category: Optional[str] = "General"

class SearchRequest(BaseModel):
    query: str
    top_k: Optional[int] = 3

@app.get("/health")
def health():
    return {"status": "Healthy", "service": "KnowledgeService", "total_docs": len(store.get_all())}

@app.get("/api/documents")
def list_documents():
    return store.get_all()

@app.post("/api/documents", status_code=201)
def create_document(doc: DocumentCreate):
    if not doc.title.strip() or not doc.content.strip():
        raise HTTPException(status_code=400, detail="Title and Content cannot be empty.")
    return store.add(doc.title, doc.content, doc.category or "General")

@app.put("/api/documents/{doc_id}")
def update_document(doc_id: str, doc: DocumentCreate):
    if not doc.title.strip() or not doc.content.strip():
        raise HTTPException(status_code=400, detail="Title and Content cannot be empty.")
    updated = store.update(doc_id, doc.title, doc.content, doc.category or "General")
    if not updated:
        raise HTTPException(status_code=404, detail="Document not found.")
    return updated

@app.delete("/api/documents/{doc_id}")
def delete_document(doc_id: str):
    if not store.delete(doc_id):
        raise HTTPException(status_code=404, detail="Document not found.")
    return {"message": "Document deleted successfully."}

@app.post("/api/documents/search")
def search_documents(req: SearchRequest):
    if not req.query.strip():
        return {"query": req.query, "results": []}
    results = store.search(req.query, req.top_k or 3)
    return {
        "query": req.query,
        "count": len(results),
        "results": results
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
