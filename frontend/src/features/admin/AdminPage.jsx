import { useMemo, useState } from 'react'
import { adminApi, filesApi, sheltersApi } from '../../shared/api/endpoints'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { PageHeader } from '../../shared/ui/PageHeader'
import { ErrorState, LoadingState } from '../../shared/ui/State'

export function AdminPage() {
  const { data, loading, error } = useApi(() => adminApi.statistics(), [])
  const { data: shelters, reload: reloadShelters } = useApi(() => sheltersApi.list({}), [])
  const { data: files } = useApi(() => filesApi.list({}), [])
  const [verificationError, setVerificationError] = useState(null)
  const [verificationMessage, setVerificationMessage] = useState('')

  const filesByUser = useMemo(() => groupFilesByUser(files ?? []), [files])

  const verifyShelter = async (shelter, isVerified) => {
    setVerificationError(null)
    setVerificationMessage('')
    try {
      await sheltersApi.verify(shelter.id, { shelterId: shelter.id, isVerified })
      setVerificationMessage(isVerified ? `${shelter.name} is verified.` : `${shelter.name} verification was rejected.`)
      reloadShelters()
    } catch (err) {
      setVerificationError(err)
    }
  }

  return <div className="grid gap-6">
    <PageHeader title="Admin" description="Review platform statistics and approve shelter verification requests." />
    {loading ? <LoadingState /> : <>
      <ErrorState error={error} />
      <div className="grid gap-3 md:grid-cols-3 xl:grid-cols-5">{Object.entries(data ?? {}).map(([k, v]) => <div key={k} className="rounded-md border border-slate-200 bg-white p-4"><div className="text-xs font-semibold uppercase text-slate-500">{formatStatLabel(k)}</div><div className="mt-2 text-2xl font-bold text-slate-950">{v}</div></div>)}</div>
    </>}

    <section className="rounded-md border border-slate-200 bg-white p-4">
      <h2 className="font-bold">Shelter verification requests</h2>
      <p className="mt-1 text-sm text-slate-600">Review shelter profile data and uploaded documents before approving verification.</p>
      {verificationMessage && <div className="mt-3 rounded-md bg-brand-50 p-3 text-sm text-brand-700">{verificationMessage}</div>}
      <div className="mt-3"><ErrorState error={verificationError} /></div>
      <div className="mt-4">
        <DataTable rows={shelters ?? []} columns={[
          { key: 'name', label: 'Shelter' },
          { key: 'location', label: 'Location' },
          { key: 'contactPhone', label: 'Contact' },
          { key: 'documents', label: 'Documents', render: shelter => <DocumentLinks files={filesByUser[shelter.userId] ?? []} /> },
          { key: 'isVerified', label: 'Status', render: shelter => <VerificationBadge verified={shelter.isVerified} /> },
          { key: 'actions', label: 'Actions', render: shelter => <div className="flex flex-wrap gap-2">
            {!shelter.isVerified && <Button variant="secondary" onClick={() => verifyShelter(shelter, true)}>Approve</Button>}
            {!shelter.isVerified && <Button variant="secondary" onClick={() => verifyShelter(shelter, false)}>Reject</Button>}
            {shelter.isVerified && <span className="text-slate-500">-</span>}
          </div> },
        ]} />
      </div>
    </section>
  </div>
}

function groupFilesByUser(files) {
  return files.filter(isVerificationDocument).reduce((result, file) => {
    if (!file.uploadedByUserId) return result
    result[file.uploadedByUserId] = [...(result[file.uploadedByUserId] ?? []), file]
    return result
  }, {})
}

function isVerificationDocument(file) {
  return !file.contentType?.toLowerCase().startsWith('image/')
}

function DocumentLinks({ files }) {
  if (!files.length) return <span className="text-slate-500">No documents uploaded</span>
  return <div className="grid gap-1">{files.slice(0, 3).map(file => <a key={file.id} className="text-brand-700 underline" href={absoluteUrl(file.url)} target="_blank" rel="noreferrer">{file.originalFileName}</a>)}</div>
}

function VerificationBadge({ verified }) {
  const className = verified ? 'bg-brand-50 text-brand-700' : 'bg-amber-50 text-amber-700'
  return <span className={`rounded-full px-2 py-1 text-xs font-semibold ${className}`}>{verified ? 'Verified' : 'Pending verification'}</span>
}

function formatStatLabel(key) {
  return key
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2')
    .replace(/^./, char => char.toUpperCase())
}

function absoluteUrl(url) {
  if (!url || url.startsWith('http')) return url
  const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001/api/v1'
  return `${apiBase.replace('/api/v1', '')}${url}`
}
