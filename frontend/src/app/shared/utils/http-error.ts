import { HttpErrorResponse } from '@angular/common/http';

/** Reads the API ProblemDetails message, with a fallback for unexpected payloads. */
export function apiErrorMessage(error: HttpErrorResponse, fallback: string): string {
  if (error.status === 0) {
    return 'Le serveur API est injoignable. Vérifiez qu’il est démarré.';
  }

  const body = error.error;
  if (typeof body === 'string' && body.trim()) {
    if (body.startsWith('<') || body.length > 300) return fallback;
    return body;
  }

  return body?.detail ?? body?.title ?? body?.value?.detail ?? fallback;
}
