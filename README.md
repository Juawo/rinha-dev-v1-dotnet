# 🎟️ Rinha de Dev - Ticket Reservation API

**Competition Link**: https://github.com/Luis-Moura/rinha-de-dev-v1

A high-performance, low-footprint Web API built with **.NET 9** for the "Rinha de Dev" challenge. The goal: handle massive concurrent ticket reservations on extremely restricted hardware (0.5 CPU / 256MB RAM) without a single overbooking.

## ⚡ Performance Highlights (The ".NET is Heavy" Myth Buster)

- **RAM Usage:** Stable at **~68MB** (under heavy load).
- **Throughput:** ~700 requests/second on 0.3 CPU.
- **Consistency:** 100% atomic operations (Zero overbooking guaranteed).

## 🛠️ Tech Stack & Strategies

- **Framework:** .NET 9 (Minimal APIs) - Chosen for its lightning-fast middleware pipeline.
- **Database:** PostgreSQL 15.
- **Data Access:** Raw **ADO.NET (Npgsql)**. No ORM overhead to save every CPU cycle.
- **Atomic Logic:** Used a **Single-Query CTE (Common Table Expression)** to handle `UPDATE` and `INSERT` in one database round-trip.
  - *Logic:* It only inserts a reservation if the ticket decrement is successful and stock is > 0.
- **Memory Optimization:** - Forced **GC Workstation Mode** to keep the managed heap small.
  - Tuned **Npgsql Connection Pool** (Max Size: 50) to balance concurrency vs RAM exhaustion.

## 🚀 How to Run

1. **Prerequisites:** Docker and Docker Compose installed.
2. **Clone and Run:**
   ```bash
   git clone https://github.com/Juawo/rinha-dev-v1-dotnet.git
   cd <repo-folder>
   docker compose up --build
Test it: Use any load testing tool (like k6) pointing to POST http://localhost:8080/reservas.

📈 Monitoring
To see the resource constraints in action:
```bash
    docker stats
```

---

    
>"Whatever you do, work at it with all your heart, as working for the Lord, not for human masters." — Colossians 3:23