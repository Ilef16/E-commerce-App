import type { EntityBase } from './entity-base.model';

export interface LigneCommande extends EntityBase {
  commandeId: number;
  produitId: number;
  quantite: number;
  prixUnitaire: number;
  totalLigne: number;
}
