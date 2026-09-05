import type { EntityBase } from './entity-base.model';

export interface Client extends EntityBase {
  nom: string;
  prenom?: string | null;
  email: string;
  telephone?: string | null;
  adresse?: string | null;
  ville?: string | null;
  codePostal?: string | null;
}
