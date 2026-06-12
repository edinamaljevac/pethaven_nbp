export function Button({ className = '', variant = 'primary', ...props }) {
  const styles = variant === 'secondary'
    ? 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
    : 'bg-brand-600 text-white hover:bg-brand-700'

  return (
    <button
      className={`inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-60 ${styles} ${className}`}
      {...props}
    />
  )
}