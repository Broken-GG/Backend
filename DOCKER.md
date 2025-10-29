# 🐳 Docker Deployment Options

## Running with Docker

### Option 1: Backend Only
If you only want to run the backend:

```bash
cd Backend
docker compose up --build
```

Access at:
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Health: http://localhost:5000/api/health

---

### Option 2: Both Backend and Frontend Together

If you have **both repos cloned** in the same parent folder:

```
YourFolder/
├── Backend/          ← Backend repo
└── Frontend/         ← Frontend repo
```

1. **Uncomment the frontend section** in `docker-compose.yml`
2. Run from Backend folder:
   ```bash
   cd Backend
   docker compose up --build
   ```

Access at:
- Frontend: http://localhost
- API: http://localhost:5000

---

### Option 3: Run Each Separately

#### Backend:
```bash
cd Backend
docker build -t brokengg-backend .
docker run -p 5000:5000 brokengg-backend
```

#### Frontend:
```bash
cd Frontend
docker build -t brokengg-frontend .
docker run -p 80:80 brokengg-frontend
```

---

## Without Docker

### Backend:
```bash
cd Backend
dotnet run
```

### Frontend:
```bash
cd Frontend
python server.py
```

---

## For Production Deployment

Use **separate deployments** since you have separate repos:

### Backend Deployment
- Azure App Service (Container)
- AWS ECS
- Google Cloud Run
- Any container platform

### Frontend Deployment
- Azure Static Web Apps
- Netlify
- Vercel
- AWS S3 + CloudFront
- Any static hosting

Or use **Kubernetes** if you want them together in production.
