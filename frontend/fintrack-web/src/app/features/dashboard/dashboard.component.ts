import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { AccountsService } from '../../core/services/accounts.service';
import { Account, AccountType } from '../../models/account.models';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly accountsService = inject(AccountsService);
  private readonly fb = inject(FormBuilder);

  readonly accounts = signal<Account[]>([]);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);
  readonly savingRename = signal(false);
  readonly pageError = signal<string | null>(null);
  readonly formError = signal<string | null>(null);
  readonly renameError = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly accountTypeOptions: Array<{ value: AccountType; label: string; hint: string }> = [
    { value: 'Checking', label: 'Conta corrente', hint: 'Uso diario e pagamentos frequentes.' },
    { value: 'Savings', label: 'Poupanca', hint: 'Reserva com saldo separado.' },
    { value: 'CreditCard', label: 'Cartao de credito', hint: 'Controle de limite e fatura.' },
  ];

  readonly createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    type: this.fb.nonNullable.control<AccountType>('Checking'),
    initialBalance: this.fb.nonNullable.control(0, [Validators.required]),
    currency: this.fb.nonNullable.control('BRL', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(3),
    ]),
    creditLimit: this.fb.control<number | null>(null),
  });

  readonly renameForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
  });

  readonly totalBalance = computed(() =>
    this.accounts().reduce((sum, account) => sum + account.balance, 0),
  );

  readonly creditAccounts = computed(() =>
    this.accounts().filter(account => account.type === 'CreditCard').length,
  );

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.loading.set(true);
    this.pageError.set(null);

    this.accountsService.getAccounts().subscribe({
      next: accounts => {
        this.accounts.set(accounts);
        this.loading.set(false);
      },
      error: () => {
        this.pageError.set('Nao foi possivel carregar suas contas agora.');
        this.loading.set(false);
      },
    });
  }

  createAccount(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.creating.set(true);
    this.formError.set(null);
    this.successMessage.set(null);

    const raw = this.createForm.getRawValue();
    const currency = raw.currency.trim().toUpperCase();
    const creditLimit =
      raw.type === 'CreditCard' && raw.creditLimit !== null && raw.creditLimit !== undefined
        ? raw.creditLimit
        : null;

    this.accountsService
      .openAccount({
        name: raw.name.trim(),
        type: raw.type,
        initialBalance: raw.initialBalance,
        currency,
        creditLimit,
      })
      .subscribe({
        next: account => {
          this.accounts.update(accounts => [account, ...accounts]);
          this.createForm.reset({
            name: '',
            type: 'Checking',
            initialBalance: 0,
            currency: 'BRL',
            creditLimit: null,
          });
          this.creating.set(false);
          this.successMessage.set('Conta criada com sucesso.');
        },
        error: (err: HttpErrorResponse) => {
          this.formError.set(this.extractError(err, 'Nao foi possivel criar a conta.'));
          this.creating.set(false);
        },
      });
  }

  startRename(account: Account): void {
    this.editingId.set(account.id);
    this.renameError.set(null);
    this.successMessage.set(null);
    this.renameForm.reset({ name: account.name });
  }

  cancelRename(): void {
    this.editingId.set(null);
    this.renameError.set(null);
    this.renameForm.reset({ name: '' });
  }

  saveRename(account: Account): void {
    if (this.renameForm.invalid) {
      this.renameForm.markAllAsTouched();
      return;
    }

    this.savingRename.set(true);
    this.renameError.set(null);
    this.successMessage.set(null);

    this.accountsService
      .renameAccount(account.id, { name: this.renameForm.getRawValue().name.trim() })
      .subscribe({
        next: updated => {
          this.accounts.update(accounts =>
            accounts.map(accountItem => (accountItem.id === updated.id ? updated : accountItem)),
          );
          this.savingRename.set(false);
          this.editingId.set(null);
          this.successMessage.set('Conta atualizada com sucesso.');
        },
        error: (err: HttpErrorResponse) => {
          this.renameError.set(this.extractError(err, 'Nao foi possivel renomear a conta.'));
          this.savingRename.set(false);
        },
      });
  }

  deleteAccount(account: Account): void {
    const confirmed = window.confirm(`Excluir a conta "${account.name}"?`);
    if (!confirmed) {
      return;
    }

    this.deletingId.set(account.id);
    this.pageError.set(null);
    this.successMessage.set(null);

    this.accountsService.deleteAccount(account.id).subscribe({
      next: () => {
        this.accounts.update(accounts => accounts.filter(accountItem => accountItem.id !== account.id));
        this.deletingId.set(null);
        if (this.editingId() === account.id) {
          this.cancelRename();
        }
        this.successMessage.set('Conta excluida com sucesso.');
      },
      error: (err: HttpErrorResponse) => {
        this.pageError.set(this.extractError(err, 'Nao foi possivel excluir a conta.'));
        this.deletingId.set(null);
      },
    });
  }

  typeLabel(type: AccountType): string {
    return this.accountTypeOptions.find(option => option.value === type)?.label ?? type;
  }

  currentTypeHint(): string {
    const selectedType = this.createForm.controls.type.value;
    return this.accountTypeOptions.find(option => option.value === selectedType)?.hint ?? '';
  }

  isCreditCardSelected(): boolean {
    return this.createForm.controls.type.value === 'CreditCard';
  }

  formatMoney(amount: number, currency: string): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: currency || 'BRL',
    }).format(amount);
  }

  private extractError(error: HttpErrorResponse, fallback: string): string {
    const validationErrors = error.error?.errors;

    if (validationErrors && typeof validationErrors === 'object') {
      const firstField = Object.values(validationErrors)[0];
      if (Array.isArray(firstField) && firstField.length > 0) {
        return String(firstField[0]);
      }
    }

    return error.error?.detail || fallback;
  }
}
