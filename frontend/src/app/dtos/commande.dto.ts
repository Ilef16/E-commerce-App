export interface CommandeLigneDto {
  id: number;
  produitId: number;
  referenceProduit: string;
  libelleProduit: string;
  quantite: number;
  prixUnitaire: number;
  totalLigne: number;
}

export interface CommandeDto {
  id: number;
  numero: string;
  clientId: number;
  clientNom: string;
  dateCommande: string;
  statut: number;
  totalHt: number;
  tva: number;
  totalTtc: number;
  createdAt: string;
  updatedAt: string | null;
  lignes: CommandeLigneDto[];
}

export interface CommandeLigneWriteDto {
  produitId: number;
  quantite: number;
}

export interface CommandeWriteDto {
  clientId: number;
  dateCommande?: string | null;
  lignes: CommandeLigneWriteDto[];
}
