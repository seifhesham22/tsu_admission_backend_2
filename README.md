# TSU Admission — Applicant's Personal Account System

A backend for university admissions: applicants build a profile, upload their passport and education documents, and pick education programmes by priority — while managers take ownership of applications and move them through the admission lifecycle.

Built as four deployable **_microservices_** on .NET 8 communicating over **_RabbitMQ_** with **_DDD_** and **_CQRS_**.

---

## Architecture

Four services, each owning its own PostgreSQL database and share integration event contracts.

| Service | Port | Database | Responsibility |
|---|---|---|---|
| `Identity.Api` | 8081 | `identity_db` | Registration, two-factor login, refresh tokens, staff accounts |
| `Admission.Api` | 8082 | `admission_db` | Applicants, admissions, program catalogue, **_synchronisation with external 1C service_** |
| `Files.Api` | 8083 | `files_db` + S3 | Document upload and retrieval |
| `Notifications.Worker` | — | stateless | Sends email from the broker |



## Tech stack

1. **.NET 8, ASP.NET Core**
2. **ASP.NET Core Identity**
3. **PostgreSQL + EF Core 8**
4. **RabbitMQ + MassTransit**
5. **S3 (Yandex Object Storage)**
6. **Docker Compose**
7. **xUnit**

---

## Patterns used

**Database per service**
Each service owns its schema and no other service may read it. This is what makes them independently deployable rather than a distributed monolith.

**Idempotent consumers**
RabbitMQ delivers at least once, so consumers should tolerate seeing the
same message twice. Existence checks in the Admission consumers and an
`OccurredAtUtc` comparison on `AdmissionAccess` make redelivery harmless.

**Rich domain model (DDD)**
Business rules live on the entities that own them, behind private setters. `ApplicantAdmission` holds one definition of "closed" and one of "owned by" instead of copies scattered across services.

**CQRS-style read/write split**
Repositories return tracked domain entities for writes; queries project straight into DTOs for reads. Listing twenty admissions no longer materialises twenty full aggregates.

---


## Tests

```bash
dotnet test
```

---

## API


```
POST   /api/v1/auth/register
POST   /api/v1/auth/login                       two-factor challenge
POST   /api/v1/auth/verify                      access and refresh tokens
POST   /api/v1/auth/refresh
POST   /api/v1/auth/logout

GET    /api/v1/applicants/me
PATCH  /api/v1/applicants/me
POST   /api/v1/applicants/me/passport
POST   /api/v1/applicants/me/education-document
GET    /api/v1/applicants/me/admission/programs
POST   /api/v1/applicants/me/admission/programs
DELETE /api/v1/applicants/me/admission/programs/{id}
PATCH  /api/v1/applicants/me/admission/programs/{id}/priority

GET    /api/v1/admissions?status=&onlyMine=&pageNumber=
POST   /api/v1/admissions/{id}/manager          take ownership
DELETE /api/v1/admissions/{id}/manager          release ownership
PUT    /api/v1/admissions/{id}/manager          assign applicant, head manager only
PATCH  /api/v1/admissions/{id}/status

GET    /api/v1/catalogue/programs?facultyIds=&pageNumber=
POST   /api/v1/catalogue/sync                   admin only

GET    /api/v1/files?applicantId=
POST   /api/v1/files                            PDF only
GET    /api/v1/files/{id}
DELETE /api/v1/files/{id}
```
