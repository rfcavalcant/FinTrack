import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Account, OpenAccountRequest, RenameAccountRequest } from '../../models/account.models';

@Injectable({ providedIn: 'root' })
export class AccountsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/accounts`;

  getAccounts() {
    return this.http.get<Account[]>(this.baseUrl);
  }

  openAccount(request: OpenAccountRequest) {
    return this.http.post<Account>(this.baseUrl, request);
  }

  renameAccount(id: string, request: RenameAccountRequest) {
    return this.http.put<Account>(`${this.baseUrl}/${id}`, request);
  }

  deleteAccount(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
