// Convertit la base agricole Cameroun (fiches régionales) vers le schéma
// attendu par l'application : Resources/Raw/cultures.json.
//
// Usage :
//   node tools/convertir-base-agricole.js <chemin-base.json>
//
// Règles :
// - les 9 cultures du jeu de départ v1 (tools/seed-v1.json) gardent leurs
//   calendriers détaillés, re-clés dans les saisons régionales ;
// - les 31 autres cultures reprennent le modèle régional de la base ;
// - une phase sans mois explicite n'est pas importée (aucune date inventée) ;
// - les cultures pérennes ne reçoivent que leur fenêtre de plantation ;
// - les conseils ne sont repris que pour les 9 cultures d'origine.

const fs = require('fs');
const path = require('path');

const racine = path.resolve(__dirname, '..');
const cheminBase = process.argv[2] ?? 'C:/Users/SMILE/Downloads/base_agricole_cameroun_calendrier.json';
const base = JSON.parse(fs.readFileSync(cheminBase, 'utf8'));
const v1 = JSON.parse(fs.readFileSync(path.join(__dirname, 'seed-v1.json'), 'utf8'));

const MOIS = {
  janvier: 1, fevrier: 2, mars: 3, avril: 4, mai: 5, juin: 6,
  juillet: 7, aout: 8, septembre: 9, octobre: 10, novembre: 11, decembre: 12,
};

const NOMS = {
  'maïs': 'Maïs',
  'sorgho': 'Sorgho',
  'mil': 'Mil',
  'riz': 'Riz',
  'manioc': 'Manioc',
  'igname': 'Igname',
  'patate douce': 'Patate douce',
  'pomme de terre': 'Pomme de terre',
  'haricot commun': 'Haricot',
  'niébé': 'Niébé',
  'arachide': 'Arachide',
  'soja': 'Soja',
  'sésame': 'Sésame',
  'macabo': 'Macabo',
  'banane plantain': 'Plantain',
  'banane dessert': 'Banane',
  'cacao': 'Cacao',
  'café robusta': 'Café robusta',
  'café arabica': 'Café arabica',
  'palmier à huile': 'Palmier à huile',
  'ananas': 'Ananas',
  'papaye': 'Papaye',
  'mangue': 'Mangue',
  'avocat': 'Avocat',
  'tomate': 'Tomate',
  'piment': 'Piment',
  'oignon': 'Oignon',
  'ail': 'Ail',
  'gombo': 'Gombo',
  'pastèque': 'Pastèque',
  'melon': 'Melon',
  'coton': 'Coton',
  'canne à sucre': 'Canne à sucre',
  'chou': 'Chou',
  'carotte': 'Carotte',
  'poireau': 'Poireau',
  'concombre': 'Concombre',
  'légumes-feuilles': 'Légumes-feuilles',
  'gingembre': 'Gingembre',
  'thé': 'Thé',
};

const CATEGORIES = {
  'maïs': 'cereale', 'sorgho': 'cereale', 'mil': 'cereale', 'riz': 'cereale',
  'manioc': 'tubercule', 'igname': 'tubercule', 'patate douce': 'tubercule',
  'pomme de terre': 'tubercule', 'macabo': 'tubercule',
  'haricot commun': 'legume', 'niébé': 'legume', 'arachide': 'legume',
  'soja': 'legume', 'tomate': 'legume', 'piment': 'legume', 'oignon': 'legume',
  'ail': 'legume', 'gombo': 'legume', 'pastèque': 'legume', 'melon': 'legume',
  'chou': 'legume', 'carotte': 'legume', 'poireau': 'legume',
  'concombre': 'legume', 'légumes-feuilles': 'legume', 'gingembre': 'legume',
  'cacao': 'rente', 'café robusta': 'rente', 'café arabica': 'rente',
  'palmier à huile': 'rente', 'ananas': 'rente', 'papaye': 'rente',
  'mangue': 'rente', 'avocat': 'rente', 'coton': 'rente',
  'canne à sucre': 'rente', 'sésame': 'rente', 'thé': 'rente',
  'banane plantain': 'tubercule', 'banane dessert': 'rente',
};

// Cultures pérennes : on n'importe que la plantation (pas de récolte annuelle inventée).
const PERENNES = new Set([
  'cacao', 'café robusta', 'café arabica', 'palmier à huile', 'mangue', 'avocat', 'thé',
]);

// Correspondance entre les 9 cultures v1 et leur nom dans la nouvelle base.
const LIEN_V1 = {
  'maïs': 'mais',
  'manioc': 'manioc',
  'banane plantain': 'plantain',
  'arachide': 'arachide',
  'tomate': 'tomate',
  'haricot commun': 'haricot',
  'cacao': 'cacao',
  'igname': 'igname',
  'piment': 'piment',
};

// Ordre d'affichage : les 4 régions déjà utilisées d'abord (Centre par défaut),
// puis les 6 nouvelles.
const ORDRE_REGIONS = [
  'centre', 'littoral', 'ouest', 'extreme_nord',
  'adamaoua', 'est', 'nord', 'nord_ouest', 'sud', 'sud_ouest',
];

const ZONE_V1 = {
  centre: 'centre',
  littoral: 'littoral',
  ouest: 'ouest',
  'extreme-nord': 'extreme_nord',
};

// Index de la saison régionale qui correspond aux anciennes clés v1.
const SAISON_V1 = {
  centre: { 'petite-pluies': 0, 'grande-pluies': 1 },
  est: { 'petite-pluies': 0, 'grande-pluies': 1 },
  sud: { 'petite-pluies': 0, 'grande-pluies': 1 },
  littoral: { 'petite-pluies': 0, 'grande-pluies': 2 },
  sud_ouest: { 'petite-pluies': 0, 'grande-pluies': 2 },
  ouest: { 'petite-pluies': 0, 'grande-pluies': 1 },
  nord_ouest: { 'petite-pluies': 0, 'grande-pluies': 1 },
  extreme_nord: { 'grande-pluies': 0 },
  nord: { 'grande-pluies': 0 },
  adamaoua: { 'grande-pluies': 0 },
};

const normaliser = (texte) => texte.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();

function idDe(nom) {
  const decompose = nom.trim().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
  let sortie = '';
  for (const caractere of decompose) {
    if (/[\p{L}\p{N}]/u.test(caractere)) {
      sortie += caractere.toLowerCase();
    } else if (sortie.length > 0 && !sortie.endsWith('-')) {
      sortie += '-';
    }
  }
  return sortie.replace(/^-+|-+$/g, '') || nom.trim().toLowerCase();
}

function moisDe(texte) {
  return MOIS[normaliser(texte)];
}

// Première plage « mars-avril » / « avril à mai » / « octobre-janvier » du texte.
function extrairePlage(texte) {
  const t = normaliser(texte);
  const motif = /\b(janvier|fevrier|mars|avril|mai|juin|juillet|aout|septembre|octobre|novembre|decembre)\b\s*(?:-|–|a|au)\s*(janvier|fevrier|mars|avril|mai|juin|juillet|aout|septembre|octobre|novembre|decembre)\b/;
  const trouve = t.match(motif);
  if (!trouve) {
    return null;
  }
  return { debut: MOIS[trouve[1]], fin: MOIS[trouve[2]] };
}

function cycleDe(texte) {
  const t = normaliser(texte);
  const trouve = t.match(/(\d+(?:[.,]\d+)?)\s*(?:-|–|a)\s*(\d+(?:[.,]\d+)?)\s*(jours?|semaines?|mois|ans?)/);
  if (!trouve) {
    throw new Error(`Durée illisible : ${texte}`);
  }
  const min = parseFloat(trouve[1].replace(',', '.'));
  const max = parseFloat(trouve[2].replace(',', '.'));
  const unite = trouve[3];
  const facteur = unite.startsWith('jour') ? 1 : unite.startsWith('semaine') ? 7 : unite.startsWith('mois') ? 30 : 365;
  const libelle = unite.startsWith('jour') ? 'jours' : unite.startsWith('semaine') ? 'semaines' : unite.startsWith('mois') ? 'mois' : 'ans';
  return {
    jours: Math.round(((min + max) / 2) * facteur),
    texte: `${trouve[1].replace('.', ',')} à ${trouve[2].replace('.', ',')} ${libelle}`,
  };
}

const fichesParRegion = new Map();
for (const fiche of base.fiches_regionales) {
  if (!fichesParRegion.has(fiche.region_id)) {
    fichesParRegion.set(fiche.region_id, fiche);
  }
}

const regions = ORDRE_REGIONS.map((id) => {
  const fiche = fichesParRegion.get(id);
  if (!fiche) {
    throw new Error(`Région absente de la base : ${id}`);
  }
  return {
    id,
    nom: fiche.region,
    zoneAgroecologique: fiche.zone_agroecologique,
    saisons: fiche.calendrier_saisonnier.map((saison) => saison.nom),
    fiche,
  };
});

const parId = new Map(regions.map((region) => [region.id, region]));

function etapesDuModele(region) {
  const etapes = [];
  for (const saison of region.fiche.calendrier_saisonnier) {
    for (const phase of ['semis', 'entretien', 'recolte']) {
      const plage = extrairePlage(saison[phase]);
      if (plage) {
        etapes.push({ phase, saison: saison.nom, debut: plage.debut, fin: plage.fin });
      }
    }
  }
  return etapes;
}

const modeles = new Map(regions.map((region) => [region.id, etapesDuModele(region)]));

function ajouterEtape(culture, phase, region, saison, debut, fin) {
  const cle = `${region}|${saison}|${phase}`;
  if (culture._cles.has(cle)) {
    return;
  }
  culture._cles.add(cle);
  culture.etapes.push({ phase, zone: region, saison, debut, fin });
}

function ajouterModeles(culture, region, perenne) {
  for (const etape of modeles.get(region.id)) {
    if (perenne && etape.phase !== 'semis') {
      continue;
    }
    ajouterEtape(culture, etape.phase, region.nom, etape.saison, etape.debut, etape.fin);
  }
}

const cultures = [];

// 1. Les 9 cultures d'origine : calendriers détaillés conservés.
for (const ancienne of v1.cultures) {
  const nomSource = Object.keys(LIEN_V1).find((cle) => LIEN_V1[cle] === ancienne.id);
  const regionV1 = {};
  for (const etape of ancienne.etapes) {
    (regionV1[etape.zone] ??= []).push(etape);
  }

  const culture = {
    id: ancienne.id,
    nom: ancienne.nom,
    categorie: ancienne.categorie,
    cycleTexte: ancienne.cycleTexte,
    dureeCycleJours: ancienne.dureeCycleJours,
    etapes: [],
    conseils: ancienne.conseils,
    _cles: new Set(),
  };
  const perenne = PERENNES.has(nomSource);

  for (const [zoneV1, etapes] of Object.entries(regionV1)) {
    const regionId = ZONE_V1[zoneV1];
    const region = parId.get(regionId);
    const correspondance = SAISON_V1[regionId];
    for (const etape of etapes) {
      if (perenne && etape.phase !== 'semis') {
        continue;
      }
      const index = correspondance[etape.saison];
      if (index === undefined) {
        continue;
      }
      ajouterEtape(culture, etape.phase, region.nom, region.saisons[index], etape.debut, etape.fin);
    }
  }

  for (const region of regions) {
    if (/^(centre|littoral|ouest|extreme_nord)$/.test(region.id)) {
      continue;
    }
    ajouterModeles(culture, region, perenne);
  }

  cultures.push(culture);
}

// 2. Les 31 autres cultures : modèle régional de la base.
for (const [nomSource, profil] of Object.entries(base.profils_cultures)) {
  if (LIEN_V1[nomSource]) {
    continue;
  }
  const cycle = cycleDe(profil.duree_cycle_indicative);
  const culture = {
    id: idDe(NOMS[nomSource]),
    nom: NOMS[nomSource],
    categorie: CATEGORIES[nomSource],
    cycleTexte: cycle.texte,
    dureeCycleJours: cycle.jours,
    etapes: [],
    _cles: new Set(),
  };
  const perenne = PERENNES.has(nomSource);
  for (const region of regions) {
    ajouterModeles(culture, region, perenne);
  }
  cultures.push(culture);
}

cultures.sort((a, b) => a.nom.localeCompare(b.nom, 'fr'));

const ordreRegion = new Map(regions.map((region, index) => [region.nom, index]));
const ordreSaison = new Map();
for (const region of regions) {
  region.saisons.forEach((saison, index) => ordreSaison.set(`${region.nom}|${saison}`, index));
}
const ordrePhase = { semis: 0, entretien: 1, recolte: 2 };

for (const culture of cultures) {
  delete culture._cles;
  culture.etapes.sort((a, b) =>
    ordreRegion.get(a.zone) - ordreRegion.get(b.zone)
    || ordreSaison.get(`${a.zone}|${a.saison}`) - ordreSaison.get(`${b.zone}|${b.saison}`)
    || ordrePhase[a.phase] - ordrePhase[b.phase]);
}

const sortie = {
  miseAJour: base.metadata.date_generation,
  note: "Modèles régionaux indicatifs, issus des calendriers MINADER/ONACC 2026. À valider avec un agronome; ne pas utiliser comme dates garanties.",
  regions: regions.map((region) => ({
    nom: region.nom,
    zoneAgroecologique: region.zoneAgroecologique,
    saisons: region.saisons,
  })),
  cultures,
};

// Contrôles avant écriture.
const ids = new Set();
for (const culture of cultures) {
  if (ids.has(culture.id)) {
    throw new Error(`Identifiant en double : ${culture.id}`);
  }
  ids.add(culture.id);
  if (!culture.nom || !culture.categorie || !culture.cycleTexte) {
    throw new Error(`Champs manquants : ${culture.id}`);
  }
  for (const etape of culture.etapes) {
    if (!(etape.debut >= 1 && etape.debut <= 12 && etape.fin >= 1 && etape.fin <= 12)) {
      throw new Error(`Mois invalide : ${culture.id} ${etape.zone} ${etape.saison} ${etape.phase}`);
    }
    const region = regions.find((r) => r.nom === etape.zone);
    if (!region || !region.saisons.includes(etape.saison)) {
      throw new Error(`Zone/saison inconnue : ${culture.id} ${etape.zone} ${etape.saison}`);
    }
  }
  if (culture.etapes.length === 0) {
    throw new Error(`Culture sans calendrier : ${culture.id}`);
  }
}

fs.writeFileSync(
  path.join(racine, 'Resources', 'Raw', 'cultures.json'),
  JSON.stringify(sortie, null, 2) + '\n',
  'utf8',
);

console.log(`Écrit : Resources/Raw/cultures.json`);
console.log(`Cultures : ${cultures.length} | Régions : ${regions.length}`);
console.log('Étapes par culture (min/max) :',
  Math.min(...cultures.map((c) => c.etapes.length)),
  '/',
  Math.max(...cultures.map((c) => c.etapes.length)));
for (const region of regions) {
  const total = cultures.reduce((n, c) => n + c.etapes.filter((e) => e.zone === region.nom).length, 0);
  console.log(` - ${region.nom} : ${region.saisons.length} saison(s), ${total} étape(s)`);
}
