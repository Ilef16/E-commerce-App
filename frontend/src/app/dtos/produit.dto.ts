export interface ProduitWriteDto {
  reference: string;
  libelle: string;
  description: string | null;
  removePhoto?: boolean;
  prixUnitaire: number;
  stock: number;
}

export interface ProduitDto {
  id: number;
  reference: string;
  libelle: string;
  description: string | null;
  photoUrl: string | null;
  prixUnitaire: number;
  stock: number;
  createdAt: string;
  updatedAt: string | null;
}
