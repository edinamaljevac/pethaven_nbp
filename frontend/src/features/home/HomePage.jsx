import { Link } from 'react-router-dom'
import { HeartHandshake, MapPin, Search, ShieldCheck } from 'lucide-react'
import { sheltersApi } from '../../shared/api/endpoints'
import { useApi } from '../../shared/hooks/useApi'
import { DataTable } from '../../shared/ui/DataTable'
import { PageHeader } from '../../shared/ui/PageHeader'
import { ErrorState, LoadingState } from '../../shared/ui/State'

const cards = [
  ['Animals', 'Search adoptable animals with health, behavior and special-needs filters.', '/animals', Search],
  ['Shelters near you', 'Use verified shelters, city search and radius filtering.', '/shelters', MapPin],
  ['Adoption workflow', 'Track applications, contracts and post-adoption reports.', '/adoptions', HeartHandshake],
  ['Admin center', 'Verify shelters, view statistics and moderate content.', '/admin', ShieldCheck],
]

export function HomePage() {
  const { data: nearby, loading, error } = useApi(() => sheltersApi.nearMe({ radiusKm: 25 }), [])
  const geo = nearby?.geo
  const shelters = nearby?.shelters ?? []

  return (
    <div className="grid gap-6">
      <PageHeader title="PetHaven" description="A connected adoption platform for shelters, fosters and adopters." />
      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {cards.map(([title, text, to, Icon]) => (
          <Link key={title} to={to} className="rounded-md border border-slate-200 bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:border-brand-200 hover:shadow-md">
            <Icon className="text-brand-600" />
            <h2 className="mt-4 font-bold text-slate-950">{title}</h2>
            <p className="mt-2 text-sm text-slate-600">{text}</p>
          </Link>
        ))}
      </section>
      <section className="rounded-md border border-slate-200 bg-white p-4">
        <div className="flex flex-wrap items-end justify-between gap-3">
          <div>
            <h2 className="font-bold text-slate-950">Shelters near you</h2>
            <p className="mt-1 text-sm text-slate-600">
              {locationLabel(geo) ? `Detected location: ${locationLabel(geo)}` : 'Location was not detected, showing verified shelters.'}
            </p>
          </div>
          <Link className="text-sm font-semibold text-brand-700" to="/shelters">View all shelters</Link>
        </div>
        <div className="mt-4">
          {loading ? <LoadingState /> : <>
            <ErrorState error={error} />
            <DataTable rows={shelters} columns={[
              { key: 'name', label: 'Name' },
              { key: 'location', label: 'Location' },
              { key: 'contactPhone', label: 'Phone' },
              { key: 'description', label: 'Description' },
            ]} />
          </>}
        </div>
      </section>
    </div>
  )
}

function locationLabel(geo) {
  if (!geo) return ''
  return [geo.city, geo.region, geo.country].filter(x => x && x !== 'Unknown').join(', ')
}
