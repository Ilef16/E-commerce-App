# Document de Conception — Gestion des Clients

## Overview

Ce module expose une API REST CRUD pour l'entité `Client` au sein de l'application `CommercialManagement.API` (.NET 10, ASP.NET Core). Il couvre la liste paginée avec recherche, la création, la consultation du détail, la mise à jour et la suppression des clients. L'architecture suit la convention existante du projet : Controller → Service (via interface) → `ApplicationDbContext` (EF Core, SQL Server), sans couche repository séparée.

Les points clés du contrat sont :
- Réponses JSON camelCase, dates ISO 8601 UTC.
- Erreurs uniformes au format **ProblemDetails** (RFC 7807) pour tous les codes 4xx/5xx.
- Pagination `PagedResult<ClientDto>` cohérente avec le DTO existant.
- Pattern `ServiceResult<T>` pour transmettre les erreurs métier (Not Found, Conflict, Bad Request) depuis la couche service vers le contrôleur, sans lever d'exceptions pour des cas fonctionnels normaux.

---

## Architecture

L'application suit une architecture en couches :

```
HTTP Request
     │
     ▼
┌─────────────────────────────┐
│    ClientsController        │  ASP.NET Core, [ApiController]
│  (validation DTO, routing)  │
└──────────────┬──────────────┘
               │ IClientService
               ▼
┌─────────────────────────────┐
│      ClientService          │  Logique métier, unicité email,
│  (ServiceResult<T>)         │  pagination, logging
└──────────────┬──────────────┘
               │ ApplicationDbContext
               ▼
┌─────────────────────────────┐
│   ApplicationDbContext      │  EF Core + SQL Server
│   DbSet<Client>             │
│   DbSet<Commande>           │
└─────────────────────────────┘
```

**Décisions architecturales :**

- **Pas de repository séparé** : Le projet est de taille modeste. Le service accède directement au `DbContext`, ce qui évite une abstraction superflue. L'interface `IClientService` suffit pour l'isolation et la testabilité.
- **ServiceResult\<T\>** : Plutôt que de lever des exceptions pour des cas métier prévisibles (404, 409), le service retourne un `ServiceResult<T>` discriminant succès, not-found et conflit. Le contrôleur traduit ce résultat en code HTTP approprié.
- **Middleware d'exceptions global** : Les exceptions non gérées (erreurs inattendues, pannes SQL, etc.) sont capturées par `app.UseExceptionHandler` et transformées en réponse HTTP 500 ProblemDetails.
- **CORS** : Configuré en développement pour accepter toutes les origines (policy nommée `AllowAll`). En production, la liste des origines autorisées sera externalisée dans la configuration.

---

## Components and Interfaces

### `IClientService`

```csharp
public interface IClientService
{
    Task<PagedResult<ClientDto>> GetPagedAsync(int page, int pageSize, string? q, CancellationToken ct = default);
    Task<ServiceResult<ClientDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<ClientDto>> CreateAsync(CreateClientDto dto, CancellationToken ct = default);
    Task<ServiceResult<ClientDto>> UpdateAsync(int id, UpdateClientDto dto, CancellationToken ct = default);
    Task<ServiceResult<Unit>> DeleteAsync(int id, CancellationToken ct = default);
}
```

### `ServiceResult<T>`

Discriminated union léger exprimant le résultat d'une opération métier :

```csharp
public abstract class ServiceResult<T>
{
    public sealed class Success(T value) : ServiceResult<T> { public T Value { get; } = value; }
    public sealed class NotFound(string message) : ServiceResult<T> { public string Message { get; } = message; }
    public sealed class Conflict(string message) : ServiceResult<T> { public string Message { get; } = message; }
    public sealed class BadRequest(string message) : ServiceResult<T> { public string Message { get; } = message; }
}

// Type marqueur pour les opérations sans valeur de retour (ex. Delete)
public readonly struct Unit { public static readonly Unit Value = default; }
```

Le contrôleur fait un `switch` sur le type concret pour choisir le code HTTP à retourner.

### `ClientsController`

Hérite de `ControllerBase`, décoré `[ApiController]` et `[Route("api/[controller]")]`, cohérent avec `HealthController`. Injecte `IClientService` via le constructeur.

Endpoints :

| Méthode | Route | Réponse succès | Erreurs possibles |
|---------|-------|---------------|-------------------|
| `GET` | `/api/clients` | 200 `PagedResult<ClientDto>` | 400 (pageSize > 100) |
| `POST` | `/api/clients` | 201 `ClientDto` + `Location` | 400 (validation), 409 (email dupliqué) |
| `GET` | `/api/clients/{id}` | 200 `ClientDto` | 404 |
| `PUT` | `/api/clients/{id}` | 200 `ClientDto` | 400 (validation), 404, 409 |
| `DELETE` | `/api/clients/{id}` | 204 | 404, 409 (commandes liées) |

### `ClientService`

Implémente `IClientService`. Accède à `ApplicationDbContext` (injecté), et `ILogger<ClientService>` pour la journalisation structurée.

**Responsabilités :**
- Construire la requête EF Core paginée et filtrée.
- Vérifier l'unicité de l'email avant création/modification.
- Vérifier l'absence de commandes avant suppression.
- Exécuter la suppression dans une transaction explicite.
- Alimenter `CreatedAt` / `UpdatedAt` automatiquement.
- Émettre les logs `Information` (opérations réussies) et `Warning` (conflits, validations échouées).
- Capturer `DbUpdateException` pour renvoyer un `Conflict` structuré si la contrainte unique email est violée au niveau base de données (filet de sécurité).

### Middleware global d'exceptions

Configuré dans `Program.cs` via `app.UseExceptionHandler(...)` (ou `app.UseExceptionHandler("/error")` avec un endpoint dédié). Produit une réponse `ProblemDetails` HTTP 500 sans exposer la stack trace en production.

---

## Data Models

### DTOs

#### `ClientDto` (lecture)

```csharp
public class ClientDto
{
    public int Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string? Prenom { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Telephone { get; init; }
    public string? Adresse { get; init; }
    public string? Ville { get; init; }
    public string? CodePostal { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int NombreCommandes { get; init; }   // inclus uniquement dans GET /api/clients/{id}
                                                 // 0 dans la liste paginée
}
```

> `NombreCommandes` est chargé via `_context.Commandes.Count(c => c.ClientId == id)` dans `GetByIdAsync` uniquement, pour éviter le coût de jointure sur la liste.

#### `CreateClientDto` (création)

```csharp
public class CreateClientDto
{
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [MaxLength(120, ErrorMessage = "Le nom ne peut pas dépasser 120 caractères.")]
    public string Nom { get; init; } = string.Empty;

    [MaxLength(80, ErrorMessage = "Le prénom ne peut pas dépasser 80 caractères.")]
    public string? Prenom { get; init; }

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "L'email n'est pas dans un format valide.")]
    [MaxLength(180, ErrorMessage = "L'email ne peut pas dépasser 180 caractères.")]
    public string Email { get; init; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Le téléphone ne peut pas dépasser 30 caractères.")]
    public string? Telephone { get; init; }

    [MaxLength(250, ErrorMessage = "L'adresse ne peut pas dépasser 250 caractères.")]
    public string? Adresse { get; init; }

    [MaxLength(100, ErrorMessage = "La ville ne peut pas dépasser 100 caractères.")]
    public string? Ville { get; init; }

    [MaxLength(20, ErrorMessage = "Le code postal ne peut pas dépasser 20 caractères.")]
    public string? CodePostal { get; init; }
}
```

#### `UpdateClientDto` (modification)

```csharp
public class UpdateClientDto
{
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [MaxLength(120, ErrorMessage = "Le nom ne peut pas dépasser 120 caractères.")]
    public string Nom { get; init; } = string.Empty;

    [MaxLength(80)]
    public string? Prenom { get; init; }

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "L'email n'est pas dans un format valide.")]
    [MaxLength(180)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(30)]
    public string? Telephone { get; init; }

    [MaxLength(250)]
    public string? Adresse { get; init; }

    [MaxLength(100)]
    public string? Ville { get; init; }

    [MaxLength(20)]
    public string? CodePostal { get; init; }
}
```

### Mapping Client ↔ DTO

Le mapping est manuel (pas de dépendance AutoMapper) pour rester cohérent avec la taille du projet. Une classe statique interne ou des méthodes d'extension dans `ClientService` effectuent la projection :

```csharp
// Projection EF Core (liste) — pas de NombreCommandes chargé
private static ClientDto ToDto(Client c) => new()
{
    Id = c.Id, Nom = c.Nom, Prenom = c.Prenom, Email = c.Email,
    Telephone = c.Telephone, Adresse = c.Adresse, Ville = c.Ville,
    CodePostal = c.CodePostal, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt,
    NombreCommandes = 0
};
```

Pour `GetByIdAsync`, `NombreCommandes` est alimenté séparément :

```csharp
var count = await _context.Commandes.CountAsync(c => c.ClientId == id, ct);
var dto = ToDto(client) with { NombreCommandes = count };
```

### Configuration de la sérialisation JSON

Dans `Program.cs`, la sérialisation est configurée pour respecter le contrat API :

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // optionnel
    });
```

Les dates `DateTime` UTC sérialisées par `System.Text.Json` produisent naturellement le format ISO 8601 UTC (`2025-01-15T10:30:00Z`) sans configuration supplémentaire.

### Pagination

La pagination utilise le DTO existant `PagedResult<T>` :

```csharp
var query = _context.Clients.AsNoTracking();

if (!string.IsNullOrWhiteSpace(q))
{
    var term = q.Trim().ToLower();
    query = query.Where(c =>
        c.Nom.ToLower().Contains(term) ||
        (c.Prenom != null && c.Prenom.ToLower().Contains(term)) ||
        c.Email.ToLower().Contains(term));
}

var total = await query.CountAsync(ct);
var items = await query
    .OrderBy(c => c.Nom).ThenBy(c => c.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(c => ToDto(c))
    .ToListAsync(ct);

return new PagedResult<ClientDto>
{
    Items = items, TotalCount = total, Page = page, PageSize = pageSize
};
```

---

## Correctness Properties

*Une propriété est une caractéristique ou un comportement qui doit rester vrai pour toutes les exécutions valides d'un système — essentiellement, un énoncé formel de ce que le système doit faire. Les propriétés servent de pont entre les spécifications lisibles par l'humain et les garanties de correction vérifiables automatiquement.*

### Property 1: Filtre de recherche — cohérence de l'inclusion

*Pour tout* ensemble de clients en base de données et tout terme de recherche `q` non vide, chaque client retourné dans le `PagedResult` doit avoir au moins l'un de ses champs `Nom`, `Prenom` ou `Email` contenant `q` (insensible à la casse), et aucun client dont aucun de ces champs ne contient `q` ne doit apparaître dans les résultats.

**Validates: Requirements 1.6**

---

### Property 2: Pagination — couverture exhaustive et disjonction

*Pour tout* ensemble de `N` clients et toute taille de page `pageSize` valide, en parcourant toutes les pages de 1 à `ceil(N/pageSize)`, l'union des items retournés doit contenir exactement les `N` clients sans doublon ni omission, et `TotalCount` doit être égal à `N` sur chaque page.

**Validates: Requirements 1.1, 1.2, 1.7**

---

### Property 3: Round-trip de création

*Pour tout* `CreateClientDto` valide, après un appel `CreateAsync` réussi, un appel `GetByIdAsync` avec l'id retourné doit produire un `ClientDto` dont les champs `Nom`, `Prenom`, `Email`, `Telephone`, `Adresse`, `Ville` et `CodePostal` sont identiques aux valeurs du DTO de création.

**Validates: Requirements 2.1, 3.1, 3.3**

---

### Property 4: Unicité de l'email — invariant de création et de mise à jour

*Pour tout* état de base de données contenant au moins un client avec un email `E`, toute tentative de création ou de mise à jour d'un autre client (identifiant différent) avec le même email `E` doit retourner un `ServiceResult.Conflict` et ne pas modifier la base de données.

**Validates: Requirements 2.5, 4.6, 6.2**

---

### Property 5: UpdatedAt — horodatage de modification

*Pour tout* client existant, après un appel `UpdateAsync` réussi, le champ `UpdatedAt` du `ClientDto` retourné doit être supérieur ou égal à l'horodatage `UpdatedAt` précédent et inférieur ou égal à l'heure UTC courante au moment de l'appel.

**Validates: Requirements 4.2**

---

### Property 6: Protection de suppression — clients avec commandes

*Pour tout* client possédant au moins une commande associée, un appel `DeleteAsync` doit retourner un `ServiceResult.Conflict` et laisser le client ainsi que toutes ses commandes intacts en base de données.

**Validates: Requirements 5.3, 5.4**

---

### Property 7: Round-trip de sérialisation JSON

*Pour tout* `ClientDto` valide sérialisé en JSON puis désérialisé, l'objet résultant doit être équivalent à l'objet d'origine (tous les champs préservés, dates au format ISO 8601 UTC).

**Validates: Requirements 7.2, 7.3, 7.4**

---

## Error Handling

### Stratégie globale

Tous les codes HTTP 4xx et 5xx retournent une réponse structurée conforme à `ProblemDetails` (RFC 7807) :

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Ressource introuvable",
  "status": 404,
  "detail": "Le client avec l'identifiant 42 n'existe pas.",
  "traceId": "00-abc123..."
}
```

### Couches de gestion

| Source d'erreur | Mécanisme | Comportement |
|----------------|-----------|--------------|
| Validation DTO (`[Required]`, `[EmailAddress]`, etc.) | `[ApiController]` + `ValidationProblemDetails` auto | 400 avec liste des erreurs de champ |
| Corps JSON malformé | `[ApiController]` built-in | 400 ProblemDetails |
| Not Found, Conflict (métier) | `ServiceResult<T>` discriminé | 404 / 409 ProblemDetails produit dans le contrôleur |
| `pageSize > 100` | Vérification explicite dans le contrôleur | 400 ProblemDetails |
| `DbUpdateException` (contrainte unique en échappé) | `catch` dans `ClientService` | `ServiceResult.Conflict` → 409 |
| Exception non gérée | `app.UseExceptionHandler` | 500 ProblemDetails, sans détails internes |

### Configuration `Program.cs` (points clés)

```csharp
// ProblemDetails pour les erreurs de validation
builder.Services.AddProblemDetails();

// Exception handler global
app.UseExceptionHandler(exHandler =>
{
    exHandler.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = 500,
            Title = "Erreur interne du serveur",
            Detail = "Une erreur inattendue s'est produite."
        };
        await context.Response.WriteAsJsonAsync(problem);
    });
});

// CORS
builder.Services.AddCors(opts =>
    opts.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
app.UseCors("AllowAll");
```

### Codes HTTP par cas d'usage

| Scénario | Code HTTP | Corps |
|----------|-----------|-------|
| Succès liste | 200 | `PagedResult<ClientDto>` |
| Succès création | 201 | `ClientDto` + `Location` header |
| Succès détail / mise à jour | 200 | `ClientDto` |
| Succès suppression | 204 | (vide) |
| Données invalides (DTO) | 400 | `ValidationProblemDetails` |
| pageSize > 100 | 400 | `ProblemDetails` |
| Corps JSON malformé | 400 | `ProblemDetails` |
| Ressource introuvable | 404 | `ProblemDetails` |
| Email dupliqué / commandes liées | 409 | `ProblemDetails` |
| Erreur interne | 500 | `ProblemDetails` (sans stack trace) |

---

## Testing Strategy

### Double approche : tests unitaires + tests de propriétés

#### Tests unitaires (`xUnit`)

Les tests unitaires ciblent des exemples concrets et des cas limites. `ApplicationDbContext` est remplacé par une instance EF Core In-Memory (ou un mock avec `Moq` / `NSubstitute`) pour isoler la logique de service.

Exemples de cas de test unitaires :

- `GetPagedAsync` avec `pageSize = 0` ou `pageSize = 101` → résultat d'erreur approprié.
- `CreateAsync` avec `Nom` vide → `BadRequest`.
- `CreateAsync` avec email déjà existant → `Conflict`.
- `GetByIdAsync` avec id inexistant → `NotFound`.
- `DeleteAsync` d'un client avec commandes → `Conflict`, client non supprimé.
- `DeleteAsync` d'un client sans commandes → `Success`, client absent de la base.
- `UpdateAsync` → `UpdatedAt` non nul et supérieur à `CreatedAt`.
- Sérialisation d'un `ClientDto` contenant une date UTC → format ISO 8601 dans le JSON.

#### Tests de propriétés (`FsCheck` — bibliothèque PBT pour .NET)

Chaque propriété de correction fait l'objet d'un test de propriété avec un minimum de **100 itérations** par propriété. FsCheck génère automatiquement les données d'entrée.

Chaque test est taggué avec un commentaire de référence :
`// Feature: gestion-des-clients, Property {N}: {texte de la propriété}`

| Propriété | Test de propriété | Générateurs nécessaires |
|-----------|------------------|------------------------|
| P1 — Filtre de recherche | Générer des listes de clients et des termes `q` aléatoires ; vérifier que chaque item retourné matche le terme | `Gen<List<Client>>`, `Gen<string>` non vide |
| P2 — Pagination exhaustive | Générer N clients et un pageSize ; parcourir toutes les pages ; vérifier union = N sans doublons | `Gen<List<Client>>`, `Gen<int>` dans [1, 100] |
| P3 — Round-trip création | Générer des `CreateClientDto` valides aléatoires ; créer puis relire ; comparer les champs | `Gen<CreateClientDto>` |
| P4 — Unicité email | Générer un email déjà présent ; tenter création/update avec le même email ; vérifier `Conflict` | `Gen<string>` format email |
| P5 — UpdatedAt | Générer des `UpdateClientDto` aléatoires ; mettre à jour ; vérifier horodatage | `Gen<UpdateClientDto>` |
| P6 — Protection suppression | Générer un client avec N ≥ 1 commandes ; tenter suppression ; vérifier `Conflict` et intégrité | `Gen<(Client, List<Commande>)>` |
| P7 — Round-trip JSON | Générer des `ClientDto` aléatoires ; sérialiser/désérialiser ; vérifier équivalence | `Gen<ClientDto>` |

**Configuration FsCheck :**

```csharp
// Exemple de test de propriété P3
[Property(MaxTest = 100)]
// Feature: gestion-des-clients, Property 3: Round-trip de création
public Property Creation_RoundTrip(ValidCreateClientDto dto)
{
    // Arrange : insérer via service, puis relire
    var created = _service.CreateAsync(dto).GetAwaiter().GetResult();
    var fetched = _service.GetByIdAsync(((ServiceResult<ClientDto>.Success)created).Value.Id)
                          .GetAwaiter().GetResult();
    var result = (ServiceResult<ClientDto>.Success)fetched;
    // Assert
    return (result.Value.Nom == dto.Nom &&
            result.Value.Email == dto.Email &&
            result.Value.Prenom == dto.Prenom)
           .ToProperty();
}
```

#### Équilibre tests unitaires / tests de propriétés

Les tests unitaires couvrent les cas limites spécifiques (chaîne vide, null, id inexistant). Les tests de propriétés couvrent l'espace d'entrée large (emails aléatoires, noms avec caractères Unicode, pages extrêmes). Les deux approches sont complémentaires.
