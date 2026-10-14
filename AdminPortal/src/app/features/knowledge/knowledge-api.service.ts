import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface KnowledgeDoc {
  id: string;
  title: string;
  category: string;
  content: string;
}

@Injectable({ providedIn: 'root' })
export class KnowledgeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:8000/api/documents';

  list(): Observable<KnowledgeDoc[]> {
    return this.http.get<KnowledgeDoc[]>(this.baseUrl);
  }

  create(doc: { title: string; category: string; content: string }): Observable<KnowledgeDoc> {
    return this.http.post<KnowledgeDoc>(this.baseUrl, doc);
  }

  update(id: string, doc: { title: string; category: string; content: string }): Observable<KnowledgeDoc> {
    return this.http.put<KnowledgeDoc>(`${this.baseUrl}/${id}`, doc);
  }

  delete(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${id}`);
  }
}
