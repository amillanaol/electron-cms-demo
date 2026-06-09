const statusBadge = document.getElementById('status-badge');
const userBadge = document.getElementById('user-badge');
const loginBtn = document.getElementById('login-btn');
const logoutBtn = document.getElementById('logout-btn');
const searchInput = document.getElementById('search-input');
const loginGate = document.getElementById('login-gate');
const loginUserInput = document.getElementById('login-user-input');
const loginPassInput = document.getElementById('login-pass-input');
const loginGateBtn = document.getElementById('login-gate-btn');
const loginError = document.getElementById('login-error');
const mainEl = document.getElementById('main');
const editBtn = document.getElementById('edit-btn');
const cancelEditBtn = document.getElementById('cancel-edit-btn');
const editModal = document.getElementById('edit-modal');
const editTitle = document.getElementById('edit-title');
const editSummary = document.getElementById('edit-summary');
const editMarkdown = document.getElementById('edit-markdown');
const saveEditBtn = document.getElementById('save-edit-btn');
const closeEditBtn = document.getElementById('close-edit-btn');
const editError = document.getElementById('edit-error');
let allDocs = [];
let currentDoc = null;

function hasEditRole() {
  const role = getRole();
  return role === 'admin' || role === 'editor';
}

function updateAuthUI() {
  const token = getToken();
  const username = getUserName();
  const role = getRole();
  if (token && username) {
    userBadge.textContent = username + (role ? ' (' + role + ')' : '');
    userBadge.classList.remove('hidden');
    loginBtn.classList.add('hidden');
    logoutBtn.classList.remove('hidden');
  } else {
    userBadge.textContent = 'Sin sesión';
    userBadge.classList.remove('hidden');
    loginBtn.classList.remove('hidden');
    logoutBtn.classList.add('hidden');
  }
}

function showLoginGate() {
  loginGate.classList.remove('hidden');
  mainEl.classList.add('hidden');
  loginUserInput.focus();
}

function hideLoginGate() {
  loginGate.classList.add('hidden');
  mainEl.classList.remove('hidden');
}

loginGateBtn.addEventListener('click', async () => {
  const user = loginUserInput.value.trim();
  const pass = loginPassInput.value.trim();
  if (!user || !pass) {
    loginError.textContent = 'Usuario y contraseña son obligatorios.';
    loginError.classList.remove('hidden');
    return;
  }
  loginError.classList.add('hidden');
  loginGateBtn.disabled = true;
  loginGateBtn.textContent = 'Ingresando…';
  try {
    await api.login(user, pass);
    updateAuthUI();
    hideLoginGate();
    loadDocuments();
  } catch (err) {
    loginError.textContent = 'Credenciales inválidas.';
    loginError.classList.remove('hidden');
  } finally {
    loginGateBtn.disabled = false;
    loginGateBtn.textContent = 'Ingresar';
  }
});

loginPassInput.addEventListener('keydown', (e) => {
  if (e.key === 'Enter') loginGateBtn.click();
});

loginBtn.addEventListener('click', () => {
  showLoginGate();
});

logoutBtn.addEventListener('click', () => {
  api.logout();
  updateAuthUI();
  editModal.classList.add('hidden');
  showLoginGate();
});

function showDoc(doc) {
  currentDoc = doc;
  documentViewer.render(doc);
  const li = document.querySelector(`li[data-slug="${doc.slug}"]`);
  if (li) {
    document.querySelectorAll('#document-list li').forEach(el => el.classList.remove('active'));
    li.classList.add('active');
  }
  editBtn.classList.toggle('hidden', !hasEditRole());
  cancelEditBtn.classList.add('hidden');
}

editBtn.addEventListener('click', () => {
  if (!currentDoc) return;
  editTitle.value = currentDoc.title || '';
  editSummary.value = currentDoc.summary || '';
  editMarkdown.value = currentDoc.markdownBody || '';
  editError.classList.add('hidden');
  editModal.classList.remove('hidden');
});

cancelEditBtn.addEventListener('click', () => {
  if (currentDoc) showDoc(currentDoc);
});

closeEditBtn.addEventListener('click', () => {
  editModal.classList.add('hidden');
});

saveEditBtn.addEventListener('click', async () => {
  if (!currentDoc) return;
  const title = editTitle.value.trim();
  const summary = editSummary.value.trim();
  const markdownBody = editMarkdown.value;
  if (!title) {
    editError.textContent = 'El título es obligatorio.';
    editError.classList.remove('hidden');
    return;
  }
  if (!markdownBody) {
    editError.textContent = 'El contenido es obligatorio.';
    editError.classList.remove('hidden');
    return;
  }
  editError.classList.add('hidden');
  saveEditBtn.disabled = true;
  saveEditBtn.textContent = 'Guardando…';
  try {
    const updated = await api.updateContent(currentDoc.id, { title, summary, markdownBody });
    editModal.classList.add('hidden');
    showDoc(updated);
    loadDocuments();
  } catch (err) {
    editError.textContent = 'Error al guardar: ' + err.message;
    editError.classList.remove('hidden');
  } finally {
    saveEditBtn.disabled = false;
    saveEditBtn.textContent = '💾 Guardar';
  }
});

async function loadDocuments() {
  documentList.showLoading();
  statusBadge.className = 'badge badge-loading';
  statusBadge.textContent = 'Conectando…';

  try {
    allDocs = await api.getPublished();
    documentList.render(allDocs, showDoc);
    statusBadge.className = 'badge badge-ok';
    statusBadge.textContent = 'Conectado';

    if (allDocs.length > 0) {
      showDoc(allDocs[0]);
    }
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
    if (!text) {
      loadDocuments();
      return;
    }
    documentList.showLoading();
    try {
      const docs = await api.search(text);
      documentList.render(docs, showDoc);
    } catch {
      documentList.showError();
    }
  }, 300);
});

document.addEventListener('DOMContentLoaded', () => {
  const token = getToken();
  updateAuthUI();
  if (token) {
    hideLoginGate();
    loadDocuments();
  } else {
    documentList.showEmpty();
    showLoginGate();
  }
});
