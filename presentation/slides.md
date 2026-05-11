---
theme: default
title: Web-Service Task Example
info: |
  Präsentation für das Interview-Projekt.
layout: cover
background: ./bg.jpg
class: text-left text-white
drawings:
  persist: false
transition: slide-left
mdc: true
---

# Web-Service Task Example

Deployment Status Tracker als kleine .NET 8 REST API

Vorstellung, Architekturentscheidungen und Live-Demo

<!--
-->

---

# Aufgabe (minimal requirements)

- Web-Service mit C# / .NET 8
- Endpunkte zum Anzeigen, Anlegen und Löschen eines Eintrags
- Einfache Datenhaltung (In-Memory oder SQLite)
- Docker build- und startbar
- Dokumentation und kurzes README

<!--
-->

---

# Vorgehen

- Erstes Grundgerüst mit Codex erstellt und analysiert
- .NET-Projektstruktur, Syntax und Konventionen verstanden
- Neues Projekt von Grund auf aufgebaut
- Funktionalität schrittweise ergänzt
- Tests, Docker, Swagger und README später bewusst nachgezogen

<!--
-->

---

# Idee

Ein Deployment Status Tracker für Services und Environments.

- realistisch im DevOps-Kontext
- klein genug für die Aufgabe
- gut geeignet für eine kurze API-Demo

<!--
-->

---

# Architektur

```text
src/ApiProject
  Controllers
  Models
  Repositories
  appsettings.json

tests/ApiProject.Tests
```

- Controller für HTTP-Verhalten
- Models für Request/Response-Strukturen
- Repository-Interface als Speicherabstraktion
- In-Memory Implementierung mit Seed-Daten
- Integrationstests und Repository-Tests

<!--
-->

---

# API

| Methode | Pfad | Zweck |
| --- | --- | --- |
| `GET` | `/api/deployments` | Deployments anzeigen und filtern |
| `GET` | `/api/deployments/{id}` | Einzelnes Deployment anzeigen |
| `GET` | `/api/deployments/latest` | Neuestes Deployment anzeigen |
| `POST` | `/api/deployments` | Deployment anlegen |
| `PUT` | `/api/deployments/{id}/status` | Status aktualisieren |
| `DELETE` | `/api/deployments/{id}` | Deployment löschen |
| `GET` | `/health` | Health Check |

<!--
-->

---

# Wichtige Entscheidungen

- .NET 8, weil LTS und aktuell unterstützt
- ASP.NET Core Web API als naheliegender REST-Stack
- In-Memory für einfachen Start (über Interface `IDeploymentRepository` für spätere Austauschbarkeit)
- Dokumentation über Swagger/OpenAPI, passend zur kleinen API
- JSON Enums als Strings für lesbare Requests
- Tests fokussiert auf Verhalten statt auf jedes Implementierungsdetail

<!--
-->

---

# Demo-Time!

1. API starten
2. Swagger öffnen
   - OpenAPI Definition
   - Dokumentation
   - API nutzen: `GET`, `POST`, `PUT`, `DELETE`

3. Health Checks (einfacher Betriebstest)

4. Tests
   - Repository-Tests für Kernlogik
   - API-Integrationstests für Routing, Validierung und JSON

<!--
-->

---

# Nächste Schritte

- Datenhaltung: SQLite, Migrationen, Tests gegen echte Persistenz
- API-Schnittstelle: Auth, Pagination, Versionierung, ProblemDetails
- Betrieb: Logging, Metriken, Tracing, erweiterte Health Checks
- Delivery: CI/CD, Docker Image Build, Security Scans
- Sicherheit: Secrets, Konfiguration, Container-Härtung

<!--
Nicht alles wäre sofort nötig. Sinnvolle Reihenfolge:
1. Datenhaltung: In-Memory durch SQLite ersetzen, Migrationen einführen und Tests ergänzen, die wirklich gegen Persistenz laufen.
2. API-Schnittstelle: Auth schützt schreibende Endpunkte; Pagination verhindert riesige Antworten; Versionierung hält spätere Änderungen kompatibel; ProblemDetails macht Fehlerantworten konsistent.
3. Betrieb: Strukturierte Logs, Metriken und Tracing helfen bei Debugging und Monitoring; Health Checks sollten später auch Datenbank oder andere Dependencies prüfen.
4. Delivery: CI/CD baut, testet und veröffentlicht das Docker Image reproduzierbar; Security Scans prüfen Abhängigkeiten und Container-Basisimages.
5. Sicherheit: Secrets gehören in Umgebungsvariablen oder einen Secret Store; Container-Härtung bedeutet z. B. non-root User, kleinere Images und Resource Limits.
-->
