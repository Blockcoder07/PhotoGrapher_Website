export default () => ({
  port: parseInt(process.env.PORT ?? '5000', 10),
  databaseUrl: process.env.DATABASE_URL,
  jwt: {
    secret: process.env.JWT_SECRET,
    expiryMinutes: parseInt(process.env.JWT_EXPIRY_MINUTES ?? '60', 10),
  },
});
