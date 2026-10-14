# Dockerfile for the React Customer Portal.
# It builds the Vite app into static files and serves them from Nginx locally.
FROM node:24-alpine AS build
WORKDIR /app
COPY CustomerPortal/package*.json ./
RUN npm ci
COPY CustomerPortal/ ./
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY CustomerPortal/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
