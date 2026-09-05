import { Routes } from '@angular/router';
import { Layout } from './core/layout/layout';
import { Clients } from './pages/clients/clients';
import { Commandes } from './pages/commandes/commandes';
import { Dashboard } from './pages/dashboard/dashboard';
import { Factures } from './pages/factures/factures';
import { Produits } from './pages/produits/produits';

export const routes: Routes = [
  {
    path: '',
    component: Layout,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'tableau-de-bord' },
      { path: 'tableau-de-bord', component: Dashboard },
      { path: 'clients', component: Clients },
      { path: 'produits', component: Produits },
      { path: 'commandes', component: Commandes },
      { path: 'factures', component: Factures },
    ],
  },
];
