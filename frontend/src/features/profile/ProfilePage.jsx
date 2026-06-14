import { useEffect, useState } from 'react'
import countryList from 'react-select-country-list'
import { profileApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { Button } from '../../shared/ui/Button'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { ErrorState, LoadingState } from '../../shared/ui/State'

const countries = countryList().getData()

export function ProfilePage() {
  const auth = useAuth()
  const [profile, setProfile] = useState(null)
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [findingCoordinates, setFindingCoordinates] = useState(false)
  const [message, setMessage] = useState('')

  useEffect(() => {
    let active = true
    setLoading(true)
    profileApi.me()
      .then((res) => { if (active) setProfile(normalizeProfile(res.data)) })
      .catch((err) => { if (active) setError(err) })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [])

  const save = async (event) => {
    event.preventDefault()
    setSaving(true)
    setError(null)
    setMessage('')
    try {
      const response = await profileApi.update(buildPayload(profile))
      setProfile(normalizeProfile(response.data))
      setMessage('Profile saved.')
    } catch (err) { setError(err) } finally { setSaving(false) }
  }

  const findCoordinates = async () => {
    setError(null)
    setMessage('')
    if (!profile.shelterLocation?.trim()) {
      setError({ userMessage: 'Enter a shelter address or location first.' })
      return
    }

    setFindingCoordinates(true)
    try {
      const response = await profileApi.geocode(profile.shelterLocation)
      setProfile(current => ({
        ...current,
        shelterLatitude: response.data.latitude,
        shelterLongitude: response.data.longitude,
      }))
      setMessage(`Coordinates found for ${response.data.displayName}. Save the profile to keep them.`)
    } catch (err) {
      setError(err)
    } finally {
      setFindingCoordinates(false)
    }
  }

  if (loading) return <LoadingState label="Loading profile..." />
  if (!profile) return <ErrorState error={error} />

  return <div className="grid gap-6">
    <PageHeader title="My Profile" description={`Complete and maintain your ${auth.role} profile.`} />
    {message && <div className="rounded-md bg-brand-50 p-3 text-sm text-brand-700">{message}</div>}
    <ErrorState error={error} />
    <form onSubmit={save} className="rounded-md border border-slate-200 bg-white p-5">
      <div className="grid gap-4 md:grid-cols-2">
        <Field label="Email"><input className={inputClass()} value={profile.email} disabled /></Field>
        <Field label="Role"><input className={inputClass()} value={auth.role ?? profile.role} disabled /></Field>
        {auth.role !== 'Shelter' && <>
          <Field label="First name"><input className={inputClass()} value={profile.firstName ?? ''} onChange={(e) => setProfile({ ...profile, firstName: e.target.value })} /></Field>
          <Field label="Last name"><input className={inputClass()} value={profile.lastName ?? ''} onChange={(e) => setProfile({ ...profile, lastName: e.target.value })} /></Field>
        </>}
        {auth.role === 'Adopter' && <AdopterProfileFields profile={profile} setProfile={setProfile} />}
        {auth.role === 'Shelter' && <ShelterProfileFields profile={profile} setProfile={setProfile} findCoordinates={findCoordinates} findingCoordinates={findingCoordinates} />}
        {auth.role === 'Foster' && <FosterProfileFields profile={profile} setProfile={setProfile} />}
      </div>
      <div className="mt-5"><Button disabled={saving}>{saving ? 'Saving...' : 'Save profile'}</Button></div>
    </form>
  </div>
}

function ShelterProfileFields({ profile, setProfile, findCoordinates, findingCoordinates }) {
  return <>
    <Field label="Shelter name"><input className={inputClass()} value={profile.shelterName ?? ''} onChange={(e) => setProfile({ ...profile, shelterName: e.target.value })} /></Field>
    <Field label="Address or location"><input className={inputClass()} value={profile.shelterLocation ?? ''} onChange={(e) => setProfile({ ...profile, shelterLocation: e.target.value, shelterLatitude: '', shelterLongitude: '' })} /></Field>
    <Field label="Contact phone"><input className={inputClass()} value={profile.shelterContactPhone ?? ''} onChange={(e) => setProfile({ ...profile, shelterContactPhone: e.target.value })} /></Field>
    <Field label="Description"><input className={inputClass()} value={profile.shelterDescription ?? ''} onChange={(e) => setProfile({ ...profile, shelterDescription: e.target.value })} /></Field>
    <Field label="Latitude"><input className={inputClass()} value={profile.shelterLatitude ?? ''} readOnly /></Field>
    <Field label="Longitude"><input className={inputClass()} value={profile.shelterLongitude ?? ''} readOnly /></Field>
    <div className="md:col-span-2"><Button type="button" variant="secondary" disabled={findingCoordinates || !profile.shelterLocation?.trim()} onClick={findCoordinates}>{findingCoordinates ? 'Finding coordinates...' : 'Find coordinates from location'}</Button></div>
    <Field label="Verified"><input className={inputClass()} value={profile.shelterIsVerified ? 'Yes' : 'No'} disabled /></Field>
  </>
}

function AdopterProfileFields({ profile, setProfile }) {
  return <>
    <Field label="Address"><input className={inputClass()} value={profile.adopterAddress ?? ''} onChange={(e) => setProfile({ ...profile, adopterAddress: e.target.value })} /></Field>
    <Field label="Country"><select className={inputClass()} value={profile.adopterCountry ?? ''} onChange={(e) => setProfile({ ...profile, adopterCountry: e.target.value })}><option value="">Choose country</option>{countries.map(country => <option key={country.value} value={country.label}>{country.label}</option>)}</select></Field>
    <Field label="Housing type"><select className={inputClass()} value={profile.adopterHousingType ?? 0} onChange={(e) => setProfile({ ...profile, adopterHousingType: Number(e.target.value) })}><option value={0}>Apartment</option><option value={1}>House</option></select></Field>
    <Field label="Household members"><input className={inputClass()} type="number" value={profile.adopterHouseholdMembers ?? 1} onChange={(e) => setProfile({ ...profile, adopterHouseholdMembers: e.target.value })} /></Field>
    <Field label="Experience"><input className={inputClass()} value={profile.adopterExperienceWithPets ?? ''} onChange={(e) => setProfile({ ...profile, adopterExperienceWithPets: e.target.value })} /></Field>
    <Field label="Adoption reason"><input className={inputClass()} value={profile.adopterAdoptionReason ?? ''} onChange={(e) => setProfile({ ...profile, adopterAdoptionReason: e.target.value })} /></Field>
    <label className="flex items-center gap-2 text-sm font-medium text-slate-700"><input type="checkbox" checked={Boolean(profile.adopterHasChildren)} onChange={(e) => setProfile({ ...profile, adopterHasChildren: e.target.checked })} /> Has children</label>
    <label className="flex items-center gap-2 text-sm font-medium text-slate-700"><input type="checkbox" checked={Boolean(profile.adopterHasOtherPets)} onChange={(e) => setProfile({ ...profile, adopterHasOtherPets: e.target.checked })} /> Has other pets</label>
  </>
}

function FosterProfileFields({ profile, setProfile }) {
  return <>
    <Field label="Preferred animal type"><input className={inputClass()} value={profile.fosterPreferredAnimalType ?? ''} onChange={(e) => setProfile({ ...profile, fosterPreferredAnimalType: e.target.value })} /></Field>
    <Field label="Capacity"><input className={inputClass()} type="number" value={profile.fosterCapacity ?? 1} onChange={(e) => setProfile({ ...profile, fosterCapacity: e.target.value })} /></Field>
    <Field label="Available from"><input className={inputClass()} type="date" value={toDate(profile.fosterAvailableFrom)} onChange={(e) => setProfile({ ...profile, fosterAvailableFrom: e.target.value })} /></Field>
    <Field label="Available to"><input className={inputClass()} type="date" value={toDate(profile.fosterAvailableTo)} onChange={(e) => setProfile({ ...profile, fosterAvailableTo: e.target.value })} /></Field>
  </>
}

function normalizeProfile(profile) {
  return {
    ...profile,
    shelterLatitude: profile.shelterLatitude ?? '',
    shelterLongitude: profile.shelterLongitude ?? '',
    adopterHouseholdMembers: profile.adopterHouseholdMembers ?? 1,
    fosterCapacity: profile.fosterCapacity ?? 1,
  }
}

function buildPayload(profile) {
  return {
    ...profile,
    shelterLatitude: profile.shelterLatitude === '' ? null : Number(profile.shelterLatitude),
    shelterLongitude: profile.shelterLongitude === '' ? null : Number(profile.shelterLongitude),
    adopterHouseholdMembers: Number(profile.adopterHouseholdMembers ?? 1),
    fosterCapacity: Number(profile.fosterCapacity ?? 1),
  }
}

function toDate(value) {
  return value ? String(value).slice(0, 10) : ''
}
