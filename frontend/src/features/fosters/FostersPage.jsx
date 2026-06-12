import { useMemo, useState } from 'react'
import { animalsApi, filesApi, fostersApi, profileApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { animalLabel, fosterProfileLabel } from '../../shared/ui/SelectOptions'
import { ErrorState } from '../../shared/ui/State'

const empty = Promise.resolve({ data: [] })

function toDateInput(value) {
  if (!value) return ''
  return new Date(value).toISOString().slice(0, 10)
}

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '-'
}

function subtitle(auth) {
  if (auth.hasRole('Foster')) return 'Manage your foster profile and submit progress reports.'
  if (auth.hasRole('Shelter')) return 'Manage foster profiles, assignments and reports.'
  return 'Monitor foster program activity and statistics.'
}

function EmptyText({ children }) {
  return <div className="rounded-md border border-dashed border-slate-300 bg-white p-6 text-center text-slate-500">{children}</div>
}

function StatCard({ label, value }) {
  return <div className="rounded-md border border-slate-200 bg-white p-4"><div className="text-sm text-slate-500">{label}</div><div className="mt-2 text-2xl font-bold text-slate-900">{value}</div></div>
}

export function FostersPage() {
  const auth = useAuth()
  const isFoster = auth.hasRole('Foster')
  const isShelter = auth.hasRole('Shelter')
  const isAdmin = auth.hasRole('Admin')
  const { data: profileData } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const { data: fosterProfile, reload: reloadMyProfile } = useApi(() => isFoster ? fostersApi.me() : empty, [isFoster])
  const { data: fosterProfiles, reload: reloadProfiles } = useApi(() => (isShelter || isAdmin) ? fostersApi.profiles({}) : empty, [isShelter, isAdmin])
  const { data: assignments, reload: reloadAssignments } = useApi(() => (isFoster || isShelter || isAdmin) ? fostersApi.assignments({}) : empty, [isFoster, isShelter, isAdmin])
  const { data: reports, reload: reloadReports } = useApi(() => (isFoster || isShelter || isAdmin) ? fostersApi.reports({}) : empty, [isFoster, isShelter, isAdmin])
  const { data: animals } = useApi(() => isShelter ? animalsApi.list({ status: 0 }) : empty, [isShelter])
  const [profileForm, setProfileForm] = useState({ preferredAnimalType: 'Dog', capacity: 1, availableFrom: '', availableTo: '' })
  const [editingProfile, setEditingProfile] = useState(false)
  const [assignment, setAssignment] = useState({ animalId: '', fosterProfileId: '', notes: '' })
  const [report, setReport] = useState({ fosterAssignmentId: '', behaviorNotes: '', progressNotes: '', photoUrl: '' })
  const [reportPhoto, setReportPhoto] = useState(null)
  const [reportAssignmentId, setReportAssignmentId] = useState('')
  const [errors, setErrors] = useState({})
  const [err, setErr] = useState(null)
  const [message, setMessage] = useState('')

  const activeAssignments = (assignments ?? []).filter(x => x.isActive)
  const availableFosterProfiles = (fosterProfiles ?? []).filter(x => x.availableCapacity > 0)
  const availableAnimals = (animals ?? []).filter(x => !profileData?.profileId || x.shelterProfileId === profileData.profileId)
  const stats = useMemo(() => {
    const rows = assignments ?? []
    return {
      profiles: (fosterProfiles ?? []).length,
      active: rows.filter(x => x.isActive).length,
      completed: rows.filter(x => !x.isActive).length
    }
  }, [assignments, fosterProfiles])

  const fillProfileForm = () => {
    setProfileForm({
      preferredAnimalType: fosterProfile?.preferredAnimalType ?? 'Dog',
      capacity: fosterProfile?.capacity ?? 1,
      availableFrom: toDateInput(fosterProfile?.availableFrom),
      availableTo: toDateInput(fosterProfile?.availableTo)
    })
    setEditingProfile(true)
  }

  const validateProfile = () => {
    const next = {}
    if (Number(profileForm.capacity) <= 0) next.capacity = 'Capacity must be greater than 0.'
    if (!profileForm.availableFrom) next.availableFrom = 'Available From is required.'
    if (!profileForm.availableTo) next.availableTo = 'Available To is required.'
    if (profileForm.availableFrom && profileForm.availableTo && profileForm.availableFrom >= profileForm.availableTo) next.availableTo = 'Available From must be before Available To.'
    setErrors(next)
    return Object.keys(next).length === 0
  }

  const saveProfile = async e => {
    e.preventDefault()
    setErr(null); setMessage('')
    if (!validateProfile()) return
    try {
      await fostersApi.createProfile({ ...profileForm, capacity: Number(profileForm.capacity) })
      setMessage('Foster profile saved.')
      setEditingProfile(false)
      reloadMyProfile()
      reloadProfiles()
    } catch (x) { setErr(x) }
  }

  const assignAnimal = async e => {
    e.preventDefault()
    setErr(null); setMessage('')
    try {
      await fostersApi.assign(assignment)
      setMessage('Animal assigned to foster profile.')
      setAssignment({ animalId: '', fosterProfileId: '', notes: '' })
      reloadAssignments()
    } catch (x) { setErr(x) }
  }

  const endPlacement = async id => {
    setErr(null); setMessage('')
    try {
      await fostersApi.endAssignment(id)
      setMessage('Foster placement completed.')
      reloadAssignments()
    } catch (x) { setErr(x) }
  }

  const validateReport = () => {
    const next = {}
    if (!report.fosterAssignmentId) next.fosterAssignmentId = 'Assigned animal is required.'
    if (!report.behaviorNotes.trim()) next.behaviorNotes = 'Behavior Notes are required.'
    if (!report.progressNotes.trim()) next.progressNotes = 'Progress Notes are required.'
    setErrors(next)
    return Object.keys(next).length === 0
  }

  const submitReport = async e => {
    e.preventDefault()
    setErr(null); setMessage('')
    if (!validateReport()) return
    try {
      let photoUrl = report.photoUrl
      if (reportPhoto) {
        const upload = await filesApi.upload(reportPhoto, profileData?.userId)
        photoUrl = upload.data.url
      }

      await fostersApi.submitReport({ ...report, photoUrl })
      setMessage('Foster report submitted.')
      setReport({ fosterAssignmentId: '', behaviorNotes: '', progressNotes: '', photoUrl: '' })
      setReportPhoto(null)
      reloadReports()
    } catch (x) { setErr(x) }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Foster Program" description={subtitle(auth)} />
      {message && <div className="rounded-md bg-brand-50 p-3 text-sm text-brand-700">{message}</div>}
      <ErrorState error={err} />

      {isFoster && (
        <>
          {fosterProfile && !editingProfile ? (
            <section className="rounded-md border border-slate-200 bg-white p-4">
              <div className="flex items-center justify-between gap-3">
                <h2 className="font-bold">My Foster Profile</h2>
                <Button variant="secondary" onClick={fillProfileForm}>Edit Profile</Button>
              </div>
              <dl className="mt-4 grid gap-3 md:grid-cols-4">
                <Info label="Preferred Animal Type" value={fosterProfile.preferredAnimalType} />
                <Info label="Capacity" value={fosterProfile.capacity} />
                <Info label="Available From" value={formatDate(fosterProfile.availableFrom)} />
                <Info label="Available To" value={formatDate(fosterProfile.availableTo)} />
              </dl>
            </section>
          ) : (
            <ProfileForm title="Create Foster Profile" form={profileForm} setForm={setProfileForm} errors={errors} onSubmit={saveProfile} />
          )}

          <section className="grid gap-3">
            <h2 className="font-bold">My assigned animals</h2>
            {activeAssignments.length ? <DataTable rows={activeAssignments} columns={assignmentColumns(false)} /> : <EmptyText>No assigned animals yet.</EmptyText>}
          </section>

          <form onSubmit={submitReport} className="rounded-md border border-slate-200 bg-white p-4">
            <h2 className="font-bold">Foster Report</h2>
            <div className="mt-3 grid gap-3 md:grid-cols-2">
              <Field label="Assigned animal">
                <select className={inputClass()} value={report.fosterAssignmentId} onChange={e => setReport({ ...report, fosterAssignmentId: e.target.value })}>
                  <option value="">Choose assigned animal</option>
                  {activeAssignments.map(a => <option key={a.id} value={a.id}>{a.animalName}</option>)}
                </select>
                {errors.fosterAssignmentId && <p className="mt-1 text-sm text-red-600">{errors.fosterAssignmentId}</p>}
              </Field>
              <Field label="Photo">
                <input
                  className={inputClass()}
                  type="file"
                  accept="image/*"
                  onChange={e => setReportPhoto(e.target.files?.[0] ?? null)}
                />
              </Field>
              <Field label="Behavior Notes">
                <textarea className={inputClass()} value={report.behaviorNotes} onChange={e => setReport({ ...report, behaviorNotes: e.target.value })} />
                {errors.behaviorNotes && <p className="mt-1 text-sm text-red-600">{errors.behaviorNotes}</p>}
              </Field>
              <Field label="Progress Notes">
                <textarea className={inputClass()} value={report.progressNotes} onChange={e => setReport({ ...report, progressNotes: e.target.value })} />
                {errors.progressNotes && <p className="mt-1 text-sm text-red-600">{errors.progressNotes}</p>}
              </Field>
              <div className="md:col-span-2"><Button>Submit report</Button></div>
            </div>
          </form>
        </>
      )}

      {isShelter && (
        <>
          <section className="grid gap-3">
            <h2 className="font-bold">Available foster profiles</h2>
            {(fosterProfiles ?? []).length ? <DataTable rows={fosterProfiles} columns={profileColumns} /> : <EmptyText>No foster profiles available.</EmptyText>}
          </section>
          <form onSubmit={assignAnimal} className="rounded-md border border-slate-200 bg-white p-4">
            <h2 className="font-bold">Assign Animal</h2>
            <div className="mt-3 grid gap-3 md:grid-cols-3">
              <Field label="Animal">
                <select className={inputClass()} value={assignment.animalId} onChange={e => setAssignment({ ...assignment, animalId: e.target.value })} required>
                  <option value="">Choose animal</option>
                  {availableAnimals.map(a => <option key={a.id} value={a.id}>{animalLabel(a)}</option>)}
                </select>
              </Field>
              <Field label="Foster profile">
                <select className={inputClass()} value={assignment.fosterProfileId} onChange={e => setAssignment({ ...assignment, fosterProfileId: e.target.value })} required>
                  <option value="">Choose foster</option>
                  {availableFosterProfiles.map(p => <option key={p.id} value={p.id}>{fosterProfileLabel(p)} ({p.activeAssignments} / {p.capacity} occupied)</option>)}
                </select>
              </Field>
              <Field label="Notes">
                <input className={inputClass()} value={assignment.notes} onChange={e => setAssignment({ ...assignment, notes: e.target.value })} />
              </Field>
              <div className="md:col-span-3"><Button>Assign animal</Button></div>
            </div>
          </form>
          <section className="grid gap-3">
            <h2 className="font-bold">Active Foster Placements</h2>
            {activeAssignments.length ? <DataTable rows={activeAssignments} columns={assignmentColumns(true, endPlacement, setReportAssignmentId)} /> : <EmptyText>No assigned animals yet.</EmptyText>}
          </section>
          <ReportsTable reports={reports ?? []} assignmentId={reportAssignmentId} onClear={() => setReportAssignmentId('')} />
        </>
      )}

      {isAdmin && (
        <>
          <div className="grid gap-4 md:grid-cols-3">
            <StatCard label="Foster profiles" value={stats.profiles} />
            <StatCard label="Active foster placements" value={stats.active} />
            <StatCard label="Completed placements" value={stats.completed} />
          </div>
          <section className="grid gap-3">
            <h2 className="font-bold">Recent foster assignments</h2>
            <DataTable rows={(assignments ?? []).slice(0, 10)} columns={assignmentColumns(false)} />
          </section>
          <ReportsTable reports={reports ?? []} />
        </>
      )}
    </div>
  )
}

function Info({ label, value }) {
  return <div><dt className="text-sm text-slate-500">{label}</dt><dd className="font-semibold text-slate-800">{value}</dd></div>
}

function ProfileForm({ title, form, setForm, errors, onSubmit }) {
  return (
    <form onSubmit={onSubmit} className="rounded-md border border-slate-200 bg-white p-4">
      <h2 className="font-bold">{title}</h2>
      <div className="mt-3 grid gap-3 md:grid-cols-2">
        <Field label="Preferred Animal Type"><input className={inputClass()} value={form.preferredAnimalType} onChange={e => setForm({ ...form, preferredAnimalType: e.target.value })} /></Field>
        <Field label="Capacity"><input className={inputClass()} type="number" min="1" value={form.capacity} onChange={e => setForm({ ...form, capacity: e.target.value })} />{errors.capacity && <p className="mt-1 text-sm text-red-600">{errors.capacity}</p>}</Field>
        <Field label="Available From"><input className={inputClass()} type="date" value={form.availableFrom} onChange={e => setForm({ ...form, availableFrom: e.target.value })} />{errors.availableFrom && <p className="mt-1 text-sm text-red-600">{errors.availableFrom}</p>}</Field>
        <Field label="Available To"><input className={inputClass()} type="date" value={form.availableTo} onChange={e => setForm({ ...form, availableTo: e.target.value })} />{errors.availableTo && <p className="mt-1 text-sm text-red-600">{errors.availableTo}</p>}</Field>
        <div className="md:col-span-2"><Button>Save profile</Button></div>
      </div>
    </form>
  )
}

const profileColumns = [
  { key: 'fosterName', label: 'Foster' },
  { key: 'preferredAnimalType', label: 'Preferred Animal Type' },
    { key: 'capacity', label: 'Capacity' },
    { key: 'activeAssignments', label: 'Occupied', render: r => `${r.activeAssignments} / ${r.capacity}` },
  { key: 'availableFrom', label: 'Available From', render: r => formatDate(r.availableFrom) },
  { key: 'availableTo', label: 'Available To', render: r => formatDate(r.availableTo) }
]

function assignmentColumns(withActions, endPlacement, viewReports) {
  const columns = [
    { key: 'animalName', label: 'Animal' },
    { key: 'fosterName', label: 'Foster' },
    { key: 'startDate', label: 'Start Date', render: r => formatDate(r.startDate) },
    { key: 'status', label: 'Status' }
  ]
  if (withActions) {
    columns.push({ key: 'actions', label: 'Actions', render: r => <div className="flex flex-wrap gap-2"><Button variant="secondary" onClick={() => viewReports(r.id)}>View Reports</Button><Button variant="secondary" onClick={() => endPlacement(r.id)}>End Placement</Button></div> })
  }
  return columns
}

function ReportsTable({ reports, assignmentId = '', onClear }) {
  const [selectedReport, setSelectedReport] = useState(null)
  const visibleReports = assignmentId ? reports.filter(report => report.fosterAssignmentId === assignmentId) : reports
  const columns = [
    { key: 'animalName', label: 'Animal' },
    { key: 'fosterName', label: 'Foster' },
    { key: 'reportDate', label: 'Report Date', render: r => formatDate(r.reportDate) },
    { key: 'actions', label: 'Actions', render: r => <Button variant="secondary" onClick={() => setSelectedReport(r)}>View details</Button> },
  ]

  return <section className="grid gap-3">
    <div className="flex items-center justify-between gap-3">
      <h2 className="font-bold">Foster reports</h2>
      {assignmentId && <Button variant="secondary" onClick={onClear}>Show all reports</Button>}
    </div>
    {visibleReports.length ? <DataTable rows={visibleReports} columns={columns} /> : <EmptyText>No foster reports yet.</EmptyText>}
    {selectedReport && <FosterReportModal report={selectedReport} onClose={() => setSelectedReport(null)} />}
  </section>
}

function FosterReportModal({ report, onClose }) {
  return <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/40 p-4">
    <section className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-md bg-white p-5 shadow-xl">
      <div className="flex items-start justify-between gap-4 border-b border-slate-200 pb-3">
        <div className="flex items-center gap-4">
          {report.photoUrl
            ? <img className="h-20 w-20 shrink-0 rounded-md border border-slate-200 object-cover" src={absoluteUrl(report.photoUrl)} alt={`Foster report for ${report.animalName}`} />
            : <div className="grid h-20 w-20 shrink-0 place-items-center rounded-md border border-dashed border-slate-300 bg-slate-50 text-xs text-slate-500">No photo</div>}
          <div>
            <h2 className="text-xl font-bold text-slate-950">{report.animalName}</h2>
            <p className="mt-1 text-sm text-slate-600">Foster report · {formatDate(report.reportDate)}</p>
            <p className="mt-1 text-sm text-slate-600">Foster: {report.fosterName}</p>
            <p className="mt-1 text-sm text-slate-600">Shelter: {report.shelterName || '-'}</p>
          </div>
        </div>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <div className="mt-5 grid gap-4">
        <ReportNotes label="Behavior notes" value={report.behaviorNotes} />
        <ReportNotes label="Progress notes" value={report.progressNotes} />
      </div>
    </section>
  </div>
}

function ReportNotes({ label, value }) {
  return <section className="rounded-md border border-slate-200 p-4">
    <h3 className="font-semibold text-slate-950">{label}</h3>
    <p className="mt-2 whitespace-pre-wrap text-sm text-slate-700">{value || '-'}</p>
  </section>
}

function absoluteUrl(url) {
  if (!url || /^https?:\/\//i.test(url)) return url
  const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7298/api/v1'
  return `${apiBase.replace(/\/api\/v1\/?$/, '')}${url}`
}
