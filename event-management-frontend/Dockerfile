FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.27-alpine
RUN apk add --no-cache openssl && \
    mkdir -p /etc/nginx/certs && \
    openssl req -x509 -nodes -days 365 \
      -subj "/C=US/ST=State/L=City/O=Event/OU=Dev/CN=localhost" \
      -addext "subjectAltName=DNS:localhost" \
      -newkey rsa:2048 \
      -keyout /etc/nginx/certs/selfsigned.key \
      -out /etc/nginx/certs/selfsigned.crt
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/event-management-frontend/browser /usr/share/nginx/html

EXPOSE 80 443
CMD ["nginx", "-g", "daemon off;"]
