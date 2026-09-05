namespace CommercialManagement.API.DTOs;

public class DashboardDto
{
    // ── Counts ─────────────────────────────────────────
    public int TotalClients { get; init; }
    public int TotalProduits { get; init; }
    public int TotalCommandes { get; init; }

    // ── Commandes par statut ────────────────────────────
    public int CommandesBrouillon { get; init; }
    public int CommandesValidees { get; init; }
    public int CommandesLivrees { get; init; }
    public int CommandesAnnulees { get; init; }

    // ── Chiffre d'affaires (commandes validées + livrées) ─
    public decimal ChiffreAffairesHt { get; init; }
    public decimal ChiffreAffairesTtc { get; init; }

    // ── Stock ──────────────────────────────────────────
    public int ProduitsStockFaible { get; init; }   // stock ≤ 5
    public int ProduitsRupture { get; init; }        // stock = 0
}
