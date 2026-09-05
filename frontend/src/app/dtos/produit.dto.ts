export interface ProduitWriteDto {
  reference: string;
  libelle: string;
  description: string | null;
  prixUnitaire: number;
  stock: number;
}

export interface ProduitDto extends ProduitWriteDto {
  id: number;
  createdAt: string;
  updatedAt: string | null;
}
