export interface DashboardDto {
  totalClients: number;
  totalProduits: number;
  totalCommandes: number;
  commandesBrouillon: number;
  commandesValidees: number;
  commandesLivrees: number;
  commandesAnnulees: number;
  chiffreAffairesHt: number;
  chiffreAffairesTtc: number;
  produitsStockFaible: number;
  produitsRupture: number;
}
