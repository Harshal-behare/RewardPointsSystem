const API_BASE_URL = 'http://localhost:5000/api';

export const ApiEndpoints = {
  // Auth
  login: `${API_BASE_URL}/auth/login`,
  logout: `${API_BASE_URL}/auth/logout`,
  
  // Users
  users: `${API_BASE_URL}/users`,
  userById: (id: number) => `${API_BASE_URL}/users/${id}`,
  
  // Events
  events: `${API_BASE_URL}/events`,
  eventById: (id: number) => `${API_BASE_URL}/events/${id}`,
  
  // Products
  products: `${API_BASE_URL}/products`,
  productById: (id: number) => `${API_BASE_URL}/products/${id}`,
  
  // Points
  points: `${API_BASE_URL}/points`,
  awardPoints: `${API_BASE_URL}/points/award`,
  
  // Redemptions
  redemptions: `${API_BASE_URL}/redemptions`,
  
  // Dashboard
  adminDashboard: `${API_BASE_URL}/admin/dashboard`,
  employeeDashboard: `${API_BASE_URL}/employee/dashboard`,
};
