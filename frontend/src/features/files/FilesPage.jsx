import { useState } from 'react'
import { animalsApi, filesApi, profileApi, sheltersApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { animalLabel, shelterLabel } from '../../shared/ui/SelectOptions'
import { DataTable } from '../../shared/ui/DataTable'
import { ErrorState } from '../../shared/ui/State'

export function FilesPage() {
  const auth = useAuth()
  const [file, setFile] = useState(null)
  const [asset, setAsset] = useState(null)
  const [animalPhoto, setAnimalPhoto] = useState({ animalId:'', url:'', isMain:false })
  const [shelterPhoto, setShelterPhoto] = useState({ shelterId:'', url:'' })
  const [error, setError] = useState(null)
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const { data: uploadedFiles, reload } = useApi(() => profile?.userId ? filesApi.list({ uploadedByUserId: profile.userId }) : Promise.resolve({ data: [] }), [profile?.userId])
  const { data: animals } = useApi(() => animalsApi.list({}), [])
  const { data: shelters } = useApi(() => sheltersApi.list({}), [])
  const upload = async e => { e.preventDefault(); setError(null); try { setAsset((await filesApi.upload(file, profile?.userId)).data); reload() } catch(err){setError(err)} }
  const addAnimal = async e => { e.preventDefault(); await filesApi.animalPhoto(animalPhoto.animalId, { url: animalPhoto.url, isMain: animalPhoto.isMain }) }
  const addShelter = async e => { e.preventDefault(); await filesApi.shelterPhoto(shelterPhoto.shelterId, { url: shelterPhoto.url }) }
  return <div className="grid gap-6"><PageHeader title="Files & media" description={auth.hasRole('Shelter') ? 'Upload verification documents and manage shelter media.' : 'Upload files and attach gallery URLs to animals or shelters.'} /><ErrorState error={error}/><form onSubmit={upload} className="rounded-md border border-slate-200 bg-white p-4"><h2 className="font-bold">{auth.hasRole('Shelter') ? 'Upload verification document' : 'Upload file'}</h2>{auth.hasRole('Shelter') && <p className="mt-1 text-sm text-slate-600">Upload registration, permit or other shelter documents. Admin will review them from the verification panel.</p>}<div className="mt-3 grid gap-3 md:grid-cols-3"><Field label="File"><input className={inputClass()} type="file" onChange={e=>setFile(e.target.files?.[0])}/></Field><Field label="Uploaded by"><input className={inputClass()} value={profile?.email ?? 'Current user'} disabled /></Field><div className="self-end"><Button disabled={!file}>Upload</Button></div></div>{asset && <p className="mt-3 text-sm text-brand-700">Uploaded: {asset.url}</p>}</form>{auth.hasRole('Shelter') && <section><h2 className="mb-3 font-bold">My verification documents</h2><DataTable rows={uploadedFiles ?? []} columns={[{key:'originalFileName',label:'File'}, {key:'contentType',label:'Type'}, {key:'sizeBytes',label:'Size',render:r=>`${Math.round(r.sizeBytes / 1024)} KB`}, {key:'url',label:'Open',render:r=><a className="text-brand-700 underline" href={absoluteUrl(r.url)} target="_blank" rel="noreferrer">View</a>}]} /></section>}<section className="grid gap-4 md:grid-cols-2"><form onSubmit={addAnimal} className="rounded-md border border-slate-200 bg-white p-4"><h2 className="font-bold">Animal photo</h2><div className="mt-3 grid gap-3"><Field label="Animal"><select className={inputClass()} value={animalPhoto.animalId} onChange={e=>setAnimalPhoto({...animalPhoto,animalId:e.target.value})}><option value="">Choose animal</option>{(animals ?? []).map(a => <option key={a.id} value={a.id}>{animalLabel(a)}</option>)}</select></Field><Field label="Url"><input className={inputClass()} value={animalPhoto.url} onChange={e=>setAnimalPhoto({...animalPhoto,url:e.target.value})}/></Field><label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={animalPhoto.isMain} onChange={e=>setAnimalPhoto({...animalPhoto,isMain:e.target.checked})}/> Main photo</label><Button>Attach</Button></div></form><form onSubmit={addShelter} className="rounded-md border border-slate-200 bg-white p-4"><h2 className="font-bold">Shelter photo</h2><div className="mt-3 grid gap-3"><Field label="Shelter"><select className={inputClass()} value={shelterPhoto.shelterId} onChange={e=>setShelterPhoto({...shelterPhoto,shelterId:e.target.value})}><option value="">Choose shelter</option>{(shelters ?? []).map(s => <option key={s.id} value={s.id}>{shelterLabel(s)}</option>)}</select></Field><Field label="Url"><input className={inputClass()} value={shelterPhoto.url} onChange={e=>setShelterPhoto({...shelterPhoto,url:e.target.value})}/></Field><Button>Attach</Button></div></form></section></div>
}

function absoluteUrl(url) {
  if (!url || url.startsWith('http')) return url
  const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001/api/v1'
  return `${apiBase.replace('/api/v1', '')}${url}`
}
