# Build stage
FROM node:22-slim AS build
WORKDIR /app

# Copy manifest files first so `npm ci` is cached unless dependencies change
COPY package.json package-lock.json ./
RUN npm ci

# Prisma Client needs the schema present (and generated) before `nest build` type-checks against it
COPY prisma ./prisma
RUN npx prisma generate

COPY . .
RUN npm run build

# Runtime stage
FROM node:22-slim AS runtime
WORKDIR /app
ENV NODE_ENV=production

COPY package.json package-lock.json ./
RUN npm ci --omit=dev

COPY prisma ./prisma
RUN npx prisma generate

COPY --from=build /app/dist ./dist

# Render assigns the listening port via $PORT; main.ts already binds to it (default 5000 otherwise).
CMD ["node", "dist/main.js"]
