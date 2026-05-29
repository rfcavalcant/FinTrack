export type AccountType = 'Checking' | 'Savings' | 'CreditCard';

export interface Account {
  id: string;
  name: string;
  type: AccountType;
  balance: number;
  currency: string;
  creditLimit: number | null;
}

export interface OpenAccountRequest {
  name: string;
  type: AccountType;
  initialBalance: number;
  currency?: string | null;
  creditLimit?: number | null;
}

export interface RenameAccountRequest {
  name: string;
}
