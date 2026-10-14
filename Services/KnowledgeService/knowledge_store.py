import os
import json
import uuid
from typing import List, Dict, Any, Optional
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
import numpy as np

DATA_FILE = os.path.join(os.path.dirname(__file__), "documents.json")

DEFAULT_DOCUMENTS = [
    {
        "id": "doc-return-policy",
        "title": "Return & Refund Policy",
        "category": "Returns",
        "content": (
            "OrderFlow Return & Refund Policy:\n"
            "1. Eligibility: Customers can return items within 14 calendar days of delivery.\n"
            "2. Condition: Items must be unused, in their original packaging, with all tags and receipts intact.\n"
            "3. Non-returnable: Opened electronics and assembled furniture (e.g., Beds) can only be returned if damaged upon arrival.\n"
            "4. Refunds: Approved refunds are credited to the original payment method within 3-5 business days after product inspection.\n"
            "5. Restocking Fee: A 15% restocking fee may apply for customer remorse returns."
        )
    },
    {
        "id": "doc-shipping-policy",
        "title": "Shipping & Delivery Policy",
        "category": "Shipping",
        "content": (
            "OrderFlow Shipping Policy:\n"
            "1. Delivery Times: Standard shipping takes 3 to 5 business days across India. Express shipping delivers within 24 to 48 hours in metro cities.\n"
            "2. Shipping Charges: Free shipping is automatically applied to all orders with a total above ₹10,000. Orders below ₹10,000 incur a flat ₹200 fee.\n"
            "3. Order Tracking: As soon as an order is 'Confirmed', real-time tracking is assigned and visible in the portal.\n"
            "4. Damaged in Transit: If a package is visibly damaged upon delivery, please refuse delivery and contact customer support immediately."
        )
    },
    {
        "id": "doc-cancellation-policy",
        "title": "Cancellation & Order Modification",
        "category": "Orders",
        "content": (
            "OrderFlow Cancellation & Modification Terms:\n"
            "1. Pending Orders: Orders in 'Pending' status can be cancelled immediately from the portal or assistant without charge.\n"
            "2. Confirmed Orders: Once an order reaches 'Confirmed' status, cancellation requests must be submitted within 2 hours.\n"
            "3. Payment Failures: If a payment attempt fails ('PaymentFailed' status), the order is automatically marked cancelled and any reserved inventory is immediately released back to available stock via Saga compensation."
        )
    }
]

class KnowledgeStore:
    def __init__(self):
        self.documents: List[Dict[str, Any]] = []
        self.chunks: List[Dict[str, Any]] = []
        self.vectorizer: Optional[TfidfVectorizer] = None
        self.tfidf_matrix: Optional[np.ndarray] = None
        self._load()

    def _load(self):
        if os.path.exists(DATA_FILE):
            try:
                with open(DATA_FILE, "r", encoding="utf-8") as f:
                    self.documents = json.load(f)
            except Exception:
                self.documents = list(DEFAULT_DOCUMENTS)
        else:
            self.documents = list(DEFAULT_DOCUMENTS)
            self._save()

        self._rebuild_index()

    def _save(self):
        with open(DATA_FILE, "w", encoding="utf-8") as f:
            json.dump(self.documents, f, indent=2)

    def _rebuild_index(self):
        self.chunks = []
        for doc in self.documents:
            # Chunk by double newlines or lines
            paragraphs = [p.strip() for p in doc["content"].split("\n") if p.strip()]
            for p in paragraphs:
                self.chunks.append({
                    "doc_id": doc["id"],
                    "title": doc["title"],
                    "category": doc.get("category", "General"),
                    "text": p
                })

        if self.chunks:
            corpus = [f"{c['title']}: {c['text']}" for c in self.chunks]
            self.vectorizer = TfidfVectorizer(ngram_range=(1, 2), stop_words="english")
            self.tfidf_matrix = self.vectorizer.fit_transform(corpus)
        else:
            self.vectorizer = None
            self.tfidf_matrix = None

    def get_all(self) -> List[Dict[str, Any]]:
        return self.documents

    def add(self, title: str, content: str, category: str = "General") -> Dict[str, Any]:
        doc = {
            "id": f"doc-{uuid.uuid4().hex[:8]}",
            "title": title.strip(),
            "category": category.strip(),
            "content": content.strip()
        }
        self.documents.append(doc)
        self._save()
        self._rebuild_index()
        return doc

    def update(self, doc_id: str, title: str, content: str, category: str = "General") -> Optional[Dict[str, Any]]:
        for doc in self.documents:
            if doc["id"] == doc_id:
                doc["title"] = title.strip()
                doc["content"] = content.strip()
                doc["category"] = category.strip()
                self._save()
                self._rebuild_index()
                return doc
        return None

    def delete(self, doc_id: str) -> bool:
        initial_len = len(self.documents)
        self.documents = [d for d in self.documents if d["id"] != doc_id]
        if len(self.documents) < initial_len:
            self._save()
            self._rebuild_index()
            return True
        return False

    def search(self, query: str, top_k: int = 3) -> List[Dict[str, Any]]:
        if not self.chunks or self.vectorizer is None or self.tfidf_matrix is None:
            return []

        query_vec = self.vectorizer.transform([query])
        scores = cosine_similarity(query_vec, self.tfidf_matrix).flatten()

        # Get top indices with score > 0
        ranked_indices = np.argsort(scores)[::-1]
        results = []
        for idx in ranked_indices[:top_k]:
            score = float(scores[idx])
            if score > 0.05:  # threshold
                chunk = self.chunks[idx]
                results.append({
                    "doc_id": chunk["doc_id"],
                    "title": chunk["title"],
                    "category": chunk["category"],
                    "snippet": chunk["text"],
                    "score": round(score, 3)
                })

        return results

store = KnowledgeStore()
