export const TUNISIA_WILAYAS = [
  'Ariana', 'Béja', 'Ben Arous', 'Bizerte', 'Gabès', 'Gafsa',
  'Jendouba', 'Kairouan', 'Kasserine', 'Kébili', 'Le Kef', 'Mahdia',
  'Manouba', 'Médenine', 'Monastir', 'Nabeul', 'Sfax', 'Sidi Bouzid',
  'Siliana', 'Sousse', 'Tataouine', 'Tozeur', 'Tunis', 'Zaghouan',
] as const;

export const ORDER_STATUS_LABELS: Record<number, string> = {
  0: 'Brouillon',
  1: 'Validée',
  2: 'Livrée',
  3: 'Annulée',
};
