# frontend/ — preparado para a fase 9

Ainda **não** há projeto Angular aqui. Esta pasta contém apenas arquivos de
**referência** de Docker, prontos para uso quando o front for criado:

- `Dockerfile` — multi-stage: `deps` → `dev` (ng serve) / `build` → `prod` (nginx).
- `nginx.conf` — serve a SPA e faz proxy de `/api/` para o serviço `api`.
- `.dockerignore`.

## Ativar (na fase 9)

1. Criar o projeto: `ng new fintrack-web` dentro de `frontend/`.
2. Mover/ajustar o `Dockerfile` para o contexto do projeto (ou ajustar o
   `context:` no compose) e conferir o caminho de saída do build em `dist/`.
3. Descomentar o serviço `web` em `docker-compose.yml`,
   `docker-compose.override.yml` e `docker-compose.prod.yml`.
