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

Server Signal Monitor als kleine .NET 8 REST API

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

Ein Server Signal Monitor für einfache Betriebsdaten.

- Server registrieren
- Heartbeat, CPU und Memory als Signals erfassen
- Alarme bei einfachen Schwellwerten anzeigen

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
| `GET` | `/api/servers` | Server anzeigen und filtern |
| `GET` | `/api/servers/{id}` | Einzelnen Server anzeigen |
| `POST` | `/api/servers` | Server registrieren |
| `POST` | `/api/servers/{id}/signals` | Signal erfassen |
| `GET` | `/api/signals` | Signals anzeigen |
| `GET` | `/api/alarms` | Alarme anzeigen |
| `PUT` | `/api/alarms/{id}/status` | Alarmstatus aktualisieren |
| `DELETE` | `/api/servers/{id}` | Server löschen |
| `GET` | `/health` | Health Check |

<!--
-->

---

# Wichtige Entscheidungen

- .NET 8, weil LTS und aktuell unterstützt
- ASP.NET Core Web API als naheliegender REST-Stack
- In-Memory für einfachen Start (über Interface `IMonitoringRepository` für spätere Austauschbarkeit)
- Dokumentation über Swagger/OpenAPI, passend zur kleinen API
- JSON Enums als Strings für lesbare Requests
- Ungültige Eingaben werden mit Validierung abgefangen: `null`, fehlende Felder, falsche Typen und ungültige Enum-Werte liefern `400 Bad Request`
- Tests fokussiert auf Verhalten statt auf jedes Implementierungsdetail

<!--
-->

---

# Invalid Input Handling

Was passiert bei kaputten oder absichtlich falschen Requests?

- Required Fields sind explizit validiert
- Strings haben Mindest-/Maximallängen und dürfen nicht nur Whitespace sein
- Enums müssen als gültige Strings kommen, Zahlen wie `1` werden abgelehnt
- Signalwerte sind auf `0` bis `100` begrenzt
- Integrationstests prüfen diese Fälle gegen die echte HTTP-Schicht

<!--
Beispiele:
- POST /api/servers mit "name": 123 oder "name": "  " => 400
- POST /api/servers/{id}/signals ohne "kind" oder mit "kind": 1 => 400
- PUT /api/alarms/{id}/status mit "status": "NotARealStatus" => 400
Wichtig: Das ist bewusst noch keine vollständige Security-Lösung. Auth, Rate Limiting, Request Size Limits, Security Headers und strukturierte ProblemDetails wären sinnvolle nächste Schritte.
-->

---

# Demo-Time!

1. API starten
2. Swagger öffnen
   - OpenAPI Definition
   - Dokumentation
   - API nutzen: `GET`, `POST`, `PUT`, `DELETE`

3. Server registrieren
4. CPU- oder Memory-Signal erfassen und Alarm zeigen
5. Alarm auf `Resolved` setzen
6. Einen kaputten Request zeigen: falscher Typ oder ungültiges Enum => `400`
7. Tests ausführen

<!--
-->

---

# Nächste Schritte

- Datenhaltung: SQLite, Migrationen, Tests gegen echte Persistenz
- API-Schnittstelle: Auth, Rollen/Rechte, Pagination, Versionierung, ProblemDetails
- Schutz vor Missbrauch: Rate Limiting, Request Size Limits, DDoS-Schutz über Proxy/Cloud Edge
- Parallelität: echte Persistenz mit Concurrency Controls, async I/O, Background Processing
- Betrieb: Logging, Metriken, Tracing, erweiterte Health Checks
- Alerting: konfigurierbare Schwellwerte, Deduplizierung, Benachrichtigungen
- Delivery: CI/CD, Docker Image Build, Security Scans

<!--
Nicht alles wäre sofort nötig. Sinnvolle Reihenfolge:
1. Datenhaltung: In-Memory durch SQLite ersetzen, Migrationen einführen und Tests ergänzen, die wirklich gegen Persistenz laufen.
2. API-Schnittstelle: Auth schützt schreibende Endpunkte; Rollen/Rechte trennen Viewer, Signal Writer, Operator und Admin; Pagination verhindert riesige Antworten; Versionierung hält spätere Änderungen kompatibel; ProblemDetails macht Fehlerantworten konsistent.
3. Schutz vor Missbrauch: Rate Limiting und Request Size Limits gehören in die App; DDoS-Schutz würde ich realistisch vor der App lösen, zum Beispiel über Reverse Proxy, API Gateway oder Cloud Edge.
4. Parallelität: Der In-Memory-Prototyp nutzt einen Lock für konsistente Demo-Daten. Für Produktion würde ich Datenbank-Concurrency, async I/O, Queues oder Background Worker für hohe Signalrate einsetzen.
5. Betrieb: Strukturierte Logs, Metriken und Tracing helfen bei Debugging und Monitoring; Health Checks sollten später auch Datenbank oder andere Dependencies prüfen.
6. Alerting: Schwellwerte gehören später in Konfiguration oder Datenbank; gleiche Alarme sollten nicht endlos doppelt erzeugt werden; Benachrichtigungen wären ein eigener Adapter.
7. Delivery: CI/CD baut, testet und veröffentlicht das Docker Image reproduzierbar; Security Scans prüfen Abhängigkeiten und Container-Basisimages.
-->
