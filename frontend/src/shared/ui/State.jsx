export function LoadingState({ label = 'Loading...' }) {
  return <div className="rounded-md border border-slate-200 bg-white p-4 text-sm text-slate-600">{label}</div>
}

export function ErrorState({ error }) {
  if (!error) return null
  return <div className="whitespace-pre-line rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error.userMessage || error.message || String(error)}</div>
}

export function EmptyState({ label = 'No data yet.' }) {
  return <div className="rounded-md border border-dashed border-slate-300 bg-white p-6 text-center text-sm text-slate-500">{label}</div>
}
