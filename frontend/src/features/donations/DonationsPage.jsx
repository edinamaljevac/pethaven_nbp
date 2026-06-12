import { useState } from 'react'
import { donationsApi, profileApi, sheltersApi } from '../../shared/api/endpoints'
import { useAuth } from '../../shared/auth/AuthContext'
import { useApi } from '../../shared/hooks/useApi'
import { Button } from '../../shared/ui/Button'
import { DataTable } from '../../shared/ui/DataTable'
import { Field, inputClass } from '../../shared/ui/Field'
import { PageHeader } from '../../shared/ui/PageHeader'
import { donationGoalLabel, shelterLabel } from '../../shared/ui/SelectOptions'

const empty = Promise.resolve({ data: [] })

export function DonationsPage() {
  const auth = useAuth()
  const isShelter = auth.hasRole('Shelter')
  const isAdmin = auth.hasRole('Admin')
  const canViewDonations = isShelter || isAdmin
  const canDonate = !isShelter && !isAdmin
  const { data: profile } = useApi(() => auth.isAuthenticated ? profileApi.me() : Promise.resolve({ data: null }), [auth.isAuthenticated])
  const { data: shelters } = useApi(() => sheltersApi.list({}), [])
  const { data: goals, reload: reloadGoals } = useApi(() => donationsApi.goals({}), [])
  const { data: donations, reload: reloadDonations } = useApi(() => canViewDonations ? donationsApi.list({}) : empty, [canViewDonations])
  const [goal, setGoal] = useState({ title: 'Food', description: '', targetAmount: 10000 })
  const [donation, setDonation] = useState({ shelterProfileId: '', donationGoalId: '', amount: 1000, purpose: '' })
  const [selectedDonationGoal, setSelectedDonationGoal] = useState(null)

  const createGoal = async e => {
    e.preventDefault()
    await donationsApi.createGoal({
      ...goal,
      targetAmount: Number(goal.targetAmount)
    })
    setGoal({ title: 'Food', description: '', targetAmount: 10000 })
    reloadGoals()
  }

  const donate = async e => {
    e.preventDefault()
    const selectedGoal = (goals ?? []).find(g => g.id === donation.donationGoalId)
    await donationsApi.donate({
      ...donation,
      shelterProfileId: selectedGoal?.shelterProfileId ?? donation.shelterProfileId,
      donationGoalId: donation.donationGoalId || null,
      amount: Number(donation.amount)
    })
    setDonation({ shelterProfileId: '', donationGoalId: '', amount: 1000, purpose: '' })
    setSelectedDonationGoal(null)
    reloadGoals()
    reloadDonations()
  }

  const openDonationForm = donationGoal => {
    setSelectedDonationGoal(donationGoal)
    setDonation({
      shelterProfileId: donationGoal.shelterProfileId,
      donationGoalId: donationGoal.id,
      amount: 1000,
      purpose: donationGoal.title,
    })
  }

  const selectedShelter = (shelters ?? []).find(shelter => shelter.id === selectedDonationGoal?.shelterProfileId)

  return (
    <div className="grid gap-6">
      <PageHeader title="Donations" description="Donation goals and mock payment tracking." />

      <DataTable
        rows={goals ?? []}
        columns={[
          { key: 'title', label: 'Goal' },
          { key: 'description', label: 'Description' },
          { key: 'targetAmount', label: 'Target' },
          { key: 'currentAmount', label: 'Current' },
          { key: 'isCompleted', label: 'Completed', render: r => r.isCompleted ? 'Yes' : 'No' },
          ...(canDonate ? [{ key: 'actions', label: 'Actions', render: r => r.isCompleted
            ? <span className="text-slate-500">Completed</span>
            : <Button variant="secondary" onClick={() => openDonationForm(r)}>Donate</Button> }] : [])
        ]}
      />

      <section className={`grid gap-4 ${isShelter && canDonate ? 'md:grid-cols-2' : ''}`}>
        {isShelter && (
          <form onSubmit={createGoal} className="rounded-md border border-slate-200 bg-white p-4">
            <h2 className="font-bold">Create donation goal</h2>
            <div className="mt-3 grid gap-3">
              <Field label="Shelter">
                <input className={inputClass()} value={profile?.shelterName ?? 'My shelter'} disabled />
              </Field>
              <Field label="Goal type">
                <select className={inputClass()} value={goal.title} onChange={e => setGoal({ ...goal, title: e.target.value })}>
                  <option>Food</option>
                  <option>Medicine</option>
                  <option>Equipment</option>
                </select>
              </Field>
              <Field label="Description">
                <input className={inputClass()} value={goal.description} onChange={e => setGoal({ ...goal, description: e.target.value })} />
              </Field>
              <Field label="Target amount">
                <input className={inputClass()} type="number" value={goal.targetAmount} onChange={e => setGoal({ ...goal, targetAmount: e.target.value })} />
              </Field>
              <Button>Save goal</Button>
            </div>
          </form>
        )}

        {canDonate && selectedDonationGoal && (
          <form onSubmit={donate} className="rounded-md border border-slate-200 bg-white p-4">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h2 className="font-bold">Donate to {selectedDonationGoal.title}</h2>
                <p className="mt-1 text-sm text-slate-600">Enter the amount and purpose to complete the mock payment.</p>
              </div>
              <Button type="button" variant="secondary" onClick={() => setSelectedDonationGoal(null)}>Close</Button>
            </div>
            <div className="mt-3 grid gap-3">
              <Field label="Shelter"><input className={inputClass()} value={selectedShelter ? shelterLabel(selectedShelter) : 'Selected shelter'} disabled /></Field>
              <Field label="Donation goal">
                <input className={inputClass()} value={donationGoalLabel(selectedDonationGoal)} disabled />
              </Field>
              <Field label="Amount">
                <input className={inputClass()} type="number" min="1" required value={donation.amount} onChange={e => setDonation({ ...donation, amount: e.target.value })} />
              </Field>
              <Field label="Purpose">
                <input className={inputClass()} required value={donation.purpose} onChange={e => setDonation({ ...donation, purpose: e.target.value })} />
              </Field>
              <Button>Donate</Button>
            </div>
          </form>
        )}
      </section>

      {canViewDonations && (
        <DataTable
          rows={donations ?? []}
          columns={[
            { key: 'amount', label: 'Amount' },
            { key: 'purpose', label: 'Purpose' },
            { key: 'isPaid', label: 'Paid', render: r => r.isPaid ? 'Yes' : 'No' }
          ]}
        />
      )}
    </div>
  )
}
