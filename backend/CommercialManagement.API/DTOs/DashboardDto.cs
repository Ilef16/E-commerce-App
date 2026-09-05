namespace CommercialManagement.API.DTOs;

public class DashboardDto
{
    public int TotalClients { get; init; }
    public int TotalProduits { get; init; }
    public int TotalCommandes { get; init; }
    public int CommandesBrouillon { get; init; }
    public int CommandesValidees { get; init; }
    public int CommandesLivrees { get; init; }
    public int CommandesAnnulees { get; init; }
    public decimal ChiffreAffairesHt { get; init; }
    public decimal ChiffreAffairesTtc { get; init; }
    public int ProduitsStockFaible { get; init; }
    public int ProduitsRupture { get; init; }
}
