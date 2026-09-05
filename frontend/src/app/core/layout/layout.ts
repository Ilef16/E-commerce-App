import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.html',
  styleUrl: './layout.scss',
})
export class Layout {
  readonly links = [
    { path: '/tableau-de-bord', label: 'Tableau de bord' },
    { path: '/clients', label: 'Clients' },
    { path: '/produits', label: 'Produits' },
    { path: '/commandes', label: 'Commandes' },
  ];
}
