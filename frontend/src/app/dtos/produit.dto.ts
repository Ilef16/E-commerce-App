export interface ProduitWriteDto {
  reference: string;
  libelle: string;
  description: string | null;
  photoUrl: string | null;
  removePhoto?: boolean;
  prixUnitaire: number;
  stock: number;
}

export interface ProduitDto extends ProduitWriteDto {
  id: number;
  createdAt: string;
  updatedAt: string | null;
}
