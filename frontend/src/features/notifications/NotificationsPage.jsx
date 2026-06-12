import { useState } from 'react'
import { Pencil, Trash2 } from 'lucide-react'
import { notificationsApi, profileApi, savedFiltersApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { animalSizes, energyLevels, notificationTypes } from '../../shared/constants/enums'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { ErrorState } from '../../shared/ui/State'

const emptyFilter = () => ({ name: '', species: '', breed: '', size: '', energyLevel: '', city: '' })

export function NotificationsPage() {
  const auth = useAuth()
  const isAdopter = auth.hasRole('Adopter')
  const isAdmin = auth.hasRole('Admin')
  const [unreadOnly, setUnreadOnly] = useState(false)
  const [filter, setFilter] = useState(emptyFilter())
  const [editingFilterId, setEditingFilterId] = useState(null)
  const [filterError, setFilterError] = useState(null)
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const notificationParams = profile?.userId ? { userId: profile.userId, unreadOnly } : { unreadOnly }
  const { data, error, reload } = useApi(() => notificationsApi.list(notificationParams), [JSON.stringify(notificationParams)])
  const { data: filters, reload: reloadFilters } = useApi(() => isAdopter ? savedFiltersApi.list({}) : Promise.resolve({ data: [] }), [isAdopter])

  const read = async id => {
    await notificationsApi.read(id)
    reload()
  }

  const notificationColumns = [
    { key: 'type', label: 'Type', render: r => notificationTypes[r.type]?.label ?? r.type },
    { key: 'title', label: 'Title' },
    { key: 'message', label: 'Message' },
    { key: 'createdAt', label: 'Created at', render: r => formatDate(r.createdAt) },
    { key: 'isRead', label: 'Status', render: r => <StatusBadge isRead={r.isRead} /> },
    { key: 'actions', label: 'Actions', render: r => r.isRead ? '-' : <Button variant="secondary" onClick={() => read(r.id)}>Mark read</Button> },
  ]

  const createFilter = async e => {
    e.preventDefault()
    setFilterError(null)
    if (!filter.name.trim()) return setFilterError({ userMessage: 'Filter name is required.' })
    if (!filter.species.trim() && !filter.breed.trim() && !filter.city.trim() && filter.size === '' && filter.energyLevel === '') {
      return setFilterError({ userMessage: 'Add at least one search criterion.' })
    }

    if (editingFilterId) {
      await savedFiltersApi.update(editingFilterId, clean(filter))
    } else {
      await savedFiltersApi.create(clean(filter))
    }
    setFilter(emptyFilter())
    setEditingFilterId(null)
    reloadFilters()
  }

  const editFilter = savedFilter => {
    setEditingFilterId(savedFilter.id)
    setFilter({
      name: savedFilter.name ?? '',
      species: savedFilter.species ?? '',
      breed: savedFilter.breed ?? '',
      city: savedFilter.city ?? '',
      size: savedFilter.size ?? '',
      energyLevel: savedFilter.energyLevel ?? '',
    })
  }

  const cancelEdit = () => {
    setEditingFilterId(null)
    setFilter(emptyFilter())
    setFilterError(null)
  }

  const deleteFilter = async id => {
    await savedFiltersApi.delete(id)
    if (editingFilterId === id) cancelEdit()
    reloadFilters()
  }

  return <div className="grid gap-6">
    <PageHeader title="Notifications" description={description(auth)} />

    {isAdmin ? <section className="rounded-md border border-slate-200 bg-white p-4">
      <h2 className="font-bold">Notification management</h2>
      <p className="mt-1 text-sm text-slate-600">Admin notifications cover new shelter verification requests and important platform events. Animal-match notifications are generated automatically when an animal becomes available.</p>
      <div className="mt-3 max-w-md">
        <Field label="Show"><select className={inputClass()} value={String(unreadOnly)} onChange={e => setUnreadOnly(e.target.value === 'true')}><option value="false">All admin notifications</option><option value="true">Unread only</option></select></Field>
      </div>
    </section> : <section className="rounded-md border border-slate-200 bg-white p-4">
      <div className="grid gap-3 md:grid-cols-2">
        <Field label="User"><input className={inputClass()} value={profile?.email ?? 'Current user'} disabled /></Field>
        <Field label="Show"><select className={inputClass()} value={String(unreadOnly)} onChange={e => setUnreadOnly(e.target.value === 'true')}><option value="false">All notifications</option><option value="true">Unread only</option></select></Field>
      </div>
    </section>}

    <section>
      <h2 className="mb-3 font-bold">{isAdmin ? 'Admin notifications' : 'My notifications'}</h2>
      <ErrorState error={error} />
      <DataTable rows={data ?? []} columns={notificationColumns} />
    </section>

    {isAdopter && <section className="grid gap-4 md:grid-cols-2">
      <form onSubmit={createFilter} className="rounded-md border border-slate-200 bg-white p-4">
        <h2 className="font-bold">Create saved animal filter</h2>
        <p className="mt-1 text-sm text-slate-600">You will get a notification when a new available animal matches this filter.</p>
        <div className="mt-3 grid gap-3">
          <ErrorState error={filterError} />
          <Field label="Filter name"><input className={inputClass()} value={filter.name} onChange={e => setFilter({ ...filter, name: e.target.value })} /></Field>
          <Field label="Species"><input className={inputClass()} value={filter.species} onChange={e => setFilter({ ...filter, species: e.target.value })} /></Field>
          <Field label="Breed"><input className={inputClass()} value={filter.breed} onChange={e => setFilter({ ...filter, breed: e.target.value })} /></Field>
          <Field label="City"><input className={inputClass()} value={filter.city} onChange={e => setFilter({ ...filter, city: e.target.value })} /></Field>
          <Field label="Size"><select className={inputClass()} value={filter.size} onChange={e => setFilter({ ...filter, size: e.target.value })}><option value="">Any</option>{animalSizes.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>
          <Field label="Energy"><select className={inputClass()} value={filter.energyLevel} onChange={e => setFilter({ ...filter, energyLevel: e.target.value })}><option value="">Any</option>{energyLevels.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>
          <div className="flex gap-2">
            <Button>{editingFilterId ? 'Update filter' : 'Save filter'}</Button>
            {editingFilterId && <Button type="button" variant="secondary" onClick={cancelEdit}>Cancel</Button>}
          </div>
        </div>
      </form>
      <section>
        <h2 className="mb-3 font-bold">My saved filters</h2>
        <SavedFilterCards filters={filters ?? []} onEdit={editFilter} onDelete={deleteFilter} />
      </section>
    </section>}
  </div>
}

function SavedFilterCards({ filters, onEdit, onDelete }) {
  if (!filters.length) {
    return <div className="rounded-md border border-dashed border-slate-300 bg-white p-6 text-center text-sm text-slate-500">No saved filters yet.</div>
  }

  return <div className="grid gap-3">
    {filters.map(filter => <article key={filter.id} className="rounded-md border border-slate-200 bg-white p-4">
      <div className="flex items-start justify-between gap-3">
        <div>
          <h3 className="font-bold text-slate-950">{filter.name}</h3>
          <p className="mt-1 text-sm text-slate-600">{filterSummary(filter)}</p>
        </div>
        <div className="flex gap-1">
          <button type="button" title="Edit filter" aria-label={`Edit ${filter.name}`} className="rounded-md border border-slate-200 p-2 text-slate-600 hover:bg-slate-50" onClick={() => onEdit(filter)}><Pencil size={16} /></button>
          <button type="button" title="Delete filter" aria-label={`Delete ${filter.name}`} className="rounded-md border border-red-200 p-2 text-red-600 hover:bg-red-50" onClick={() => onDelete(filter.id)}><Trash2 size={16} /></button>
        </div>
      </div>
    </article>)}
  </div>
}

function filterSummary(filter) {
  const values = [
    filter.species && `Species: ${filter.species}`,
    filter.breed && `Breed: ${filter.breed}`,
    filter.city && `City: ${filter.city}`,
    filter.size !== null && filter.size !== undefined && `Size: ${animalSizes[filter.size]?.label ?? filter.size}`,
    filter.energyLevel !== null && filter.energyLevel !== undefined && `Energy: ${energyLevels[filter.energyLevel]?.label ?? filter.energyLevel}`,
  ].filter(Boolean)
  return values.join(' · ') || 'Any available animal'
}

function clean(obj) {
  return Object.fromEntries(Object.entries(obj).filter(([, v]) => v !== '' && v !== null).map(([k, v]) => [['size', 'energyLevel'].includes(k) ? [k, Number(v)] : [k, v]][0]))
}

function description(auth) {
  if (auth.hasRole('Admin')) return 'New shelter verification requests and important platform events.'
  if (auth.hasRole('Adopter')) return 'Animal match alerts, adoption status updates and post-adoption report reminders.'
  if (auth.hasRole('Shelter')) return 'Review shelter account and security notifications.'
  return 'Review security and activity notifications.'
}

function StatusBadge({ isRead }) {
  const className = isRead ? 'bg-slate-100 text-slate-600' : 'bg-brand-50 text-brand-700'
  return <span className={`rounded-full px-2 py-1 text-xs font-semibold ${className}`}>{isRead ? 'Read' : 'Unread'}</span>
}

function formatDate(value) {
  if (!value) return '-'
  return new Date(value).toLocaleString()
}
