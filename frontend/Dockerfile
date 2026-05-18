FROM node:18-alpine AS build
WORKDIR /app

# Instala as dependências de pacotes
COPY package*.json ./
RUN npm install

# Copia o código-fonte e compila a aplicação via Vite
COPY . .
RUN npm run build

# Servidor leve para servir os arquivos estáticos compilados (SPA)
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html

# Copia uma configuração customizada para o Nginx não quebrar as rotas do React-Router
RUN echo 'server { listen 5173; location / { root /usr/share/nginx/html; index index.html index.htm; try_files $uri $uri/ /index.html; } }' > /etc/nginx/conf.d/default.conf

EXPOSE 5173
CMD ["nginx", "-g", "daemon off;"]