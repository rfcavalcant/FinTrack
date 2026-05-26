# FinTrack

Aplicação de controle financeiro pessoal construída como vitrine de **Domain-Driven Design** sobre **.NET 9** (back-end) e **Angular** (front-end, fase futura). Ver [`CLAUDE.md`](./CLAUDE.md) para a arquitetura e o roteiro completo.

- **backend/** — solução .NET (Domain, Application, Infrastructure, API, Tests).
- **frontend/** — Angular (a partir da fase 9; hoje só com Docker de referência).

---

## Infraestrutura (Docker)

A stack roda em containers: **db** (PostgreSQL) e **api** (.NET). O serviço **web** (Angular) já está preparado, mas comentado até a fase 9.

### Pré-requisitos
- Docker + Docker Compose v2
- Na primeira vez, criar o `.env` a partir do template:
  ```bash
  cp .env.example .env
  ```
  O `.env` (gitignored) guarda usuário/senha/base do Postgres. Nenhum segredo é versionado.

### Desenvolvimento (hot reload)
`docker-compose.override.yml` é carregado automaticamente: a API roda com `dotnet watch` e o código é montado por volume.
```bash
docker compose up --build
```
- API: http://localhost:5045
- Postgres: `localhost:5432`

As migrations são aplicadas automaticamente no startup da API.

### Produção (imagem runtime enxuta)
Build multi-stage publica só o runtime (sem SDK na imagem final). O banco **não** é exposto ao host.
```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
```
- API: http://localhost:8080

### Rodar só o banco (API local, fora do Docker)
Para desenvolver a API com `dotnet run`/IDE e usar só o Postgres em container:
```bash
docker compose up db
```
Depois, na pasta `backend/`:
```bash
dotnet run --project FinTrack.API
```
A API local lê a connection string de `appsettings.Development.json` (`Host=localhost`).
Em container, ela vem da variável de ambiente `ConnectionStrings__FinTrackDb` (`Host=db`) — as duas formas coexistem sem conflito.

### Comandos úteis
```bash
docker compose logs -f api      # logs da API
docker compose down             # parar (mantém o volume pgdata)
docker compose down -v          # parar e APAGAR os dados do banco
```
