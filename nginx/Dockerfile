FROM nginx:1.27-alpine

RUN apk add --no-cache openssl
RUN mkdir -p /etc/nginx/certs
RUN openssl req -x509 -nodes -days 3650 -newkey rsa:2048 \
  -keyout /etc/nginx/certs/localhost.key \
  -out /etc/nginx/certs/localhost.crt \
  -subj "/CN=localhost"

COPY nginx.conf /etc/nginx/nginx.conf
