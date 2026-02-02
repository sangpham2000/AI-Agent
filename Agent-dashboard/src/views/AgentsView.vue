<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import AppIcon from '@/components/ui/AppIcon.vue'
import agentsApi, { type Agent, type CreateAgent } from '@/api/agents'

const { t } = useI18n()

const agents = ref<Agent[]>([])
const isLoading = ref(false)
const isModalOpen = ref(false)
const isEditing = ref(false)
const currentAgent = ref<Partial<Agent>>({
  name: '',
  flowiseChatflowId: '',
  isActive: true,
  isDefault: false,
})

const fetchAgents = async () => {
  isLoading.value = true
  try {
    const response = await agentsApi.getAll()
    agents.value = response.data
  } catch (error) {
    console.error('Failed to fetch agents', error)
  } finally {
    isLoading.value = false
  }
}

const openCreateModal = () => {
  isEditing.value = false
  currentAgent.value = {
    name: '',
    flowiseChatflowId: '',
    isActive: true, // Default to active
    isDefault: false,
    description: '',
    systemPrompt: '',
    flowiseConfig: '',
  }
  isModalOpen.value = true
}

const openEditModal = (agent: Agent) => {
  isEditing.value = true
  currentAgent.value = { ...agent }
  isModalOpen.value = true
}

const saveAgent = async () => {
  try {
    const payload = {
      name: currentAgent.value.name || '',
      flowiseChatflowId: currentAgent.value.flowiseChatflowId || '',
      isActive: currentAgent.value.isActive || false,
      isDefault: currentAgent.value.isDefault || false,
      description: currentAgent.value.description,
      systemPrompt: currentAgent.value.systemPrompt,
      flowiseConfig: currentAgent.value.flowiseConfig,
    } as CreateAgent

    if (isEditing.value && currentAgent.value.id) {
      await agentsApi.update(currentAgent.value.id, { ...payload, id: currentAgent.value.id })
    } else {
      await agentsApi.create(payload)
    }
    await fetchAgents()
    isModalOpen.value = false
  } catch (error) {
    console.error('Failed to save agent', error)
    alert('Failed to save agent. Please check the inputs.')
  }
}

const deleteAgent = async (id: string) => {
  if (!confirm('Are you sure you want to delete this agent?')) return
  try {
    await agentsApi.delete(id)
    await fetchAgents()
  } catch (error) {
    console.error('Failed to delete agent', error)
  }
}

onMounted(() => {
  fetchAgents()
})
</script>

<template>
  <div class="space-y-5">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">AI Agents</h1>
        <p class="text-sm text-base-content/50 mt-0.5">
          Manage your AI assistants and their personalities.
        </p>
      </div>
      <button class="btn btn-primary btn-sm gap-2 rounded-lg" @click="openCreateModal">
        <AppIcon name="plus" class="w-4 h-4" />
        New Agent
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex justify-center py-20">
      <div class="flex flex-col items-center gap-4">
        <span class="loading loading-spinner loading-lg text-primary"></span>
        <p class="text-base-content/50 text-sm animate-pulse">Loading agents...</p>
      </div>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="agents.length === 0"
      class="flex flex-col items-center justify-center py-24 px-4 bg-base-100 rounded-2xl border border-dashed border-base-300 text-center"
    >
      <div class="w-16 h-16 rounded-2xl bg-base-200 flex items-center justify-center mb-6">
        <AppIcon name="sparkles" class="w-8 h-8 text-base-content/30" />
      </div>
      <h3 class="text-lg font-bold">No Agents Found</h3>
      <p class="text-base-content/60 max-w-sm mt-2 mb-8">
        You haven't created any AI agents yet. Set up your first agent to start automating
        conversations.
      </p>
      <button class="btn btn-primary btn-outline gap-2" @click="openCreateModal">
        <AppIcon name="plus" class="w-4 h-4" />
        Create First Agent
      </button>
    </div>

    <!-- Agents Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <div
        v-for="agent in agents"
        :key="agent.id"
        class="card bg-base-100 border border-base-200 rounded-2xl group relative overflow-hidden hover:border-base-300 transition-all duration-200"
      >
        <!-- Card Body -->
        <div class="card-body p-5">
          <!-- Header -->
          <div class="flex items-start justify-between mb-2">
            <div class="flex items-center gap-3.5">
              <div
                class="w-12 h-12 rounded-xl bg-gradient-to-br from-primary/10 to-primary/5 border border-primary/10 flex items-center justify-center text-primary group-hover:scale-110 transition-transform duration-300"
              >
                <AppIcon name="sparkles" class="w-6 h-6" />
              </div>
              <div>
                <h3 class="font-bold text-lg leading-tight">{{ agent.name }}</h3>
                <div class="flex items-center gap-2 mt-1">
                  <span
                    v-if="agent.isDefault"
                    class="badge badge-xs badge-info font-medium px-2 py-2"
                    >Default</span
                  >
                  <span
                    class="text-[10px] font-mono opacity-50 bg-base-200 px-1.5 py-0.5 rounded truncate max-w-[80px]"
                    :title="agent.flowiseChatflowId"
                  >
                    {{ agent.flowiseChatflowId.substring(0, 8) }}...
                  </span>
                </div>
              </div>
            </div>

            <!-- Action Dropdown -->
            <div class="dropdown dropdown-end">
              <button tabindex="0" class="btn btn-ghost btn-circle btn-sm min-h-0 h-8 w-8">
                <AppIcon name="dots-horizontal" class="w-5 h-5 opacity-60" />
              </button>
              <ul
                tabindex="0"
                class="dropdown-content z-[1] menu p-1 shadow-lg bg-base-100 rounded-xl w-40 border border-base-200"
              >
                <li>
                  <a @click="openEditModal(agent)" class="text-xs font-medium py-2"
                    ><AppIcon name="pencil" class="w-3.5 h-3.5" /> Edit</a
                  >
                </li>
                <li>
                  <a @click="deleteAgent(agent.id)" class="text-xs font-medium text-error py-2"
                    ><AppIcon name="trash" class="w-3.5 h-3.5" /> Delete</a
                  >
                </li>
              </ul>
            </div>
          </div>

          <!-- Description -->
          <p class="text-sm text-base-content/70 mt-3 line-clamp-2 min-h-[2.5rem]">
            {{ agent.description || 'No description provided for this agent.' }}
          </p>

          <!-- Footer -->
          <div class="mt-5 pt-4 border-t border-base-100 flex items-center justify-between">
            <div class="flex items-center gap-2 text-xs font-medium">
              <span
                :class="[
                  'w-2 h-2 rounded-full',
                  agent.isActive
                    ? 'bg-success shadow-[0_0_8px_rgba(34,197,94,0.4)]'
                    : 'bg-base-300',
                ]"
              ></span>
              <span :class="agent.isActive ? 'text-base-content' : 'text-base-content/40'">
                {{ agent.isActive ? 'Active' : 'Disabled' }}
              </span>
            </div>
            <button
              class="btn btn-xs btn-ghost text-primary hover:bg-primary/10"
              @click="openEditModal(agent)"
            >
              Configure
              <AppIcon name="arrow-right" class="w-3 h-3" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Edit/Create Modal -->
    <dialog
      class="modal modal-bottom sm:modal-middle backdrop-blur-sm"
      :class="{ 'modal-open': isModalOpen }"
    >
      <div class="modal-box w-11/12 max-w-4xl bg-base-100 shadow-2xl p-0 overflow-hidden">
        <!-- Modal Header -->
        <div
          class="px-6 py-4 border-b border-base-200 flex items-center justify-between bg-base-100 sticky top-0 z-10"
        >
          <div>
            <h3 class="font-bold text-lg">{{ isEditing ? 'Edit Agent' : 'Create New Agent' }}</h3>
            <p class="text-xs text-base-content/60">
              Configure your agent's personality and connection.
            </p>
          </div>
          <button class="btn btn-sm btn-circle btn-ghost" @click="isModalOpen = false">✕</button>
        </div>

        <!-- Modal Content -->
        <div class="p-6 md:p-8 space-y-6 max-h-[70vh] overflow-y-auto">
          <!-- Form Layout in a Single Grid for Alignment -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4">
            <!-- Agent Name (Col 1) -->
            <div class="form-control w-full">
              <label class="label pt-0 pb-1.5"
                ><span class="label-text font-semibold">Agent Name</span></label
              >
              <input
                v-model="currentAgent.name"
                type="text"
                class="input input-bordered w-full focus:input-primary transition-all"
                placeholder="e.g. Finance Assistant"
              />
            </div>

            <!-- Flowise Chatflow ID (Col 2) -->
            <div class="form-control w-full">
              <div class="label pt-0 pb-1.5 flex justify-between">
                <span class="label-text font-semibold">Flowise Chatflow ID</span>
                <a href="#" class="label-text-alt text-primary link link-hover">Where to find?</a>
              </div>
              <div class="relative">
                <input
                  v-model="currentAgent.flowiseChatflowId"
                  type="text"
                  class="input input-bordered w-full font-mono text-sm pl-10 focus:input-primary transition-all"
                  placeholder="UUID"
                />
                <div class="absolute left-3 top-1/2 -translate-y-1/2 text-base-content/40">
                  <AppIcon name="key" class="w-4 h-4" />
                </div>
              </div>
            </div>

            <!-- Description (Full Width) -->
            <div class="form-control w-full md:col-span-2">
              <label class="label pt-0 pb-1.5"
                ><span class="label-text font-semibold">Description</span></label
              >
              <textarea
                v-model="currentAgent.description"
                class="textarea textarea-bordered h-20 w-full focus:textarea-primary transition-all leading-normal"
                placeholder="Describe the purpose and capabilities of this agent..."
              ></textarea>
            </div>

            <!-- Section Divider -->
            <div
              class="md:col-span-2 divider text-xs text-base-content/40 uppercase tracking-widest font-semibold my-2"
            >
              Advanced Configuration
            </div>

            <!-- System Prompt (Full Width) -->
            <div class="form-control w-full md:col-span-2 group">
              <div class="flex items-center justify-between mb-1.5">
                <label class="label-text font-semibold text-base flex items-center gap-2">
                  System Prompt
                  <div class="tooltip" data-tip="Overrides the default system prompt in Flowise">
                    <AppIcon name="information-circle" class="w-4 h-4 opacity-50 cursor-help" />
                  </div>
                </label>
                <span class="text-xs bg-base-200 px-2 py-1 rounded">Markdown Supported</span>
              </div>
              <div class="relative">
                <textarea
                  v-model="currentAgent.systemPrompt"
                  class="textarea textarea-bordered font-mono text-sm h-48 w-full leading-relaxed focus:textarea-primary transition-all p-4"
                  placeholder="# Identity&#10;You are a helpful AI assistant...&#10;&#10;# Goals&#10;1. Assist with..."
                ></textarea>
                <div
                  class="absolute bottom-2 right-2 text-[10px] text-base-content/30 pointer-events-none"
                >
                  {{ currentAgent.systemPrompt?.length || 0 }} chars
                </div>
              </div>
            </div>

            <!-- JSON Config (Full Width) -->
            <div class="form-control w-full md:col-span-2">
              <div class="flex items-center justify-between mb-1.5">
                <label class="label-text font-semibold flex items-center gap-2">
                  Flowise Overrides
                  <span class="badge badge-xs badge-ghost">JSON</span>
                </label>
                <button
                  class="btn btn-xs btn-ghost text-xs font-normal"
                  @click.prevent="() => {} /* Add formatter logic later */"
                >
                  Format JSON
                </button>
              </div>
              <textarea
                v-model="currentAgent.flowiseConfig"
                class="textarea textarea-bordered font-mono text-xs h-32 w-full focus:textarea-primary transition-all bg-base-200/30"
                placeholder='{&#10;  "temperature": 0.7,&#10;  "streaming": true&#10;}'
              ></textarea>
            </div>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="p-6 bg-base-100 border-t border-base-200 flex items-center justify-between">
          <div class="flex items-center gap-4">
            <label
              class="label cursor-pointer gap-3 border border-base-200 p-2 rounded-lg hover:bg-base-50 transition-colors"
            >
              <span class="label-text font-medium">Active Status</span>
              <input
                v-model="currentAgent.isActive"
                type="checkbox"
                class="toggle toggle-success toggle-sm"
              />
            </label>

            <label
              class="label cursor-pointer gap-3 border border-base-200 p-2 rounded-lg hover:bg-base-50 transition-colors"
            >
              <span class="label-text font-medium">Default Agent</span>
              <input
                v-model="currentAgent.isDefault"
                type="checkbox"
                class="checkbox checkbox-primary checkbox-sm"
              />
            </label>
          </div>

          <div class="flex items-center gap-3">
            <button class="btn btn-ghost" @click="isModalOpen = false">Cancel</button>
            <button class="btn btn-primary px-8" @click="saveAgent">
              {{ isEditing ? 'Update Agent' : 'Create Agent' }}
            </button>
          </div>
        </div>
      </div>
      <div class="modal-backdrop bg-black/40" @click="isModalOpen = false"></div>
    </dialog>
  </div>
</template>
