FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

COPY nginx.conf /etc/nginx/nginx.conf
COPY src/ ./

EXPOSE 80
