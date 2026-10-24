# Cahier des charges — CalendAgro

Oct 15, 2026 · @ado

## Présentation générale

CalendAgro est une application mobile qui indique aux petits agriculteurs camerounais quand semer, entretenir et récolter chaque culture, sans connexion Internet.

| Rubrique | Détail |
| --- | --- |
| Nom du projet | CalendAgro |
| Domaine | Agriculture |
| Type | Application mobile Android (.NET MAUI), utilisable hors ligne |
| Public visé | Petits agriculteurs du Cameroun |
| Cadre | Atelier de développement mobile : maquettes d'abord, développement ensuite |
| Version du document | 1.0, 15 oct. 2026 |

## Contexte et problématique

Les petits agriculteurs n'ont pas de repère simple sur les périodes de semis, d'entretien et de récolte de leurs cultures.

Ces périodes changent selon la culture, la zone (Littoral, Centre, Ouest, Extrême-Nord) et la saison. L'information est souvent dispersée ou transmise à l'oral, et le réseau mobile est faible ou absent dans de nombreuses zones rurales, ce qui rend les solutions en ligne peu fiables.

Le besoin est donc un outil simple, local et rapide à consulter, qui fonctionne sans réseau. Ce contexte est une hypothèse de travail : elle sera confirmée par des échanges avec des agriculteurs.

## Objectifs

L'objectif général est de fournir un calendrier cultural embarqué, par culture et par saison, avec des conseils courts, utilisable sans connexion.

Objectifs spécifiques :

- Trouver la période de semis, d'entretien ou de récolte d'une culture en quelques touches.
- Donner des conseils courts et compréhensibles à chaque étape.
- Aider à suivre ses parcelles et à ne manquer aucune étape grâce aux rappels.
- Offrir une interface lisible en plein soleil sur un smartphone d'entrée de gamme.

Cibles proposées pour la première version (à valider avec l'enseignant) :

| Indicateur | Cible |
| --- | --- |
| Fonctions principales disponibles sans réseau | 100 % |
| Touches pour atteindre la période d'une culture depuis l'accueil | 3 au maximum |
| Longueur d'un conseil | 12 mots au maximum |
| Cultures au lancement (jeu de départ modifiable) | 9 : maïs, manioc, plantain, arachide, tomate, haricot, cacao, igname, piment |
| Zones couvertes | 4 : Littoral, Centre, Ouest, Extrême-Nord |
| Requêtes réseau nécessaires au fonctionnement | 0 |

## Utilisateurs cibles et contexte d'usage

L'utilisateur principal est l'exploitant familial, qui consulte l'application sur le terrain, souvent sans réseau.

| Profil | Besoin | Contraintes |
| --- | --- | --- |
| Exploitant familial (principal) | Savoir quand semer, entretenir et récolter | Smartphone Android d'entrée de gamme, plein soleil, lecture parfois limitée |
| Jeune agriculteur ou étudiant (secondaire) | Apprendre le calendrier d'une nouvelle culture | À l'aise avec le smartphone, suit plusieurs parcelles |
| Encadreur agricole (secondaire) | Consulter vite un calendrier pour conseiller | Se déplace, consulte hors ligne |

Contexte d'usage à prendre en compte dans la conception :

- Réseau faible ou absent : aucune fonction ne doit dépendre d'une connexion.
- Lecture en plein soleil : contraste élevé, textes de 14 px minimum.
- Utilisation rapide, parfois à une main : boutons de 56 px de haut, zones tactiles de 48 px minimum.
- Textes courts et icônes accompagnées d'un libellé. Interface en français, avec l'anglais prévu ensuite.

## Périmètre fonctionnel

La première version couvre six fonctionnalités, toutes utilisables hors ligne.

| N° | Fonctionnalité | Description |
| --- | --- | --- |
| 1 | Catalogue de cultures | Liste des cultures avec filtres par catégorie |
| 2 | Calendrier cultural | Périodes de semis, d'entretien et de récolte par culture, zone et saison |
| 3 | Conseils par étape | 3 à 4 conseils courts pour chaque étape |
| 4 | Rappels de saison | Notifications locales avant chaque étape |
| 5 | Fiche de parcelle | Suivi d'une parcelle : culture, superficie, date de semis, progression |
| 6 | Recherche par culture | Recherche par nom, avec suggestions |

Hors périmètre de cette version :

- Paiement et vente en ligne.
- Réseau social et messagerie.
- Météo en ligne et cartes.
- Serveur et comptes en ligne.
- Version iOS (facultative selon le cours).

## Exigences fonctionnelles

Quinze exigences décrivent ce que l'application doit faire ; dix sont de priorité haute.

| Réf. | Exigence | Priorité |
| --- | --- | --- |
| EF-01 | Afficher le catalogue des cultures (nom, icône, durée du cycle) avec des filtres par catégorie | Haute |
| EF-02 | Rechercher une culture par nom, sans tenir compte des accents ni des majuscules, avec suggestions | Haute |
| EF-03 | Afficher la fiche d'une culture : choix de la zone et de la saison, frise de 12 mois par étape | Haute |
| EF-04 | Afficher le détail d'une étape : période et 3 à 4 conseils de 12 mots au maximum | Haute |
| EF-05 | Afficher le calendrier de la saison : tâches par semaine, filtre par type d'étape | Haute |
| EF-06 | Créer, modifier et supprimer une parcelle (nom, culture, superficie en hectares, localité, date de semis) | Haute |
| EF-07 | Calculer les prochaines étapes et la progression d'une parcelle à partir de sa date de semis | Moyenne |
| EF-08 | Activer un rappel pour une étape et recevoir une notification locale, sans réseau | Haute |
| EF-09 | S'inscrire et se connecter en local ; mot de passe stocké haché ; récupération par question secrète locale | Moyenne |
| EF-10 | Permettre de « Continuer sans compte » | Moyenne |
| EF-11 | Régler la langue (français ou anglais), la région par défaut et les notifications | Moyenne |
| EF-12 | Gérer sur chaque écran les états de chargement, vide et erreur, avec un message en français | Haute |
| EF-13 | Valider les formulaires en direct, avec un message clair sous chaque champ en erreur | Haute |
| EF-14 | Mettre à jour les données de référence par un téléchargement optionnel, sans bloquer l'usage hors ligne | Basse |
| EF-15 | Créer, modifier et supprimer une culture, ses étapes (type, zone, saison, mois) et ses conseils (12 mots au maximum) | Haute |

## Exigences non fonctionnelles

L'application doit d'abord fonctionner sans réseau, puis rester lisible et rapide sur un smartphone d'entrée de gamme.

| Domaine | Exigence |
| --- | --- |
| Hors ligne | Toutes les fonctions du périmètre marchent sans réseau ; aucune requête HTTP n'est nécessaire |
| Performance | Démarrage et ouverture des écrans rapides sur appareil d'entrée de gamme ; listes chargées à la demande (cible chiffrée à fixer lors des essais) |
| Ergonomie | Boutons de 56 px de haut, zones tactiles de 48 px minimum, textes de 14 px minimum, phrases courtes |
| Accessibilité | Contraste d'au moins 4,5:1 ; jamais la couleur seule : chaque étape porte une icône et un libellé |
| Fiabilité | Les parcelles et rappels sont conservés après fermeture de l'application ; aucun écran blanc : états de chargement, vide et erreur prévus |
| Sécurité et confidentialité | Données de l'utilisateur stockées sur l'appareil uniquement ; mot de passe haché ; aucun secret dans le dépôt ; aucune collecte de données |
| Compatibilité | Android en priorité ; version minimale à fixer avec l'enseignant |
| Maintenabilité | Architecture MVVM, logique dans des services testables, textes dans des fichiers de ressources |

## Écoconception

Le choix le plus efficace est de tout garder sur l'appareil : sans réseau, l'application consomme peu de données et peu de batterie.

| Principe | Mesure appliquée |
| --- | --- |
| Limiter les échanges | Données de référence embarquées ; aucune requête réseau nécessaire |
| Alléger l'interface | Aucune photo ; icônes vectorielles (SVG) ; pas de dégradé, de flou ni d'ombre lourde |
| Limiter les animations | Aucune animation décorative |
| Économiser l'énergie | Notifications locales planifiées ; pas de traitement permanent en arrière-plan |
| Ménager la mémoire | Listes virtualisées, chargées à la demande |
| Réduire le poids | Peu de dépendances ; une seule police (Inter) ; données JSON compactes |
| Servir le terrain | Thème clair à fort contraste, lisible en plein soleil ; mode sombre en option |

Indicateurs à suivre pendant le développement : taille du paquet Android, nombre de requêtes réseau (objectif 0), nombre de dépendances et mémoire utilisée sur les écrans de liste.

## Données et contenu

Toutes les données sont stockées localement et modifiables. L'application livre un jeu de départ de 9 cultures d'exemple, importé au premier lancement ; l'utilisateur peut le corriger, le compléter et ajouter ses propres cultures, étapes et conseils.

| Entité | Champs principaux | Stockage |
| --- | --- | --- |
| Culture | Nom, catégorie, durée du cycle, icône | SQLite (local), jeu de départ importé depuis un JSON embarqué, modifiable |
| Étape | Culture, type (semis, entretien, récolte), zone, saison, mois de début et de fin | SQLite (local), modifiable |
| Conseil | Culture et phase, texte de 12 mots au maximum, ordre | SQLite (local), modifiable |
| Parcelle | Nom, culture, superficie en hectares, localité, date de semis | SQLite (local) |
| Rappel | Étape ou parcelle, date, actif ou non | SQLite (local) |
| Préférences | Langue, région par défaut, notifications | SQLite (local) |

Contenu agricole : les périodes et conseils du jeu de départ sont des exemples plausibles, non officiels. Avant toute diffusion, ils doivent être validés auprès d'une source fiable (IRAD, MINADER ou un agronome) et la source doit être citée dans l'application. Les données saisies par l'utilisateur sont sous sa responsabilité ; l'application ne les vérifie pas et le rappelle sur l'écran de saisie.

Le fichier de données porte une date de mise à jour, affichée à l'utilisateur sous la forme « Données à jour au 03 oct. ».

## Écrans et navigation

&#91;embedded content: arborescence de navigation · 15 écrans, 4 onglets\]

Quinze écrans s'organisent autour de la barre du bas : après la connexion ou « Continuer sans compte », ses quatre onglets ouvrent l'accueil, le catalogue de cultures, le calendrier et les parcelles. Les rappels s'ouvrent par la cloche et le profil par l'avatar, depuis l'accueil.

## Contraintes et choix techniques

La pile retenue est .NET MAUI avec une architecture MVVM, car elle est celle du cours et permet de partager le code et de tester la logique à part.

| Sujet | Choix | Raison |
| --- | --- | --- |
| Framework | .NET MAUI, Android en priorité | Imposé par le cours |
| Architecture | MVVM (CommunityToolkit.Mvvm), navigation Shell, injection de dépendances | Logique séparée de l'interface, testable |
| Données utilisateur | SQLite local | Fonctionne sans réseau |
| Cultures, étapes, conseils | SQLite local ; jeu de départ importé depuis un JSON embarqué au premier lancement | Modifiable par l'utilisateur, aucune connexion requise |
| Rappels | Notifications locales planifiées | Pas de serveur |
| Textes | Fichiers de ressources .resx (français, puis anglais) | Aucun texte en dur |
| Tests | xUnit sur les services (périodes, rappels, recherche, validation) | Fiabilité de la logique métier |

Les versions exactes de .NET et des paquets sont à confirmer avec l'enseignant. Les règles de code et de conception sont détaillées dans le fichier AGENTS.md du dépôt.

Contraintes de projet : travail individuel, aucun serveur dans cette phase, charte graphique issue des maquettes Figma.

## Livrables et planning

Le projet produit d'abord un dossier de conception, puis une application fonctionnelle.

| Livrable | Format | Phase |
| --- | --- | --- |
| Maquettes (8 écrans minimum, dont connexion et inscription) et lien de partage en consultation | Fichier Figma | Conception |
| Export de tous les écrans, un par page | PDF | Conception |
| Schéma de l'arborescence de navigation | PDF ou image | Conception |
| Document de présentation et de justification (une page maximum) | PDF | Conception |
| Cahier des charges | Ce document | Conception |
| Consignes pour les agents de code | AGENTS.md | Développement |
| Application et code source | Projet .NET MAUI | Développement |

Étapes prévues, dans l'ordre (dates à fixer avec le calendrier des séances) :

1. Terminer les maquettes : composants, états vide, chargement et erreur.
2. Produire l'arborescence et le document d'une page.
3. Créer le projet MAUI : thème, styles et composants communs.
4. Mettre en place les données : JSON embarqué, SQLite, services.
5. Développer l'authentification, le catalogue, la fiche culture et le calendrier.
6. Développer les parcelles et les rappels.
7. Gérer les états vide, chargement et erreur ; écrire les tests.
8. Recette finale et livraison.

## Critères de recette

L'application est acceptée si chaque écran fonctionne hors ligne et respecte la charte des maquettes.

- [ ] Le projet compile sans nouvel avertissement et les tests passent.
- [ ] Toutes les fonctions du périmètre marchent avec le mode avion activé.
- [ ] La période d'une culture s'atteint en 3 touches au maximum depuis l'accueil.
- [ ] Chaque écran gère les états normal, chargement, vide et erreur.
- [ ] Les formulaires affichent un message clair sous chaque champ en erreur.
- [ ] Les zones tactiles font 48 px minimum et les textes 14 px minimum.
- [ ] Aucun texte n'est écrit en dur ; tout vient des fichiers de ressources.
- [ ] Un rappel planifié se déclenche sans réseau.
- [ ] Les parcelles et rappels sont conservés après fermeture de l'application.
- [ ] Aucune photo ni requête réseau dans l'application.
- [ ] Les écrans correspondent aux maquettes Figma.

## Risques et évolutions

Le principal risque est un contenu agricole inexact : il doit être validé avant toute utilisation réelle.

| Risque | Conséquence | Parade |
| --- | --- | --- |
| Contenu agricole non validé | Conseils inexacts pour l'agriculteur | Valider avec IRAD, MINADER ou un agronome ; citer la source |
| Manque de temps | Fonctions incomplètes | Développer d'abord les fonctions de priorité haute |
| Appareils d'entrée de gamme lents | Application peu fluide | Listes virtualisées, peu de dépendances, essais sur appareil réel |
| Récupération du mot de passe hors ligne | Utilisateur bloqué | Question secrète locale ; réinitialisation de l'application en dernier recours |
| Notifications bloquées par le système | Rappels non reçus | Expliquer et demander l'autorisation au bon moment |

Évolutions possibles, hors de cette version : mode sombre complet, mise à jour des données en ligne, davantage de cultures et de zones, version iOS, sauvegarde des parcelles.
