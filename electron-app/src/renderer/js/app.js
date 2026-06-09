const statusBadge = document.getElementById('status-badge');
const searchInput = document.getElementById('search-input');
const btnDeleted = document.getElementById('btn-deleted');
const loginBtn = document.getElementById('login-btn');
const loginDialog = document.getElementById('login-dialog');
const loginUserInput = document.getElementById('login-user-input');
const loginPassInput = document.getElementById('login-pass-input');
const loginConfirmBtn = document.getElementById('login-confirm-btn');
const loginCancelBtn = document.getElementById('login-cancel-btn');
const loginError = document.getElementById('login-error');
const userBadge = document.getElementById('user-badge');

let showingDeleted = false;

function updateAuthUI() {
  const token = api.getToken();
  if (token) {
    userBadge.textContent = 'Con sesión';
    userBadge.classList.remove('hidden');
    loginBtn.classList.add('hidden');
  } else {
    userBadge.textContent = 'Sin sesión';
    userBadge.classList.remove('hidden');
    loginBtn.classList.remove('hidden');
  }
}

function showLoginDialog() {
  loginDialog.classList.remove('hidden');
  loginUserInput.focus();
}

function hideLoginDialog() {
  loginDialog.classList.add('hidden');
  loginError.classList.add('hidden');
  loginUserInput.value = '';
  loginPassInput.value = '';
}

loginBtn.addEventListener('click', showLoginDialog);

loginConfirmBtn.addEventListener('click', async () => {
  const user = loginUserInput.value.trim();
  const pass = loginPassInput.value.trim();
  if (!user || !pass) {
    loginError.textContent = 'Usuario y contraseña son obligatorios.';
    loginError.classList.remove('hidden');
    return;
  }
  loginError.classList.add('hidden');
  loginConfirmBtn.disabled = true;
  loginConfirmBtn.textContent = 'Ingresando…';
  try {
    await api.login(user, pass);
    updateAuthUI();
    hideLoginDialog();
  } catch {
    loginError.textContent = 'Credenciales inválidas.';
    loginError.classList.remove('hidden');
  } finally {
    loginConfirmBtn.disabled = false;
    loginConfirmBtn.textContent = 'Ingresar';
  }
});

loginCancelBtn.addEventListener('click', hideLoginDialog);

loginPassInput.addEventListener('keydown', (e) => {
  if (e.key === 'Enter') loginConfirmBtn.click();
});

async function loadDocuments() {
  showingDeleted = false;
  btnDeleted.textContent = '🗑️';
  documentList.showLoading();
  statusBadge.className = 'badge badge-loading';
  statusBadge.textContent = 'Conectando…';

  try {
    const docs = await api.getPublished();
    documentList.render(docs);
    statusBadge.className = 'badge badge-ok';
    statusBadge.textContent = 'Conectado';

    if (docs.length > 0) {
      const first = await api.getBySlug(docs[0].slug);
      documentViewer.render(first);
      const firstLi = document.querySelector(`li[data-slug="${docs[0].slug}"]`);
      if (firstLi) firstLi.classList.add('active');
    }
  } catch {
    documentList.showError();
    statusBadge.className = 'badge badge-error';
    statusBadge.textContent = 'Error';
  }
}

async function loadDeleted() {
  showingDeleted = true;
  btnDeleted.textContent = '✕';
  documentList.showLoading();
  statusBadge.className = 'badge badge-loading';
  statusBadge.textContent = 'Cargando eliminados…';

  try {
    const docs = await api.getDeleted();
    documentList.renderDeleted(docs);
    statusBadge.className = 'badge badge-ok';
    statusBadge.textContent = 'Eliminados';
    documentViewer.showEmpty();
  } catch {
    documentList.showError();
    statusBadge.className = 'badge badge-error';
    statusBadge.textContent = 'Error';
  }
}

let searchTimeout;
searchInput.addEventListener('input', () => {
  clearTimeout(searchTimeout);
  const text = searchInput.value.trim();
  searchTimeout = setTimeout(async () => {
    if (showingDeleted) {
      loadDocuments();
      return;
    }
    if (!text) {
      loadDocuments();
      return;
    }
    documentList.showLoading();
    try {
      const docs = await api.search(text);
      documentList.render(docs);
    } catch {
      documentList.showError();
    }
  }, 300);
});

btnDeleted.addEventListener('click', () => {
  if (showingDeleted) {
    loadDocuments();
  } else {
    loadDeleted();
  }
});

document.addEventListener('DOMContentLoaded', () => {
  updateAuthUI();
  loadDocuments();
});
