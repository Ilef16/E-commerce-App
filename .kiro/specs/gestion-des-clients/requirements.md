# Requirements Document

## Introduction

Ce module permet la gestion complète des clients dans l'application de gestion commerciale. Il expose une API REST (ASP.NET Core / .NET 10) consommée par un frontend et couvre les opérations CRUD : lister, ajouter, modifier, supprimer et consulter le détail d'un client. Le modèle `Client` existe déjà partiellement dans le code ; ce document formalise les exigences fonctionnelles et non fonctionnelles qui guideront la complétion de l'implémentation.

---

## Glossary

- **API** : Interface REST exposée par le backend ASP.NET Core.
- **Client** : Entité représentant une personne physique ou morale enregistrée dans le système commercial.
- **ClientService** : Composant applicatif chargé de la logique métier liée aux clients.
- **ClientsController** : Contrôleur ASP.NET Core exposant les endpoints REST pour les clients.
- **ClientRepository** : Composant d'accès aux données pour l'entité Client (via Entity Framework Core).
- **DbContext** : `ApplicationDbContext` gérant la persistance via SQLite / Entity Framework Core.
- **DTO** : Objet de transfert de données (Data Transfer Object) distinct du modèle de domaine.
- **CreateClientDto** : DTO contenant les champs requis pour créer un nouveau client.
- **UpdateClientDto** : DTO contenant les champs modifiables pour mettre à jour un client.
- **ClientDto** : DTO de lecture retourné par l'API pour représenter un client.
- **PagedResult** : Conteneur générique retournant une page de résultats avec métadonnées de pagination.
- **Frontend** : Application cliente (navigateur) consommant l'API REST.
- **Email** : Adresse électronique unique identifiant un client ; format conforme à RFC 5322.
- **RaisonSociale** : Dénomination légale d'une personne morale, stockée dans le champ `Prenom` lorsque le client est une entreprise.

---

## Requirements

---

### Exigence 1 : Lister les clients avec pagination

**User Story :** En tant qu'utilisateur, je veux consulter la liste paginée des clients, afin de naviguer facilement dans un grand nombre de clients sans surcharger l'interface.

#### Critères d'acceptation

1. WHEN une requête GET est envoyée à `/api/clients`, THE ClientsController SHALL retourner un `PagedResult<ClientDto>` avec le code HTTP 200.
2. WHEN les paramètres `page` et `pageSize` sont fournis dans la requête, THE ClientsController SHALL retourner uniquement les clients correspondant à la page demandée.
3. WHEN le paramètre `page` est absent, THE ClientsController SHALL utiliser la valeur par défaut `page = 1`.
4. WHEN le paramètre `pageSize` est absent, THE ClientsController SHALL utiliser la valeur par défaut `pageSize = 20`.
5. WHEN le paramètre `pageSize` dépasse 100, THE ClientsController SHALL retourner le code HTTP 400 avec un message d'erreur explicite.
6. WHEN un paramètre de recherche `q` est fourni, THE ClientsController SHALL filtrer les clients dont le `Nom`, le `Prenom` ou l'`Email` contient la valeur fournie (recherche insensible à la casse).
7. THE ClientsController SHALL inclure dans `PagedResult` les champs `TotalCount`, `Page` et `PageSize`.

---

### Exigence 2 : Créer un nouveau client

**User Story :** En tant qu'utilisateur, je veux ajouter un nouveau client, afin d'enregistrer un prospect ou un partenaire commercial dans le système.

#### Critères d'acceptation

1. WHEN une requête POST est envoyée à `/api/clients` avec un `CreateClientDto` valide, THE ClientsController SHALL persister le client et retourner le `ClientDto` créé avec le code HTTP 201 et l'en-tête `Location` pointant vers `/api/clients/{id}`.
2. THE ClientService SHALL assigner automatiquement la valeur `CreatedAt` à l'horodatage UTC courant lors de la création.
3. IF le champ `Nom` est absent ou vide dans `CreateClientDto`, THEN THE ClientsController SHALL retourner le code HTTP 400 avec un message d'erreur indiquant que le nom est obligatoire.
4. IF le champ `Email` est absent ou ne respecte pas le format RFC 5322, THEN THE ClientsController SHALL retourner le code HTTP 400 avec un message d'erreur indiquant que l'email est invalide.
5. IF un client avec le même `Email` existe déjà dans la base de données, THEN THE ClientService SHALL retourner une erreur de conflit et THE ClientsController SHALL retourner le code HTTP 409 avec un message d'erreur indiquant que l'email est déjà utilisé.
6. WHEN le champ `Prenom` est fourni, THE ClientService SHALL accepter une chaîne représentant soit un prénom (personne physique) soit une raison sociale (personne morale).
7. THE ClientService SHALL ignorer tout `Id` fourni dans `CreateClientDto` et laisser la base de données générer l'identifiant.

---

### Exigence 3 : Consulter le détail d'un client

**User Story :** En tant qu'utilisateur, je veux consulter la fiche complète d'un client, afin de visualiser toutes ses informations avant de prendre une décision commerciale.

#### Critères d'acceptation

1. WHEN une requête GET est envoyée à `/api/clients/{id}`, THE ClientsController SHALL retourner le `ClientDto` correspondant avec le code HTTP 200.
2. IF l'identifiant `{id}` ne correspond à aucun client dans la base de données, THEN THE ClientsController SHALL retourner le code HTTP 404.
3. THE ClientDto retourné SHALL contenir les champs : `Id`, `Nom`, `Prenom`, `Email`, `Telephone`, `Adresse`, `Ville`, `CodePostal`, `CreatedAt`, `UpdatedAt`.
4. WHEN une requête GET est envoyée à `/api/clients/{id}`, THE ClientsController SHALL inclure le nombre de commandes associées au client (`NombreCommandes`) dans le `ClientDto`.

---

### Exigence 4 : Modifier les informations d'un client

**User Story :** En tant qu'utilisateur, je veux mettre à jour les informations d'un client existant, afin de maintenir les données commerciales à jour.

#### Critères d'acceptation

1. WHEN une requête PUT est envoyée à `/api/clients/{id}` avec un `UpdateClientDto` valide, THE ClientsController SHALL mettre à jour le client et retourner le `ClientDto` mis à jour avec le code HTTP 200.
2. THE ClientService SHALL mettre à jour automatiquement le champ `UpdatedAt` à l'horodatage UTC courant lors de chaque modification.
3. IF l'identifiant `{id}` ne correspond à aucun client, THEN THE ClientsController SHALL retourner le code HTTP 404.
4. IF le champ `Nom` est absent ou vide dans `UpdateClientDto`, THEN THE ClientsController SHALL retourner le code HTTP 400.
5. IF le champ `Email` ne respecte pas le format RFC 5322, THEN THE ClientsController SHALL retourner le code HTTP 400.
6. IF l'`Email` fourni dans `UpdateClientDto` est déjà utilisé par un autre client (identifiant différent), THEN THE ClientService SHALL retourner une erreur de conflit et THE ClientsController SHALL retourner le code HTTP 409.
7. THE ClientService SHALL ignorer toute tentative de modification du champ `Id` ou `CreatedAt` via `UpdateClientDto`.

---

### Exigence 5 : Supprimer un client

**User Story :** En tant qu'utilisateur, je veux supprimer un client, afin de retirer du système les entrées obsolètes ou erronées.

#### Critères d'acceptation

1. WHEN une requête DELETE est envoyée à `/api/clients/{id}`, THE ClientsController SHALL supprimer le client et retourner le code HTTP 204 sans contenu.
2. IF l'identifiant `{id}` ne correspond à aucun client, THEN THE ClientsController SHALL retourner le code HTTP 404.
3. IF le client possède au moins une `Commande` associée dans la base de données, THEN THE ClientService SHALL refuser la suppression et THE ClientsController SHALL retourner le code HTTP 409 avec un message indiquant que le client possède des commandes liées.
4. WHILE une suppression est en cours, THE DbContext SHALL utiliser une transaction afin de garantir l'atomicité de l'opération.

---

### Exigence 6 : Validation des données et cohérence du modèle

**User Story :** En tant que développeur, je veux que toutes les données client soient validées avant persistance, afin de garantir l'intégrité de la base de données.

#### Critères d'acceptation

1. THE ClientsController SHALL valider les DTOs entrants via les annotations de validation de ASP.NET Core (Data Annotations ou FluentValidation) avant d'appeler le ClientService.
2. THE ClientService SHALL vérifier l'unicité de l'`Email` en base de données avant toute opération d'écriture (création ou modification).
3. WHEN le champ `Telephone` est fourni, THE ClientService SHALL accepter uniquement des chaînes d'au maximum 30 caractères.
4. WHEN le champ `Email` est fourni, THE ClientService SHALL accepter uniquement des chaînes d'au maximum 180 caractères.
5. WHEN le champ `Nom` est fourni, THE ClientService SHALL accepter uniquement des chaînes d'au maximum 120 caractères non vides.
6. WHEN le champ `Prenom` est fourni, THE ClientService SHALL accepter uniquement des chaînes d'au maximum 80 caractères.
7. IF une contrainte de base de données est violée lors d'une opération d'écriture, THEN THE ClientService SHALL capturer l'exception EF Core et retourner une erreur métier structurée plutôt que de propager l'exception brute.

---

### Exigence 7 : Contrat d'API et sérialisation

**User Story :** En tant que développeur frontend, je veux un contrat d'API stable et bien documenté, afin d'intégrer facilement le module client dans l'interface utilisateur.

#### Critères d'acceptation

1. THE ClientsController SHALL exposer la documentation OpenAPI (Swagger) pour tous les endpoints du module client.
2. THE API SHALL sérialiser et désérialiser les DTOs en JSON avec des noms de propriétés en `camelCase`.
3. THE API SHALL retourner les champs de type `DateTime` au format ISO 8601 UTC (ex. `2025-01-15T10:30:00Z`).
4. FOR ALL `ClientDto` valides sérialisés en JSON puis désérialisés, THE API SHALL produire un objet équivalent à l'objet d'origine (propriété de round-trip de sérialisation).
5. WHEN l'API reçoit un corps de requête JSON malformé, THE ClientsController SHALL retourner le code HTTP 400 avec un message d'erreur descriptif.
6. THE API SHALL inclure les en-têtes CORS appropriés pour permettre les appels depuis le Frontend.

---

### Exigence 8 : Gestion des erreurs et observabilité

**User Story :** En tant que développeur, je veux que les erreurs soient gérées de manière cohérente et tracées, afin de faciliter le débogage et la surveillance en production.

#### Critères d'acceptation

1. IF une exception non gérée se produit dans le ClientService, THEN THE API SHALL retourner une réponse HTTP 500 au format `ProblemDetails` (RFC 7807) sans exposer les détails internes de l'erreur.
2. WHEN une opération sur un client aboutit (création, modification, suppression), THE ClientService SHALL émettre une entrée de log de niveau `Information` contenant l'identifiant du client et le type d'opération.
3. WHEN une erreur de validation ou de conflit se produit, THE ClientService SHALL émettre une entrée de log de niveau `Warning` contenant le contexte de l'erreur.
4. THE API SHALL retourner systématiquement des réponses d'erreur structurées au format `ProblemDetails` pour tous les codes HTTP 4xx et 5xx.
