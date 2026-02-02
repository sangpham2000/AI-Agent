/**
 * Utility functions for color generation and management
 */

// A curated palette of "nice" colors that work well with white text
// Using Tailwind CSS color names/values that are vibrant but readable
const AVATAR_COLORS = [
  'bg-red-500',
  'bg-orange-500',
  'bg-amber-500',
  'bg-yellow-500',
  'bg-lime-500',
  'bg-green-500',
  'bg-emerald-500',
  'bg-teal-500',
  'bg-cyan-500',
  'bg-sky-500',
  'bg-blue-500',
  'bg-indigo-500',
  'bg-violet-500',
  'bg-purple-500',
  'bg-fuchsia-500',
  'bg-pink-500',
  'bg-rose-500',
]

/**
 * Generates a deterministic color class from a string input (e.g. username or name)
 * @param name The string to generate a color for
 * @returns A Tailwind CSS class string for the background color
 */
export function getAvatarColor(name: string): string | undefined {
  if (!name) return AVATAR_COLORS[0]

  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }

  const index = Math.abs(hash) % AVATAR_COLORS.length
  return AVATAR_COLORS[index] || AVATAR_COLORS[0]
}
