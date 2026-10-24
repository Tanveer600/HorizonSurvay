import axios, { AxiosError } from 'axios';
import type {
  ApprovalReceipt,
  ApprovalRequest,
  EquipmentCategory,
  CheckoutReceipt,
  CheckoutRequest,
  EquipmentInventoryItem,
  InventorySnapshot,
  MovementRecord,
  OpenCheckout,
  RegisterEquipmentRequest,
  ReturnRequest,
} from './types';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

export function getApiErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { detail?: string; title?: string; message?: string; errors?: Record<string, string[]> }
      | undefined;
    const validation = data?.errors
      ? Object.values(data.errors).flat().filter(Boolean).join(' ')
      : '';
    return validation || data?.detail || data?.message || data?.title ||
      (error.response
        ? `The server returned ${error.response.status}. Please review your request and retry.`
        : 'The API could not be reached. Check your connection and retry.');
  }
  return error instanceof Error ? error.message : 'Something went wrong. Please retry.';
}

export const inventoryApi = {
  async getInventory(): Promise<InventorySnapshot> {
    return (await apiClient.get<InventorySnapshot>('/inventory')).data;
  },
  async registerEquipment(request: RegisterEquipmentRequest): Promise<EquipmentInventoryItem> {
    return (await apiClient.post<EquipmentInventoryItem>('/equipment', request)).data;
  },
};

export const categoryApi = {
  async getAll(): Promise<EquipmentCategory[]> {
    return (await apiClient.get<EquipmentCategory[]>('/categories')).data;
  },
  async create(name: string): Promise<EquipmentCategory> {
    return (await apiClient.post<EquipmentCategory>('/categories', { name })).data;
  },
  async update(id: string, name: string): Promise<EquipmentCategory> {
    return (await apiClient.put<EquipmentCategory>(`/categories/${id}`, { name })).data;
  },
  async delete(id: string): Promise<void> {
    await apiClient.delete(`/categories/${id}`);
  },
};

export const checkoutApi = {
  async getOpenCheckouts(): Promise<OpenCheckout[]> {
    return (await apiClient.get<OpenCheckout[]>('/checkouts')).data;
  },
  async checkout(request: CheckoutRequest): Promise<CheckoutReceipt> {
    return (await apiClient.post<CheckoutReceipt>('/checkouts', request)).data;
  },
  async returnItems(id: string, request: ReturnRequest): Promise<void> {
    await apiClient.post(`/checkouts/${id}/returns`, request);
  },
};

export const approvalApi = {
  async record(request: ApprovalRequest): Promise<ApprovalReceipt> {
    return (await apiClient.post<ApprovalReceipt>('/approvals', request)).data;
  },
};

export const historyApi = {
  async get(from: string, to: string): Promise<MovementRecord[]> {
    return (await apiClient.get<MovementRecord[]>('/history', { params: { from, to } })).data;
  },
  async deleteOne(id: string): Promise<void> {
    await apiClient.delete(`/history/${id}`);
  },
  async deleteRange(from: string, to: string): Promise<number> {
    const response = await apiClient.delete<{ deletedCount: number }>('/history', { params: { from, to } });
    return response.data.deletedCount;
  },
};

export function isApiError(error: unknown): error is AxiosError {
  return axios.isAxiosError(error);
}
