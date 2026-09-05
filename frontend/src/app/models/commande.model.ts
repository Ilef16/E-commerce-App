import type { StatutCommande } from '../enums/statut-commande';
import type { EntityBase } from './entity-base.model';
import type { LigneCommande } from './ligne-commande.model';

export interface Commande extends EntityBase {
  numero: string;
  clientId: number;
  dateCommande: string;
  statut: StatutCommande;
  total: number;
  lignes: LigneCommande[];
}
