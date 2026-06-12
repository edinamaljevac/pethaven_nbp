import { useEffect, useMemo, useState } from 'react'
import { Heart, PawPrint } from 'lucide-react'
import { animalsApi, filesApi, profileApi, sheltersApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { animalSizes, animalStatuses, energyLevels } from '../../shared/constants/enums'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { animalLabel } from '../../shared/ui/SelectOptions'
import { shelterLabel } from '../../shared/ui/SelectOptions'
import { ErrorState, LoadingState } from '../../shared/ui/State'

export function AnimalsPage() {
  const auth = useAuth()
  const canManageCare = auth.hasRole('Shelter')
  const isAdmin = auth.hasRole('Admin')
  const isShelter = auth.hasRole('Shelter')
  const canSeeStatusFilter = isAdmin || isShelter
  const [filters, setFilters] = useState({ status: canSeeStatusFilter ? '' : 0, sortBy: 'intakeDateOldest' })
  const [form, setForm] = useState({ name: '', species: '', breed: '', age: 1, gender: 'Female', size: 1, energyLevel: 1, description: '', intakeDate: todayInput(), shelterProfileId: '' })
  const [careAnimalId, setCareAnimalId] = useState('')
  const [selectedAnimal, setSelectedAnimal] = useState(null)
  const [health, setHealth] = useState({ isVaccinated: false, isSterilized: false, isMicrochipped: false, chronicDiseases: '', medications: '' })
  const [behavior, setBehavior] = useState({ energyLevel: 1, goodWithChildren: false, goodWithDogs: false, goodWithCats: false, hasSpecialNeeds: false, behaviorDescription: '' })
  const { data, loading, error, reload } = useApi(() => animalsApi.list(clean(filters)), [JSON.stringify(filters), auth.role])
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const { data: shelters } = useApi(() => auth.hasRole('Admin') ? sheltersApi.list({}) : Promise.resolve({ data: [] }), [auth.role])
  const [saving, setSaving] = useState(false)
  const [saveError, setSaveError] = useState(null)
  const [careError, setCareError] = useState(null)
  const [careMessage, setCareMessage] = useState('')
  const [photoAnimalId, setPhotoAnimalId] = useState('')
  const [photoFiles, setPhotoFiles] = useState([])
  const [mainPhotoIndex, setMainPhotoIndex] = useState(0)
  const [photoSaving, setPhotoSaving] = useState(false)
  const [photoError, setPhotoError] = useState(null)
  const [photoMessage, setPhotoMessage] = useState('')
  const [videoAnimalId, setVideoAnimalId] = useState('')
  const [videoFile, setVideoFile] = useState(null)
  const [videoSaving, setVideoSaving] = useState(false)
  const [videoError, setVideoError] = useState(null)
  const [videoMessage, setVideoMessage] = useState('')
  const spotlightAnimals = useMemo(() => (data ?? []).filter(a => a.isSpecialNeedsSpotlight).slice(0, 3), [data])
  const [showSpotlight, setShowSpotlight] = useState(false)

  useEffect(() => {
    setFilters(current => ({ ...current, status: canSeeStatusFilter ? '' : 0 }))
  }, [canSeeStatusFilter])

  const create = async (event) => {
    event.preventDefault()
    setSaving(true); setSaveError(null)
    try {
      const payload = {
        ...numberPayload(form, ['age', 'size', 'energyLevel']),
        shelterProfileId: auth.hasRole('Shelter') ? profile?.profileId : form.shelterProfileId,
      }
      await animalsApi.create(payload)
      setForm({ ...form, name: '', description: '', intakeDate: todayInput() })
      reload()
    }
    catch (err) { setSaveError(err) } finally { setSaving(false) }
  }

  const saveHealth = async (event) => {
    event.preventDefault()
    setCareError(null)
    try {
      const response = await animalsApi.health(careAnimalId, health)
      await reload()
      setSelectedAnimal((current) => current?.id === careAnimalId ? { ...current, healthRecord: response.data } : current)
      setCareMessage('Health profile saved.')
    } catch (err) { setCareError(err) }
  }

  const saveBehavior = async (event) => {
    event.preventDefault()
    setCareError(null)
    try {
      const response = await animalsApi.behavior(careAnimalId, { ...behavior, energyLevel: Number(behavior.energyLevel) })
      await reload()
      setSelectedAnimal((current) => current?.id === careAnimalId ? { ...current, behaviorProfile: response.data } : current)
      setCareMessage('Behavior profile saved.')
    } catch (err) { setCareError(err) }
  }

  const uploadPhotos = async event => {
    event.preventDefault()
    setPhotoError(null)
    setPhotoMessage('')
    if (!photoAnimalId) return setPhotoError({ userMessage: 'Please choose an animal.' })
    if (!photoFiles.length) return setPhotoError({ userMessage: 'Please choose at least one image.' })

    setPhotoSaving(true)
    try {
      const uploadedPhotos = []
      for (const [index, file] of photoFiles.entries()) {
        const uploaded = await filesApi.upload(file, profile?.userId)
        const response = await filesApi.animalPhoto(photoAnimalId, { url: uploaded.data.url, isMain: index === mainPhotoIndex })
        uploadedPhotos.push(response.data)
      }
      setPhotoFiles([])
      setMainPhotoIndex(0)
      setPhotoMessage(`${photoFiles.length} ${photoFiles.length === 1 ? 'photo was' : 'photos were'} uploaded.`)
      await reload()
      setSelectedAnimal(current => {
        if (current?.id !== photoAnimalId) return current

        const existingPhotos = current.photos ?? []
        const nextPhotos = [
          ...existingPhotos.map(photo => uploadedPhotos.some(uploaded => uploaded.isMain) ? { ...photo, isMain: false } : photo),
          ...uploadedPhotos,
        ]

        return { ...current, photos: nextPhotos }
      })
    } catch (err) {
      setPhotoError(err)
    } finally {
      setPhotoSaving(false)
    }
  }

  const uploadVideo = async event => {
    event.preventDefault()
    setVideoError(null)
    setVideoMessage('')
    if (!videoAnimalId) return setVideoError({ userMessage: 'Please choose an animal.' })
    if (!videoFile) return setVideoError({ userMessage: 'Please choose a video.' })

    setVideoSaving(true)
    try {
      const uploaded = await filesApi.upload(videoFile, profile?.userId)
      await animalsApi.video(videoAnimalId, { videoUrl: uploaded.data.url })
      setVideoFile(null)
      setVideoMessage('Animal video uploaded.')
      await reload()
      setSelectedAnimal(current => current?.id === videoAnimalId ? { ...current, videoUrl: uploaded.data.url } : current)
    } catch (err) {
      setVideoError(err)
    } finally {
      setVideoSaving(false)
    }
  }

  const columns = [
    { key: 'name', label: 'Name' },
    { key: 'species', label: 'Species' },
    { key: 'breed', label: 'Breed' },
    ...(isAdmin ? [{ key: 'shelterName', label: 'Shelter', render: r => r.shelterName || '-' }] : []),
    { key: 'intakeDate', label: 'Intake date', render: r => formatDate(r.intakeDate) },
    { key: 'age', label: 'Age' },
    { key: 'status', label: 'Status', render: r => animalStatuses[r.status]?.label ?? r.status },
    { key: 'actions', label: 'Actions', render: r => <Button variant="secondary" onClick={() => setSelectedAnimal(r)}>View profile</Button> },
  ]

  return (
    <div className="grid gap-6">
      <PageHeader title="Animals" description={description(auth)} />
      {!loading && <SpecialNeedsSpotlight
        animals={spotlightAnimals}
        allAnimals={data ?? []}
        isOpen={showSpotlight}
        onToggle={() => setShowSpotlight(current => !current)}
        onView={setSelectedAnimal}
      />}
      <section className="rounded-md border border-slate-200 bg-white p-4">
        <div className={`grid gap-3 ${canSeeStatusFilter ? 'md:grid-cols-6' : 'md:grid-cols-5'}`}>
          <Field label="Species"><input className={inputClass()} value={filters.species ?? ''} onChange={(e) => setFilters({ ...filters, species: e.target.value })} /></Field>
          <Field label="Breed"><input className={inputClass()} value={filters.breed ?? ''} onChange={(e) => setFilters({ ...filters, breed: e.target.value })} /></Field>
          <Field label="Size"><select className={inputClass()} value={filters.size ?? ''} onChange={(e) => setFilters({ ...filters, size: e.target.value })}><option value="">Any</option>{animalSizes.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>
          <Field label="Energy"><select className={inputClass()} value={filters.energyLevel ?? ''} onChange={(e) => setFilters({ ...filters, energyLevel: e.target.value })}><option value="">Any</option>{energyLevels.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>
          {canSeeStatusFilter && <Field label="Status"><select className={inputClass()} value={filters.status ?? ''} onChange={(e) => setFilters({ ...filters, status: e.target.value })}><option value="">Any</option>{animalStatuses.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>}
          <Field label="Sort by"><select className={inputClass()} value={filters.sortBy ?? ''} onChange={(e) => setFilters({ ...filters, sortBy: e.target.value })}><option value="intakeDateOldest">Longest in shelter first</option><option value="ageYoungest">Age: youngest first</option><option value="ageOldest">Age: oldest first</option></select></Field>
        </div>
      </section>
      {loading ? <LoadingState /> : <><ErrorState error={error} /><DataTable rows={data ?? []} columns={columns} /></>}
      {canManageCare && <section className="rounded-md border border-slate-200 bg-white p-4">
        <h2 className="font-bold text-slate-950">Create animal</h2>
        <form onSubmit={create} className="mt-4 grid gap-3 md:grid-cols-4">
          <div className="md:col-span-4"><ErrorState error={saveError} /></div>
          {['name','species','breed','gender'].map(k => <Field key={k} label={label(k)}><input className={inputClass()} value={form[k]} onChange={(e) => setForm({ ...form, [k]: e.target.value })} /></Field>)}
          {auth.hasRole('Admin') && <Field label="Shelter"><select className={inputClass()} value={form.shelterProfileId} onChange={(e) => setForm({ ...form, shelterProfileId: e.target.value })}><option value="">Choose shelter</option>{(shelters ?? []).map(s => <option key={s.id} value={s.id}>{shelterLabel(s)}</option>)}</select></Field>}
          {auth.hasRole('Shelter') && <Field label="Shelter"><input className={inputClass()} value={profile?.shelterName ?? 'My shelter'} disabled /></Field>}
          <Field label="Intake date"><input className={inputClass()} type="date" value={form.intakeDate} onChange={(e) => setForm({ ...form, intakeDate: e.target.value })} /></Field>
          <Field label="Age"><input className={inputClass()} type="number" value={form.age} onChange={(e) => setForm({ ...form, age: e.target.value })} /></Field>
          <Field label="Size"><select className={inputClass()} value={form.size} onChange={(e) => setForm({ ...form, size: e.target.value })}>{animalSizes.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>
          <Field label="Energy"><select className={inputClass()} value={form.energyLevel} onChange={(e) => setForm({ ...form, energyLevel: e.target.value })}>{energyLevels.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>
          <Field label="Description"><textarea className={inputClass()} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></Field>
          <div className="md:col-span-4"><Button disabled={saving}>{saving ? 'Creating...' : 'Create animal'}</Button></div>
        </form>
      </section>}
      {canManageCare && <section className="rounded-md border border-slate-200 bg-white p-4">
        <h2 className="font-bold text-slate-950">Animal health and behavior</h2>
        <div className="mt-4 grid gap-4">
          <ErrorState error={careError} />
          {careMessage && <div className="rounded-md bg-brand-50 p-3 text-sm text-brand-700">{careMessage}</div>}
          <Field label="Animal">
            <select className={inputClass()} value={careAnimalId} onChange={(e) => setCareAnimalId(e.target.value)}>
              <option value="">Choose animal</option>
              {(data ?? []).map(a => <option key={a.id} value={a.id}>{animalLabel(a)}</option>)}
            </select>
          </Field>
          <div className="grid gap-4 lg:grid-cols-2">
            <form onSubmit={saveHealth} className="rounded-md border border-slate-200 p-4">
              <h3 className="font-semibold text-slate-950">Health profile</h3>
              <div className="mt-3 grid gap-3">
                <Checkbox label="Vaccinated" checked={health.isVaccinated} onChange={(value) => setHealth({ ...health, isVaccinated: value })} />
                <Checkbox label="Sterilized" checked={health.isSterilized} onChange={(value) => setHealth({ ...health, isSterilized: value })} />
                <Checkbox label="Microchipped" checked={health.isMicrochipped} onChange={(value) => setHealth({ ...health, isMicrochipped: value })} />
                <Field label="Chronic diseases"><input className={inputClass()} value={health.chronicDiseases} onChange={(e) => setHealth({ ...health, chronicDiseases: e.target.value })} /></Field>
                <Field label="Medications"><input className={inputClass()} value={health.medications} onChange={(e) => setHealth({ ...health, medications: e.target.value })} /></Field>
                <Button disabled={!careAnimalId}>Save health</Button>
              </div>
            </form>
            <form onSubmit={saveBehavior} className="rounded-md border border-slate-200 p-4">
              <h3 className="font-semibold text-slate-950">Behavior profile</h3>
              <div className="mt-3 grid gap-3">
                <Field label="Energy"><select className={inputClass()} value={behavior.energyLevel} onChange={(e) => setBehavior({ ...behavior, energyLevel: e.target.value })}>{energyLevels.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select></Field>
                <Checkbox label="Good with children" checked={behavior.goodWithChildren} onChange={(value) => setBehavior({ ...behavior, goodWithChildren: value })} />
                <Checkbox label="Good with dogs" checked={behavior.goodWithDogs} onChange={(value) => setBehavior({ ...behavior, goodWithDogs: value })} />
                <Checkbox label="Good with cats" checked={behavior.goodWithCats} onChange={(value) => setBehavior({ ...behavior, goodWithCats: value })} />
                <Checkbox label="Has special needs" checked={behavior.hasSpecialNeeds} onChange={(value) => setBehavior({ ...behavior, hasSpecialNeeds: value })} />
                <Field label="Behavior description"><textarea className={inputClass()} value={behavior.behaviorDescription} onChange={(e) => setBehavior({ ...behavior, behaviorDescription: e.target.value })} /></Field>
                <Button disabled={!careAnimalId}>Save behavior</Button>
              </div>
            </form>
          </div>
        </div>
      </section>}
      {canManageCare && <section className="rounded-md border border-slate-200 bg-white p-4">
        <h2 className="font-bold text-slate-950">Animal photos</h2>
        <p className="mt-1 text-sm text-slate-600">Upload multiple photos and choose which one should be the main profile photo.</p>
        <form onSubmit={uploadPhotos} className="mt-4 grid gap-4">
          <ErrorState error={photoError} />
          {photoMessage && <div className="rounded-md bg-brand-50 p-3 text-sm text-brand-700">{photoMessage}</div>}
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Animal"><select className={inputClass()} value={photoAnimalId} onChange={e => setPhotoAnimalId(e.target.value)}><option value="">Choose animal</option>{(data ?? []).map(animal => <option key={animal.id} value={animal.id}>{animalLabel(animal)}</option>)}</select></Field>
            <Field label="Images"><input className={inputClass()} type="file" accept="image/*" multiple onChange={e => { setPhotoFiles(Array.from(e.target.files ?? [])); setMainPhotoIndex(0) }} /></Field>
          </div>
          {photoFiles.length > 0 && <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {photoFiles.map((file, index) => <label key={`${file.name}-${index}`} className={`cursor-pointer overflow-hidden rounded-md border bg-white ${mainPhotoIndex === index ? 'border-brand-500 ring-2 ring-brand-100' : 'border-slate-200'}`}>
              <img className="h-32 w-full object-cover" src={URL.createObjectURL(file)} alt={file.name} />
              <span className="flex items-center gap-2 p-3 text-sm">
                <input type="radio" name="mainPhoto" checked={mainPhotoIndex === index} onChange={() => setMainPhotoIndex(index)} />
                <span className="min-w-0 truncate">{file.name}</span>
              </span>
              {mainPhotoIndex === index && <span className="block bg-brand-50 px-3 py-1 text-xs font-semibold text-brand-700">Main photo</span>}
            </label>)}
          </div>}
          <div><Button disabled={photoSaving || !photoAnimalId || !photoFiles.length}>{photoSaving ? 'Uploading...' : 'Upload photos'}</Button></div>
        </form>
      </section>}
      {canManageCare && <section className="rounded-md border border-slate-200 bg-white p-4">
        <h2 className="font-bold text-slate-950">Animal video</h2>
        <p className="mt-1 text-sm text-slate-600">Upload a short video that will be shown in the animal profile.</p>
        <form onSubmit={uploadVideo} className="mt-4 grid gap-4">
          <ErrorState error={videoError} />
          {videoMessage && <div className="rounded-md bg-brand-50 p-3 text-sm text-brand-700">{videoMessage}</div>}
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Animal"><select className={inputClass()} value={videoAnimalId} onChange={e => setVideoAnimalId(e.target.value)}><option value="">Choose animal</option>{(data ?? []).map(animal => <option key={animal.id} value={animal.id}>{animalLabel(animal)}</option>)}</select></Field>
            <Field label="Video"><input className={inputClass()} type="file" accept="video/*" onChange={e => setVideoFile(e.target.files?.[0] ?? null)} /></Field>
          </div>
          <div><Button disabled={videoSaving || !videoAnimalId || !videoFile}>{videoSaving ? 'Uploading...' : 'Upload video'}</Button></div>
        </form>
      </section>}
      {selectedAnimal && <AnimalProfileModal animal={selectedAnimal} onClose={() => setSelectedAnimal(null)} />}
    </div>
  )
}

function clean(obj) { return Object.fromEntries(Object.entries(obj).filter(([, v]) => v !== '' && v !== undefined && v !== null).map(([k, v]) => [k, ['size','energyLevel','status'].includes(k) ? Number(v) : v])) }
function numberPayload(obj, keys) { return Object.fromEntries(Object.entries(obj).map(([k, v]) => [k, keys.includes(k) ? Number(v) : v])) }
function label(k) { return k.replace(/([A-Z])/g, ' $1').replace(/^./, c => c.toUpperCase()) }

function description(auth) {
  if (auth.hasRole('Shelter')) return 'Manage animal listings for your shelter and maintain health and behavior profiles.'
  if (auth.hasRole('Admin')) return 'Review all animal listings across the platform.'
  return 'Browse available animals and view detailed profiles.'
}

function healthSummary(healthRecord) {
  if (!healthRecord) return '-'
  const flags = [
    healthRecord.isVaccinated ? 'vaccinated' : null,
    healthRecord.isSterilized ? 'sterilized' : null,
    healthRecord.isMicrochipped ? 'microchipped' : null,
  ].filter(Boolean)
  const notes = [healthRecord.chronicDiseases, healthRecord.medications].filter(Boolean)
  return [...flags, ...notes].join(', ') || 'No notes'
}

function behaviorSummary(behaviorProfile) {
  if (!behaviorProfile) return '-'
  const flags = [
    behaviorProfile.goodWithChildren ? 'children' : null,
    behaviorProfile.goodWithDogs ? 'dogs' : null,
    behaviorProfile.goodWithCats ? 'cats' : null,
    behaviorProfile.hasSpecialNeeds ? 'special needs' : null,
  ].filter(Boolean)
  const energy = energyLevels[behaviorProfile.energyLevel]?.label
  return [energy, ...flags, behaviorProfile.behaviorDescription].filter(Boolean).join(', ') || 'No notes'
}

function SpecialNeedsSpotlight({ animals, allAnimals, isOpen, onToggle, onView }) {
  const spotlightTotal = allAnimals.filter(a => a.isSpecialNeedsSpotlight).length
  const averageDays = spotlightTotal
    ? Math.round(allAnimals.filter(a => a.isSpecialNeedsSpotlight).reduce((sum, animal) => sum + (animal.daysInShelter ?? 0), 0) / spotlightTotal)
    : 0
  const shelterCount = new Set(allAnimals.filter(a => a.isSpecialNeedsSpotlight).map(a => a.shelterProfileId).filter(Boolean)).size

  return <section className="grid gap-4">
    <div className="rounded-md border border-rose-100 bg-rose-50 p-5">
      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wide text-rose-700"><Heart size={14} /> Special needs</div>
          <h2 className="mt-2 text-2xl font-bold text-rose-950">They wait a little longer</h2>
          <p className="mt-2 max-w-3xl text-sm font-medium text-rose-800">Senior animals, pets with chronic conditions and animals with special needs often wait longer for a home, but they can be the most grateful companions.</p>
          <div className="mt-4">
            <Button type="button" variant="secondary" onClick={onToggle}>{isOpen ? 'Hide special needs animals' : 'View special needs animals'}</Button>
          </div>
        </div>
        <PawPrint className="hidden text-rose-500 sm:block" size={42} />
      </div>
    </div>
    {isOpen && <>
      {spotlightTotal === 0
        ? <div className="rounded-md border border-dashed border-slate-300 bg-white p-6 text-center text-sm text-slate-600">There are currently no animals that match the special needs spotlight criteria.</div>
        : <>
          <div className="grid gap-3 sm:grid-cols-3">
            <SpotlightStat value={spotlightTotal} label="waiting for a home" />
            <SpotlightStat value={averageDays} label="average days" />
            <SpotlightStat value={shelterCount} label="shelters included" />
          </div>
          <div className="grid gap-4 md:grid-cols-3">
            {animals.map(animal => <button key={animal.id} type="button" onClick={() => onView(animal)} className="overflow-hidden rounded-md border border-slate-200 bg-white text-left shadow-sm transition hover:border-brand-300 hover:shadow-md">
              <div className="grid h-28 place-items-center bg-slate-50 text-slate-400"><PawPrint size={44} /></div>
              <div className="p-4">
                <div className="flex items-center justify-between gap-3">
                  <h3 className="font-bold text-slate-950">{animal.name}</h3>
                  <span className="rounded-full bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">{animal.daysInShelter ?? 0} days</span>
                </div>
                <p className="mt-1 text-sm text-slate-700">{[animal.breed || animal.species, `${animal.age} yrs`, animal.gender].filter(Boolean).join(' - ')}</p>
                <div className="mt-3 flex flex-wrap gap-2">
                  {(animal.specialNeedsReasons ?? []).slice(0, 3).map(reason => <span key={reason} className="rounded-full bg-brand-50 px-2 py-1 text-xs font-semibold text-brand-700">{reason}</span>)}
                </div>
                <p className="mt-3 text-xs text-slate-500">{animal.shelterName || 'Shelter care'}</p>
              </div>
            </button>)}
          </div>
        </>}
    </>}
  </section>
}

function SpotlightStat({ value, label }) {
  return <div className="rounded-md border border-slate-200 bg-white p-4 text-center">
    <div className="text-2xl font-bold text-slate-950">{value}</div>
    <div className="text-xs font-semibold text-slate-500">{label}</div>
  </div>
}

function Checkbox({ label, checked, onChange }) {
  return <label className="flex items-center gap-2 text-sm text-slate-700"><input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)} /> {label}</label>
}

function AnimalProfileModal({ animal, onClose }) {
  const mainPhoto = animal.photos?.find(photo => photo.isMain) ?? animal.photos?.[0]
  return <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/40 p-4">
    <section className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-md bg-white p-5 shadow-xl">
      <div className="flex items-start justify-between gap-4 border-b border-slate-200 pb-3">
        <div className="flex items-center gap-4">
          {mainPhoto
            ? <img className="h-20 w-20 rounded-md border border-slate-200 object-cover" src={absoluteUrl(mainPhoto.url)} alt={`${animal.name} main`} />
            : <div className="grid h-20 w-20 place-items-center rounded-md border border-slate-200 bg-slate-50 text-slate-400"><PawPrint size={32} /></div>}
          <div>
            <h2 className="text-2xl font-bold text-slate-950">{animal.name}</h2>
            <p className="text-sm text-slate-600">{[animal.species, animal.breed, animalStatuses[animal.status]?.label].filter(Boolean).join(' - ')}</p>
          </div>
        </div>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <div className="mt-5 grid gap-5">
        <section>
          <h3 className="font-semibold text-slate-950">Photos</h3>
          {animal.photos?.length
            ? <div className="mt-2 grid gap-3 grid-cols-2 sm:grid-cols-3">
              {animal.photos.map(photo => <figure key={photo.id} className="relative overflow-hidden rounded-md border border-slate-200 bg-slate-50">
                <img className="h-36 w-full object-cover" src={absoluteUrl(photo.url)} alt={animal.name} />
                {photo.isMain && <figcaption className="absolute left-2 top-2 rounded-full bg-brand-600 px-2 py-1 text-xs font-semibold text-white">Main photo</figcaption>}
              </figure>)}
            </div>
            : <div className="mt-2 rounded-md border border-dashed border-slate-300 p-5 text-center text-sm text-slate-500">No photos uploaded yet.</div>}
        </section>
        <section>
          <h3 className="font-semibold text-slate-950">Video</h3>
          {animal.videoUrl
            ? <video className="mt-2 max-h-96 w-full rounded-md border border-slate-200 bg-black" controls src={absoluteUrl(animal.videoUrl)} />
            : <div className="mt-2 rounded-md border border-dashed border-slate-300 p-5 text-center text-sm text-slate-500">No video uploaded yet.</div>}
        </section>
        <ProfileSection title="Basic information" rows={[
          ['Species', animal.species],
          ['Breed', animal.breed],
          ...(animal.shelterName ? [['Shelter', animal.shelterName]] : []),
          ['Intake date', formatDate(animal.intakeDate)],
          ['Age', animal.age],
          ['Gender', animal.gender],
          ['Size', animalSizes[animal.size]?.label ?? animal.size],
          ['Energy', energyLevels[animal.energyLevel]?.label ?? animal.energyLevel],
          ['Status', animalStatuses[animal.status]?.label ?? animal.status],
          ['Description', animal.description],
        ]} />

        <ProfileSection title="Health profile" rows={animal.healthRecord ? [
          ['Vaccinated', yesNo(animal.healthRecord.isVaccinated)],
          ['Sterilized', yesNo(animal.healthRecord.isSterilized)],
          ['Microchipped', yesNo(animal.healthRecord.isMicrochipped)],
          ['Chronic diseases', animal.healthRecord.chronicDiseases || '-'],
          ['Medications', animal.healthRecord.medications || '-'],
        ] : [['Health profile', 'Not added yet']]} />

        <ProfileSection title="Behavior profile" rows={animal.behaviorProfile ? [
          ['Energy', energyLevels[animal.behaviorProfile.energyLevel]?.label ?? animal.behaviorProfile.energyLevel],
          ['Good with children', yesNo(animal.behaviorProfile.goodWithChildren)],
          ['Good with dogs', yesNo(animal.behaviorProfile.goodWithDogs)],
          ['Good with cats', yesNo(animal.behaviorProfile.goodWithCats)],
          ['Special needs', yesNo(animal.behaviorProfile.hasSpecialNeeds)],
          ['Behavior description', animal.behaviorProfile.behaviorDescription || '-'],
        ] : [['Behavior profile', 'Not added yet']]} />
      </div>
    </section>
  </div>
}

function ProfileSection({ title, rows }) {
  return <section>
    <h3 className="font-semibold text-slate-950">{title}</h3>
    <dl className="mt-2 grid gap-2 rounded-md border border-slate-200 p-3 sm:grid-cols-2">
      {rows.map(([labelText, value]) => <div key={labelText}>
        <dt className="text-xs font-semibold uppercase text-slate-500">{labelText}</dt>
        <dd className="mt-1 text-sm text-slate-800">{value || '-'}</dd>
      </div>)}
    </dl>
  </section>
}

function yesNo(value) {
  return value ? 'Yes' : 'No'
}

function todayInput() {
  return new Date().toISOString().slice(0, 10)
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
