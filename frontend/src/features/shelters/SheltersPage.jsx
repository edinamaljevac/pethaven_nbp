import { useState } from 'react'
import { sheltersApi } from '../../shared/api/endpoints'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { ErrorState, LoadingState } from '../../shared/ui/State'

export function SheltersPage() {
  const [filters, setFilters] = useState({})
  const [locationError, setLocationError] = useState('')
  const { data, loading, error } = useApi(() => sheltersApi.list(clean(filters)), [JSON.stringify(filters)])

  const useCurrentLocation = () => {
    setLocationError('')

    if (!navigator.geolocation) {
      setLocationError('Your browser does not support location lookup.')
      return
    }

    navigator.geolocation.getCurrentPosition(
      position => setFilters({
        ...filters,
        location: '',
        latitude: position.coords.latitude.toFixed(6),
        longitude: position.coords.longitude.toFixed(6),
        radiusKm: filters.radiusKm || 25,
      }),
      () => setLocationError('Location permission was not granted. You can still search by city.'),
    )
  }

  const clearRadiusSearch = () => setFilters({ ...filters, latitude: '', longitude: '', radiusKm: '' })

  return <div className="grid gap-6">
    <PageHeader title="Shelters" description="Find verified shelters by city or by distance from your current location." />
    <section className="rounded-md border border-slate-200 bg-white p-4">
      <div className="grid gap-3 md:grid-cols-4">
        <Field label="City or location"><input className={inputClass()} value={filters.location ?? ''} onChange={(e) => setFilters({ ...filters, location: e.target.value, latitude: '', longitude: '' })} /></Field>
        <Field label="Verified"><select className={inputClass()} value={filters.isVerified ?? ''} onChange={(e) => setFilters({ ...filters, isVerified: e.target.value })}><option value="">Any</option><option value="true">Verified</option><option value="false">Unverified</option></select></Field>
        <Field label="Radius"><select className={inputClass()} value={filters.radiusKm ?? 25} onChange={(e) => setFilters({ ...filters, radiusKm: e.target.value })}><option value="10">10 km</option><option value="25">25 km</option><option value="50">50 km</option><option value="100">100 km</option></select></Field>
        <div className="flex items-end gap-2">
          <Button type="button" variant="secondary" onClick={useCurrentLocation}>Use my location</Button>
          {(filters.latitude || filters.longitude) && <Button type="button" variant="secondary" onClick={clearRadiusSearch}>Clear</Button>}
        </div>
      </div>
      {(filters.latitude || filters.longitude) && <div className="mt-3 rounded-md bg-brand-50 p-3 text-sm text-brand-700">
        Distance search is active from your current browser location within {filters.radiusKm || 25} km.
      </div>}
      {locationError && <div className="mt-3 rounded-md bg-red-50 p-3 text-sm text-red-700">{locationError}</div>}
    </section>
    {loading ? <LoadingState /> : <><ErrorState error={error} /><DataTable rows={data ?? []} columns={[{key:'name',label:'Name'}, {key:'location',label:'Location'}, {key:'contactPhone',label:'Phone'}, {key:'isVerified',label:'Verified', render:r => r.isVerified ? 'Yes' : 'No'}, {key:'coordinates',label:'Map coordinates', render:r => r.latitude ? `${r.latitude}, ${r.longitude}` : 'Not added'}]} /></>}
  </div>
}
function clean(obj) { return Object.fromEntries(Object.entries(obj).filter(([,v]) => v !== '').map(([k,v]) => [['latitude','longitude','radiusKm'].includes(k) ? [k, Number(v)] : [k, v]][0])) }
