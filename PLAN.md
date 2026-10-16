# PLAN.md — CalendAgro : planning d'exécution

À lire avec `AGENTS.md`. **Règle prioritaire (AGENTS.md §0)** : une seule tâche à la fois → rapport dans le chat → le développeur teste → il valide → commit → tâche suivante. Ne jamais enchaîner sans validation.

Principe : on construit les **écrans d'abord avec des données factices** (phase B), puis on branche les **vraies données** (phases C à E), puis la **synchronisation Supabase** (phase F). L'application reste **offline first** : SQLite est la source principale, Supabase sert de compte et de sauvegarde. Chaque ViewModel dépend d'une **interface** (`ICultureService`, `IParcelleService`, `IRappelService`, `IAuthService`, `ISyncService`) ; les services factices sont remplacés plus tard sans toucher aux écrans. Aucun texte en dur : tout passe par `AppResources.resx`.

Statut : `[ ]` à faire · `[~]` en test · `[x]` validé et commité.

---

## Phase A : Fondations

- [ ] **1. Solution et structure** — Créer la solution .NET MAUI (Android), les dossiers de AGENTS.md §4, le projet `tests/CalendAgro.Tests` (xUnit). Test : l'app se lance vide sur l'émulateur ; `dotnet test` passe. Commit : `chore: initialise la solution et la structure`
- [x] **2. Charte graphique** — `Colors.xaml` (tokens §8), `Styles.xaml` (titre 24, corps 16, bouton 56, rayon 12), police Inter seule, `AppResources.resx` (fr). Test : une page de démo affiche couleurs, textes et boutons. Commit : `feat: ajoute la charte graphique`
- [x] **3. Navigation Shell** — 4 onglets (Accueil, Cultures, Calendrier, Parcelles), pages vides, icônes SVG + libellé. Test : les 4 onglets s'ouvrent. Commit : `feat: ajoute la navigation par onglets`
- [x] **4. Composants réutilisables** (`Controls/`) — bouton principal/secondaire, carte, badge d'étape (pastille + libellé Semis/Entretien/Récolte), champ de saisie avec erreur (`ValidatedField`), vues état vide / chargement (squelette) / erreur (« Réessayer »). Test : page de démo des composants. Commit : `feat: ajoute les composants réutilisables`

## Phase B : Écrans avec données factices

Pour chaque écran : ViewModel + vue + service factice via interface, états normal / chargement / vide / erreur, respect de la maquette Figma.

- [x] **5. Démarrage** (logo, accroche, accès connexion / « Continuer sans compte »). Commit : `feat: écran de démarrage`
- [x] **6. Connexion** (email, mot de passe, erreurs, lien mot de passe oublié). Commit : `feat: écran de connexion`
- [x] **7. Inscription** (nom, email, mot de passe, confirmation, case de consentement à la sauvegarde en ligne, téléphone `+237` optionnel, messages d'erreur). Commit : `feat: écran d'inscription`
- [x] **8. Mot de passe oublié** (flux code en 2 étapes, maquette Figma ; le lien email viendra en phase E tâche 30). Commit : `feat: écran mot de passe oublié`
- [x] **9. Accueil** (tâches à venir, chiffres, résumé parcelles). Commit : `feat: écran d'accueil`
- [x] **10. Catalogue de cultures** (liste virtualisée, `CollectionView`). Commit : `feat: catalogue de cultures`
- [x] **11. Recherche** (fusionnée dans le catalogue : champ, résultats, aucun résultat). Commit : `feat: recherche par culture (interface)`
- [x] **12. Fiche culture** (infos, frise 12 mois, conseils, accès ajout parcelle). Commit : `feat: fiche culture`
- [x] **13. Détail d'étape** (période, conseils de 12 mots maximum, ouvert depuis la fiche). Commit : `feat: détail d'étape`
- [x] **14. Calendrier de la saison** (mois en cours, frise fiche avec choix zone et saison). Commit : `feat: calendrier de la saison`
- [x] **15. Mes parcelles** (liste, état vide avec action). Commit : `feat: liste des parcelles`
- [ ] **16. Créer une parcelle et fiche parcelle** (formulaire fait ; reste la fiche). Commit : `feat: création et fiche parcelle`
- [ ] **17. Rappels** (liste, activer / désactiver). Commit : `feat: écran des rappels`
- [ ] **18. Profil et paramètres** (avatar, langue, mise à jour des données, déconnexion). Commit : `feat: profil et paramètres`

## Phase C : Vraies données de référence (hors ligne)

- [ ] **19. Modèles** — `Culture`, `Etape`, `Conseil`, `Zone`, `Saison` (AGENTS.md §6). Commit : `feat: modèles de données`
- [ ] **20. `cultures.json`** — 9 cultures, 4 zones, 4 saisons, conseils ≤ 12 mots, mention « à valider avec IRAD / MINADER ». Test : tests de chargement du JSON. Commit : `feat: données de référence embarquées`
- [ ] **21. Service de calendrier** — périodes par culture, zone et saison, avec tests xUnit. Commit : `feat: service de calendrier`
- [ ] **22. Service de recherche** — accents et majuscules, avec tests. Test : « mais », « Maïs », « MAIS » donnent le même résultat. Commit : `feat: service de recherche`
- [ ] **23. Brancher les écrans** — remplacer les services factices (catalogue, recherche, fiche, étape, calendrier) ; afficher « Données à jour au … ». Test : tout fonctionne en mode avion. Commit : `feat: branche les données réelles`

## Phase D : Données utilisateur (SQLite)

- [ ] **24. Couche SQLite** — `sqlite-net-pcl`, dépôt asynchrone, création de la base au premier lancement, tests. Les tables utilisateur ont dès le départ `Id` (GUID), `UserId`, `UpdatedAt` (UTC), `IsDeleted`. Commit : `feat: couche SQLite`
- [ ] **25. Parcelles réelles** — créer, modifier, supprimer, persistance. Test : créer, fermer l'app, retrouver la parcelle. Commit : `feat: parcelles persistées`
- [ ] **26. Notifications locales** — planification des rappels (`Plugin.LocalNotification`), permission Android. Test (idéalement sur vrai appareil) : la notification arrive à l'heure, sans réseau. Commit : `feat: notifications locales`
- [ ] **27. Rappels réels** — générés depuis les étapes et les parcelles, activer / désactiver. Commit : `feat: rappels de saison`

## Phase E : Compte Supabase

- [ ] **28. Projet et configuration Supabase** — le développeur crée le projet Supabase ; l'agent écrit `supabase/schema.sql` (tables `profiles`, `parcelles`, `rappels`, préférences, avec `Id`, `UserId`, `UpdatedAt`, `IsDeleted`), active **Row Level Security** avec la politique `UserId = auth.uid()`, ajoute `SupabaseConfig` (URL + clé `anon` lues depuis un fichier non versionné + fichier d'exemple, `.gitignore`). Test : sans connexion, une requête avec la clé `anon` ne renvoie aucune ligne. Commit : `feat: configuration Supabase et sécurité des tables`
- [ ] **29. Inscription et connexion réelles** — Supabase Auth par REST (`HttpClient`), session dans `SecureStorage` (reste connecté hors ligne), consentement à l'inscription, tests avec client simulé. Commit : `feat: compte Supabase`
- [ ] **30. Mot de passe oublié** — lien de réinitialisation envoyé par email par Supabase. Test : l'email arrive, le nouveau mot de passe fonctionne. Commit : `feat: récupération du mot de passe par email`
- [ ] **31. « Continuer sans compte », déconnexion, création de compte plus tard** — les données locales existantes sont rattachées au nouveau compte. Commit : `feat: mode sans compte et passage à un compte`

## Phase F : Synchronisation (dossier `Sync/`)

- [ ] **32. File d'attente locale** — toute écriture locale (parcelle, rappel, préférence) ajoute une opération en attente, avec tests (hors ligne, redémarrage). Commit : `feat: file d'attente de synchronisation`
- [ ] **33. Envoi vers Supabase (push)** — envoi en arrière-plan, nouvelles tentatives espacées, jamais bloquant pour l'écran. Commit : `feat: envoi des données vers Supabase`
- [ ] **34. Récupération (pull), conflits, suppression logique** — la dernière modification gagne (`UpdatedAt`), les lignes `IsDeleted` se propagent, tests. Commit : `feat: récupération et résolution des conflits`
- [ ] **35. Indicateur et reprise automatique** — « Synchronisé » / « En attente de réseau », reprise au retour du réseau. Commit : `feat: état de synchronisation`
- [ ] **36. Test de bout en bout** — créer une parcelle hors ligne, revenir en ligne, désinstaller, réinstaller, se reconnecter : la parcelle est retrouvée. Commit : `test: synchronisation de bout en bout`

## Phase G : Finition et livraison

- [ ] **37. Passe sur les états** — vérifier normal / chargement / vide / erreur sur tous les écrans, y compris les erreurs de connexion et de synchronisation. Commit : `fix: états des écrans`
- [ ] **38. Écoconception** — aucune photo, aucune animation décorative, dépendances minimales, taille de l'APK mesurée, synchronisation économe en données. Commit : `chore: écoconception`
- [ ] **39. Accessibilité** — zones ≥ 48, textes ≥ 14, contraste ≥ 4,5:1, icône + libellé partout. Commit : `fix: accessibilité`
- [ ] **40. Comparaison Figma, README, captures, build final** — APK installé sur un vrai téléphone. Commit : `docs: README et livraison`

---

## Rapport attendu après chaque tâche

```
Tâche : <numéro et nom>
Fait : <2 à 4 lignes>
Fichiers : <liste>
À tester : <étapes + résultat attendu>
Reste / doutes : <ou "rien">
En attente : ta validation avant le commit et la tâche suivante.
```
