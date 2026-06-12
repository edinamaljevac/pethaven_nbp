import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authApi } from '../../shared/api/endpoints'
import { roles } from '../../shared/constants/enums'
import { useAuth } from '../../shared/auth/AuthContext'
import { Button } from '../../shared/ui/Button'
import { Field, inputClass } from '../../shared/ui/Field'
import { ErrorState } from '../../shared/ui/State'

const roleNames = { 0: 'Adopter', 1: 'Shelter', 2: 'Foster' }

export function RegisterPage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ firstName: '', lastName: '', shelterName: '', email: '', password: '', role: 0 })
  const [documents, setDocuments] = useState([])
  const [error, setError] = useState(null)
  const [fieldErrors, setFieldErrors] = useState({})
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)
  const role = Number(form.role)

  const submit = async (event) => {
    event.preventDefault()
    setError(null)
    setFieldErrors({})
    setMessage('')
    const validationErrors = validate(form, documents)
    if (Object.keys(validationErrors).length) {
      setFieldErrors(validationErrors)
      return
    }

    setLoading(true)
    try {
      if (role === 1) {
        await authApi.registerShelter(buildShelterPayload(form, documents))
        setMessage('Shelter registration submitted. You will be able to log in after admin verification.')
        setForm({ firstName: '', lastName: '', shelterName: '', email: '', password: '', role: 1 })
        setDocuments([])
      } else {
        await auth.register(buildPayload(form))
        navigate('/profile')
      }
    } catch (err) { setError(toFriendlyRegistrationError(err)) } finally { setLoading(false) }
  }

  return (
    <div className="mx-auto max-w-xl rounded-md border border-slate-200 bg-white p-6 shadow-sm">
      <h1 className="text-2xl font-bold text-slate-950">Create account</h1>
      <p className="mt-1 text-sm text-slate-600">Register quickly, then complete your profile after login.</p>
      <form onSubmit={submit} className="mt-5 grid gap-4 md:grid-cols-2">
        <div className="md:col-span-2"><ErrorState error={error} /></div>
        {message && <div className="rounded-md bg-brand-50 p-3 text-sm text-brand-700 md:col-span-2">{message}</div>}
        <Field label="Role"><select className={inputClass()} value={form.role} onChange={(e) => setForm({ ...form, role: Number(e.target.value) })}>{roles.filter(r => r.label !== 'Admin').map(r => <option key={r.value} value={r.value}>{r.label}</option>)}</select></Field>
        <Field label="Email"><input className={inputClass()} value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />{fieldErrors.email && <FieldError message={fieldErrors.email} />}</Field>
        <Field label="Password"><input className={inputClass()} type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />{fieldErrors.password && <FieldError message={fieldErrors.password} />}</Field>

        {role === 1 ? <>
          <Field label="Shelter name"><input className={inputClass()} value={form.shelterName} onChange={(e) => setForm({ ...form, shelterName: e.target.value })} />{fieldErrors.shelterName && <FieldError message={fieldErrors.shelterName} />}</Field>
          <div className="md:col-span-2">
            <Field label="Verification documents"><input className={inputClass()} type="file" multiple accept=".pdf,image/*" onChange={(e) => setDocuments(Array.from(e.target.files ?? []))} /></Field>
            <p className="mt-1 text-xs text-slate-500">Upload registration, permit or other shelter verification documents.</p>
            {fieldErrors.documents && <FieldError message={fieldErrors.documents} />}
          </div>
        </> : <>
          <Field label="First name"><input className={inputClass()} value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />{fieldErrors.firstName && <FieldError message={fieldErrors.firstName} />}</Field>
          <Field label="Last name"><input className={inputClass()} value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />{fieldErrors.lastName && <FieldError message={fieldErrors.lastName} />}</Field>
        </>}

        <div className="md:col-span-2"><Button disabled={loading}>{loading ? 'Creating...' : `Register as ${roleNames[role]}`}</Button></div>
        <p className="text-sm text-slate-600 md:col-span-2">Already registered? <Link className="font-semibold text-brand-700" to="/login">Login</Link></p>
      </form>
    </div>
  )
}

function FieldError({ message }) {
  return <p className="mt-1 text-xs text-red-600">{message}</p>
}

function validate(form, documents) {
  const errors = {}
  const role = Number(form.role)

  if (!form.email.trim()) errors.email = 'Email is required.'
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) errors.email = 'Enter a valid email address.'

  if (!form.password) errors.password = 'Password is required.'
  else if (form.password.length < 8) errors.password = 'Password must be at least 8 characters long.'

  if (role === 1) {
    if (!form.shelterName.trim()) errors.shelterName = 'Shelter name is required.'
    if (!documents.length) errors.documents = 'Please upload at least one verification document.'
  } else {
    if (!form.firstName.trim()) errors.firstName = 'First name is required.'
    if (!form.lastName.trim()) errors.lastName = 'Last name is required.'
  }

  return errors
}

function toFriendlyRegistrationError(error) {
  const message = error.userMessage || error.message || String(error)
  const knownMessages = [
    ['A user with this email already exists.', 'An account with this email already exists. Please log in or use another email.'],
    ['At least one verification document is required.', 'Please upload at least one verification document.'],
    ['Email, password and shelter name are required.', 'Please fill in email, password and shelter name.'],
    ['Network Error', 'Cannot connect to the server. Please check that the backend is running.'],
  ]
  const replacement = knownMessages.find(([source]) => message.includes(source))?.[1]
  return { userMessage: replacement || message }
}

function buildShelterPayload(form, documents) {
  const payload = new FormData()
  payload.append('email', form.email)
  payload.append('password', form.password)
  payload.append('shelterName', form.shelterName)
  documents.forEach(document => payload.append('documents', document))
  return payload
}

function buildPayload(form) {
  const role = Number(form.role)
  const payload = { email: form.email, password: form.password, role }
  if (role === 1) payload.shelterName = form.shelterName
  else {
    payload.firstName = form.firstName
    payload.lastName = form.lastName
  }
  return payload
}
