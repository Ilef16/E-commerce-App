import { Component, input } from '@angular/core';
import { PageHeader } from '../shared/components/page-header/page-header';

@Component({
  selector: 'app-placeholder-page',
  imports: [PageHeader],
  template: `
    <app-page-header [title]="title()" [subtitle]="subtitle()" />
    <section class="empty">
      <p>Ce module est prêt. Les écrans de liste et de formulaire arriveront ensuite.</p>
    </section>
  `,
  styles: `
    .empty {
      padding: 2rem;
      border: 1px dashed var(--line);
      border-radius: 1.1rem;
      background: var(--card);
      color: var(--muted);
    }
  `,
})
export class PlaceholderPage {
  readonly title = input.required<string>();
  readonly subtitle = input.required<string>();
}
