import axios from 'axios'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001/api/v1',
})

let refreshPromise = null

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('pethaven_access_token')
  if (token && !config.skipAuth) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !originalRequest.url?.includes('/auth/')) {
      originalRequest._retry = true
      try {
        const accessToken = await refreshAccessToken()
        originalRequest.headers.Authorization = `Bearer ${accessToken}`
        return api(originalRequest)
      } catch {
        clearSession()
      }
    }

    const data = error.response?.data
    const validationMessages = data?.errors
      ? Object.entries(data.errors).flatMap(([field, messages]) =>
          messages.map((message) => `${field}: ${message}`),
        )
      : []
    const detail = validationMessages.length
      ? validationMessages.join('\n')
      : typeof data === 'string'
        ? data
        : data?.message || data?.detail || data?.title || error.message
    error.userMessage = detail
    return Promise.reject(error)
  },
)

async function refreshAccessToken() {
  if (!refreshPromise) {
    const refreshToken = localStorage.getItem('pethaven_refresh_token')
    if (!refreshToken) throw new Error('Missing refresh token.')

    refreshPromise = axios.post(`${api.defaults.baseURL}/auth/refresh-token`, { refreshToken })
      .then((response) => {
        const data = response.data
        localStorage.setItem('pethaven_access_token', data.accessToken)
        localStorage.setItem('pethaven_refresh_token', data.refreshToken)
        localStorage.setItem('pethaven_access_expires_at', data.accessTokenExpiresAt)
        return data.accessToken
      })
      .finally(() => {
        refreshPromise = null
      })
  }

  return refreshPromise
}

function clearSession() {
  localStorage.removeItem('pethaven_access_token')
  localStorage.removeItem('pethaven_refresh_token')
  localStorage.removeItem('pethaven_access_expires_at')
}
