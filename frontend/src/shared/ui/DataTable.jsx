import { EmptyState } from './State'

export function DataTable({ columns, rows, keyField = 'id' }) {
  if (!rows?.length) return <EmptyState />

  return (
    <div className="overflow-hidden rounded-md border border-slate-200 bg-white">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-slate-200 text-sm">
          <thead className="bg-slate-50 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
            <tr>{columns.map((column) => <th key={column.key} className="px-4 py-3">{column.label}</th>)}</tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {rows.map((row, index) => (
              <tr key={row[keyField] ?? index} className="align-top">
                {columns.map((column) => <td key={column.key} className="px-4 py-3 text-slate-700">{column.render ? column.render(row) : row[column.key]}</td>)}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}