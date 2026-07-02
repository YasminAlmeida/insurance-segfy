// Application State
let policies = [];
let activeFilter = 'all'; // 'all' or 'expiring'
let selectedPolicyIdForDelete = null;
const apiBase = '/api/policies';

// DOM Elements
const policiesTableBody = document.getElementById('policiesTableBody');
const loader = document.getElementById('loader');
const emptyState = document.getElementById('emptyState');
const tableContainer = document.getElementById('tableContainer');
const searchInput = document.getElementById('searchInput');

// Stat Elements
const statTotal = document.getElementById('statTotal');
const statActive = document.getElementById('statActive');
const statExpiring = document.getElementById('statExpiring');
const cardExpiringSoon = document.getElementById('cardExpiringSoon');

// Tabs
const tabAll = document.getElementById('tabAll');
const tabExpiring = document.getElementById('tabExpiring');

// Modal Elements
const policyModal = document.getElementById('policyModal');
const btnOpenNewModal = document.getElementById('btnOpenNewModal');
const btnCloseModal = document.getElementById('btnCloseModal');
const btnCancelModal = document.getElementById('btnCancelModal');
const policyForm = document.getElementById('policyForm');
const modalTitle = document.getElementById('modalTitle');

// Form Inputs
const inputId = document.getElementById('policyId');
const inputCpfCnpj = document.getElementById('inputCpfCnpj');
const inputPlaca = document.getElementById('inputPlaca');
const inputPremio = document.getElementById('inputPremio');
const inputDataInicio = document.getElementById('inputDataInicio');
const inputDataFim = document.getElementById('inputDataFim');
const formGlobalErrors = document.getElementById('formGlobalErrors');
const globalErrorsList = document.getElementById('globalErrorsList');

// Confirmation Modal Elements
const confirmModal = document.getElementById('confirmModal');
const deletePolicyNumber = document.getElementById('deletePolicyNumber');
const btnCancelConfirm = document.getElementById('btnCancelConfirm');
const btnConfirmDelete = document.getElementById('btnConfirmDelete');

// Toast Container
const toastContainer = document.getElementById('toastContainer');

// Initialize App
document.addEventListener('DOMContentLoaded', () => {
    loadPolicies();
    setupEventListeners();
    setupMasks();
});

// Event Listeners Configuration
function setupEventListeners() {
    // Search
    searchInput.addEventListener('input', filterAndRender);

    // Tab Filters
    tabAll.addEventListener('click', () => setFilter('all'));
    tabExpiring.addEventListener('click', () => setFilter('expiring'));
    cardExpiringSoon.addEventListener('click', () => setFilter('expiring'));

    // Modal Actions
    btnOpenNewModal.addEventListener('click', () => openFormModal());
    btnCloseModal.addEventListener('click', closeFormModal);
    btnCancelModal.addEventListener('click', closeFormModal);
    policyForm.addEventListener('submit', handleFormSubmit);

    // Confirm Actions
    btnCancelConfirm.addEventListener('click', closeConfirmModal);
    btnConfirmDelete.addEventListener('click', executeDelete);
}

// State Setter
function setFilter(filter) {
    activeFilter = filter;
    if (filter === 'all') {
        tabAll.classList.add('active');
        tabExpiring.classList.remove('active');
    } else {
        tabAll.classList.remove('active');
        tabExpiring.classList.add('active');
    }
    loadPolicies();
}

// Fetch Policies from Web API
async function loadPolicies() {
    showLoader(true);
    const url = activeFilter === 'all' ? apiBase : `${apiBase}/expiring-soon`;
    
    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error('Falha ao buscar apólices');
        
        policies = await response.json();
        
        // If we are on "all" filter, we can update the stats using these policies
        // If we are on "expiring" filter, we can update the expiring stat count
        if (activeFilter === 'all') {
            updateStatsUI(policies);
        } else {
            statExpiring.textContent = policies.length;
        }

        filterAndRender();
    } catch (error) {
        console.error(error);
        showToast('Erro ao carregar dados do servidor.', 'error');
        showLoader(false);
    }
}

// Form Submission Handler (Create / Update)
async function handleFormSubmit(e) {
    e.preventDefault();
    clearFormErrors();

    const cpfCnpj = inputCpfCnpj.value.replace(/\D/g, '');
    const placa = inputPlaca.value.trim().toUpperCase();
    const premio = parseFloat(inputPremio.value);
    const dataInicio = inputDataInicio.value;
    const dataFim = inputDataFim.value;

    // Client Side validations
    const errors = [];
    if (!cpfCnpj || (cpfCnpj.length !== 11 && cpfCnpj.length !== 14)) {
        errors.push('O documento deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos) válido.');
        setFieldInvalid('inputCpfCnpj', true);
    }
    
    const cleanPlaca = placa.replace('-', '');
    if (!cleanPlaca || cleanPlaca.length !== 7) {
        errors.push('A placa deve conter exatamente 7 caracteres (padrão AAA-9999 ou AAA9A99).');
        setFieldInvalid('inputPlaca', true);
    }

    if (isNaN(premio) || premio <= 0) {
        errors.push('O valor do prêmio deve ser superior a zero.');
        setFieldInvalid('inputPremio', true);
    }

    if (!dataInicio) {
        errors.push('Data de início de vigência é obrigatória.');
        setFieldInvalid('inputDataInicio', true);
    }

    if (!dataFim) {
        errors.push('Data de término de vigência é obrigatória.');
        setFieldInvalid('inputDataFim', true);
    }

    if (dataInicio && dataFim && new Date(dataFim) <= new Date(dataInicio)) {
        errors.push('A data de término deve ser posterior à data de início.');
        setFieldInvalid('inputDataFim', true);
    }

    if (errors.length > 0) {
        showGlobalErrors(errors);
        return;
    }

    // Call API
    const isEdit = !!inputId.value;
    const payload = {
        cpfCnpjSegurado: cpfCnpj,
        placaVeiculo: placa,
        valorPremio: premio,
        dataInicioVigencia: dataInicio,
        dataFimVigencia: dataFim
    };

    const method = isEdit ? 'PUT' : 'POST';
    const url = isEdit ? `${apiBase}/${inputId.value}` : apiBase;

    try {
        showSubmitLoading(true);
        const response = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            showToast(isEdit ? 'Apólice atualizada com sucesso!' : 'Apólice cadastrada com sucesso!', 'success');
            closeFormModal();
            loadPolicies();
        } else {
            const data = await response.json();
            if (data.errors) {
                showGlobalErrors(data.errors);
            } else if (data.message) {
                showGlobalErrors([data.message]);
            } else {
                showGlobalErrors(['Ocorreu um erro ao salvar a apólice. Tente novamente.']);
            }
        }
    } catch (error) {
        console.error(error);
        showToast('Erro de conexão com o servidor.', 'error');
    } finally {
        showSubmitLoading(false);
    }
}

// Cancel Insurance Policy (Change status to Cancelled)
async function handleCancel(id, number) {
    try {
        const response = await fetch(`${apiBase}/${id}/cancel`, {
            method: 'PUT'
        });

        if (response.ok) {
            showToast(`Apólice ${number} cancelada com sucesso!`, 'success');
            loadPolicies();
        } else {
            const data = await response.json();
            showToast(data.message || 'Falha ao cancelar apólice.', 'error');
        }
    } catch (error) {
        console.error(error);
        showToast('Erro de conexão.', 'error');
    }
}

// Delete Insurance Policy Flow
function handleDelete(id, number) {
    selectedPolicyIdForDelete = id;
    deletePolicyNumber.textContent = number;
    confirmModal.classList.remove('hidden');
}

async function executeDelete() {
    if (!selectedPolicyIdForDelete) return;

    try {
        const response = await fetch(`${apiBase}/${selectedPolicyIdForDelete}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showToast('Apólice excluída com sucesso!', 'success');
            closeConfirmModal();
            loadPolicies();
        } else {
            const data = await response.json();
            showToast(data.message || 'Falha ao excluir apólice.', 'error');
        }
    } catch (error) {
        console.error(error);
        showToast('Erro de conexão com o servidor.', 'error');
    }
}

// UI Rendering & Calculations
function filterAndRender() {
    showLoader(false);
    const search = searchInput.value.toLowerCase().replace(/[.\-\/]/g, '').trim();

    const filtered = policies.filter(p => {
        const cleanCpfCnpj = p.cpfCnpjSegurado.replace(/\D/g, '');
        const cleanPlaca = p.placaVeiculo.replace(/\D/g, '');
        return p.numeroApolice.toLowerCase().includes(search) ||
               cleanCpfCnpj.includes(search) ||
               p.placaVeiculo.toLowerCase().replace('-', '').includes(search);
    });

    if (filtered.length === 0) {
        emptyState.classList.remove('hidden');
        tableContainer.classList.add('hidden');
    } else {
        emptyState.classList.add('hidden');
        tableContainer.classList.remove('hidden');
        
        policiesTableBody.innerHTML = filtered.map(p => `
            <tr>
                <td><span class="policy-number">${p.numeroApolice}</span></td>
                <td>${p.cpfCnpjSegurado}</td>
                <td><span style="font-weight: 600;">${p.placaVeiculo}</span></td>
                <td>R$ ${p.valorPremio.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                <td>
                    ${formatDate(p.dataInicioVigencia)} - ${formatDate(p.dataFimVigencia)}
                </td>
                <td>
                    <span class="badge badge-${p.status.toLowerCase()}">${p.status}</span>
                </td>
                <td>
                    <div class="actions-cell">
                        ${p.status === 'Ativa' ? `
                            <button class="btn-link cancel" onclick="handleCancel('${p.id}', '${p.numeroApolice}')">
                                Cancelar
                            </button>
                        ` : ''}
                        <button class="btn-link edit" onclick="handleEdit('${p.id}')">
                            Editar
                        </button>
                        <button class="btn-link delete" onclick="handleDelete('${p.id}', '${p.numeroApolice}')">
                            Excluir
                        </button>
                    </div>
                </td>
            </tr>
        `).join('');
    }
}

// Edit Form Trigger
async function handleEdit(id) {
    try {
        const response = await fetch(`${apiBase}/${id}`);
        if (!response.ok) throw new Error('Não foi possível carregar a apólice');
        
        const policy = await response.json();
        openFormModal(policy);
    } catch (error) {
        console.error(error);
        showToast('Erro ao carregar detalhes para edição.', 'error');
    }
}

// UI State Updates
function updateStatsUI(allPoliciesList) {
    statTotal.textContent = allPoliciesList.length;
    
    const activeCount = allPoliciesList.filter(p => p.status === 'Ativa').length;
    statActive.textContent = activeCount;

    // Check expiring count
    const today = new Date();
    today.setHours(0,0,0,0);
    const thirtyDaysLater = new Date();
    thirtyDaysLater.setDate(today.getDate() + 30);
    thirtyDaysLater.setHours(23,59,59,999);

    const expiringCount = allPoliciesList.filter(p => {
        if (p.status !== 'Ativa') return false;
        const end = new Date(p.dataFimVigencia);
        return end >= today && end <= thirtyDaysLater;
    }).length;

    statExpiring.textContent = expiringCount;
}

// Modal Form Controllers
function openFormModal(policy = null) {
    clearFormErrors();
    policyForm.reset();

    if (policy) {
        modalTitle.textContent = 'Editar Detalhes da Apólice';
        inputId.value = policy.id;
        inputCpfCnpj.value = policy.cpfCnpjSegurado;
        inputPlaca.value = policy.placaVeiculo;
        inputPremio.value = policy.valorPremio;
        inputDataInicio.value = policy.dataInicioVigencia.substring(0, 10);
        inputDataFim.value = policy.dataFimVigencia.substring(0, 10);
    } else {
        modalTitle.textContent = 'Cadastrar Nova Apólice';
        inputId.value = '';
        // Set default dates: start as today, end as today + 1 year
        const today = new Date();
        const nextYear = new Date();
        nextYear.setFullYear(today.getFullYear() + 1);

        inputDataInicio.value = today.toISOString().substring(0, 10);
        inputDataFim.value = nextYear.toISOString().substring(0, 10);
    }

    policyModal.classList.remove('hidden');
    inputCpfCnpj.focus();
}

function closeFormModal() {
    policyModal.classList.add('hidden');
}

function closeConfirmModal() {
    confirmModal.classList.add('hidden');
    selectedPolicyIdForDelete = null;
}

// Validation Error UI
function clearFormErrors() {
    formGlobalErrors.classList.add('hidden');
    globalErrorsList.innerHTML = '';
    
    const inputs = ['inputCpfCnpj', 'inputPlaca', 'inputPremio', 'inputDataInicio', 'inputDataFim'];
    inputs.forEach(id => {
        const el = document.getElementById(id);
        el.style.borderColor = 'var(--glass-border)';
    });
}

function showGlobalErrors(errors) {
    formGlobalErrors.classList.remove('hidden');
    globalErrorsList.innerHTML = errors.map(err => `<li>${err}</li>`).join('');
}

function setFieldInvalid(id, isInvalid) {
    const el = document.getElementById(id);
    el.style.borderColor = isInvalid ? 'var(--danger)' : 'var(--glass-border)';
}

// Input Masks Setup
function setupMasks() {
    // Mask CPF/CNPJ
    inputCpfCnpj.addEventListener('input', (e) => {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length <= 11) {
            // CPF Mask: 000.000.000-00
            value = value.replace(/(\d{3})(\d)/, '$1.$2');
            value = value.replace(/(\d{3})(\d)/, '$1.$2');
            value = value.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
        } else {
            // CNPJ Mask: 00.000.000/0000-00
            value = value.substring(0, 14);
            value = value.replace(/^(\d{2})(\d)/, '$1.$2');
            value = value.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
            value = value.replace(/\.(\d{3})(\d)/, '.$1/$2');
            value = value.replace(/(\d{4})(\d)/, '$1-$2');
        }
        e.target.value = value;
    });

    // Mask Placa
    inputPlaca.addEventListener('input', (e) => {
        let value = e.target.value.replace(/[^a-zA-Z0-9]/g, '').toUpperCase();
        
        // Auto-add dash if traditional plate
        // Standard plate is AAA-9999
        if (value.length > 3) {
            // Check if Mercosul AAA1A11 or traditional AAA-1111
            // Mercosul fifth char is a letter, traditional is numeric
            if (value.length === 7) {
                // If it is Mercosul format, don't insert dash
                const isMercosul = isLetter(value[4]);
                if (!isMercosul) {
                    e.target.value = `${value.substring(0, 3)}-${value.substring(3)}`;
                    return;
                }
            } else {
                // By default while typing we format with dash: AAA-1234
                e.target.value = `${value.substring(0, 3)}-${value.substring(3, 7)}`;
                return;
            }
        }
        e.target.value = value.substring(0, 8);
    });
}

function isLetter(char) {
    return char && /[a-zA-Z]/.test(char);
}

// UI Helpers
function showLoader(show) {
    if (show) {
        loader.classList.remove('hidden');
        emptyState.classList.add('hidden');
        tableContainer.classList.add('hidden');
    } else {
        loader.classList.add('hidden');
    }
}

function showSubmitLoading(isLoading) {
    const submitBtn = document.getElementById('btnSubmitForm');
    if (isLoading) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = 'Salvando...';
    } else {
        submitBtn.disabled = false;
        submitBtn.innerHTML = 'Salvar Apólice';
    }
}

function formatDate(dateStr) {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toLocaleDateString('pt-BR');
}

function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    
    toast.innerHTML = `
        <span>${message}</span>
    `;

    toastContainer.appendChild(toast);

    // Auto-remove after 4 seconds
    setTimeout(() => {
        toast.classList.add('removing');
        toast.addEventListener('animationend', () => {
            toast.remove();
        });
    }, 4000);
}
