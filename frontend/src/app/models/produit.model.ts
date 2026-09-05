import type { EntityBase } from './entity-base.model';

export interface Produit extends EntityBase {
  reference: string;
  libelle: string;
  description?: string | null;
  prixUnitaire: number;
  stock: number;
}
