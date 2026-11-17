# Premium Finance portal

React 18 + TypeScript (Vite) front end for brokers and staff. One app: the role in the user's JWT decides which area they see (broker screens, call centre, underwriting, operations, compliance). The API enforces every role.

## Run locally

```
npm install
npm run dev
```

Opens on http://localhost:5173 (fixed port, so the API's CORS settings can allow exactly this origin).

## Other scripts

- `npm run build`: type-check and build to `dist/`
- `npm run lint`: ESLint
