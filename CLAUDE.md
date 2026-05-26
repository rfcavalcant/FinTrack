# FinTrack — Sistema de Gestão Financeira Pessoal (DDD)

> Arquivo de contexto para o Claude Code. Descreve objetivo, stack, arquitetura DDD, padrões táticos e estratégicos, e regras de negócio. **Leia este arquivo inteiro antes de qualquer tarefa.** Este projeto é uma vitrine de Domain-Driven Design aplicado a um domínio financeiro, voltado a uma vaga sênior em banco.

---

## 1. Visão Geral

FinTrack é uma aplicação full-stack de controle financeiro pessoal, construída como demonstração séria de **Domain-Driven Design** sobre **.NET**. O foco principal é a **modelagem rica do domínio** — entidades que protegem seus invariantes, value objects, agregados com fronteiras bem definidas e eventos de domínio. CRUD anêmico é proibido.

Este é um **projeto de portfólio para entrevista técnica**. Em ordem de prioridade: (1) qualidade da modelagem de domínio, (2) clareza arquitetural, (3) testes, (4) features.

**Stack:**
- **Back-end:** ASP.NET Core (.NET 9) — API REST
- **Padrões:** DDD (tático + estratégico), CQRS com MediatR, Clean Architecture
- **Front-end:** Angular (LTS) + TypeScript
- **Banco:** PostgreSQL via Entity Framework Core
- **Gráficos:** ngx-charts (preferencial) ou ng2-charts/Chart.js
- **Autenticação:** JWT (Bearer tokens)
- **Validação:** FluentValidation
- **Testes:** xUnit + FluentAssertions + NSubstitute (ou Moq)

---

## 2. Princípios DDD Inegociáveis

Estas regras valem para todo o código de domínio. Violá-las é o erro mais grave possível neste projeto.

1. **Domínio rico, nunca anêmico.** Entidades e value objects expõem comportamento (métodos com intenção de negócio), não setters públicos. Estado muda apenas através de métodos que validam invariantes.
2. **Invariantes protegidos.** Um objeto de domínio nunca pode existir em estado inválido. Validação acontece no construtor e em cada método mutador. Construtores públicos parametrizados ou factory methods estáticos (`Account.Open(...)`, `Transaction.RegisterExpense(...)`).
3. **Encapsulamento total.** Coleções expostas como `IReadOnlyCollection<T>`; mutação só por métodos (`account.AddTransaction(...)`). Setters são `private`/`protected`.
4. **Value Objects são imutáveis** e comparados por valor. Dinheiro nunca é `decimal` solto — é o VO `Money`.
5. **A fronteira do agregado é sagrada.** Referência entre agregados é por **Id**, nunca por navegação direta de objeto. Uma transação por agregado por vez.
6. **Linguagem ubíqua.** Nomes de classes, métodos e propriedades usam o vocabulário do domínio financeiro (ver glossário, seção 8). Sem termos técnicos genéricos onde existe termo de negócio.
7. **O Domain não depende de nada.** Sem EF Core, sem ASP.NET, sem MediatR no projeto Domain. Domain é C# puro.

---

## 3. Estrutura do Repositório

```
fintrack/
├── backend/
│   ├── FinTrack.sln
│   ├── FinTrack.Domain/          # Núcleo: agregados, VOs, eventos, interfaces de repo
│   │   ├── Common/               # Entity, AggregateRoot, ValueObject, IDomainEvent (base)
│   │   ├── Accounts/             # Agregado Account (raiz), VOs, eventos, IAccountRepository
│   │   ├── Transactions/         # Agregado Transaction
│   │   ├── Budgeting/            # Agregado Budget
│   │   ├── Goals/                # Agregado Goal
│   │   ├── Categories/           # Agregado Category
│   │   └── Identity/             # Agregado User
│   ├── FinTrack.Application/     # CQRS: commands, queries, handlers (MediatR), DTOs, validators
│   ├── FinTrack.Infrastructure/  # EF Core, repositórios concretos, JWT, despacho de eventos
│   ├── FinTrack.API/             # Controllers finos, DI, middlewares
│   └── FinTrack.Tests/           # Unit (domínio) + integração (handlers/endpoints)
│
├── frontend/
│   └── fintrack-web/             # Angular (core/, shared/, features/, models/)
│
├── docs/
│   ├── ubiquitous-language.md    # Glossário do domínio
│   └── context-map.md            # Bounded contexts e relações
└── CLAUDE.md
```

---

## 4. DDD Estratégico

### Bounded Contexts

Mesmo sendo um projeto de uma pessoa, modelamos contextos explícitos para demonstrar o desenho estratégico:

- **Identity** — autenticação e usuário. Genérico/de suporte.
- **Ledger (Lançamentos)** — núcleo. Contas, transações, saldo. É o *Core Domain*.
- **Budgeting (Orçamento)** — limites de gasto por categoria e período.
- **Goals (Metas)** — objetivos de economia.
- **Reporting** — leitura/agregação para dashboards e gráficos (lado *query* do CQRS).

### Context Map

- **Ledger** é o coração. **Budgeting** e **Goals** são *downstream* de Ledger: reagem a transações via **domain events** (ex: ao registrar despesa, Budgeting recalcula consumo). Relação *Customer/Supplier*.
- **Reporting** é um contexto de leitura: consome dados via queries otimizadas (read models), sem passar pelos agregados de escrita.
- **Identity** é *Generic Subdomain*; os demais contextos só conhecem o `UserId`.

Documentar isso em `docs/context-map.md` com um diagrama em texto/mermaid e a justificativa de cada relação.

### Ubiquitous Language

Manter `docs/ubiquitous-language.md` com o glossário (seção 8 é o ponto de partida). Todo termo do negócio deve aparecer com o mesmo nome no código.

---

## 5. DDD Tático — Building Blocks

### Tipos base (Domain/Common)
- `ValueObject` — base com igualdade por componentes.
- `Entity` — identidade por `Id`.
- `AggregateRoot` — `Entity` que acumula `IReadOnlyCollection<IDomainEvent>` e expõe `RaiseDomainEvent` / `ClearDomainEvents`.
- `IDomainEvent` — marcador (implementa `INotification` do MediatR só na Application; no Domain é interface própria pra não acoplar).

### Value Objects (obrigatórios)
- **`Money`** — `decimal Amount` + `Currency`. Imutável. Operações `Add`, `Subtract`, `IsNegative`. Rejeita misturar moedas. Nunca usar `double`/`float`; `decimal` apenas dentro do VO.
- **`Email`** — valida formato no construtor.
- **`DateRange`** — período para orçamentos (início/fim válidos).
- **`Percentage`** / **`Color`** — onde fizer sentido (progresso de meta, cor de categoria).

### Agregados e Raízes
- **`User`** (raiz, ctx Identity) — credenciais, email único.
- **`Account`** (raiz, ctx Ledger) — tipo (Corrente/Poupança/Cartão), `Money Balance`. Métodos: `Open`, `Credit(Money)`, `Debit(Money)`. Protege saldo; cartão de crédito trata limite. **Saldo só muda por aqui.**
- **`Transaction`** (raiz, ctx Ledger) — `Money`, data, descrição, referências por Id a `AccountId` e `CategoryId`. Factory `RegisterIncome` / `RegisterExpense`. Levanta evento `TransactionRegistered`.
- **`Budget`** (raiz, ctx Budgeting) — `CategoryId`, `DateRange`, `Money Limit`. Calcula consumo e levanta `BudgetExceeded` quando ultrapassado.
- **`Goal`** (raiz, ctx Goals) — alvo, valor atual, prazo. `Contribute(Money)`, levanta `GoalReached`.
- **`Category`** (raiz) — tipo (Receita/Despesa), nome, cor.

> **Referência entre agregados sempre por Id.** Ex: `Transaction` tem `AccountId`, não um objeto `Account`.

### Domain Events
- `TransactionRegistered`, `BudgetExceeded`, `GoalReached`.
- Levantados pelas raízes; despachados **após** persistência bem-sucedida (na Infrastructure, no `SaveChanges`). Handlers ficam na Application.

### Repositories
- Uma interface **por agregado**, definida no Domain (`IAccountRepository`, `ITransactionRepository`, ...). Sem repositório genérico.
- Métodos expressam intenção (`GetByIdAsync`, `GetActiveByUserAsync`), não um `IQueryable` cru vazando pra fora.
- Implementação concreta na Infrastructure com EF Core.

### Specifications (onde a consulta tiver regra de negócio)
- Encapsular critérios reutilizáveis (ex: "transações do período X na categoria Y") como specifications, mantendo a regra no domínio.

---

## 6. CQRS com MediatR (Application)

Separar **escrita** (commands) de **leitura** (queries).

- **Commands** (`RegisterExpenseCommand`, `OpenAccountCommand`, ...) → `IRequestHandler` que carrega o agregado pelo repositório, chama o método de domínio, persiste. Commands não retornam dados de leitura (no máximo um Id).
- **Queries** (`GetMonthlyReportQuery`, `GetExpensesByCategoryQuery`, ...) → handlers que leem **read models** otimizados, podendo ir direto ao banco/projeções, **sem** carregar agregados de escrita. É aqui que vive o contexto Reporting.
- **Validators** (FluentValidation) por command/query, plugados via pipeline behavior do MediatR.
- **Pipeline behaviors**: validação e (opcional) logging.
- Handlers de **domain events** também vivem aqui (ex: ao `TransactionRegistered`, atualizar consumo de Budget).

A camada API só monta o command/query e envia via `IMediator`. Controllers são finos.

---

## 7. Camadas e Dependências (Clean Architecture)

```
        API  ──►  Application  ──►  Domain
         │             │
         └─────────────┴──►  Infrastructure  (implementa interfaces de Application/Domain)
```

- **Domain**: C# puro. Nenhuma dependência de framework.
- **Application**: depende só de Domain. Conhece MediatR, FluentValidation, DTOs. Define interfaces de infra (ex: `IJwtTokenGenerator`, `IUnitOfWork`).
- **Infrastructure**: depende de Application+Domain. EF Core, repositórios, JWT, despacho de eventos.
- **API**: composição. DI, controllers, middlewares, Swagger.

---

## 8. Linguagem Ubíqua (glossário inicial)

- **Account (Conta)** — onde o dinheiro reside (corrente, poupança, cartão).
- **Ledger (Lançamentos)** — o conjunto de transações que afetam contas.
- **Transaction (Lançamento)** — movimento de entrada (Income/Receita) ou saída (Expense/Despesa).
- **Balance (Saldo)** — montante atual de uma conta, em `Money`.
- **Category (Categoria)** — classificação de um lançamento.
- **Budget (Orçamento)** — limite de gasto para uma categoria num período (`DateRange`).
- **Consumption (Consumo)** — quanto de um orçamento já foi gasto.
- **Goal (Meta)** — objetivo de acumular um valor até um prazo.
- **Contribution (Aporte)** — valor adicionado a uma meta.

> Expandir em `docs/ubiquitous-language.md`. Usar exatamente estes termos no código.

---

## 9. Regras de Negócio (invariantes)

- Toda `Transaction` pertence a um `User` e referencia `Account` e `Category` válidas (por Id).
- Despesa **debita** a conta; receita **credita** — sempre via métodos de `Account`, nunca setando saldo diretamente.
- Cartão de crédito consome limite; bloquear débito que ultrapasse o limite disponível.
- `Money` nunca mistura moedas; operação inválida lança exceção de domínio.
- `Budget` com consumo > limite levanta `BudgetExceeded` (sinaliza alerta no relatório).
- `Goal.Contribute` não aceita valor negativo; ao atingir o alvo, levanta `GoalReached`.
- Exceções de domínio são tipos próprios (`DomainException` e derivados), não `Exception` genérica.

---

## 10. Endpoints (REST) — base `/api/v1`, JWT exceto auth

| Método | Rota | Tipo CQRS |
|--------|------|-----------|
| POST | `/auth/register` | Command |
| POST | `/auth/login` | Query/serviço |
| POST/PUT/DELETE/GET | `/accounts` | Commands + Queries |
| POST/PUT/DELETE/GET | `/categories` | Commands + Queries |
| POST/PUT/DELETE/GET | `/transactions` (filtros período/categoria) | Commands + Queries |
| POST/PUT/DELETE/GET | `/budgets` | Commands + Queries |
| POST/PUT/DELETE/GET | `/goals` (`/goals/{id}/contributions`) | Commands + Queries |
| GET | `/reports/monthly?month=&year=` | Query (read model) |
| GET | `/reports/expenses-by-category?month=&year=` | Query (pizza/donut) |
| GET | `/reports/cashflow?year=` | Query (receita vs despesa) |
| GET | `/reports/balance-evolution?from=&to=` | Query (linha) |
| GET | `/reports/budget-status?month=&year=` | Query (gasto vs limite) |

Status HTTP corretos (200/201/204/400/401/404). Erros de validação → 400 com corpo padronizado (ProblemDetails).

---

## 11. Front-end (Angular) e Gráficos

- **Standalone components**, signals onde fizer sentido, `strict: true`.
- `core/` (services singleton, `AuthGuard`, `AuthInterceptor`), `features/` lazy-loaded, `shared/`, `models/` (interfaces espelhando DTOs).
- HTTP tipado via `HttpClient` retornando `Observable<T>`. Lógica em services, não em templates.

**Gráficos (dashboard/reports)** — biblioteca **ngx-charts**. Cada gráfico consome um endpoint dedicado de `/reports` (back-end agrega; front não faz cálculo pesado):
- **Pizza/Donut** — despesas por categoria (`/reports/expenses-by-category`), usando a cor da Category.
- **Barras** — receita vs despesa por mês (`/reports/cashflow`).
- **Linha** — evolução do saldo (`/reports/balance-evolution`).
- **Barras horizontais** — status dos orçamentos, destacando estourados (`/reports/budget-status`).
- **Progresso** — metas (valor atual vs alvo).

Padrões: componente de gráfico é "burro" (recebe dados via `@Input`/signal), HTTP num `ReportsService`, tratar loading/vazio/erro, formatar R$ e datas em pt-BR, layout responsivo, interfaces TS por payload.

---

## 12. Padrões de Código

**C#:** `nullable` habilitado; DTOs distintos de entrada/saída (entidade nunca vaza pro controller); async/await em toda I/O (sufixo `Async`); DI por construtor; exceções de domínio tipadas; PascalCase tipos/métodos, `_camelCase` campos privados.

**TypeScript:** sem `any`; interface pra todo DTO; arquivos kebab-case; tratamento de erro via interceptor.

**Geral:** Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `refactor:`); sem segredos no código (`appsettings.Development.json` gitignored + env vars).

---

## 13. Testes

- **Domínio (prioridade máxima):** testar invariantes e comportamento dos agregados/VOs. Ex: `Money` rejeita mistura de moedas; `Account.Debit` bloqueia saldo insuficiente; `Goal.Contribute` levanta `GoalReached`. xUnit + FluentAssertions.
- **Application:** testar handlers de command/query com repositórios fakes (NSubstitute), incluindo validators e reação a domain events.
- **Integração:** endpoints principais com banco em memória ou Testcontainers.
- Toda regra de negócio nova → teste correspondente. Domínio bem testado é o que mais impressiona nesta vaga.

---

## 14. Comandos Úteis

**Back-end:**
```bash
cd backend
dotnet restore && dotnet build
dotnet ef migrations add <Nome> -p FinTrack.Infrastructure -s FinTrack.API
dotnet ef database update -p FinTrack.Infrastructure -s FinTrack.API
dotnet run --project FinTrack.API
dotnet test
```
**Front-end:**
```bash
cd frontend/fintrack-web
npm install && ng serve
ng build && ng test
```

**Docker (infra):** `docker-compose.*` + `.env` na raiz; Dockerfile multi-stage em `backend/`. As migrations sobem no startup da API via `Database.Migrate()` (flag `RunMigrationsAtStartup`). Em container a connection string usa `Host=db` (env `ConnectionStrings__FinTrackDb`); fora do Docker usa `Host=localhost` (`appsettings.Development.json`).
```bash
cp .env.example .env                                              # 1ª vez
docker compose up --build                                         # dev (hot reload, watch)
docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d   # prod
docker compose up db                                              # só o banco (API local)
```
> Segredos só em `.env` (gitignored). `frontend/` tem Dockerfile de referência; o serviço `web` está comentado nos compose até a fase 9.

---

## 15. Roteiro de Implementação (uma fase por vez)

1. ✅ **Setup back-end** — solução, camadas, EF Core, PostgreSQL, migration inicial. *(concluída — revisar para alinhar à nova modelagem DDD: pastas por bounded context, tipos base em Common)*
2. **Núcleo de domínio** — tipos base (`Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`, `DomainException`); VO `Money` + testes.
3. **Identity** — agregado `User`, VO `Email`, registro/login, hashing, JWT, autenticação na API. CQRS + validators.
4. **Ledger** — agregados `Account` e `Transaction` ricos, repositórios, commands/queries de CRUD, atualização de saldo via domínio. Testes de domínio.
5. **Domain events** — infraestrutura de despacho pós-persistência; `TransactionRegistered`.
6. **Budgeting** — agregado `Budget`, reação a `TransactionRegistered`, `BudgetExceeded`.
7. **Goals** — agregado `Goal`, aportes, `GoalReached`.
8. **Reporting** — read models e queries dos relatórios/gráficos.
9. **Front-end base** — Angular, auth (interceptor/guard), telas core.
10. **Gráficos** — ngx-charts + `ReportsService` + dashboard.
11. **Testes** — fechar cobertura de domínio e handlers.
12. **Docs** — `ubiquitous-language.md`, `context-map.md`, README com instruções, prints e Swagger.

> Ao concluir cada fase: rodar `dotnet test` e `dotnet build`, commitar, então seguir.

---

## 16. Instruções para o Claude Code

- Antes de implementar, confirme entendimento e proponha um plano curto. Em tarefas de domínio, explique **por que** cada agregado/VO está modelado assim — isso ajuda no estudo para a entrevista.
- **Nunca** crie entidades anêmicas. Sem setters públicos em domínio. Mutação só por métodos que validam invariantes.
- Referência entre agregados por **Id**. Uma transação por agregado por vez.
- Respeite a regra de dependência: Domain não importa framework algum.
- Toda regra de negócio nova → teste de domínio junto.
- Não introduza dependências novas sem justificar.
- Commits pequenos no padrão Conventional Commits.
- Se algo no código existente divergir destes princípios, sinalize e proponha refatoração antes de empilhar features em cima.