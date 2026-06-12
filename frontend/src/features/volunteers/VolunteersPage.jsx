import { useMemo, useState } from 'react'
import { profileApi, sheltersApi, volunteersApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { shelterLabel } from '../../shared/ui/SelectOptions'

const empty = Promise.resolve({ data: [] })

const applicationColumns = [
  { key: 'shelterName', label: 'Shelter' },
  { key: 'preferredActivities', label: 'Preferred Activities' },
  { key: 'availability', label: 'Availability' },
  { key: 'status', label: 'Status', render: r => <StatusBadge status={statusOf(r)} /> },
  { key: 'createdAt', label: 'Created At', render: r => formatDate(r.createdAt) }
]

function formatDate(value) {
  if (!value) return '-'
  return new Date(value).toLocaleDateString()
}

function statusOf(application) {
  if (application.status) return application.status
  return application.isApproved ? 'Approved' : 'Submitted'
}

function StatusBadge({ status }) {
  const styles = {
    Submitted: 'border-slate-200 bg-slate-100 text-slate-700',
    Approved: 'border-brand-200 bg-brand-50 text-brand-700',
    Rejected: 'border-red-200 bg-red-50 text-red-700'
  }

  return (
    <span className={`inline-flex rounded-full border px-2 py-1 text-xs font-semibold ${styles[status] ?? styles.Submitted}`}>
      {status}
    </span>
  )
}

function StatCard({ label, value }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-4">
      <div className="text-sm text-slate-500">{label}</div>
      <div className="mt-2 text-2xl font-bold text-slate-900">{value}</div>
    </div>
  )
}

export function VolunteersPage() {
  const auth = useAuth()
  const isAdopter = auth.hasRole('Adopter')
  const isFoster = auth.hasRole('Foster')
  const isShelter = auth.hasRole('Shelter')
  const isAdmin = auth.hasRole('Admin')
  const shouldLoadApplications = isAdopter || isShelter || isAdmin
  const { data: applications, reload } = useApi(() => shouldLoadApplications ? volunteersApi.list({}) : empty, [shouldLoadApplications])
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const { data: shelters } = useApi(() => isAdopter ? sheltersApi.list({}) : empty, [isAdopter])
  const [form, setForm] = useState({ shelterProfileId: '', preferredActivities: '', availability: '', motivation: '' })
  const [errors, setErrors] = useState({})
  const [message, setMessage] = useState('')

  const adminStats = useMemo(() => {
    const rows = applications ?? []
    return {
      total: rows.length,
      pending: rows.filter(x => statusOf(x) === 'Submitted').length,
      approved: rows.filter(x => statusOf(x) === 'Approved').length,
      rejected: rows.filter(x => statusOf(x) === 'Rejected').length
    }
  }, [applications])

  const validate = () => {
    const nextErrors = {}
    if (!form.shelterProfileId) nextErrors.shelterProfileId = 'Shelter is required.'
    if (!form.preferredActivities.trim()) nextErrors.preferredActivities = 'Preferred activities are required.'
    if (!form.availability.trim()) nextErrors.availability = 'Availability is required.'
    if (!form.motivation.trim()) nextErrors.motivation = 'Motivation is required.'
    setErrors(nextErrors)
    return Object.keys(nextErrors).length === 0
  }

  const submit = async e => {
    e.preventDefault()
    setMessage('')
    if (!validate()) return

    await volunteersApi.submit(form)
    setForm({ shelterProfileId: '', preferredActivities: '', availability: '', motivation: '' })
    setErrors({})
    setMessage('Volunteer application submitted.')
    reload()
  }

  const review = async (id, isApproved) => {
    await volunteersApi.approve(id, { applicationId: id, isApproved })
    reload()
  }

  const renderShelterActions = application => {
    if (statusOf(application) !== 'Submitted') {
      return <span className="text-slate-500">-</span>
    }

    return (
      <div className="flex flex-wrap gap-2">
        <Button variant="secondary" onClick={() => review(application.id, true)}>Approve</Button>
        <Button variant="secondary" onClick={() => review(application.id, false)}>Reject</Button>
      </div>
    )
  }

  if (isFoster) {
    return (
      <div className="grid gap-6">
        <PageHeader title="Volunteers" description="Applications, activities, schedules and shelter approval." />
        <div className="rounded-md border border-slate-200 bg-white p-4 text-slate-700">
          Volunteer applications are available only for adopters.
        </div>
      </div>
    )
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Volunteers" description="Applications, activities, schedules and shelter approval." />

      {message && <div className="rounded-md bg-brand-50 p-3 text-sm text-brand-700">{message}</div>}

      {isAdopter && (
        <>
          <form onSubmit={submit} className="rounded-md border border-slate-200 bg-white p-4">
            <h2 className="font-bold">Volunteer application</h2>
            <div className="mt-3 grid gap-3 md:grid-cols-2">
              <Field label="Applicant email">
                <input className={inputClass()} value={profile?.email ?? ''} readOnly />
              </Field>
              <Field label="Shelter">
                <select className={inputClass()} value={form.shelterProfileId} onChange={e => setForm({ ...form, shelterProfileId: e.target.value })}>
                  <option value="">Choose shelter</option>
                  {(shelters ?? []).map(s => <option key={s.id} value={s.id}>{shelterLabel(s)}</option>)}
                </select>
                {errors.shelterProfileId && <p className="mt-1 text-sm text-red-600">{errors.shelterProfileId}</p>}
              </Field>
              <Field label="Preferred activities">
                <input className={inputClass()} value={form.preferredActivities} onChange={e => setForm({ ...form, preferredActivities: e.target.value })} />
                {errors.preferredActivities && <p className="mt-1 text-sm text-red-600">{errors.preferredActivities}</p>}
              </Field>
              <Field label="Availability">
                <input className={inputClass()} value={form.availability} onChange={e => setForm({ ...form, availability: e.target.value })} />
                {errors.availability && <p className="mt-1 text-sm text-red-600">{errors.availability}</p>}
              </Field>
              <Field label="Motivation">
                <textarea className={inputClass()} value={form.motivation} onChange={e => setForm({ ...form, motivation: e.target.value })} />
                {errors.motivation && <p className="mt-1 text-sm text-red-600">{errors.motivation}</p>}
              </Field>
              <div className="md:col-span-2"><Button>Submit</Button></div>
            </div>
          </form>

          <section className="grid gap-3">
            <h2 className="font-bold">My Volunteer Applications</h2>
            <DataTable rows={applications ?? []} columns={applicationColumns} />
          </section>
        </>
      )}

      {isShelter && (
        <section className="grid gap-3">
          <h2 className="font-bold">Volunteer applications for your shelter</h2>
          <DataTable
            rows={applications ?? []}
            columns={[
              { key: 'applicantEmail', label: 'Applicant' },
              { key: 'preferredActivities', label: 'Preferred Activities' },
              { key: 'availability', label: 'Availability' },
              { key: 'motivation', label: 'Motivation' },
              { key: 'status', label: 'Status', render: r => <StatusBadge status={statusOf(r)} /> },
              {
                key: 'actions',
                label: 'Actions',
                render: renderShelterActions
              }
            ]}
          />
        </section>
      )}

      {isAdmin && (
        <section className="grid gap-4">
          <div className="grid gap-4 md:grid-cols-4">
            <StatCard label="Total Applications" value={adminStats.total} />
            <StatCard label="Pending Applications" value={adminStats.pending} />
            <StatCard label="Approved Applications" value={adminStats.approved} />
            <StatCard label="Rejected Applications" value={adminStats.rejected} />
          </div>
          <h2 className="font-bold">All volunteer applications</h2>
          <DataTable
            rows={applications ?? []}
            columns={[
              { key: 'applicantEmail', label: 'Applicant' },
              { key: 'shelterName', label: 'Shelter' },
              { key: 'preferredActivities', label: 'Preferred Activities' },
              { key: 'availability', label: 'Availability' },
              { key: 'status', label: 'Status', render: r => <StatusBadge status={statusOf(r)} /> },
              { key: 'createdAt', label: 'Created At', render: r => formatDate(r.createdAt) }
            ]}
          />
        </section>
      )}
    </div>
  )
}
