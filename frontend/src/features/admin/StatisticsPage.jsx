import { adminApi } from '../../shared/api/endpoints'
import { useApi } from '../../shared/hooks/useApi'
import { PageHeader } from '../../shared/ui/PageHeader'
import { ErrorState, LoadingState } from '../../shared/ui/State'

export function StatisticsPage() {
  const { data, loading, error } = useApi(() => adminApi.dashboardStatistics(), [])
  if (loading) return <LoadingState />

  const totals = data?.totals ?? {}
  const cards = [
    ['Users', totals.users],
    ['Verified shelters', totals.verifiedShelters],
    ['Pending shelters', data?.pendingShelters],
    ['Available animals', totals.availableAnimals],
    ['Adopted animals', totals.adoptedAnimals],
    ['Active foster placements', data?.activeFosterPlacements],
    ['Resolved Lost & Found', data?.resolvedLostFoundReports],
    ['Paid donations', formatMoney(totals.paidDonationTotal)],
    ['Average adoption time', `${data?.averageAdoptionDays ?? 0} days`],
  ]

  return <div className="grid gap-6">
    <PageHeader title="Statistics" description="Read-only overview of platform activity and trends." />
    <ErrorState error={error} />
    <section className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
      {cards.map(([label, value]) => <StatCard key={label} label={label} value={value ?? 0} />)}
    </section>
    <section className="grid gap-4 lg:grid-cols-2">
      <DonutChart title="Animals by status" points={data?.animalsByStatus ?? []} />
      <BarChart title="Adoption applications by status" points={data?.applicationsByStatus ?? []} />
      <VerticalBarChart title="Completed adoptions by month" points={data?.adoptionsByMonth ?? []} />
      <VerticalBarChart title="Paid donations by month" points={data?.donationsByMonth ?? []} money />
    </section>
  </div>
}

function StatCard({ label, value }) {
  return <article className="rounded-md border border-slate-200 bg-white p-4">
    <div className="text-xs font-semibold uppercase text-slate-500">{label}</div>
    <div className="mt-2 text-2xl font-bold text-slate-950">{value}</div>
  </article>
}

function BarChart({ title, points, money = false }) {
  const max = Math.max(...points.map(point => Number(point.value)), 1)
  return <article className="rounded-md border border-slate-200 bg-white p-4">
    <h2 className="font-bold">{title}</h2>
    {!points.length ? <p className="mt-4 text-sm text-slate-500">No data yet.</p> : <div className="mt-4 grid gap-3">
      {points.map(point => <div key={point.label} className="grid grid-cols-[9rem_1fr_auto] items-center gap-3 text-sm">
        <span className="truncate text-slate-600" title={point.label}>{point.label}</span>
        <div className="h-3 overflow-hidden rounded-full bg-slate-100">
          <div className="h-full rounded-full bg-brand-600" style={{ width: `${Math.max((Number(point.value) / max) * 100, point.value ? 3 : 0)}%` }} />
        </div>
        <span className="min-w-12 text-right font-semibold text-slate-800">{money ? formatMoney(point.value) : point.value}</span>
      </div>)}
    </div>}
  </article>
}

function DonutChart({ title, points }) {
  const total = points.reduce((sum, point) => sum + Number(point.value), 0)
  const colors = ['#168260', '#3b82f6', '#f59e0b', '#8b5cf6', '#ef4444', '#64748b']
  let current = 0
  const segments = points.map((point, index) => {
    const start = current
    current += total ? (Number(point.value) / total) * 100 : 0
    return `${colors[index % colors.length]} ${start}% ${current}%`
  })

  return <article className="rounded-md border border-slate-200 bg-white p-4">
    <h2 className="font-bold">{title}</h2>
    {!total ? <p className="mt-4 text-sm text-slate-500">No data yet.</p> : <div className="mt-4 grid items-center gap-5 sm:grid-cols-[11rem_1fr]">
      <div className="relative mx-auto h-40 w-40 rounded-full" style={{ background: `conic-gradient(${segments.join(', ')})` }}>
        <div className="absolute inset-8 grid place-items-center rounded-full bg-white text-center">
          <div><div className="text-2xl font-bold">{total}</div><div className="text-xs text-slate-500">animals</div></div>
        </div>
      </div>
      <div className="grid gap-2">
        {points.map((point, index) => <div key={point.label} className="flex items-center justify-between gap-3 text-sm">
          <span className="flex items-center gap-2 text-slate-600"><span className="h-3 w-3 rounded-sm" style={{ backgroundColor: colors[index % colors.length] }} />{point.label}</span>
          <span className="font-semibold">{point.value}</span>
        </div>)}
      </div>
    </div>}
  </article>
}

function VerticalBarChart({ title, points, money = false }) {
  const max = Math.max(...points.map(point => Number(point.value)), 1)
  return <article className="rounded-md border border-slate-200 bg-white p-4">
    <h2 className="font-bold">{title}</h2>
    {!points.length ? <p className="mt-4 text-sm text-slate-500">No data yet.</p> : <div className="mt-4 flex h-56 items-end gap-3 overflow-x-auto border-b border-slate-200 pb-2">
      {points.map(point => <div key={point.label} className="flex min-w-14 flex-1 flex-col items-center justify-end gap-2">
        <span className="text-xs font-semibold text-slate-700">{money ? formatMoney(point.value) : point.value}</span>
        <div className="w-full max-w-14 rounded-t-md bg-brand-600" style={{ height: `${Math.max((Number(point.value) / max) * 150, point.value ? 6 : 0)}px` }} />
        <span className="text-xs text-slate-500">{point.label}</span>
      </div>)}
    </div>}
  </article>
}

function formatMoney(value) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'RSD', maximumFractionDigits: 0 }).format(value ?? 0)
}
