import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { AdminPage } from './features/admin/AdminPage'
import { StatisticsPage } from './features/admin/StatisticsPage'
import { AdoptionsPage } from './features/adoptions/AdoptionsPage'
import { AnimalsPage } from './features/animals/AnimalsPage'
import { LoginPage } from './features/auth/LoginPage'
import { RegisterPage } from './features/auth/RegisterPage'
import { DonationsPage } from './features/donations/DonationsPage'
import { FostersPage } from './features/fosters/FostersPage'
import { HomePage } from './features/home/HomePage'
import { LostFoundPage } from './features/lostFound/LostFoundPage'
import { NotificationsPage } from './features/notifications/NotificationsPage'
import { ProfilePage } from './features/profile/ProfilePage'
import { SheltersPage } from './features/shelters/SheltersPage'
import { VolunteersPage } from './features/volunteers/VolunteersPage'
import { ProtectedRoute } from './shared/auth/ProtectedRoute'
import { AppLayout, MobileMenu } from './shared/layout/AppLayout'

const router = createBrowserRouter([
  {
    element: <AppLayout />,
    children: [
      { path: '/', element: <HomePage /> },
      { path: '/animals', element: <AnimalsPage /> },
      { path: '/shelters', element: <SheltersPage /> },
      { path: '/lost-found', element: <LostFoundPage /> },
      { path: '/donations', element: <DonationsPage /> },
      { path: '/login', element: <LoginPage /> },
      { path: '/register', element: <RegisterPage /> },
      { path: '/menu', element: <MobileMenu /> },
      { element: <ProtectedRoute roles={['Adopter', 'Shelter', 'Admin']} />, children: [{ path: '/adoptions', element: <AdoptionsPage /> }] },
      { element: <ProtectedRoute roles={['Foster', 'Shelter', 'Admin']} />, children: [{ path: '/fosters', element: <FostersPage /> }] },
      { element: <ProtectedRoute roles={['Adopter', 'Foster', 'Shelter', 'Admin']} />, children: [{ path: '/profile', element: <ProfilePage /> }, { path: '/volunteers', element: <VolunteersPage /> }] },
      { element: <ProtectedRoute roles={['Adopter']} />, children: [{ path: '/notifications', element: <NotificationsPage /> }] },
      { element: <ProtectedRoute roles={['Admin']} />, children: [{ path: '/admin', element: <AdminPage /> }, { path: '/statistics', element: <StatisticsPage /> }] },
    ],
  },
])

export default function App() {
  return <RouterProvider router={router} />
}
