const documentList = {
  listEl: document.getElementById('document-list'),
  loadingEl: document.getElementById('loading-msg'),
  emptyEl: document.getElementById('empty-msg'),
  errorEl: document.getElementById('error-msg'),

  showLoading() {
    this.listEl.innerHTML = '';
    this.loadingEl.classList.remove('hidden');
    this.emptyEl.classList.add('hidden');
    this.errorEl.classList.add('hidden');
  },

  showError() {
    this.listEl.innerHTML = '';
    this.loadingEl.classList.add('hidden');
    this.emptyEl.classList.add('hidden');
    this.errorEl.classList.remove('hidden');
  },

  render(docs) {
    this.loadingEl.classList.add('hidden');
    this.errorEl.classList.add('hidden');

    if (docs.length === 0) {
      this.emptyEl.classList.remove('hidden');
      this.emptyEl.textContent = 'No hay documentos publicados.';
      this.listEl.innerHTML = '';
      return;
    }

    this.emptyEl.classList.add('hidden');

    this.listEl.innerHTML = docs.map(doc => `
      <li data-slug="${doc.slug}">
        <strong>${doc.title}</strong>
        ${doc.summary ? `<br><small>${doc.summary}</small>` : ''}
        ${doc.tags && doc.tags.length > 0 ? `<br><small class="tag-list">${doc.tags.map(t => `<span class="tag-sm">${t}</span>`).join(' ')}</small>` : ''}
      </li>
    `).join('');

    this.listEl.querySelectorAll('li').forEach(li => {
      li.addEventListener('click', () => {
        documentViewer.showLoading();
        const slug = li.dataset.slug;
        api.getBySlug(slug)
          .then(doc => {
            documentViewer.render(doc);
            this.listEl.querySelectorAll('li').forEach(el => el.classList.remove('active'));
            li.classList.add('active');
          })
          .catch(() => {
            documentViewer.showError();
          });
      });
    });
  },

  renderDeleted(docs) {
    this.loadingEl.classList.add('hidden');
    this.errorEl.classList.add('hidden');

    if (docs.length === 0) {
      this.emptyEl.classList.remove('hidden');
      this.emptyEl.textContent = 'No hay documentos eliminados.';
      this.listEl.innerHTML = '';
      return;
    }

    this.emptyEl.classList.add('hidden');

    this.listEl.innerHTML = docs.map(doc => `
      <li class="deleted-item" data-id="${doc.id}">
        <strong>${doc.title}</strong>
        <br><small>Eliminado: ${doc.deletedAt ? new Date(doc.deletedAt).toLocaleString() : 'desconocido'}</small>
        <br><button class="btn-restore" data-id="${doc.id}">Restaurar</button>
      </li>
    `).join('');

    this.listEl.querySelectorAll('.btn-restore').forEach(btn => {
      btn.addEventListener('click', async (e) => {
        e.stopPropagation();
        const id = btn.dataset.id;
        btn.disabled = true;
        btn.textContent = 'Restaurando…';
        try {
          await api.restoreDocument(id);
          loadDeleted();
        } catch {
          btn.textContent = 'Error';
        }
      });
    });
  }
};
