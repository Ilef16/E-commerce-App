/** Aligné sur backend/Constants/BusinessConstants.cs */
export const TVA_RATE = 0.19;
export const DEFAULT_PAGE_SIZE = 10;
export const LOOKUP_PAGE_SIZE = 100;
export const LOW_STOCK_THRESHOLD = 5;

export const OrderStatus = {
  Brouillon: 0,
  Validee: 1,
  Livree: 2,
  Annulee: 3,
} as const;

export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus];

export const ORDER_STATUS_LABELS: Record<number, string> = {
  [OrderStatus.Brouillon]: 'Brouillon',
  [OrderStatus.Validee]: 'Validée',
  [OrderStatus.Livree]: 'Livrée',
  [OrderStatus.Annulee]: 'Annulée',
};

export const Etattaxe = {
  active: 1,
  inactive:0,
} as const;
export type Etattaxe = (typeof Etattaxe )[keyof typeof Etattaxe] 
export const Etat: Record<number, string> ={
  [Etattaxe.active]:'active',
  [Etattaxe.inactive]: 'inactive',
};