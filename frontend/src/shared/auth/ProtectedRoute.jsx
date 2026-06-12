import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from './AuthContext'

export function ProtectedRoute({ roles }) {
  const auth = useAuth()
  const location = useLocation()

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (roles?.length && !roles.includes(auth.role)) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}