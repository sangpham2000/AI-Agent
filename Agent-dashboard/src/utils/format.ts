export function formatDate(date: string | Date | undefined | null): string {
  if (!date) return 'N/A'
  const d = new Date(date)
  if (isNaN(d.getTime())) return 'N/A'

  return d.toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

export function formatDateTime(date: string | Date | undefined | null): string {
  if (!date) return 'N/A'
  const d = new Date(date)
  if (isNaN(d.getTime())) return 'N/A'

  return d.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
