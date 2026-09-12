export class ApiResponse<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
  errors: string[] = [];

  constructor(init: { success: boolean; message: string; data?: T; errors?: string[] }) {
    this.success = init.success;
    this.message = init.message;
    this.data = init.data;
    this.errors = init.errors ?? [];
  }

  static ok<T>(message: string, data?: T): ApiResponse<T> {
    return new ApiResponse<T>({ success: true, message, data });
  }

  static fail(message: string, errors: string[] = []): ApiResponse {
    return new ApiResponse({ success: false, message, errors });
  }
}

export class PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;

  constructor(init: { items: T[]; pageNumber: number; pageSize: number; totalCount: number }) {
    this.items = init.items;
    this.pageNumber = init.pageNumber;
    this.pageSize = init.pageSize;
    this.totalCount = init.totalCount;
    this.totalPages = Math.ceil(init.totalCount / init.pageSize);
  }
}
