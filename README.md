# Velux – Supplier Evaluation Tool (SEP7 prototype)

.NET 10-løsning med fire projekter, bygget efter jeres domænemodel og MVVM-klassediagram.

| Projekt | Hvad det er | Lag i arkitekturen |
|---|---|---|
| `Velux.Shared` | Domæneklasserne (`Models/`) – deles af klient og server | Domain model |
| `Velux.Api` | ASP.NET Core Web API: controllers, services, EF Core, SQLite | Web API + Service + Data access |
| `Velux.Client` | Blazor WebAssembly med MVVM: `Views/`, `ViewModels/`, `ApiClients/` | Presentation |
| `Velux.Tests` | xUnit-tests af beregninger, teknisk vurdering og sammenligning | – |

## Kom i gang i Rider

1. **Installér .NET 10 SDK**, hvis det ikke allerede er installeret (`dotnet --list-sdks` i en terminal viser, hvad du har).
2. **Pak zip-filen ud** og åbn `Velux.sln` i Rider (**File → Open**).
3. Vent, til Rider har hentet NuGet-pakkerne (nederst i statuslinjen). Ellers: højreklik på solution → **Restore NuGet Packages**.
4. **Lav en run-konfiguration, der starter API og klient samtidig:**
   **Run → Edit Configurations → + → Compound**, kald den "API + Client", og tilføj
   `Velux.Api: https` og `Velux.Client: https`. Klik OK.
5. Vælg "API + Client" øverst til højre og tryk **Run** (eller **Debug** for breakpoints).
6. Browseren åbner klienten på `https://localhost:7002`. API'et kører på `https://localhost:7001`
   (prøv `https://localhost:7001/api/suppliers` i browseren for at se rå JSON).

Første gang API'et starter, opretter det databasen `velux.db` i `Velux.Api`-mappen og fylder den med testdata.
Rider kan første gang spørge, om du vil stole på udviklingscertifikatet (HTTPS) – svar ja.

**Kør testene:** højreklik på `Velux.Tests` → **Run Unit Tests**.

## Sådan hænger koden sammen med diagrammerne

```
SupplierDataPage.razor  ──binds to──▶  SupplierDataViewModel  ──▶  ISupplierApiClient / SupplierApiClient
SupplierComparisonPage  ──binds to──▶  SupplierComparisonViewModel ──▶ IComparisonApiClient / ComparisonApiClient
                                                                   │ HTTPS/JSON
                                                                   ▼
          Controllers ──▶ SupplierService / ComparisonService ──▶ AppDbContext (EF Core) ──▶ SQLite
                                     │
                                     └─▶ CO2eCalculator · CostCalculator · TechnicalAssessor
```

- **UC1 (Submit and Update Supplier Information)** = `SupplierDataPage` + `SupplierDataViewModel`.
  Metoderne svarer til systemhændelserne i jeres SSD (`LoadAlternativeAsync` = selectMaterial,
  `CreateAlternative` = createMaterial, `SaveAlternativeAsync` = submitInformation, `UploadDocumentAsync` = uploadDocument).
- **UC5 (Compare Supplier Alternatives)** = `SupplierComparisonPage` + `SupplierComparisonViewModel`.
  Beregningen sker på serveren i `ComparisonService`, så den kan unit-testes og ikke kan manipuleres i browseren.
- **Afledte attributter** (`/totalCO2e` osv.) er properties på `ComparisonEntry`, som beregnes og ikke gemmes.
- **Sporbarhed:** når en sammenligning gemmes, gemmer hver entry hvilket `EnvironmentalData` og hvilken `MaterialPrice` den brugte.
- **Constraints fra domænemodellen** håndhæves i `ComparisonService.ValidateAsync` og `CalculateAsync`.

### Bevidste forskelle fra domænemodellen (designvalg – skriv dem gerne i rapporten)

- **Baseline** er et `IsBaseline`-flag på `ComparisonEntry` i stedet for en association, fordi en fremmednøgle
  begge veje mellem `SupplierComparison` og `ComparisonEntry` giver en cirkulær afhængighed i databasen.
- **TechnicalAssessment** beregnes ved hver sammenligning og gemmes ikke, så resultatet altid afspejler de aktuelle krav.
- **Login** er ikke implementeret endnu. `User` findes i modellen og databasen, og `CreatedByUserId` er nullable.

## Leverandørdata

Alle tal i `Velux.Api/Data/SeedData.cs` er **illustrative**. Erstat emissionsfaktorerne med værdier fra rigtige,
offentlige EPD'er (fx EPD Danmark eller The International EPD System), og skriv kilden i `SourceReference`.
**Slet `velux.db`** og start API'et igen for at indlæse ændret seed-data.

Data kan også indtastes i klienten under *Supplier data*. Uploadede dokumenter gemmes i `Velux.Api/uploads/`.

## Næste skridt

1. **EF Core migrations i stedet for `EnsureCreated`**, når modellen er stabil:
   `dotnet tool install --global dotnet-ef`, slet `velux.db`, fjern `EnsureCreated()` i `Program.cs`, kør
   `dotnet ef migrations add Initial` og `dotnet ef database update` i `Velux.Api`-mappen
   (og kald `db.Database.Migrate()` i stedet).
2. **Login og roller** med ASP.NET Core Identity og `[Authorize(Roles = "...")]` på controllerne (US13–US14).
3. **Flere skærme** efter samme mønster: View → ViewModel → I…ApiClient → Controller → Service.

## Fejlfinding

| Problem | Løsning |
|---|---|
| Klienten viser "Could not reach the API" | Kør API'et samtidig (Compound-konfigurationen). Tjek at porten i `Velux.Client/wwwroot/appsettings.json` matcher `Velux.Api/Properties/launchSettings.json`. |
| CORS-fejl i browserens konsol | Klientens adresse skal stå under `ClientOrigins` i `Velux.Api/appsettings.json`. |
| Build-fejl om target framework | Du har ikke .NET 10 SDK. Installér den, eller skift `net10.0` til jeres version i alle fire `.csproj`-filer og ret pakkeversionerne `10.0.0` til samme hovedversion. |
| Gamle data efter ændring af seed | Stop API'et og slet `Velux.Api/velux.db`. |
| Hvid side i klienten | Tryk F12 i browseren og se fejlen i konsollen. Opret evt. et nyt "Blazor WebAssembly Standalone"-projekt i Rider og sammenlign dets `wwwroot/index.html` med vores. |
