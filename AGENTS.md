# AGENTS.md — CalendAgro

Ce fichier guide les agents de code (Claude Code, Codex, Copilot, etc.) qui travaillent sur ce dépôt.
Lis-le en entier avant de modifier du code. En cas de doute, applique les règles de ce fichier plutôt que tes habitudes.

> Les passages marqués **[À adapter]** sont des hypothèses : le développeur les corrige selon son environnement.

---

## 0. Méthode de travail : une tâche à la fois (règle prioritaire)

Cette règle passe avant toutes les autres. Le développeur garde la main à chaque étape.

Pour chaque tâche, dans cet ordre :

1. **Faire une seule tâche.** Ne jamais commencer la suivante, ni préparer son code, avant la fin du cycle ci-dessous.
2. **Faire un rapport dans le chat.** Court et concret : ce qui a été fait, les fichiers modifiés, comment tester (commandes, écrans à ouvrir, résultat attendu), ce qui reste ou reste incertain.
3. **S'arrêter et attendre.** Le développeur fait le test lui-même sur l'émulateur ou l'appareil.
4. **Attendre la validation explicite** du développeur (par exemple « validé »). Sans validation, la tâche n'est pas terminée.
5. **Committer après la validation**, et seulement après. Un commit par tâche validée, avec un message clair (voir section 14).
6. **Passer à la tâche suivante uniquement après ce commit**, et en annonçant laquelle.

Conséquences :

- Si le test échoue ou si le développeur demande une correction : corriger **la même tâche**, refaire un rapport, attendre une nouvelle validation. Pas de commit entre-temps.
- Ne jamais committer ni pousser du code non validé. Ne jamais regrouper plusieurs tâches dans un commit.
- Si une tâche paraît trop grosse, la **découper** et proposer le découpage avant de coder.
- Si un doute bloque la tâche, poser la question dans le chat au lieu de deviner.

Modèle de rapport :

```
Tâche : <nom de la tâche>
Fait : <résumé en 2 à 4 lignes>
Fichiers : <liste des fichiers créés ou modifiés>
À tester : <étapes + résultat attendu>
Reste / doutes : <points ouverts, ou "rien">
En attente : ta validation avant le commit et la tâche suivante.
```

---

## 1. Le projet en bref

**CalendAgro** est une application mobile de **calendrier cultural embarqué** pour les **petits agriculteurs du Cameroun**.

- **Problème** : les petits agriculteurs n'ont pas de repère simple sur les périodes de semis, d'entretien et de récolte de leurs cultures.
- **Solution** : un calendrier cultural par culture et par saison, avec des conseils courts, **utilisable sans connexion Internet**.
- **Fonctionnalités du périmètre** :
  1. Catalogue de cultures
  2. Calendrier semis / entretien / récolte
  3. Conseils par étape
  4. Rappels de saison
  5. Fiche de parcelle
  6. Recherche par culture
- **Contexte du dépôt** : projet d'atelier de développement mobile. Les maquettes Figma sont la référence visuelle. Le seul service distant est **Supabase** (comptes et sauvegarde des données utilisateur, voir §6 bis) ; l'application reste **offline first**.

## 2. Utilisateurs et contraintes terrain

- Exploitants familiaux, smartphones Android d'entrée de gamme, réseau faible ou absent, lecture en plein soleil.
- Interface **en français** (option English plus tard). Textes courts : **12 mots maximum par phrase de conseil**.
- Monnaie et unités locales : superficies en **hectares**, dates au format `12 oct. 2026`, téléphone `+237 6XX XX XX XX`.
- Cultures d'exemple : maïs, manioc, plantain, arachide, tomate, haricot, cacao, igname, piment.
- Zones : Littoral, Centre, Ouest, Extrême-Nord. Saisons : grande saison sèche, petite saison des pluies, petite saison sèche, grande saison des pluies.

## 3. Pile technique **[À adapter]**

- **Framework** : .NET MAUI, cible **Android** en priorité (iOS facultatif). Utiliser la version de .NET du cours.
- **Langage** : C# avec `nullable` activé.
- **Architecture** : **MVVM** avec `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`).
- **Navigation** : `Shell` avec routes nommées.
- **Stockage local** : **SQLite** (`sqlite-net-pcl`) pour les parcelles, rappels et préférences ; **données de référence** (cultures, calendriers, conseils) embarquées en **JSON** dans `Resources/Raw/` et chargées au premier lancement.
- **Compte et sauvegarde en ligne** : **Supabase** (Auth par email + mot de passe, base Postgres avec Row Level Security). Accès par **REST (`HttpClient`)** de préférence, sans gros SDK (écoconception). Utilisé **uniquement** pour le compte et la synchronisation.
- **Notifications locales** : `Plugin.LocalNotification` (ou équivalent) pour les rappels, sans serveur.
- **Injection de dépendances** : conteneur MAUI (`MauiProgram.cs`).
- **Tests** : xUnit pour la logique (services, calcul des périodes). Pas de tests d'interface obligatoires.

## 4. Structure du dépôt **[À adapter]**

```
CalendAgro/
├─ AGENTS.md
├─ CalendAgro.sln
└─ src/CalendAgro/
   ├─ MauiProgram.cs
   ├─ AppShell.xaml(.cs)            # routes et navigation
   ├─ Models/                       # Culture, Etape, Conseil, Parcelle, Rappel...
   ├─ Data/                         # accès SQLite, chargement du JSON embarqué
   ├─ Sync/                         # client Supabase, file d'attente de synchronisation
   ├─ Services/                     # logique métier (calendrier, rappels, recherche)
   ├─ ViewModels/                   # un ViewModel par page
   ├─ Views/                        # pages XAML (Auth, Accueil, Cultures, Parcelles...)
   ├─ Controls/                     # composants réutilisables (bouton, carte, frise...)
   ├─ Resources/
   │  ├─ Styles/Colors.xaml         # tokens de couleur
   │  ├─ Styles/Styles.xaml         # styles globaux
   │  ├─ Raw/                       # cultures.json, etc.
   │  ├─ Fonts/                     # Inter uniquement
   │  └─ Images/                    # icônes vectorielles (SVG)
   └─ Resources/Strings/            # AppResources.resx (fr) et .en.resx
tests/CalendAgro.Tests/
```

## 5. Commandes

```bash
dotnet restore
dotnet build -f net10.0-android            # [À adapter] version du cadre cible
dotnet test
dotnet build -t:Run -f net10.0-android    # lancer sur émulateur ou appareil
```

Avant de proposer un changement : le projet doit **compiler sans avertissement nouveau** et les tests doivent passer.

## 6. Modèle de données (référence)

| Entité | Champs principaux |
|---|---|
| `Culture` | Id, Nom, Categorie (céréale, tubercule, légume, culture de rente), DureeCycleJours, IconeKey |
| `Zone` / `Saison` | Zone (Littoral, Centre, Ouest, Extrême-Nord), Saison |
| `Etape` | CultureId, Type (`Semis`, `Entretien`, `Recolte`), Zone, Saison, MoisDebut, MoisFin |
| `Conseil` | EtapeId, Texte (≤ 12 mots), Ordre |
| `Parcelle` | Id, Nom, CultureId, SuperficieHa, Localite, DateSemis |
| `Rappel` | Id, EtapeId ou ParcelleId, Date, Actif |

Règles :
- Les **données de référence** (cultures, étapes, conseils) sont **en lecture seule** dans l'application.
- Les **données utilisateur** (parcelles, rappels, préférences) sont stockées **d'abord en local (SQLite)**. Si l'utilisateur a un compte et du réseau, elles sont **aussi synchronisées vers Supabase**. Sans compte, elles ne quittent jamais l'appareil.
- Les périodes agricoles dépendent de la zone et de la saison : ne jamais coder une date en dur dans le code. Tout vient du JSON.
- Le contenu agricole d'exemple est **à valider avec une source fiable** (IRAD, MINADER ou un agronome). Ne jamais présenter ces données comme officielles.

## 6 bis. Compte et synchronisation Supabase

**Principe** : la base locale SQLite est la **source principale**. Supabase est une **sauvegarde** et permet de retrouver ses données sur un autre téléphone. L'application ne doit jamais attendre le réseau pour afficher ou enregistrer.

- **Compte** : Supabase Auth, **email + mot de passe**. Le téléphone `+237` est un champ optionnel du profil. Mot de passe oublié : lien envoyé **par email** par Supabase.
- **Sans compte** : « Continuer sans compte » reste possible ; tout reste local, aucune synchronisation. L'utilisateur pourra créer un compte plus tard : ses données locales sont alors envoyées.
- **Ce qui est synchronisé** : parcelles, rappels, préférences, profil. **Pas** les données de référence (cultures, étapes, conseils), qui restent embarquées dans l'application.
- **Colonnes de synchronisation** sur chaque table utilisateur (locale et distante) : `Id` (GUID créé sur l'appareil), `UserId`, `UpdatedAt` (UTC), `IsDeleted` (suppression logique : on marque, on n'efface pas tout de suite).
- **Conflits** : **la dernière modification gagne** (comparaison de `UpdatedAt`).
- **File d'attente** : toute écriture locale ajoute une opération à envoyer. L'envoi se fait en arrière-plan quand le réseau revient, avec reprise automatique et nouvelles tentatives espacées. Un échec réseau n'est **jamais** une erreur bloquante pour l'utilisateur ; afficher seulement un indicateur discret (« Synchronisé » / « En attente de réseau »).
- **Sécurité côté Supabase** : **Row Level Security activée sur toutes les tables**, avec une politique « chaque utilisateur ne lit et n'écrit que ses lignes » (`UserId = auth.uid()`).
- **Clés** : seule la clé publique `anon` et l'URL du projet peuvent être dans l'application. **Jamais** la clé `service_role`. URL et clé sont lues depuis un fichier de configuration non versionné (`.gitignore`), avec un fichier d'exemple dans le dépôt.
- **Consentement** : à l'inscription, informer clairement que les données (parcelles, rappels) sont sauvegardées en ligne, et recueillir l'accord de l'utilisateur.

## 7. Règles de code

- C# propre : noms en **PascalCase** (types, méthodes) et `_camelCase` (champs privés). Noms de code en **anglais**, textes affichés en **français**.
- **Aucune logique dans le code-behind (`.xaml.cs`)** hors initialisation : tout passe par les ViewModels et les services.
- Pas de `async void` (sauf gestionnaires d'événements). Toute I/O (SQLite, fichiers) est **asynchrone**.
- Un ViewModel par page, injecté par DI. Pas d'accès direct à la base depuis une vue.
- Pas de nombres magiques : constantes nommées ou ressources.
- Pas de code mort, pas de `TODO` sans description.
- Commentaires courts et utiles, uniquement pour expliquer le « pourquoi ».

## 7 bis. Textes et localisation

- **Aucun texte en dur** dans le XAML ou le C# : tout passe par `AppResources.resx` (français par défaut).
- Phrases courtes, vocabulaire simple, pas de jargon agronomique sans explication.
- Messages d'erreur utiles, par exemple : « Numéro invalide », « Les mots de passe ne correspondent pas », « Champ obligatoire », « Synchronisation impossible, réessayez plus tard ».

## 8. Charte graphique à respecter (issue des maquettes)

Utiliser des **ressources de style**, jamais de couleurs ou de tailles écrites dans les pages.

**Couleurs (thème clair à fort contraste, lisible en plein soleil)** :

| Token | Valeur | Usage |
|---|---|---|
| `Bg` | `#FAF8F1` | fond |
| `Surface` | `#FFFFFF` | cartes |
| `Text` | `#1B2A1F` | texte principal |
| `TextMuted` | `#5B6B60` | texte secondaire |
| `Primary` | `#2F6B3A` | boutons, actions |
| `Accent` | `#D98E04` | accent, récolte |
| `Error` | `#C0392B` | erreurs |
| `Success` | `#2E7D32` | succès |
| `Semis` | `#2F6B3A` | étape semis (icône graine) |
| `Entretien` | `#1E6FB5` | étape entretien (icône goutte) |
| `Recolte` | `#D98E04` | étape récolte (icône panier) |

**Typographie** : police unique **Inter**. Titre 24 gras, corps 16, **minimum 14**. Jamais plus petit.

**Mise en page** : grille de 8, coins arrondis de 12, boutons de **56 de haut**, **zones tactiles d'au moins 48 × 48**.

**Accessibilité** :
- Contraste minimum **4,5:1**.
- Ne **jamais** utiliser la couleur seule pour porter une information : toujours **icône + libellé** (Semis / Entretien / Récolte).
- Tailles de police dynamiques supportées, pas de texte coupé.

**Navigation du bas** : 4 onglets — Accueil, Cultures, Calendrier, Parcelles. Profil et paramètres via l'avatar ; rappels via la cloche.

## 9. Écrans (alignés sur les maquettes)

Démarrage → Connexion / Inscription / Mot de passe oublié → Accueil → Catalogue de cultures → Recherche → Fiche culture → Détail d'étape → Calendrier de la saison → Mes parcelles → Fiche parcelle → Créer une parcelle → Rappels → Profil et paramètres.

Chaque écran gère ses **états** : normal, chargement (squelette), **vide** (message + action), **erreur** (message + « Réessayer »). Ne jamais laisser un écran blanc.

Authentification : **Supabase Auth** (email + mot de passe). L'application ne stocke jamais le mot de passe ; la session (jeton) est conservée dans `SecureStorage`, ce qui permet de rester connecté **hors ligne**. Un bouton « Continuer sans compte » doit rester possible.

## 10. Hors ligne d'abord (règle prioritaire)

- **L'application doit être entièrement utilisable sans réseau.** Aucun écran ne doit attendre une requête HTTP pour s'afficher ou enregistrer.
- La **seule** exception réseau autorisée est la communication avec **Supabase** (compte, synchronisation), faite en arrière-plan, uniquement pour un utilisateur connecté, via le dossier `Sync/`.
- Ne **pas** ajouter d'autre appel réseau, de SDK d'analytics, de publicité ou de service tiers sans demande explicite.
- Afficher la date de dernière mise à jour des données : « Données à jour au 03 oct. ».
- Si une mise à jour des données est ajoutée plus tard, elle sera **optionnelle**, légère (≈ 2 Mo) et reprenable.

## 11. Écoconception (obligatoire)

- **Aucune photo** dans l'interface : uniquement des **icônes vectorielles** (SVG) et des formes.
- Pas d'animation décorative, pas de dégradés, pas d'ombres lourdes, pas de flou.
- Limiter la taille de l'application et des ressources : pas de dépendance NuGet sans nécessité réelle.
- Charger les listes **à la demande** (`CollectionView` avec virtualisation), jamais tout en mémoire.
- Éviter les traitements en arrière-plan permanents ; les rappels utilisent des **notifications locales planifiées**.
- Garder un thème clair par défaut ; un mode sombre est une amélioration optionnelle.
- Justifier toute exception dans la description de la modification.

## 12. Sécurité et données personnelles

- Pas de secrets, clés ou identifiants dans le dépôt.
- Les données de l'utilisateur restent sur l'appareil, sauf la synchronisation vers **son propre espace Supabase** s'il a un compte (voir §6 bis). Aucune autre collecte, aucun autre envoi.
- Jamais de clé `service_role` dans l'application ni dans le dépôt. Row Level Security obligatoire sur toutes les tables Supabase.
- Ne jamais journaliser le mot de passe, le jeton de session ni les données personnelles.
- Valider toutes les saisies (téléphone, superficie, dates) côté application et afficher des messages clairs.

## 13. Tests

- Tester en priorité : calcul des périodes par culture, zone et saison ; génération des rappels ; recherche par culture (accents, majuscules) ; validation des formulaires.
- Tester aussi la **synchronisation** : file d'attente hors ligne, reprise au retour du réseau, résolution de conflit (dernière modification gagne), suppression logique. Le client Supabase est derrière une interface pour être simulé dans les tests.
- Les services doivent être testables sans interface (injection d'interfaces).
- Un bogue corrigé = un test qui le reproduit, si c'est raisonnable.

## 14. Travail avec Git

- Branches courtes : `feat/…`, `fix/…`, `docs/…`.
- Commits petits, au présent, en français ou en anglais de façon **cohérente** : `feat: ajoute la fiche culture`.
- Ne jamais réécrire l'historique partagé.
- Une modification = un sujet. Pas de reformatage massif mélangé à une fonctionnalité.
- **Un commit par tâche, uniquement après la validation du développeur** (voir section 0). Le message de commit décrit la tâche validée.

## 15. Ce que l'agent doit faire

- Lire les maquettes et ce fichier avant de coder une page.
- Réutiliser les composants du dossier `Controls/` et les styles avant d'en créer de nouveaux.
- Proposer un plan court pour toute tâche qui touche plus de 3 fichiers.
- Appliquer la section 0 : une tâche à la fois, rapport dans le chat, attendre le test et la validation, committer, puis seulement passer à la suite.
- Terminer chaque tâche par un court résumé (modèle de la section 0) : ce qui a changé, comment le tester, ce qui reste.

## 16. Ce que l'agent ne doit PAS faire

- Ajouter des fonctionnalités hors périmètre (paiement, réseau social, cartes, météo en ligne...).
- Ajouter une dépendance lourde, une photo ou une police supplémentaire.
- Coder des couleurs, des tailles ou des textes en dur dans les pages.
- Mettre de la logique métier dans le code-behind.
- Inventer des données agricoles « officielles » ou des mentions de certification.
- Supprimer ou renommer des fichiers sans l'indiquer.
- Modifier la charte graphique sans accord explicite.
- Faire dépendre un écran du réseau, ou appeler Supabase en dehors du dossier `Sync/`.
- Utiliser ou écrire la clé `service_role`, ou désactiver Row Level Security.
- Enchaîner plusieurs tâches sans rapport, sans validation et sans commit entre chacune.
- Committer ou pousser du code que le développeur n'a pas validé.

## 17. Définition de « terminé »

Une tâche est terminée quand :
1. le projet compile sans nouvel avertissement ;
2. les tests passent ;
3. l'écran fonctionne **hors ligne** (et, s'il touche aux données utilisateur, la modification est mise en file pour la synchronisation) ;
4. les états normal, chargement, vide et erreur sont gérés ;
5. les zones tactiles font au moins 48 px et les textes au moins 14 ;
6. aucun texte n'est écrit en dur ;
7. l'écran correspond à la maquette Figma ;
8. le rapport a été posté dans le chat, le développeur a testé et **validé**, et le commit a été fait.
