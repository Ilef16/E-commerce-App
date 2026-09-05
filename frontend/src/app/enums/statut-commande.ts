export const StatutCommande = {
  Brouillon: 0,
  Validee: 1,
  Livree: 2,
  Annulee: 3,
} as const;

export type StatutCommande = (typeof StatutCommande)[keyof typeof StatutCommande];
