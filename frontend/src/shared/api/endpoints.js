import { api } from './client'

export const authApi = {
  login: (payload) => api.post('/auth/login', payload),
  register: (payload) => api.post('/auth/register', payload),
  registerShelter: (payload) => api.post('/auth/register-shelter', payload, { headers: { 'Content-Type': 'multipart/form-data' } }),
  refresh: (payload) => api.post('/auth/refresh-token', payload),
  revoke: (payload) => api.post('/auth/revoke-token', payload),
}

export const animalsApi = {
  list: (params) => api.get('/animals', { params }),
  create: (payload) => api.post('/animals', payload),
  health: (animalId, payload) => api.put(`/animals/${animalId}/care/health-record`, payload),
  behavior: (animalId, payload) => api.put(`/animals/${animalId}/care/behavior-profile`, payload),
  video: (animalId, payload) => api.patch(`/animals/${animalId}/video`, payload),
}

export const sheltersApi = {
  list: (params) => api.get('/shelters', { params }),
  nearMe: (params) => api.get('/shelters/near-me', { params }),
  create: (payload) => api.post('/shelters', payload),
  verify: (id, payload) => api.patch(`/shelters/${id}/verification`, payload),
}

export const adoptionsApi = {
  list: (params) => api.get('/adoptions', { params }),
  submit: (payload) => api.post('/adoptions', payload),
  status: (id, payload) => api.patch(`/adoptions/${id}/status`, payload),
  contract: (id) => api.post(`/adoptions/${id}/contract`),
  getContract: (id) => api.get(`/adoptions/${id}/contract`),
  requestReports: (id) => api.post(`/adoptions/${id}/post-adoption-reports`),
  reports: (params) => api.get('/adoptions/post-adoption-reports', { params }),
  submitReport: (id, payload) => api.patch(`/adoptions/post-adoption-reports/${id}/submit`, payload),
}

export const fostersApi = {
  profiles: (params) => api.get('/fosters', { params }),
  me: () => api.get('/fosters/me'),
  createProfile: (payload) => api.post('/fosters', payload),
  assignments: (params) => api.get('/fosters/assignments', { params }),
  assign: (payload) => api.post('/fosters/assignments', payload),
  endAssignment: (id) => api.patch(`/fosters/assignments/${id}/end`),
  reports: (params) => api.get('/fosters/reports', { params }),
  submitReport: (payload) => api.post('/fosters/reports', payload),
}

export const lostFoundApi = {
  list: (params) => api.get('/lost-found', { params }),
  create: (payload) => api.post('/lost-found', payload),
  matches: (id) => api.get(`/lost-found/${id}/matches`),
  resolve: (id) => api.patch(`/lost-found/${id}/resolve`),
  visibility: (id, isHidden) => api.patch(`/lost-found/${id}/visibility`, { reportId: id, isHidden }),
}

export const donationsApi = {
  list: (params) => api.get('/donations', { params }),
  donate: (payload) => api.post('/donations/mock-payment', payload),
  goals: (params) => api.get('/donations/goals', { params }),
  createGoal: (payload) => api.post('/donations/goals', payload),
}

export const volunteersApi = {
  list: (params) => api.get('/volunteers/applications', { params }),
  submit: (payload) => api.post('/volunteers/applications', payload),
  approve: (id, payload) => api.patch(`/volunteers/applications/${id}/approval`, payload),
}

export const notificationsApi = {
  list: (params) => api.get('/notifications', { params }),
  read: (id) => api.patch(`/notifications/${id}/read`),
}

export const savedFiltersApi = {
  list: (params) => api.get('/saved-filters', { params }),
  create: (payload) => api.post('/saved-filters', payload),
  update: (id, payload) => api.put(`/saved-filters/${id}`, payload),
  delete: (id) => api.delete(`/saved-filters/${id}`),
}

export const filesApi = {
  list: (params) => api.get('/files', { params }),
  upload: (file, uploadedByUserId) => {
    const data = new FormData()
    data.append('file', file)
    if (uploadedByUserId) data.append('uploadedByUserId', uploadedByUserId)
    return api.post('/files/upload', data, { headers: { 'Content-Type': 'multipart/form-data' } })
  },
  animalPhoto: (animalId, payload) => api.post(`/media/animals/${animalId}/photos`, payload),
  shelterPhoto: (shelterId, payload) => api.post(`/media/shelters/${shelterId}/photos`, payload),
}

export const adminApi = {
  statistics: () => api.get('/admin/statistics'),
  dashboardStatistics: () => api.get('/admin/statistics/dashboard'),
  moderateAnimal: (id, payload) => api.patch(`/admin/animals/${id}/moderation`, payload),
  moderateContent: (payload) => api.post('/admin/moderation', payload),
}
export const profileApi = {
  me: () => api.get('/profile/me'),
  update: (payload) => api.put('/profile/me', payload),
  geocode: (location) => api.get('/profile/geocode', { params: { location } }),
}
