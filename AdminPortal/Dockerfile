# Dockerfile for the Angular Admin Portal.
# It builds the Angular app into static files and serves them from Nginx locally.
FROM node:24-alpine AS build
WORKDIR /app
COPY AdminPortal/package*.json ./
RUN npm ci
COPY AdminPortal/ ./
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY AdminPortal/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/AdminPortal/browser /usr/share/nginx/html
EXPOSE 80
