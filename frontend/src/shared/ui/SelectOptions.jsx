export function animalLabel(animal) {
  return [animal.name, animal.species, animal.breed].filter(Boolean).join(' - ') || animal.id
}

export function shelterLabel(shelter) {
  return [shelter.name, shelter.location].filter(Boolean).join(' - ') || shelter.id
}

export function adoptionLabel(application, animal) {
  const status = application.statusLabel ?? application.status
  return [animal?.name, application.notes, status].filter(Boolean).join(' - ') || application.id
}

export function fosterProfileLabel(profile) {
  return [profile.fosterName, profile.preferredAnimalType, `capacity ${profile.capacity}`].filter(Boolean).join(' - ') || profile.id
}

export function donationGoalLabel(goal) {
  return [goal.title, goal.targetAmount ? `${goal.targetAmount}` : null].filter(Boolean).join(' - ') || goal.id
}
