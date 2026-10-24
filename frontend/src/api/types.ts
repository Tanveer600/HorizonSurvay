export type MovementType = 'Checkout' | 'Return' | 'Damage';

export interface EquipmentCategory {
  id: string;
  name: string;
}

export interface EquipmentInventoryItem {
  id: string;
  name: string;
  categoryId: string | null;
  category: string | null;
  isAvailable: boolean;
  currentHolder: string | null;
  expectedReturnAt: string | null;
  isOverdue: boolean;
  latestDamageDescription: string | null;
}

export interface PooledInventoryItem {
  id: string;
  name: string;
  totalQuantity: number;
  availableQuantity: number;
  overdueQuantity: number;
  latestDamageDescription: string | null;
}

export interface InventorySnapshot {
  equipment: EquipmentInventoryItem[];
  pooledStock: PooledInventoryItem[];
}

export interface OpenCheckout {
  id: string;
  equipmentAssetId: string | null;
  pooledStockId: string | null;
  itemName: string;
  category: string | null;
  holder: string;
  expectedReturnAt: string;
  outstandingQuantity: number;
  isOverdue: boolean;
}

export interface CheckoutRequest {
  holder: string;
  expectedReturnAt: string;
  equipmentAssetId?: string;
  pooledStockId?: string;
  quantity: number;
  approvalId?: string;
}

export interface CheckoutReceipt {
  id: string;
  holder: string;
  expectedReturnAt: string;
  quantity: number;
  outstandingQuantity: number;
  assetAvailable: boolean | null;
  stockAvailableQuantity: number | null;
}

export interface ReturnRequest {
  returner: string;
  quantity: number;
  damageDescription?: string;
  damageQuantity?: number;
}

export interface ApprovalRequest {
  equipmentAssetId: string;
  holder: string;
  approvedBy: string;
}

export interface ApprovalReceipt {
  id: string;
  equipmentAssetId: string;
  holder: string;
  approvedBy: string;
  approvedAt: string;
}

export interface RegisterEquipmentRequest {
  name: string;
  categoryId: string;
}

export interface MovementRecord {
  id: string;
  checkoutId: string;
  itemName: string;
  type: MovementType;
  quantity: number;
  occurredAt: string;
  performedBy: string;
  description: string | null;
}
