# syntax=docker/dockerfile:1.4

# Stage 1: Production-only deps (runs in parallel with the build stage)
FROM node:22-alpine AS deps-prod
WORKDIR /app
COPY package*.json ./
RUN --mount=type=cache,target=/root/.npm npm ci --omit=dev

# Stage 2: Build the app (full deps including devDependencies)
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN --mount=type=cache,target=/root/.npm npm ci
COPY . .
RUN npm run build

# Stage 3: Run the app
FROM node:22-alpine
WORKDIR /app
ENV NODE_ENV=production

# Bring in pre-built prod node_modules + compiled output
COPY --from=deps-prod /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist
COPY --from=build /app/package*.json ./

# Drop privileges to the built-in node user
USER node

# Expose port
EXPOSE 8081

HEALTHCHECK --interval=10s --timeout=3s --start-period=15s --retries=5 \
  CMD wget -qO- http://localhost:8081/health || exit 1

# Start the app
CMD ["node", "dist/main"]
