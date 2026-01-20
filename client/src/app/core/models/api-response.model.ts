export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
  errors?: { [key: string]: string[] }; // Field-level validation errors from backend
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Extracts validation error messages from an API error response
 * Handles both FluentValidation errors and regular error responses
 */
export function extractValidationErrors(error: any): string[] {
  const messages: string[] = [];

  // Check for validation errors object (FluentValidation format)
  const errors = error?.error?.errors || error?.errors;
  if (errors && typeof errors === 'object') {
    Object.keys(errors).forEach(field => {
      const fieldErrors = errors[field];
      if (Array.isArray(fieldErrors)) {
        fieldErrors.forEach(msg => messages.push(msg));
      } else if (typeof fieldErrors === 'string') {
        messages.push(fieldErrors);
      }
    });
  }

  // Check for single message
  const message = error?.error?.message || error?.message;
  if (message && messages.length === 0) {
    messages.push(message);
  }

  // Check for title (ASP.NET validation response)
  const title = error?.error?.title;
  if (title && messages.length === 0) {
    messages.push(title);
  }

  // Default message if nothing found
  if (messages.length === 0) {
    messages.push('An unexpected error occurred. Please try again.');
  }

  return messages;
}

/**
 * Extracts a single combined error message from validation errors
 */
export function extractErrorMessage(error: any): string {
  const messages = extractValidationErrors(error);
  return messages.join(' ');
}
