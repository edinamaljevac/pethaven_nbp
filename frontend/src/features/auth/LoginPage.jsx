import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../shared/auth/AuthContext'
import { Button } from '../../shared/ui/Button'
import { Field, inputClass } from '../../shared/ui/Field'
import { ErrorState } from '../../shared/ui/State'

export function LoginPage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(false)
  const [securityWarning, setSecurityWarning] = useState('')

  const submit = async (event) => {
    event.preventDefault()
    setLoading(true)
    setError(null)
    setSecurityWarning('')
    try {
      const response = await auth.login(form)
      if (response.securityWarning) {
        setSecurityWarning(response.securityWarning)
      } else {
        navigate('/')
      }
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="mx-auto max-w-md rounded-md border border-slate-200 bg-white p-6 shadow-sm">
      <h1 className="text-2xl font-bold text-slate-950">Login</h1>
      <p className="mt-1 text-sm text-slate-600">Access adopter, shelter, foster and admin workflows.</p>
      <form onSubmit={submit} className="mt-5 grid gap-4">
        <ErrorState error={error} />
        {securityWarning && <div className="rounded-md border border-amber-300 bg-amber-50 p-3 text-sm text-amber-800"><strong>Security warning:</strong> {securityWarning}<div className="mt-3"><Button type="button" variant="secondary" onClick={() => navigate('/')}>Continue</Button></div></div>}
        <Field label="Email"><input className={inputClass()} value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></Field>
        <Field label="Password"><input className={inputClass()} type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} /></Field>
        <Button disabled={loading}>{loading ? 'Signing in...' : 'Login'}</Button>
        <p className="text-sm text-slate-600">No account? <Link className="font-semibold text-brand-700" to="/register">Register</Link></p>
      </form>
    </div>
  )
}
