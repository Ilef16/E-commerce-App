export interface ClientWriteDto {
  identifiant: string;
  nom: string;
  prenom?: string | null;
  email: string;
  telephone?: string | null;
  adresse?: string | null;
  ville?: string | null;
  codePostal?: string | null;
}

export interface ClientDto {
  id: number;
  nom: string;
  identifiant: string;
  prenom: string | null;
  email: string;
  telephone: string | null;
  adresse: string | null;
  ville: string | null;
  codePostal: string | null;
  createdAt: string;
  updatedAt: string | null;
  nombreCommandes: number;
}
