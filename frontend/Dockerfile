# syntax=docker/dockerfile:1
#
# REFERÊNCIA — para a fase 9 (Angular). Ainda não está em uso.
# Pressupõe o projeto em frontend/fintrack-web (context do build).
# Quando o front existir, descomente o serviço "web" nos docker-compose.*.yml.

# ---------- dependências ----------
FROM node:22-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci

# ---------- dev (ng serve / hot reload) ----------
FROM node:22-alpine AS dev
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
EXPOSE 4200
# host 0.0.0.0 para ser acessível fora do container
CMD ["npm", "start", "--", "--host", "0.0.0.0", "--poll", "2000"]

# ---------- build de produção ----------
FROM node:22-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build -- --configuration production

# ---------- serve estático com nginx (prod) ----------
FROM nginx:1.27-alpine AS prod
COPY nginx.conf /etc/nginx/conf.d/default.conf
# Ajuste o caminho conforme a saída do build do Angular (dist/<nome-do-projeto>).
COPY --from=build /app/dist/fintrack-web/browser /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
