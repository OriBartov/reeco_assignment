import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
  DEFAULT_PAGE_SIZE,
  PAGE_SIZE_OPTIONS,
  type SortableOrderColumn,
} from '../lib/order-constants';
import type { OrderListQuery, OrderPriority, OrderStatus } from '../types';

const FILTER_KEYS = [
  'status',
  'priority',
  'supplier_id',
  'warehouse',
  'date_from',
  'date_to',
  'search',
  'sort',
  'order',
  'limit',
  'offset',
] as const;

export interface OrderListParamsState {
  query: OrderListQuery;
  statuses: OrderStatus[];
  sort: SortableOrderColumn | undefined;
  sortOrder: 'asc' | 'desc';
  page: number;
  pageSize: number;
}

function parseStatuses(value: string | null): OrderStatus[] {
  if (!value) {
    return [];
  }
  return value.split(',').map((s) => s.trim()).filter(Boolean) as OrderStatus[];
}

function parsePageSize(value: string | null): number {
  const parsed = Number(value);
  if (PAGE_SIZE_OPTIONS.includes(parsed as (typeof PAGE_SIZE_OPTIONS)[number])) {
    return parsed;
  }
  return DEFAULT_PAGE_SIZE;
}

function parseOffset(value: string | null): number {
  const parsed = Number(value);
  if (!Number.isFinite(parsed) || parsed < 0) {
    return 0;
  }
  return parsed;
}

function isSortableColumn(value: string | null): value is SortableOrderColumn {
  return (
    value === 'created_at' ||
    value === 'total_price' ||
    value === 'quantity' ||
    value === 'status' ||
    value === 'priority' ||
    value === 'warehouse'
  );
}

export function useOrderListParams() {
  const [searchParams, setSearchParams] = useSearchParams();

  const state = useMemo((): OrderListParamsState => {
    const pageSize = parsePageSize(searchParams.get('limit'));
    const offset = parseOffset(searchParams.get('offset'));
    const statuses = parseStatuses(searchParams.get('status'));
    const sortParam = searchParams.get('sort');
    const sort = isSortableColumn(sortParam) ? sortParam : undefined;
    const sortOrder = searchParams.get('order') === 'desc' ? 'desc' : 'asc';

    const query: OrderListQuery = {
      limit: pageSize,
      offset,
    };

    if (statuses.length > 0) {
      query.status = statuses.join(',');
    }

    const priority = searchParams.get('priority');
    if (priority) {
      query.priority = priority as OrderPriority;
    }

    const supplierId = searchParams.get('supplier_id');
    if (supplierId) {
      query.supplier_id = supplierId;
    }

    const warehouse = searchParams.get('warehouse');
    if (warehouse) {
      query.warehouse = warehouse;
    }

    const dateFrom = searchParams.get('date_from');
    if (dateFrom) {
      query.date_from = dateFrom;
    }

    const dateTo = searchParams.get('date_to');
    if (dateTo) {
      query.date_to = dateTo;
    }

    const search = searchParams.get('search');
    if (search) {
      query.search = search;
    }

    if (sort) {
      query.sort = sort;
      query.order = sortOrder;
    }

    return {
      query,
      statuses,
      sort,
      sortOrder,
      page: Math.floor(offset / pageSize) + 1,
      pageSize,
    };
  }, [searchParams]);

  const patchParams = useCallback(
    (updates: Record<string, string | null>, resetPage = false) => {
      setSearchParams(
        (prev) => {
          const next = new URLSearchParams(prev);

          for (const [key, value] of Object.entries(updates)) {
            if (value === null || value === '') {
              next.delete(key);
            } else {
              next.set(key, value);
            }
          }

          if (resetPage) {
            next.delete('offset');
          }

          return next;
        },
        { replace: true },
      );
    },
    [setSearchParams],
  );

  const setStatuses = useCallback(
    (statuses: OrderStatus[]) => {
      patchParams(
        { status: statuses.length > 0 ? statuses.join(',') : null },
        true,
      );
    },
    [patchParams],
  );

  const setFilter = useCallback(
    (key: 'priority' | 'supplier_id' | 'warehouse' | 'date_from' | 'date_to' | 'search', value: string) => {
      patchParams({ [key]: value || null }, true);
    },
    [patchParams],
  );

  const setSort = useCallback(
    (column: SortableOrderColumn) => {
      const isSameColumn = state.sort === column;
      const nextOrder = isSameColumn && state.sortOrder === 'asc' ? 'desc' : 'asc';
      patchParams({
        sort: column,
        order: nextOrder,
      });
    },
    [patchParams, state.sort, state.sortOrder],
  );

  const setPage = useCallback(
    (page: number) => {
      const safePage = Math.max(1, page);
      patchParams({ offset: String((safePage - 1) * state.pageSize) });
    },
    [patchParams, state.pageSize],
  );

  const setPageSize = useCallback(
    (pageSize: number) => {
      patchParams({ limit: String(pageSize), offset: null }, true);
    },
    [patchParams],
  );

  const clearFilters = useCallback(() => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        for (const key of FILTER_KEYS) {
          if (key !== 'limit') {
            next.delete(key);
          }
        }
        if (!next.get('limit')) {
          next.set('limit', String(DEFAULT_PAGE_SIZE));
        }
        return next;
      },
      { replace: true },
    );
  }, [setSearchParams]);

  const hasActiveFilters =
    state.statuses.length > 0 ||
    Boolean(state.query.priority) ||
    Boolean(state.query.supplier_id) ||
    Boolean(state.query.warehouse) ||
    Boolean(state.query.date_from) ||
    Boolean(state.query.date_to) ||
    Boolean(state.query.search);

  return {
    ...state,
    setStatuses,
    setFilter,
    setSort,
    setPage,
    setPageSize,
    clearFilters,
    hasActiveFilters,
  };
}
