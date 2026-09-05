import { Component } from '@angular/core';
import { PlaceholderPage } from '../placeholder-page';

@Component({
  selector: 'app-factures',
  imports: [PlaceholderPage],
  template: `<app-placeholder-page title="Factures" subtitle="Facturation, échéances et encaissements." />`,
})
export class Factures {}
