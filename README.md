# E-commerce-App
﻿# Plateforme de Vente en Ligne

## Présentation du projet

Cette application est une **plateforme de vente en ligne** permettant de gérer les produits, les clients et les commandes.

Elle offre une interface permettant aux utilisateurs de consulter les produits disponibles et de passer des commandes, ainsi qu'un espace de gestion permettant d'assurer le suivi des clients, des produits, du stock et des commandes.

L'objectif du projet est de proposer une solution simple et moderne pour faciliter la gestion des ventes et des opérations commerciales.

---

# Technologies utilisées

| Couche            | Technologies                                  |
| ----------------- | --------------------------------------------- |
| Back-end          | ASP.NET Core 10, C#, Entity Framework Core 10 |
| Base de données   | SQL Server                                    |
| Front-end         | Angular 20, TypeScript, SCSS                  |
| Documentation API | Swagger / OpenAPI                             |

---

# Fonctionnalités principales

## Tableau de bord

Le tableau de bord permet d'obtenir une vue générale de l'activité de la plateforme.

Il affiche notamment :

* Le nombre total de clients
* Le nombre total de produits
* Le nombre total de commandes
* La répartition des commandes selon leur statut
* Les alertes de rupture de stock

---

## Gestion des clients

La plateforme permet de :

* Consulter la liste des clients
* Rechercher un client
* Ajouter un nouveau client
* Modifier les informations d'un client
* Supprimer un client
* Consulter les informations détaillées d'un client

---

## Gestion des produits

Les fonctionnalités disponibles sont :

* Consulter la liste des produits
* Ajouter un nouveau produit
* Modifier les informations d'un produit
* Ajouter une photo pour un produit
* Supprimer un produit
* Gérer les quantités disponibles en stock
* Consulter les détails d'un produit

---

## Gestion des commandes

La plateforme permet de gérer les commandes des clients.

Les fonctionnalités comprennent :

* Consulter la liste des commandes
* Créer une nouvelle commande
* Sélectionner un client
* Ajouter plusieurs produits à une commande
* Modifier les quantités
* Calculer automatiquement les montants HT, TVA et TTC
* Consulter le détail d'une commande
* Valider une commande
* Mettre à jour automatiquement le stock après validation
* Annuler une commande

Le taux de TVA utilisé est de **19 %**.

---

# Structure du projet

```text
CommercialManagement/
│
├── backend/
│   └── CommercialManagement.API/
│       ├── Constants/
│       ├── Controllers/
│       ├── Data/
│       ├── DTOs/
│       ├── Enums/
│       ├── Interfaces/
│       ├── Models/
│       ├── Services/
│       ├── appsettings.json
│       └── Program.cs
│
└── frontend/
    └── src/app/
        ├── core/
        ├── dtos/
        ├── pages/
        ├── services/
        └── shared/
```

---

# Prérequis

## Back-end

Les éléments suivants sont nécessaires :

* .NET 10 SDK
* SQL Server Management Studio (SSMS)
* Entity Framework Core CLI

Installation de l'outil Entity Framework :

```bash
dotnet tool install --global dotnet-ef
```

## Front-end

Les éléments suivants sont nécessaires :

* Node.js 20
* npm
* Angular CLI

Installation d'Angular CLI :

```bash
npm install -g @angular/cli
```

---

# Configuration de la base de données

La chaîne de connexion se trouve dans le fichier :

```text
backend/CommercialManagement.API/appsettings.json
```

Exemple de configuration avec Windows Authentication :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=Monserveur;Database=CommercialManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```


---

# Installation et lancement du Back-end

Se placer dans le dossier du projet :

```bash
cd backend/CommercialManagement.API
```

Restaurer les dépendances :

```bash
dotnet restore
```

Appliquer les migrations :

```bash
dotnet ef database update
```

Démarrer l'API :

```bash
dotnet run
```

L'API sera accessible sur :

```text
http://localhost:5150
```

## Documentation API

Swagger est accessible à l'adresse :

```text
http://localhost:5150/swagger
```

---

# Installation et lancement du Front-end

Se placer dans le dossier du front-end :

```bash
cd frontend
```

Installer les dépendances :

```bash
npm install
```

Démarrer l'application :

```bash
npm start
```

L'application sera accessible sur :

```text
http://localhost:4200
```

> Le Back-end doit être démarré avant le Front-end afin que les appels vers l'API fonctionnent correctement.

---


# Endpoints principaux

| Méthode | Endpoint                    | Description                          |
| ------- | --------------------------- | ------------------------------------ |
| GET     | `/api/clients`              | Récupérer la liste des clients       |
| POST    | `/api/clients`              | Créer un client                      |
| PUT     | `/api/clients/{id}`         | Modifier un client                   |
| DELETE  | `/api/clients/{id}`         | Supprimer un client                  |
| GET     | `/api/products`             | Récupérer la liste des produits      |
| POST    | `/api/products`             | Créer un produit                     |
| PUT     | `/api/products/{id}`        | Modifier un produit                  |
| DELETE  | `/api/products/{id}`        | Supprimer un produit                 |
| GET     | `/api/orders`               | Récupérer la liste des commandes     |
| POST    | `/api/orders`               | Créer une commande                   |
| POST    | `/api/orders/{id}/validate` | Valider une commande                 |
| POST    | `/api/orders/{id}/cancel`   | Annuler une commande                 |
| GET     | `/api/dashboard`            | Récupérer les statistiques générales |

La documentation complète est disponible via Swagger.

---

# Base de données

La base de données est gérée **Entity Framework Core**.

Les migrations permettent de créer et mettre à jour automatiquement la structure de la base de données.

Pour appliquer les migrations :

```bash
dotnet ef database update
```

# Livrables

Ce projet contient les éléments demandés dans le cadre du test technique :

* Le code source complet du **Back-end ASP.NET Core**
* Le code source complet du **Front-end Angular**
* Les migrations de la base de données
* Le fichier **README.md** contenant les instructions d'installation et d'exécution

---

# Lancement rapide

Pour lancer le projet :

### 1. Configurer SQL Server

Vérifier la chaîne de connexion dans :

```text
backend/CommercialManagement.API/appsettings.json
```

### 2. Démarrer le Back-end

```bash
cd backend/CommercialManagement.API
dotnet ef database update
dotnet run
```

### 3. Démarrer le Front-end

```bash
cd frontend
npm install
npm start
```

### 4. Accéder à l'application

```text
http://localhost:4200
```
