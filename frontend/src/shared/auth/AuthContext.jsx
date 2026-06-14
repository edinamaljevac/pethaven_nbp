import { createContext, useContext, useMemo, useState } from 'react'
import { authApi } from '../api/endpoints'

const AuthContext = createContext(null)

function parseJwt(token) {
  if (!token) return null
  try {
    const payload = token.split('.')[1]
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(decodeURIComponent(escape(atob(normalized))))
  } catch {
    return null
  }
}

function roleFromToken(token) {
  const payload = parseJwt(token)
  return payload?.role || payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null
}

function readSession() {
  const accessToken = localStorage.getItem('pethaven_access_token')
  return {
    accessToken,
    refreshToken: localStorage.getItem('pethaven_refresh_token'),
    expiresAt: localStorage.getItem('pethaven_access_expires_at'),
    role: roleFromToken(accessToken),
  }
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(readSession)

  const saveSession = (response) => {
    const data = response.data
    localStorage.setItem('pethaven_access_token', data.accessToken)
    localStorage.setItem('pethaven_refresh_token', data.refreshToken)
    localStorage.setItem('pethaven_access_expires_at', data.accessTokenExpiresAt)
    setSession(readSession())
  }

  const login = async (payload) => {
    const response = await authApi.login(payload)
    saveSession(response)
    return response.data
  }
  const register = async (payload) => saveSession(await authApi.register(payload))

  const logout = async () => {
    const refreshToken = localStorage.getItem('pethaven_refresh_token')
    try {
      if (refreshToken) await authApi.revoke({ refreshToken })
    } finally {
      localStorage.removeItem('pethaven_access_token')
      localStorage.removeItem('pethaven_refresh_token')
      localStorage.removeItem('pethaven_access_expires_at')
      setSession(readSession())
    }
  }

  const value = useMemo(() => ({
    ...session,
    isAuthenticated: Boolean(session.accessToken),
    hasRole: (...roles) => roles.includes(session.role),
    login,
    register,
    logout,
  }), [session])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider')
  return context
}
