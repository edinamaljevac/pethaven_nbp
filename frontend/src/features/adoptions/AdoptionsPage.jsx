import { useState } from 'react'
import { adoptionsApi, animalsApi, filesApi, profileApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { adoptionStatuses } from '../../shared/constants/enums'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { animalLabel } from '../../shared/ui/SelectOptions'
import { ErrorState, LoadingState } from '../../shared/ui/State'

const empty = Promise.resolve({ data: [] })
const reportTypes = ['30-day report', '90-day report']

export function AdoptionsPage() {
  const auth = useAuth()
  const isAdopter = auth.hasRole('Adopter')
  const isShelter = auth.hasRole('Shelter')
  const isAdmin = auth.hasRole('Admin')
  const { data, loading, error, reload } = useApi(() => adoptionsApi.list({}), [auth.role])
  const { data: reports, reload: reloadReports } = useApi(() => (isAdopter || isShelter || isAdmin) ? adoptionsApi.reports({}) : empty, [isAdopter, isShelter, isAdmin])
  const { data: animals, reload: reloadAnimals } = useApi(() => isAdopter ? animalsApi.list({ status: 0 }) : empty, [isAdopter])
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const [form, setForm] = useState({ animalId: '', notes: '' })
  const [reportForm, setReportForm] = useState({ reportId: '', reportText: '' })
  const [reportFile, setReportFile] = useState(null)
  const [selectedReport, setSelectedReport] = useState(null)
  const [selectedApplication, setSelectedApplication] = useState(null)
  const [scheduleRequest, setScheduleRequest] = useState(null)
  const [scheduledAt, setScheduledAt] = useState('')
  const [message, setMessage] = useState('')
  const [saveError, setSaveError] = useState(null)

  const submit = async event => {
    event.preventDefault()
    setSaveError(null)
    setMessage('')
    if (!form.animalId) return setSaveError({ userMessage: 'Please choose an animal.' })
    try {
      await adoptionsApi.submit(form)
      setForm({ animalId: '', notes: '' })
      setMessage('Application submitted.')
      reload()
      reloadAnimals()
    } catch (err) {
      setSaveError(err)
    }
  }

  const updateStatus = async (applicationId, status, appointment = null) => {
    setSaveError(null)
    setMessage('')
    try {
      await adoptionsApi.status(applicationId, { status, notes: '', scheduledAt: appointment ? new Date(appointment).toISOString() : null })
      setMessage('Application status updated.')
      reload()
      reloadAnimals()
      setScheduleRequest(null)
      setScheduledAt('')
    } catch (err) {
      setSaveError(err)
    }
  }

  const contract = async id => {
    const res = await adoptionsApi.contract(id)
    setMessage(`Contract generated: ${absoluteUrl(res.data.pdfUrl)}`)
  }

  const viewContract = async id => {
    setSaveError(null)
    setMessage('')
    try {
      const res = await adoptionsApi.getContract(id)
      window.open(absoluteUrl(res.data.pdfUrl), '_blank', 'noopener,noreferrer')
    } catch (err) {
      setSaveError(err)
    }
  }

  const requestReports = async id => {
    await adoptionsApi.requestReports(id)
    reloadReports()
    setMessage('30/90 post-adoption reports requested.')
  }

  const submitReport = async event => {
    event.preventDefault()
    setSaveError(null)
    setMessage('')
    if (!reportForm.reportId) return setSaveError({ userMessage: 'Please choose a report.' })
    if (!reportForm.reportText.trim()) return setSaveError({ userMessage: 'Report text is required.' })

    try {
      let photoUrl = ''
      if (reportFile) {
        const uploadResponse = await filesApi.upload(reportFile, profile?.userId)
        photoUrl = uploadResponse.data.url
      }

      await adoptionsApi.submitReport(reportForm.reportId, { reportText: reportForm.reportText, photoUrl })
      setReportForm({ reportId: '', reportText: '' })
      setReportFile(null)
      setMessage('Post-adoption report submitted.')
      reloadReports()
    } catch (err) {
      setSaveError(err)
    }
  }

  return <div className="grid gap-6">
    <PageHeader title="Adoptions" description={description(auth)} />
    {message && <div className="rounded-md bg-brand-50 p-3 text-sm text-brand-700">{message}</div>}
    <ErrorState error={saveError} />

    {isAdopter && <AdopterView
      applications={data ?? []}
      loading={loading}
      error={error}
      animals={animals ?? []}
      profile={profile}
      form={form}
      setForm={setForm}
      submit={submit}
      reports={reports ?? []}
      reportForm={reportForm}
      setReportForm={setReportForm}
      setReportFile={setReportFile}
      submitReport={submitReport}
      viewContract={viewContract}
    />}

    {isShelter && <ShelterView
      applications={data ?? []}
      loading={loading}
      error={error}
      reports={reports ?? []}
      updateStatus={updateStatus}
      setScheduleRequest={setScheduleRequest}
      contract={contract}
      requestReports={requestReports}
      setSelectedReport={setSelectedReport}
    />}

    {isAdmin && <AdminView applications={data ?? []} loading={loading} error={error} reports={reports ?? []} setSelectedApplication={setSelectedApplication} setSelectedReport={setSelectedReport} />}
    {selectedReport && <ReportDetailsModal report={selectedReport} onClose={() => setSelectedReport(null)} />}
    {selectedApplication && <ApplicationDetailsModal application={selectedApplication} reports={(reports ?? []).filter(report => report.adoptionApplicationId === selectedApplication.id)} onViewReport={setSelectedReport} onClose={() => setSelectedApplication(null)} />}
    {scheduleRequest && <ScheduleModal request={scheduleRequest} scheduledAt={scheduledAt} setScheduledAt={setScheduledAt} onConfirm={() => updateStatus(scheduleRequest.applicationId, scheduleRequest.status, scheduledAt)} onClose={() => { setScheduleRequest(null); setScheduledAt('') }} />}
  </div>
}

function AdopterView({ applications, loading, error, animals, profile, form, setForm, submit, reports, reportForm, setReportForm, setReportFile, submitReport, viewContract }) {
  const pendingReports = reports.filter(report => !report.isSubmitted)
  return <>
    <section className="rounded-md border border-slate-200 bg-white p-4">
      <h2 className="font-bold">Submit application</h2>
      <form onSubmit={submit} className="mt-4 grid gap-3">
        <Field label="Animal"><select className={inputClass()} value={form.animalId} onChange={e => setForm({ ...form, animalId: e.target.value })}><option value="">Choose animal</option>{animals.filter(a => a.status === 0).map(a => <option key={a.id} value={a.id}>{animalLabel(a)}</option>)}</select></Field>
        <Field label="Applicant"><input className={inputClass()} value={`${profile?.firstName ?? ''} ${profile?.lastName ?? ''}`.trim() || 'My profile'} disabled /></Field>
        <Field label="Notes"><input className={inputClass()} value={form.notes} onChange={e => setForm({ ...form, notes: e.target.value })} /></Field>
        <Button>Submit</Button>
      </form>
    </section>
    <section>
      <h2 className="mb-3 font-bold">My adoption applications</h2>
      {loading ? <LoadingState /> : <><ErrorState error={error} /><ApplicationsTable rows={applications} mode="adopter" viewContract={viewContract} /></>}
    </section>
    <section className="grid gap-4 md:grid-cols-2">
      <form onSubmit={submitReport} className="rounded-md border border-slate-200 bg-white p-4">
        <h2 className="font-bold">Submit post-adoption report</h2>
        <div className="mt-4 grid gap-3">
          <Field label="Report"><select className={inputClass()} value={reportForm.reportId} onChange={e => setReportForm({ ...reportForm, reportId: e.target.value })}><option value="">Choose report</option>{pendingReports.map(r => <option key={r.id} value={r.id}>{reportLabel(r)}</option>)}</select></Field>
          <Field label="Report text"><textarea className={inputClass()} rows="4" value={reportForm.reportText} onChange={e => setReportForm({ ...reportForm, reportText: e.target.value })} /></Field>
          <Field label="Photo"><input className={inputClass()} type="file" accept="image/*" onChange={e => setReportFile(e.target.files?.[0] ?? null)} /></Field>
          <Button>Submit report</Button>
        </div>
      </form>
      <PostAdoptionReports rows={reports} title="My post-adoption reports" showActions={false} />
    </section>
  </>
}

function ShelterView({ applications, loading, error, reports, updateStatus, setScheduleRequest, contract, requestReports, setSelectedReport }) {
  return <>
    <section>
      <h2 className="mb-3 font-bold">Applications for your shelter</h2>
      {loading ? <LoadingState /> : <><ErrorState error={error} /><ApplicationsTable rows={applications} mode="shelter" updateStatus={updateStatus} setScheduleRequest={setScheduleRequest} contract={contract} requestReports={requestReports} /></>}
    </section>
    <PostAdoptionReports rows={reports} title="Post-adoption reports for your shelter" onView={setSelectedReport} />
  </>
}

function AdminView({ applications, loading, error, reports, setSelectedApplication, setSelectedReport }) {
  const [filters, setFilters] = useState({ search: '', shelter: '', status: '' })
  const filtered = applications.filter(application => {
    const search = filters.search.toLowerCase()
    return (!search || `${animalName(application)} ${application.adopterName} ${application.adopterEmail}`.toLowerCase().includes(search))
      && (!filters.shelter || application.shelterName === filters.shelter)
      && (filters.status === '' || application.status === Number(filters.status))
  })
  const overdueReports = reports.filter(report => !report.isSubmitted && new Date(report.dueDate) < new Date())
  const activeStatuses = [0, 1, 2, 3, 4]
  const cards = [
    ['Total applications', applications.length],
    ['Active applications', applications.filter(application => activeStatuses.includes(application.status)).length],
    ['Approved', applications.filter(application => application.status === 4).length],
    ['Rejected', applications.filter(application => application.status === 5).length],
    ['Adopted', applications.filter(application => application.status === 6).length],
    ['Overdue reports', overdueReports.length],
  ]
  const shelters = [...new Set(applications.map(application => application.shelterName).filter(Boolean))].sort()

  return <>
    <section className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
      {cards.map(([label, value]) => <article key={label} className="rounded-md border border-slate-200 bg-white p-4"><div className="text-xs font-semibold uppercase text-slate-500">{label}</div><div className="mt-2 text-2xl font-bold text-slate-950">{value}</div></article>)}
    </section>
    <section className="rounded-md border border-slate-200 bg-white p-4">
      <h2 className="font-bold">Filter applications</h2>
      <div className="mt-3 grid gap-3 md:grid-cols-3">
        <Field label="Animal or adopter"><input className={inputClass()} value={filters.search} onChange={e => setFilters({ ...filters, search: e.target.value })} /></Field>
        <Field label="Shelter"><select className={inputClass()} value={filters.shelter} onChange={e => setFilters({ ...filters, shelter: e.target.value })}><option value="">All shelters</option>{shelters.map(shelter => <option key={shelter} value={shelter}>{shelter}</option>)}</select></Field>
        <Field label="Status"><select className={inputClass()} value={filters.status} onChange={e => setFilters({ ...filters, status: e.target.value })}><option value="">All statuses</option>{adoptionStatuses.map(status => <option key={status.value} value={status.value}>{status.label}</option>)}</select></Field>
      </div>
    </section>
    <section>
      <h2 className="mb-3 font-bold">All adoption applications</h2>
      {loading ? <LoadingState /> : <><ErrorState error={error} /><AdminApplicationsTable rows={filtered} onView={setSelectedApplication} /></>}
    </section>
    <PostAdoptionReports rows={overdueReports} title="Overdue post-adoption reports" onView={setSelectedReport} />
  </>
}

function AdminApplicationsTable({ rows, onView }) {
  return <DataTable rows={rows} columns={[
    { key: 'animal', label: 'Animal', render: animalName },
    { key: 'adopterName', label: 'Adopter', render: r => r.adopterName || r.adopterEmail || '-' },
    { key: 'shelterName', label: 'Shelter', render: r => r.shelterName || '-' },
    { key: 'status', label: 'Status', render: r => <AdoptionStatusBadge status={r.status} /> },
    { key: 'createdAt', label: 'Submitted', render: r => formatDate(r.createdAt) },
    { key: 'actions', label: 'Actions', render: r => <Button variant="secondary" onClick={() => onView(r)}>View details</Button> },
  ]} />
}

function AdoptionStatusBadge({ status }) {
  const classes = status === 6 ? 'bg-brand-50 text-brand-700' : status === 5 ? 'bg-red-50 text-red-700' : status === 4 ? 'bg-emerald-50 text-emerald-700' : 'bg-blue-50 text-blue-700'
  return <span className={`rounded-full px-2 py-1 text-xs font-semibold ${classes}`}>{adoptionStatuses[status]?.label ?? status}</span>
}

function ApplicationsTable({ rows, mode, updateStatus, setScheduleRequest, contract, requestReports, viewContract }) {
  const columns = [
    { key: 'animal', label: 'Animal', render: r => animalName(r) },
    { key: 'adopterName', label: 'Adopter', render: r => r.adopterName || r.adopterEmail || '-' },
    { key: 'status', label: 'Status', render: r => adoptionStatuses[r.status]?.label ?? r.status },
    { key: 'createdAt', label: 'Submitted', render: r => formatDate(r.createdAt) },
  ]
  if (mode === 'adopter') columns.splice(3, 0, { key: 'appointment', label: 'Appointment', render: appointmentText })

  if (mode === 'admin') {
    columns.splice(2, 0, { key: 'shelterName', label: 'Shelter', render: r => r.shelterName || '-' })
  }

  if (mode === 'shelter') {
    columns.push({
      key: 'actions',
      label: 'Actions',
      render: r => <ShelterApplicationActions application={r} updateStatus={updateStatus} setScheduleRequest={setScheduleRequest} contract={contract} requestReports={requestReports} />,
    })
  }

  if (mode === 'adopter') {
    columns.push({
      key: 'actions',
      label: 'Actions',
      render: r => r.status >= 4 && r.status !== 5
        ? <Button variant="secondary" onClick={() => viewContract(r.id)}>View contract</Button>
        : '-',
    })
  }

  return <DataTable rows={rows} columns={columns} />
}

function ShelterApplicationActions({ application, updateStatus, setScheduleRequest, contract, requestReports }) {
  const nextStatus = {
    0: { value: 1, label: 'Start review' },
    1: { value: 2, label: 'Schedule interview' },
    2: { value: 3, label: 'Schedule home visit' },
  }[application.status]

  if (nextStatus) {
    const needsAppointment = nextStatus.value === 2 || nextStatus.value === 3
    return <Button onClick={() => needsAppointment ? setScheduleRequest({ applicationId: application.id, status: nextStatus.value, label: nextStatus.label }) : updateStatus(application.id, nextStatus.value)}>{nextStatus.label}</Button>
  }

  if (application.status === 3) {
    return <div className="flex flex-wrap gap-2">
      <Button onClick={() => updateStatus(application.id, 4)}>Approve</Button>
      <Button variant="secondary" onClick={() => updateStatus(application.id, 5)}>Reject</Button>
    </div>
  }

  if (application.status === 4) {
    return <div className="flex flex-wrap gap-2">
      <Button variant="secondary" onClick={() => contract(application.id)}>Contract</Button>
      <Button onClick={() => updateStatus(application.id, 6)}>Mark as adopted</Button>
    </div>
  }

  if (application.status === 6) {
    return <div className="flex flex-wrap gap-2">
      <Button variant="secondary" onClick={() => contract(application.id)}>Contract</Button>
      <Button variant="secondary" onClick={() => requestReports(application.id)}>Create reports</Button>
    </div>
  }

  return '-'
}

function appointmentText(application) {
  const value = application.status === 2 ? application.interviewScheduledAt : application.status === 3 ? application.homeVisitScheduledAt : null
  return value ? new Date(value).toLocaleString() : '-'
}

function ScheduleModal({ request, scheduledAt, setScheduledAt, onConfirm, onClose }) {
  return <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/40 p-4">
    <section className="w-full max-w-md rounded-md bg-white p-5 shadow-xl">
      <h2 className="text-xl font-bold">{request.label}</h2>
      <div className="mt-4"><Field label="Date and time"><input className={inputClass()} type="datetime-local" min={new Date().toISOString().slice(0, 16)} value={scheduledAt} onChange={e => setScheduledAt(e.target.value)} /></Field></div>
      <div className="mt-4 flex gap-2"><Button onClick={onConfirm} disabled={!scheduledAt}>Confirm</Button><Button variant="secondary" onClick={onClose}>Cancel</Button></div>
    </section>
  </div>
}

function PostAdoptionReports({ rows, title, onView, showActions = true }) {
  const columns = [
    { key: 'animalName', label: 'Animal', render: r => r.animalName || '-' },
    { key: 'adopterName', label: 'Adopter', render: r => r.adopterName || '-' },
    { key: 'type', label: 'Type', render: r => reportTypes[r.type] ?? r.type },
    { key: 'dueDate', label: 'Due', render: r => formatDate(r.dueDate) },
    { key: 'isSubmitted', label: 'Submitted', render: r => r.isSubmitted ? 'Yes' : 'No' },
  ]

  if (showActions && onView) {
    columns.push({ key: 'actions', label: 'Actions', render: r => r.isSubmitted ? <Button variant="secondary" onClick={() => onView(r)}>View report</Button> : '-' })
  }

  return <section>
    <h2 className="mb-3 font-bold">{title}</h2>
    <DataTable rows={rows} columns={columns} />
  </section>
}

function description(auth) {
  if (auth.hasRole('Adopter')) return 'Submit an adoption application and track your application status.'
  if (auth.hasRole('Shelter')) return 'Review applications for your shelter animals and manage the adoption workflow.'
  return 'Read-only overview of adoption activity across the platform.'
}

function animalName(application) {
  const suffix = application.animalSpecies ? ` - ${application.animalSpecies}` : ''
  return `${application.animalName || 'Animal'}${suffix}`
}

function reportLabel(report) {
  return `${report.animalName || 'Animal'} - ${reportTypes[report.type] ?? report.type} - due ${formatDate(report.dueDate)}`
}

function ReportDetailsModal({ report, onClose }) {
  return <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/40 p-4">
    <section className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-md bg-white p-5 shadow-xl">
      <div className="flex items-start justify-between gap-4 border-b border-slate-200 pb-3">
        <div>
          <h2 className="text-xl font-bold text-slate-950">{reportTypes[report.type] ?? report.type}</h2>
          <p className="text-sm text-slate-600">{report.animalName || 'Animal'} - {report.adopterName || 'Adopter'}</p>
        </div>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      {report.photoUrl ? <img className="mt-4 max-h-80 w-full rounded-md object-cover" src={absoluteUrl(report.photoUrl)} alt="Post-adoption report" /> : <div className="mt-4 rounded-md border border-dashed border-slate-300 p-6 text-center text-sm text-slate-500">No photo uploaded.</div>}
      <dl className="mt-4 grid gap-3 sm:grid-cols-2">
        <Detail label="Due date" value={formatDate(report.dueDate)} />
        <Detail label="Submitted at" value={formatDate(report.submittedAt)} />
        <div className="sm:col-span-2"><Detail label="Report text" value={report.reportText} /></div>
      </dl>
    </section>
  </div>
}

function ApplicationDetailsModal({ application, reports, onViewReport, onClose }) {
  return <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/40 p-4">
    <section className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-md bg-white p-5 shadow-xl">
      <div className="flex items-start justify-between gap-4 border-b border-slate-200 pb-3">
        <div><h2 className="text-xl font-bold">{animalName(application)}</h2><p className="mt-1 text-sm text-slate-600">Read-only adoption application details</p></div>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <dl className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <Detail label="Adopter" value={application.adopterName || application.adopterEmail} />
        <Detail label="Shelter" value={application.shelterName} />
        <Detail label="Status" value={adoptionStatuses[application.status]?.label ?? application.status} />
        <Detail label="Submitted" value={formatDate(application.createdAt)} />
        <div className="sm:col-span-2"><Detail label="Notes" value={application.notes} /></div>
      </dl>
      <div className="mt-5 border-t border-slate-200 pt-4">
        <h3 className="font-bold">Post-adoption reports</h3>
        <div className="mt-3"><PostAdoptionReports rows={reports} title="" onView={onViewReport} /></div>
      </div>
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
