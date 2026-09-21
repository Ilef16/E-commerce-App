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

  // TVA saisie en pourcentage
  tva: number;

  // Montant calculé de la TVA
  montantTva?: number;

  totalTtc: number;

  // Remise saisie en pourcentage
  remise: number;

  // Montant calculé de la remise
  montantRemise?: number;

  // Total après remise
  totalApresRemise?: number;

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

  // Pourcentages saisis par l'utilisateur
  tva: number;
  remise: number;

  lignes: CommandeLigneWriteDto[];
}