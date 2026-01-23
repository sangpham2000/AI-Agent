<script setup lang="ts">
import { ref, computed, watch } from 'vue'

const props = defineProps<{
  columns: { key: string; label: string; sortable?: boolean; class?: string }[]
  data: any[]
  loading?: boolean
  selectable?: boolean
  emptyText?: string
}>()

const emit = defineEmits<{
  select: [item: any]
  delete: [item: any]
  edit: [item: any]
  action: [action: string, item: any]
}>()

const selectedItems = ref<Set<string>>(new Set())
const sortKey = ref('')
const sortOrder = ref<'asc' | 'desc'>('asc')

const sortedData = computed(() => {
  if (!sortKey.value) return props.data

  return [...props.data].sort((a, b) => {
    const aVal = a[sortKey.value]
    const bVal = b[sortKey.value]

    if (aVal < bVal) return sortOrder.value === 'asc' ? -1 : 1
    if (aVal > bVal) return sortOrder.value === 'asc' ? 1 : -1
    return 0
  })
})

function toggleSort(key: string) {
  if (sortKey.value === key) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = key
    sortOrder.value = 'asc'
  }
}

function toggleSelectAll() {
  if (selectedItems.value.size === props.data.length) {
    selectedItems.value.clear()
  } else {
    selectedItems.value = new Set(props.data.map((item) => item.id))
  }
}

function toggleSelect(id: string) {
  if (selectedItems.value.has(id)) {
    selectedItems.value.delete(id)
  } else {
    selectedItems.value.add(id)
  }
}

watch(
  () => props.data,
  () => {
    selectedItems.value.clear()
  },
)
</script>

<template>
  <div class="overflow-x-auto">
    <table class="table table-zebra">
      <thead>
        <tr>
          <th v-if="selectable">
            <label>
              <input
                type="checkbox"
                class="checkbox checkbox-sm"
                :checked="selectedItems.size === data.length && data.length > 0"
                :indeterminate="selectedItems.size > 0 && selectedItems.size < data.length"
                @change="toggleSelectAll"
              />
            </label>
          </th>
          <th
            v-for="col in columns"
            :key="col.key"
            :class="[col.class, col.sortable ? 'cursor-pointer hover:bg-base-200' : '']"
            @click="col.sortable && toggleSort(col.key)"
          >
            <div class="flex items-center gap-1">
              {{ col.label }}
              <template v-if="col.sortable">
                <svg
                  v-if="sortKey === col.key && sortOrder === 'asc'"
                  xmlns="http://www.w3.org/2000/svg"
                  class="h-4 w-4"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M5 15l7-7 7 7"
                  />
                </svg>
                <svg
                  v-else-if="sortKey === col.key && sortOrder === 'desc'"
                  xmlns="http://www.w3.org/2000/svg"
                  class="h-4 w-4"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
                <svg
                  v-else
                  xmlns="http://www.w3.org/2000/svg"
                  class="h-4 w-4 opacity-30"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M7 16V4m0 0L3 8m4-4l4 4m6 0v12m0 0l4-4m-4 4l-4-4"
                  />
                </svg>
              </template>
            </div>
          </th>
          <th v-if="$slots.actions">Actions</th>
        </tr>
      </thead>
      <tbody>
        <template v-if="loading">
          <tr>
            <td
              :colspan="columns.length + (selectable ? 1 : 0) + ($slots.actions ? 1 : 0)"
              class="text-center py-8"
            >
              <span class="loading loading-spinner loading-lg text-primary"></span>
            </td>
          </tr>
        </template>
        <template v-else-if="sortedData.length === 0">
          <tr>
            <td
              :colspan="columns.length + (selectable ? 1 : 0) + ($slots.actions ? 1 : 0)"
              class="text-center py-8 text-base-content/60"
            >
              {{ emptyText || 'No data available' }}
            </td>
          </tr>
        </template>
        <template v-else>
          <tr v-for="item in sortedData" :key="item.id" class="hover">
            <th v-if="selectable">
              <label>
                <input
                  type="checkbox"
                  class="checkbox checkbox-sm"
                  :checked="selectedItems.has(item.id)"
                  @change="toggleSelect(item.id)"
                />
              </label>
            </th>
            <td v-for="col in columns" :key="col.key" :class="col.class">
              <slot :name="`cell-${col.key}`" :item="item" :value="item[col.key]">
                {{ item[col.key] }}
              </slot>
            </td>
            <td v-if="$slots.actions">
              <slot name="actions" :item="item" />
            </td>
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>
