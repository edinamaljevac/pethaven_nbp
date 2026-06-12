import { useState } from 'react'
import { filesApi, lostFoundApi, profileApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { lostFoundTypes } from '../../shared/constants/enums'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { ErrorState, LoadingState } from '../../shared/ui/State'

const emptyForm = () => ({
  userId: '',
  type: 0,
  species: '',
  breed: '',
  color: '',
  description: '',
  location: '',
  reportDate: new Date().toISOString().slice(0, 10),
  imageUrl: '',
  contactPhone: '',
})

export function LostFoundPage() {
  const auth = useAuth()
  const isShelter = auth.hasRole('Shelter')
  const isAdmin = auth.hasRole('Admin')
  const canCreateReport = auth.hasRole('Adopter', 'Foster', 'Shelter')
  const [filters, setFilters] = useState({ includeResolved: isAdmin || auth.hasRole('Adopter', 'Foster') })
  const { data, loading, error, reload } = useApi(() => lostFoundApi.list(filters), [JSON.stringify(filters)])
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const [form, setForm] = useState(emptyForm())
  const [imageFile, setImageFile] = useState(null)
  const [saveError, setSaveError] = useState(null)
  const [actionError, setActionError] = useState(null)
  const [matches, setMatches] = useState([])
  const [selectedMatch, setSelectedMatch] = useState(null)

  const reports = data ?? []
  const visibleReports = isShelter ? reports.filter(report => !report.isResolved) : reports
  const stats = buildStats(reports, matches.length)

  const create = async e => {
    e.preventDefault()
    setSaveError(null)
    try {
      let imageUrl = form.imageUrl
      if (imageFile) {
        const uploadResponse = await filesApi.upload(imageFile, profile?.userId)
        imageUrl = uploadResponse.data.url
      }

      await lostFoundApi.create({ ...form, imageUrl, userId: profile?.userId, type: isShelter ? 1 : Number(form.type) })
      setForm(emptyForm())
      setImageFile(null)
      reload()
    } catch (err) {
      setSaveError(err)
    }
  }

  const findMatches = async id => {
    setActionError(null)
    setSelectedMatch(null)
    try {
      setMatches((await lostFoundApi.matches(id)).data)
    } catch (err) {
      setActionError(err)
    }
  }

  const resolveReport = async id => {
    setActionError(null)
    try {
      await lostFoundApi.resolve(id)
      reload()
    } catch (err) {
      setActionError(err)
    }
  }

  const setReportVisibility = async (id, isHidden) => {
    setActionError(null)
    try {
      await lostFoundApi.visibility(id, isHidden)
      reload()
    } catch (err) {
      setActionError(err)
    }
  }

  return <div className="grid gap-6">
    <PageHeader title="Lost & Found" description={descriptionForRole(auth)} />

    {isAdmin && <LostFoundStats stats={stats} />}

    <section className="rounded-md border border-slate-200 bg-white p-4">
      <div className="grid gap-3 md:grid-cols-4">
        {['species', 'location'].map(k => <Field key={k} label={label(k)}>
          <input className={inputClass()} value={filters[k] ?? ''} onChange={e => setFilters({ ...filters, [k]: e.target.value })} />
        </Field>)}
        <Field label="Type">
          <select className={inputClass()} value={filters.type ?? ''} onChange={e => setFilters({ ...filters, type: e.target.value })}>
            <option value="">Any</option>
            {lostFoundTypes.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </Field>
        {(isShelter || isAdmin) && <Field label="Resolved">
          <select className={inputClass()} value={String(filters.includeResolved ?? false)} onChange={e => setFilters({ ...filters, includeResolved: e.target.value === 'true' })}>
            <option value="false">Active only</option>
            <option value="true">Include resolved</option>
          </select>
        </Field>}
      </div>
    </section>

    {loading ? <LoadingState /> : <>
      <ErrorState error={error} />
      <ErrorState error={actionError} />
      <DataTable rows={visibleReports} columns={columnsForRole({ isAuthenticated: auth.isAuthenticated, isShelter, isAdmin, findMatches, resolveReport, setReportVisibility })} />
    </>}

    {canCreateReport && <CreateReportForm
      form={form}
      setForm={setForm}
      imageFile={imageFile}
      setImageFile={setImageFile}
      profile={profile}
      saveError={saveError}
      create={create}
      isShelter={isShelter}
    />}

    {matches.length > 0 && !isAdmin && <section>
      <h2 className="mb-3 font-bold">Matches</h2>
      <DataTable rows={matches} columns={[
        { key: 'score', label: 'Score' },
        { key: 'reason', label: 'Reason' },
        { key: 'report', label: 'Report', render: r => `${r.report.species} in ${r.report.location}` },
        { key: 'actions', label: 'Actions', render: r => <Button variant="secondary" onClick={() => setSelectedMatch(r)}>View Contact</Button> },
      ]} />
    </section>}
    {selectedMatch && <MatchDetailsModal match={selectedMatch} onClose={() => setSelectedMatch(null)} />}
  </div>
}

function CreateReportForm({ form, setForm, imageFile, setImageFile, profile, saveError, create, isShelter }) {
  return <form onSubmit={create} className="rounded-md border border-slate-200 bg-white p-4">
    <h2 className="font-bold">{isShelter ? 'Create found report' : 'Create report'}</h2>
    <div className="mt-3 grid gap-3 md:grid-cols-4">
      <div className="md:col-span-4"><ErrorState error={saveError} /></div>
      <Field label="Reporter"><input className={inputClass()} value={profile?.email ?? 'Current user'} disabled /></Field>
      <Field label="Type"><select className={inputClass()} value={isShelter ? 1 : form.type} disabled={isShelter} onChange={e => setForm({ ...form, type: e.target.value })}>{lostFoundTypes.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>
      <Field label="Species"><input className={inputClass()} value={form.species} onChange={e => setForm({ ...form, species: e.target.value })} /></Field>
      <Field label="Breed"><input className={inputClass()} value={form.breed} onChange={e => setForm({ ...form, breed: e.target.value })} /></Field>
      <Field label="Color"><input className={inputClass()} value={form.color} onChange={e => setForm({ ...form, color: e.target.value })} /></Field>
      <Field label="Description"><input className={inputClass()} value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} /></Field>
      <Field label="Location"><input className={inputClass()} value={form.location} onChange={e => setForm({ ...form, location: e.target.value })} /></Field>
      <Field label="Report date"><input className={inputClass()} type="date" value={form.reportDate} onChange={e => setForm({ ...form, reportDate: e.target.value })} /></Field>
      <Field label="Image"><input className={inputClass()} type="file" accept="image/*" value={imageFile ? undefined : ''} onChange={e => setImageFile(e.target.files?.[0] ?? null)} /></Field>
      <Field label="Contact phone"><input className={inputClass()} value={form.contactPhone} onChange={e => setForm({ ...form, contactPhone: e.target.value })} /></Field>
      <div className="md:col-span-4"><Button>Create report</Button></div>
    </div>
  </form>
}

function LostFoundStats({ stats }) {
  const cards = [
    { label: 'Lost reports', value: stats.lost },
    { label: 'Found reports', value: stats.found },
    { label: 'Resolved reports', value: stats.resolved },
  ]
  return <section className="grid gap-3 md:grid-cols-3">
    {cards.map(card => <div key={card.label} className="rounded-md border border-slate-200 bg-white p-4">
      <p className="text-sm font-semibold text-slate-500">{card.label}</p>
      <p className="mt-2 text-2xl font-bold text-slate-950">{card.value}</p>
    </div>)}
  </section>
}

function columnsForRole({ isAuthenticated, isShelter, isAdmin, findMatches, resolveReport, setReportVisibility }) {
  const baseColumns = [
    { key: 'type', label: 'Type', render: r => lostFoundTypes[r.type]?.label },
    { key: 'species', label: 'Species' },
    { key: 'color', label: 'Color' },
    { key: 'location', label: 'Location' },
    { key: 'isResolved', label: 'Resolved', render: r => r.isResolved ? 'Yes' : 'No' },
  ]

  if (isAdmin) return [
    ...baseColumns,
    { key: 'isHidden', label: 'Visibility', render: r => r.isHidden ? 'Hidden' : 'Visible' },
    { key: 'actions', label: 'Actions', render: r => <Button variant="secondary" onClick={() => setReportVisibility(r.id, !r.isHidden)}>{r.isHidden ? 'Restore' : 'Hide'}</Button> },
  ]

  if (!isAuthenticated) return baseColumns

  if (!isShelter) {
    return isAuthenticated
      ? [...baseColumns, { key: 'actions', label: 'Actions', render: r => <div className="flex flex-wrap gap-2">
        {!r.isResolved && <Button variant="secondary" onClick={() => findMatches(r.id)}>Matches</Button>}
        {!r.isResolved && <Button variant="secondary" onClick={() => resolveReport(r.id)}>Mark resolved</Button>}
      </div> }]
      : baseColumns
  }

  return [
    ...baseColumns,
    {
      key: 'actions',
      label: 'Actions',
      render: r => <div className="flex flex-wrap gap-2">
        <Button variant="secondary" onClick={() => findMatches(r.id)}>Matches</Button>
        {!r.isResolved && <Button variant="secondary" onClick={() => resolveReport(r.id)}>Mark resolved</Button>}
      </div>,
    },
  ]
}

function buildStats(reports, matchesCount) {
  return {
    lost: reports.filter(r => r.type === 0).length,
    found: reports.filter(r => r.type === 1).length,
    resolved: reports.filter(r => r.isResolved).length,
    matches: matchesCount,
  }
}

function descriptionForRole(auth) {
  if (auth.hasRole('Admin')) return 'Monitor Lost & Found reports, statistics and resolution status.'
  if (auth.hasRole('Shelter')) return 'Review active reports, contact reporters and help resolve cases.'
  return 'Report animals and find potential matches by description and location.'
}

function label(key) {
  return key.replace(/([A-Z])/g, ' $1').replace(/^./, c => c.toUpperCase())
}

function MatchDetailsModal({ match, onClose }) {
  const report = match.report
  return <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/40 p-4">
    <section className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-md bg-white p-5 shadow-xl">
      <div className="flex items-start justify-between gap-4 border-b border-slate-200 pb-3">
        <div>
          <h2 className="text-xl font-bold text-slate-950">Contact for {lostFoundTypes[report.type]?.label} {report.species}</h2>
          <p className="text-sm text-slate-600">Match score: {match.score}</p>
        </div>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      {report.imageUrl ? <img className="mt-4 max-h-80 w-full rounded-md object-cover" src={absoluteUrl(report.imageUrl)} alt={`${report.species} ${report.breed}`} /> : <div className="mt-4 rounded-md border border-dashed border-slate-300 p-6 text-center text-sm text-slate-500">No image uploaded.</div>}

      <dl className="mt-4 grid gap-3 sm:grid-cols-2">
        <Detail label="Type" value={lostFoundTypes[report.type]?.label} />
        <Detail label="Species" value={report.species} />
        <Detail label="Breed" value={report.breed} />
        <Detail label="Color" value={report.color} />
        <Detail label="Location" value={report.location} />
        <Detail label="Report date" value={formatDate(report.reportDate)} />
        <Detail label="Reporter email" value={report.reporterEmail} />
        <Detail label="Contact phone" value={report.contactPhone} />
        <Detail label="Resolved" value={report.isResolved ? 'Yes' : 'No'} />
        <div className="sm:col-span-2"><Detail label="Description" value={report.description} /></div>
        <div className="sm:col-span-2"><Detail label="Reason" value={match.reason} /></div>
      </dl>
    </section>
  </div>
}

function Detail({ label, value }) {
  return <div>
    <dt className="text-xs font-semibold uppercase text-slate-500">{label}</dt>
    <dd className="mt-1 text-sm text-slate-800">{value || '-'}</dd>
  </div>
}

function formatDate(value) {
  if (!value) return '-'
  return new Date(value).toLocaleDateString()
}

function absoluteUrl(url) {
  if (!url || url.startsWith('http')) return url
  const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001/api/v1'
  return `${apiBase.replace('/api/v1', '')}${url}`
}
