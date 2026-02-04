import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'

dayjs.extend(utc)

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

/**
 * Parse ISO date string to YYYY-MM-DD format for input[type="date"]
 * Dùng khi populate form từ API response
 */
export function toDateInputFormat(date: string | Date | undefined | null): string {
  if (!date) return ''
  const parsed = dayjs(date)
  return parsed.isValid() ? parsed.format('YYYY-MM-DD') : ''
}

/**
 * Convert date to UTC ISO string for sending to API
 * Dùng khi submit form lên server (PostgreSQL requires UTC)
 */
export function toUtcIsoString(date: string | Date | undefined | null): string | undefined {
  if (!date) return undefined
  const parsed = dayjs(date)
  return parsed.isValid() ? parsed.utc().toISOString() : undefined
}
