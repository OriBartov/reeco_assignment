export type QueryParamValue = string | number | boolean | undefined | null;

/**
 * Builds a URL query string from flat key/value pairs.
 * Omits undefined, null, and empty-string values.
 */
export function buildQueryString(params: object): string {
  const searchParams = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') {
      continue;
    }
    searchParams.set(key, String(value));
  }

  const query = searchParams.toString();
  return query ? `?${query}` : '';
}
