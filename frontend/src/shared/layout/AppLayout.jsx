import { Heart, LogOut, Menu, PawPrint, UserRound } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { profileApi } from '../api/endpoints'
import { useAuth } from '../auth/AuthContext'
import { useApi } from '../hooks/useApi'
import { Button } from '../ui/Button'

const links = [
  { to: '/animals', label: 'Animals' },
  { to: '/shelters', label: 'Shelters', hiddenFor: ['Shelter'] },
  { to: '/adoptions', label: 'Adoptions', roles: ['Adopter', 'Shelter', 'Admin'] },
  { to: '/fosters', label: 'Foster', roles: ['Foster', 'Shelter', 'Admin'] },
  { to: '/lost-found', label: 'Lost & Found' },
  { to: '/donations', label: 'Donations' },
  { to: '/volunteers', label: 'Volunteers', roles: ['Adopter', 'Shelter', 'Admin'] },
  { to: '/notifications', label: 'Notifications', roles: ['Adopter'] },
  { to: '/admin', label: 'Admin', roles: ['Admin'] },
  { to: '/statistics', label: 'Statistics', roles: ['Admin'] },
]

function visibleLinks(auth) {
  return links.filter((link) => (!link.roles || link.roles.includes(auth.role)) && !link.hiddenFor?.includes(auth.role))
}

export function AppLayout() {
  const auth = useAuth()
  const navLinks = visibleLinks(auth)
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const profileName = displayName(profile, auth.role)

  return (
    <div className="flex min-h-screen flex-col bg-slate-50">
      <header className="sticky top-0 z-20 border-b border-slate-200 bg-white/95 backdrop-blur">
        <div className="mx-auto flex max-w-[112rem] items-center gap-2 px-3 py-2.5">
          <NavLink to="/" className="flex shrink-0 items-center gap-2 pr-2 font-bold text-brand-700">
            <span className="grid h-10 w-10 place-items-center rounded-md bg-brand-100"><PawPrint size={21} /></span>
            <span className="hidden sm:inline">PetHaven</span>
          </NavLink>
          <nav className="hidden min-w-0 flex-1 items-center justify-start gap-0.5 xl:flex">
            {navLinks.map(({ to, label }) => (
              <NavLink key={to} to={to} className={({ isActive }) => `relative shrink-0 whitespace-nowrap rounded-md px-2 py-2 text-sm font-medium transition ${isActive ? 'bg-brand-50 text-brand-700 after:absolute after:inset-x-2 after:-bottom-2.5 after:h-0.5 after:bg-brand-600' : 'text-slate-600 hover:bg-slate-50 hover:text-slate-950'}`}>{label}</NavLink>
            ))}
          </nav>
          <div className="ml-auto flex shrink-0 items-center gap-2">
            {auth.isAuthenticated && <NavLink to="/profile" title="Open my profile" className="hidden items-center gap-2 rounded-md border border-slate-200 bg-white px-2.5 py-1.5 text-slate-700 transition hover:bg-slate-50 lg:flex">
              <span className="grid h-8 w-8 place-items-center rounded-full bg-brand-50 text-brand-700"><UserRound size={16} /></span>
              <span className="grid max-w-36 leading-tight">
                <span className="truncate text-sm font-semibold">{profileName}</span>
                <span className="text-xs text-slate-500">{auth.role}</span>
              </span>
            </NavLink>}
            <NavLink title="Open navigation" aria-label="Open navigation" className="rounded-md border border-slate-200 p-2.5 text-slate-600 transition hover:bg-slate-50 xl:hidden" to="/menu"><Menu size={19} /></NavLink>
            {auth.isAuthenticated ? <Button title="Logout" aria-label="Logout" className="h-10 w-10 p-0" variant="secondary" onClick={auth.logout}><LogOut size={17} /></Button> : <NavLink to="/login"><Button>Login</Button></NavLink>}
          </div>
        </div>
      </header>
      <main className="mx-auto w-full max-w-7xl flex-1 px-4 py-6"><Outlet /></main>
      <footer className="border-t border-slate-200 bg-white py-6 text-center text-sm text-slate-500">
        <span className="inline-flex items-center gap-2"><Heart size={16} /> PetHaven adoption platform</span>
      </footer>
    </div>
  )
}

export function MobileMenu() {
  const auth = useAuth()
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  return <div className="grid gap-2">
    {auth.isAuthenticated && <NavLink to="/profile" className="rounded-md border border-brand-200 bg-brand-50 px-4 py-3 text-brand-700"><span className="font-semibold">{displayName(profile, auth.role)}</span><span className="ml-2 text-xs">{auth.role}</span></NavLink>}
    {visibleLinks(auth).map(({ to, label }) => <NavLink key={to} to={to} className="rounded-md border border-slate-200 bg-white px-4 py-3 text-slate-700">{label}</NavLink>)}
  </div>
}

function displayName(profile, role) {
  if (role === 'Shelter' && profile?.shelterName) return profile.shelterName
  const name = `${profile?.firstName ?? ''} ${profile?.lastName ?? ''}`.trim()
  return name || profile?.email || role || 'My profile'
}
