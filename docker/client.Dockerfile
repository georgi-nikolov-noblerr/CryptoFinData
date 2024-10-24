FROM node:18-alpine as build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build:dev

FROM nginx:alpine
WORKDIR /usr/share/nginx/html

COPY --from=build /app/dist/crypto-fin-data/browser /usr/share/nginx/html

COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
